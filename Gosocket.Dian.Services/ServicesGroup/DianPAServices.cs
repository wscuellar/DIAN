using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Models;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static Gosocket.Dian.Logger.Logger;

namespace Gosocket.Dian.Services.ServicesGroup
{
    public class DianPAServices : IDisposable
    {
        private TableManager TableManagerDianFileMapper = new TableManager("DianFileMapper");
        private TableManager TableManagerDianProcessResult = new TableManager("DianProcessResult");
        private TableManager TableManagerGlobalBigContributorRequestAuthorization = new TableManager("GlobalBigContributorRequestAuthorization");
        private TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private TableManager TableManagerGlobalDocValidatorDocument = new TableManager("GlobalDocValidatorDocument");
        private TableManager TableManagerGlobalDocValidatorRuntime = new TableManager("GlobalDocValidatorRuntime");
        private TableManager TableManagerGlobalDocValidatorTracking = new TableManager("GlobalDocValidatorTracking");

        private TableManager TableManagerGlobalBatchFileMapper = new TableManager("GlobalBatchFileMapper");
        private TableManager TableManagerGlobalBatchFileRuntime = new TableManager("GlobalBatchFileRuntime");
        private TableManager TableManagerGlobalBatchFileResult = new TableManager("GlobalBatchFileResult");
        private TableManager TableManagerGlobalBatchFileStatus = new TableManager("GlobalBatchFileStatus");
        private TableManager TableManagerGlobalContributor = new TableManager("GlobalContributor");
        private TableManager TableManagerGlobalDocRegisterProviderAR = new TableManager("GlobalDocRegisterProviderAR");

        private TableManager TableManagerGlobalNumberRange = new TableManager("GlobalNumberRange");
        //private TableManager TableManagerDianOfeControl = new TableManager("DianOfeControl");
        private TableManager TableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");

        private TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private TableManager TableManagerGlobalDocReferenceAttorney = new TableManager("GlobalDocReferenceAttorney");
        private TableManager TableManagerGlobalDocHolderExchange = new TableManager("GlobalDocHolderExchange");

        private TableManager TableManagerGlobalDocEvent = new TableManager("GlobalDocEvent");

        private FileManager fileManager = new FileManager();

        private readonly string blobContainer = "global";
        private readonly string blobContainerFolder = "batchValidator";

        public DianPAServices()
        {

        }

        public void Dispose()
        {
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <param name="authCode"></param>
        /// <param name="testSetId"></param>
        /// <returns></returns>
        public UploadDocumentResponse ProcessBatchZipFile(string fileName, byte[] contentFile, string authCode = null, string testSetId = null)
        {
            var zipKey = Guid.NewGuid().ToString();
            UploadDocumentResponse responseMessages = new UploadDocumentResponse
            {
                ZipKey = zipKey
            };
            var utcNow = DateTime.UtcNow;
            var blobPath = $"{utcNow.Year}/{utcNow.Month.ToString().PadLeft(2, '0')}/{utcNow.Day.ToString().PadLeft(2, '0')}";
            var result = fileManager.Upload(blobContainer, $"{blobContainerFolder}/{blobPath}/{zipKey}.zip", contentFile);
            if (!result)
            {
                responseMessages.ZipKey = "";
                responseMessages.ErrorMessageList = new List<XmlParamsResponseTrackId>
                {
                    new XmlParamsResponseTrackId { Success = false, ProcessedMessage = "Error al almacenar archivo zip." }
                };
                return responseMessages;
            }

            TableManagerGlobalBatchFileRuntime.InsertOrUpdate(new GlobalBatchFileRuntime(zipKey, "UPLOAD", fileName));
            TableManagerGlobalBatchFileMapper.InsertOrUpdate(new GlobalBatchFileMapper(fileName, zipKey));
            TableManagerGlobalBatchFileStatus.InsertOrUpdate(new GlobalBatchFileStatus(zipKey, zipKey)
            {
                AuthCode = authCode,
                FileName = fileName,
                StatusCode = "",
                StatusDescription = "",
                ZipKey = zipKey
            });

            var request = new { authCode, blobPath, testSetId, zipKey };
            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "Process.Batch.Zip.Event",
                    Data = JsonConvert.SerializeObject(request),
                    EventTime = DateTime.UtcNow,
                    Subject = $"|BATCH.DOCUMENTS.ZIP|",
                    DataVersion = "2.0"
                }
            };

            EventGridManager.Instance().SendMessagesToEventGrid(eventsList);

            return responseMessages;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        public DianResponse UploadDocumentSync(string fileName, byte[] contentFile, string authCode = null)
        {

            var start = DateTime.UtcNow;
            var globalStart = DateTime.UtcNow;
            var contentFileList = contentFile.ExtractMultipleZip();
            var unzip = new GlobalLogger("", "1 Unzip") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };

            // ZONE 1
            start = DateTime.UtcNow;
            DianResponse dianResponse = new DianResponse
            {
                IsValid = false,
                XmlFileName = contentFileList.First().XmlFileName,
                ErrorMessage = new List<string>()
            };

            if (contentFileList.Any(f => f.MaxQuantityAllowedFailed || f.UnzipError))
            {
                var countZeroMess = "Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML valido.";
                var countOverOneMess = "Encontrados mas de un XML descomprimiendo el archivo ZIP: Se debe enviar solo un fichero XML.";

                dianResponse.StatusCode = "89";
                dianResponse.StatusMessage = contentFileList.Any(f => f.MaxQuantityAllowedFailed) ? countOverOneMess : countZeroMess;
                dianResponse.XmlFileName = fileName;
                return dianResponse;
            }

            if (contentFileList.First().HasError || contentFileList.Count > 1)
            {
                dianResponse.XmlFileName = contentFileList.First().XmlFileName;
                dianResponse.StatusMessage = contentFileList.First().XmlErrorMessage;

                if (contentFileList.Count > 1)
                    dianResponse.StatusMessage = "El método síncrono solo puede recibir un documento.";

                dianResponse.StatusCode = "89";
                return dianResponse;
            }
            var zone1 = new GlobalLogger("", "Zone 1") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // ZONE 1

            // ZONE 2
            start = DateTime.UtcNow;
            var xmlBase64 = Convert.ToBase64String(contentFileList.First().XmlBytes);
            var zone2 = new GlobalLogger("", "Zone 2") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // ZONE 2


            // Parser
            //start = DateTime.UtcNow;
            //var xmlBytes = contentFileList.First().XmlBytes;
            //var xmlParser = new XmlParser(xmlBytes);
            //if (!xmlParser.Parser())
            //    throw new Exception(xmlParser.ParserError);

            start = DateTime.UtcNow;
            var xmlBytes = contentFileList.First().XmlBytes;
            XmlParser xmlParser = new XmlParser();
            try
            {
                xmlParser = new XmlParser(xmlBytes);
                if (!xmlParser.Parser())
                    throw new Exception(xmlParser.ParserError);
            }
            catch (Exception ex)
            {
                var failedList = new List<string> { $"Regla: ZB01, Rechazo: Fallo en el esquema XML del archivo" };
                dianResponse.IsValid = false;
                dianResponse.StatusCode = "99";
                dianResponse.StatusMessage = "Validación contiene errores en campos mandatorios. " + ex.Message;
                dianResponse.StatusDescription = "Documento con errores en campos mandatorios.";
                dianResponse.ErrorMessage.AddRange(failedList);
                return dianResponse;
            }
          
            var documentParsed = xmlParser.Fields.ToObject<DocumentParsed>();
            DocumentParsed.SetValues(ref documentParsed);
            var parser = new GlobalLogger("", "Parser") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // Parser

            // ZONE 3
            start = DateTime.UtcNow;
            //Validar campos mandatorios basicos para el trabajo del WS
            if (!DianServicesUtils.ValidateParserValuesSync(documentParsed, ref dianResponse)) return dianResponse;

            var senderCode = documentParsed.SenderCode;
            var docTypeCode = documentParsed.DocumentTypeId;
            var serie = documentParsed.Serie;
            var serieAndNumber = documentParsed.SerieAndNumber;
            var trackId = documentParsed.DocumentKey.ToLower();
            var eventCode = documentParsed.ResponseCode;
            var zone3 = new GlobalLogger("", "Zone 3") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // ZONE 3

            // Auth
            start = DateTime.UtcNow;
            var authEntity = GetAuthorization(senderCode, authCode);
            if (authEntity == null)
            {
                dianResponse.XmlFileName = $"{fileName}";
                dianResponse.StatusCode = "89";
                dianResponse.StatusDescription = $"NIT {authCode} no autorizado a enviar documentos para emisor con NIT {senderCode}.";
                var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                if (globalEnd >= 10)
                {
                    var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", trackId) { Message = globalEnd.ToString(), Action = "Auth" };
                    TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
                }
                return dianResponse;
            }
            var auth = new GlobalLogger("", "3 Auth") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // Auth

            // Duplicity
            start = DateTime.UtcNow;
            var response = CheckDocumentDuplicity(senderCode, docTypeCode, serie, serieAndNumber, trackId, eventCode);
            if (response != null) return response;
            var duplicity = new GlobalLogger(trackId, "Duplicity") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // Duplicity

            unzip.PartitionKey = trackId;
            parser.PartitionKey = trackId;
            auth.PartitionKey = trackId;
            zone1.PartitionKey = trackId;
            zone2.PartitionKey = trackId;
            zone3.PartitionKey = trackId;

            // ZONE MAPPER
            start = DateTime.UtcNow;
            if (contentFileList.First().XmlFileName.Split('/').Count() > 1 && contentFileList.First().XmlFileName.Split('/').Last() != null)
                contentFileList.First().XmlFileName = contentFileList.First().XmlFileName.Split('/').Last();

            var trackIdMapperEntity = new GlobalOseTrackIdMapper(contentFileList[0].XmlFileName, trackId);
            TableManagerDianFileMapper.InsertOrUpdate(trackIdMapperEntity);
            var mapper = new GlobalLogger(trackId, "Zone 4 Mapper") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // ZONE MAPPER

            // upload xml
            start = DateTime.UtcNow;
            var uploadXmlRequest = new { xmlBase64, fileName = contentFileList[0].XmlFileName, documentTypeId = docTypeCode, trackId };
            var uploadXmlResponse = ApiHelpers.ExecuteRequest<ResponseUploadXml>(ConfigurationManager.GetValue("UploadXmlUrl"), uploadXmlRequest);
            if (!uploadXmlResponse.Success)
            {
                dianResponse.XmlFileName = $"{fileName}";
                dianResponse.StatusCode = "89";
                dianResponse.StatusDescription = uploadXmlResponse.Message;
                var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                if (globalEnd >= 10)
                {
                    var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", trackId) { Message = globalEnd.ToString(), Action = "Upload" };
                    TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
                }
                return dianResponse;
            }
            var upload = new GlobalLogger(trackId, "5 Upload") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // upload xml

            // send to validate document sync
            start = DateTime.UtcNow;
            var requestObjTrackId = new { trackId, draft = "false" };
            var validations = ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("ValidateDocumentUrl"), requestObjTrackId);
            var validate = new GlobalLogger(trackId, "6 Validate") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // send to validate document sync

            if (validations.Count == 0)
            {
                dianResponse.XmlFileName = contentFileList.First().XmlFileName;
                dianResponse.StatusDescription = string.Empty;
                dianResponse.StatusCode = "66";
                var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                if (globalEnd >= 10)
                {
                    var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", trackId) { Message = globalEnd.ToString(), Action = "Validate" };
                    TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
                }
                return dianResponse;
            }
            else
            {
                // ZONE APPLICATION
                start = DateTime.UtcNow;
                string message = "";
                bool existDocument = false;
                GlobalDocValidatorDocumentMeta documentMeta = null;
                List<Task> arrayTasks = new List<Task>();

                Task secondLocalRun = Task.Run(() =>
                {
                    documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                    //var prefix = !string.IsNullOrEmpty(serie) ? serie : string.Empty;
                    message = $"La {documentMeta.DocumentTypeName} {serieAndNumber}, ha sido autorizada."; // (string.IsNullOrEmpty(prefix)) ? $"La {documentMeta.DocumentTypeName} {serieAndNumber}, ha sido autorizada." : $"La {documentMeta.DocumentTypeName} {prefix}-{number}, ha sido autorizada.";
                    existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
                });

                var errors = validations.Where(r => !r.IsValid && r.Mandatory).ToList();
                var notifications = validations.Where(r => r.IsNotification).ToList();

                if (!errors.Any() && !notifications.Any())
                {
                    dianResponse.IsValid = true;
                    dianResponse.StatusMessage = message;
                }

                if (errors.Any())
                {
                    var failedList = new List<string>();
                    foreach (var f in errors)
                        failedList.Add($"Regla: {f.ErrorCode}, Rechazo: {f.ErrorMessage}");

                    dianResponse.IsValid = false;
                    dianResponse.StatusMessage = "Documento con errores en campos mandatorios.";
                    dianResponse.ErrorMessage.AddRange(failedList);
                }

                if (notifications.Any())
                {
                    var notificationList = new List<string>();
                    foreach (var n in notifications)
                        notificationList.Add($"Regla: {n.ErrorCode}, Notificación: {n.ErrorMessage}");

                    dianResponse.IsValid = errors.Any() ? false : true;
                    dianResponse.StatusMessage = errors.Any() ? "Documento con errores en campos mandatorios." : message;
                    dianResponse.ErrorMessage.AddRange(notificationList);
                }

                arrayTasks.Add(secondLocalRun);
                Task.WhenAll(arrayTasks).Wait();

                var applicationResponse = XmlUtil.GetApplicationResponseIfExist(documentMeta);
                dianResponse.XmlBase64Bytes = applicationResponse ?? XmlUtil.GenerateApplicationResponseBytes(trackId, documentMeta, validations);

                dianResponse.XmlDocumentKey = trackId;

                GlobalDocValidatorDocument validatorDocument = null;
                if (dianResponse.IsValid)
                {
                    dianResponse.StatusCode = "00";
                    dianResponse.StatusMessage = message;
                    dianResponse.StatusDescription = "Procesado Correctamente.";
                    validatorDocument = new GlobalDocValidatorDocument(documentMeta?.Identifier, documentMeta?.Identifier) { DocumentKey = trackId, EmissionDateNumber = documentMeta?.EmissionDate.ToString("yyyyMMdd") };
                }
                else
                {
                    dianResponse.IsValid = false;
                    dianResponse.StatusCode = "99";
                    dianResponse.StatusDescription = "Validación contiene errores en campos mandatorios.";
                }
                var application = new GlobalLogger(trackId, "7 Aplication SendBillSync") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
                // ZONE APPLICATION

                // LAST ZONE
                start = DateTime.UtcNow;
                arrayTasks = new List<Task>
                {
                    TableManagerGlobalLogger.InsertOrUpdateAsync(unzip),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(parser),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(auth),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(duplicity),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(upload),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(validate),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(application),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone1),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone2),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone3),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(mapper),
                };
                if (dianResponse.IsValid && !existDocument)
                    arrayTasks.Add(TableManagerGlobalDocValidatorDocument.InsertOrUpdateAsync(validatorDocument));

                Task.WhenAll(arrayTasks);

                var lastZone = new GlobalLogger(trackId, "Last Zone") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
                TableManagerGlobalLogger.InsertOrUpdate(lastZone);
                // LAST ZONE

                return dianResponse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ExchangeEmailResponse GetExchangeEmails(string authCode)
        {
            var contributor = GetGlobalContributor(authCode);
            if (contributor == null)
                return new ExchangeEmailResponse { StatusCode = "89", Success = false, Message = $"NIT {authCode} no autorizado para consultar correos de recepción de facturas.", CsvBase64Bytes = null };

            var bytes = fileManager.GetBytes("dian", $"exchange/emails.csv");
            var response = new ExchangeEmailResponse { StatusCode = "0", Success = true, CsvBase64Bytes = bytes };
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public List<DianResponse> GetBatchStatus(string trackId)
        {
            var batchStatus = TableManagerGlobalBatchFileStatus.Find<GlobalBatchFileStatus>(trackId, trackId);
            if (batchStatus == null) return GetStatusZip(trackId);

            var responses = new List<DianResponse>();

            if (batchStatus != null && !string.IsNullOrEmpty(batchStatus.StatusCode))
            {
                responses.Add(new DianResponse
                {
                    StatusCode = batchStatus.StatusCode,
                    StatusDescription = batchStatus.StatusDescription
                });
                return responses;
            }

            var resultsEntities = TableManagerGlobalBatchFileResult.FindByPartition<GlobalBatchFileResult>(trackId);
            if (resultsEntities.Count == 1) return GetStatusZip(trackId);

            var exist = fileManager.Exists(blobContainer, $"{blobContainerFolder}/applicationResponses/{trackId}.zip");
            if (!exist)
            {
                responses.Add(new DianResponse
                {
                    StatusCode = batchStatus.StatusCode,
                    StatusDescription = "Batch en proceso de validación."
                });
                return responses;
            }

            if (exist)
            {
                var zipBytes = fileManager.GetBytes(blobContainer, $"{blobContainerFolder}/applicationResponses/{trackId}.zip");
                if (zipBytes != null)
                {
                    responses.Add(new DianResponse { IsValid = true, StatusCode = "00", StatusDescription = "Procesado Correctamente.", XmlBase64Bytes = zipBytes });
                    return responses;
                }
            }

            return responses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public DianResponse GetStatus(string trackId)
        {
            var globalStart = DateTime.UtcNow;
            var start = DateTime.UtcNow;

            var response = new DianResponse() { ErrorMessage = new List<string>() };
            var validatorRuntimes = TableManagerGlobalDocValidatorRuntime.FindByPartition(trackId);
            var runtime = new GlobalLogger(trackId, "1 GetStatus Runtime") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };

            //if (validatorRuntimes.Any(v => v.RowKey == "UPLOAD")) isUploaded = true;
            if (validatorRuntimes.Any(v => v.RowKey == "UPLOAD"))
            {
                //var isFinished = DianServicesUtils.CheckIfDocumentValidationsIsFinished(trackId);
                if (validatorRuntimes.Any(v => v.RowKey == "END"))
                {
                    start = DateTime.UtcNow;
                    GlobalDocValidatorDocumentMeta documentMeta = null;
                    bool applicationResponseExist = false;
                    bool existDocument = false;
                    //bool existNsu = false;
                    var validations = new List<GlobalDocValidatorTracking>();
                    List<Task> arrayTasks = new List<Task>();

                    //Task firstLocalRun = Task.Run(() =>
                    //{
                    //    var applicationResponse = ApiHelpers.ExecuteRequest<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), new { trackId });
                    //    response.XmlBase64Bytes = !applicationResponse.Success ? null : applicationResponse.Content;
                    //    if (!applicationResponse.Success)
                    //        Debug.WriteLine(applicationResponse.Message);
                    //});

                    Task secondLocalRun = Task.Run(() =>
                    {
                        documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                        if (!string.IsNullOrEmpty(documentMeta.Identifier))
                            existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
                        applicationResponseExist = XmlUtil.ApplicationResponseExist(documentMeta);
                    });

                    Task fourLocalRun = Task.Run(() =>
                    {
                        validations = TableManagerGlobalDocValidatorTracking.FindByPartition<GlobalDocValidatorTracking>(trackId);
                        //validations = ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("GetValidationsByTrackIdUrl"), new { trackId });
                    });

                    //arrayTasks.Add(firstLocalRun);
                    arrayTasks.Add(secondLocalRun);
                    arrayTasks.Add(fourLocalRun);
                    Task.WhenAll(arrayTasks).Wait();

                    var applicationResponse = XmlUtil.GetApplicationResponseIfExist(documentMeta);
                    response.XmlBase64Bytes = (applicationResponse != null) ? XmlUtil.GenerateApplicationResponseBytes(trackId, documentMeta, validations) : null;

                    response.XmlDocumentKey = trackId;
                    response.XmlFileName = documentMeta.FileName;

                    if (documentMeta == null)
                    {
                        response.StatusCode = "66";
                        response.StatusDescription = "TrackId no encontrado.";
                        return response;
                    }

                    var failed = validations.Where(r => r.Mandatory && !r.IsValid).ToList();
                    var notifications = validations.Where(r => r.IsNotification).ToList();
                    var message = (string.IsNullOrEmpty(documentMeta.Serie)) ? $"La {documentMeta.DocumentTypeName} {documentMeta.Number}, ha sido autorizada." : $"La {documentMeta.DocumentTypeName} {documentMeta.Serie}-{documentMeta.Number}, ha sido autorizada.";

                    if (!failed.Any() && !notifications.Any())
                    {
                        response.IsValid = true;
                        response.StatusMessage = message;
                    }

                    if (failed.Any() && !applicationResponseExist)
                    {
                        var failedList = new List<string>();
                        foreach (var f in failed)
                            failedList.Add($"Regla: {f.ErrorCode}, Rechazo: {f.ErrorMessage}");

                        response.IsValid = false;
                        response.StatusMessage = "Documento con errores en campos mandatorios.";
                        response.ErrorMessage.AddRange(failedList);
                    }

                    if (notifications.Any())
                    {
                        var notificationList = new List<string>();
                        foreach (var n in notifications)
                            notificationList.Add($"Regla: {n.ErrorCode}, Notificación: {n.ErrorMessage}");

                        response.IsValid = failed.Any() ? response.IsValid : true;
                        response.StatusMessage = failed.Any() ? response.StatusMessage : message;
                        response.ErrorMessage.AddRange(notificationList);
                    }

                    if (response.IsValid || applicationResponseExist)
                    {
                        response.IsValid = true;
                        response.StatusCode = "00";
                        response.StatusMessage = message;
                        response.StatusDescription = "Procesado Correctamente.";
                    }
                    else
                    {
                        response.StatusCode = "99";
                        response.StatusDescription = "Validación contiene errores en campos mandatorios.";
                    }
                }
                else
                {
                    response.StatusCode = "98";
                    response.StatusDescription = "En Proceso";
                }
            }
            else
            {
                response.StatusCode = "66";
                response.StatusDescription = "TrackId no existe en los registros de la DIAN.";
            }

            var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
            var finish = new GlobalLogger("GetStatus", trackId) { Message = globalEnd.ToString() };
            TableManagerGlobalLogger.InsertOrUpdate(finish);

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public List<DianResponse> GetStatusZip(string trackId)
        {
            var globalStart = DateTime.UtcNow;
            var responses = new List<DianResponse>();

            bool existsTrackId = false;
            var resultsEntities = TableManagerGlobalBatchFileResult.FindByPartition<GlobalBatchFileResult>(trackId);
            if (resultsEntities.Any())
                existsTrackId = true;

            if (existsTrackId)
            {
                foreach (var item in resultsEntities)
                {
                    try
                    {
                        var response = new DianResponse();
                        if (item.StatusCode == 0)
                        {
                            response.StatusCode = item.StatusCode.ToString();
                            response.StatusDescription = item.StatusDescription;
                            responses.Add(response);
                            continue;
                        }
                        response = GetStatus(item.RowKey);
                        responses.Add(response);
                    }
                    catch (Exception ex)
                    {
                        Log("GetStatusZip", (int)InsightsLogType.Error, ex.Message);
                        Log("GetStatusZip", (int)InsightsLogType.Error, ex.ToStringMessage());
                        responses.Add(new DianResponse
                        {
                            StatusCode = "66",
                            StatusDescription = "Error al generar ApplicationResponse. Inténtelo más tarde."
                        });
                        continue;
                    }
                }
            }
            else
            {
                responses.Add(new DianResponse
                {
                    StatusCode = "66",
                    StatusDescription = "TrackId no existe en los registros de la DIAN."
                });
                return responses;
            }

            var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
            var finish = new GlobalLogger("GetStatusZip", trackId) { Message = globalEnd.ToString() };
            TableManagerGlobalLogger.InsertOrUpdate(finish);


            return responses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public DianResponse GetStatusEvent(string trackId)
        {
            var globalStart = DateTime.UtcNow;
            var start = DateTime.UtcNow;

            var response = new DianResponse() { ErrorMessage = new List<string>() };
            var validatorRuntimes = TableManagerGlobalDocValidatorRuntime.FindByPartition(trackId);
            var runtime = new GlobalLogger(trackId, "1 GetStatus Runtime") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };

            if (validatorRuntimes.Any(v => v.RowKey == "UPLOAD"))
            {
                if (validatorRuntimes.Any(v => v.RowKey == "END"))
                {
                    start = DateTime.UtcNow;
                    GlobalDocValidatorDocumentMeta documentMeta = null;
                    bool applicationResponseExist = false;
                    bool existDocument = false;
                    var validations = new List<GlobalDocValidatorTracking>();
                    List<Task> arrayTasks = new List<Task>();
                    var events = new List<GlobalDocValidatorDocumentMeta>();
                    var originalEvents = new List<GlobalDocValidatorDocumentMeta>();
                    var originalEventsValidations = new List<GlobalDocValidatorTracking>();
                    var atLeastOneApproved = false;

                    Task firstLocalRun = Task.Run(() =>
                    {
                        documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                        if (!string.IsNullOrEmpty(documentMeta.Identifier))
                            existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
                        applicationResponseExist = XmlUtilEvents.ApplicationResponseExist(documentMeta);
                    });

                    Task secondLocalRun = Task.Run(() =>
                    {
                        // se consultan los eventos asociados al trackId.
                        events = TableManagerGlobalDocValidatorDocumentMeta.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(trackId, "96");
                        if (events != null && events.Count > 0)
                        {
                            events = events.OrderBy(x => x.SigningTimeStamp).ToList();
                            events.ForEach(e =>
                            {
                                var approved = TableManagerGlobalDocValidatorDocument.FindByDocumentKey<GlobalDocValidatorDocument>(e?.Identifier, e?.Identifier, e?.PartitionKey);
                                if (approved != null)
                                {
                                    atLeastOneApproved = true;
                                    // se consulta el evento por el código y así obtener su descripción.
                                    var docEvent = TableManagerGlobalDocEvent.FindGlobalEvent<GlobalDocEvent>(e.EventCode, e.CustomizationID, "96");
                                    e.EventCodeDescription = (docEvent != null) ? docEvent.Description : string.Empty;
                                    // se consulta la información del evento original.
                                    originalEvents.Add(TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(e.DocumentKey, e.DocumentKey));
                                    // se consulta las validaciones del evento original.
                                    var originalValidations = TableManagerGlobalDocValidatorTracking.FindByPartition<GlobalDocValidatorTracking>(e.DocumentKey);
                                    if (originalValidations != null && originalValidations.Count > 0)
                                    {
                                        originalEventsValidations.AddRange(originalValidations);
                                    }
                                }
                            });
                        }
                    });

                    Task thirdLocalRun = Task.Run(() =>
                    {
                        validations = TableManagerGlobalDocValidatorTracking.FindByPartition<GlobalDocValidatorTracking>(trackId);
                    });

                    arrayTasks.Add(firstLocalRun);
                    arrayTasks.Add(secondLocalRun);
                    arrayTasks.Add(thirdLocalRun);
                    Task.WhenAll(arrayTasks).Wait();

                    var applicationResponse = XmlUtilEvents.GetApplicationResponseIfExist(documentMeta);
                    response.XmlBase64Bytes = (applicationResponse != null) ? XmlUtilEvents.GenerateApplicationResponseBytes(trackId, documentMeta, validations, events, originalEvents, originalEventsValidations) : null;

                    response.XmlDocumentKey = trackId;
                    response.XmlFileName = documentMeta.FileName;

                    if (!atLeastOneApproved)
                    {
                        response.StatusCode = "67";
                        response.StatusDescription = "EL CUFE o Factura consultada no tiene a la fecha eventos asociados.";
                        return response;
                    }

                    if (documentMeta == null)
                    {
                        response.StatusCode = "66";
                        response.StatusDescription = "TrackId no encontrado.";
                        return response;
                    }

                    var failed = validations.Where(r => r.Mandatory && !r.IsValid).ToList();
                    var notifications = validations.Where(r => r.IsNotification).ToList();
                    var message = (string.IsNullOrEmpty(documentMeta.Serie)) ? $"La {documentMeta.DocumentTypeName} {documentMeta.Number}, ha sido autorizada." : $"La {documentMeta.DocumentTypeName} {documentMeta.Serie}-{documentMeta.Number}, ha sido autorizada.";

                    if (!failed.Any() && !notifications.Any())
                    {
                        response.IsValid = true;
                        response.StatusMessage = message;
                    }

                    if (failed.Any() && !applicationResponseExist)
                    {
                        var failedList = new List<string>();
                        foreach (var f in failed)
                            failedList.Add($"Regla: {f.ErrorCode}, Rechazo: {f.ErrorMessage}");

                        response.IsValid = false;
                        response.StatusMessage = "Documento con errores en campos mandatorios.";
                        response.ErrorMessage.AddRange(failedList);
                    }

                    if (notifications.Any())
                    {
                        var notificationList = new List<string>();
                        foreach (var n in notifications)
                            notificationList.Add($"Regla: {n.ErrorCode}, Notificación: {n.ErrorMessage}");

                        response.IsValid = failed.Any() ? response.IsValid : true;
                        response.StatusMessage = failed.Any() ? response.StatusMessage : message;
                        response.ErrorMessage.AddRange(notificationList);
                    }

                    if (response.IsValid || applicationResponseExist)
                    {
                        response.IsValid = true;
                        response.StatusCode = "00";
                        response.StatusMessage = message;
                        response.StatusDescription = "Procesado Correctamente.";
                    }
                    else
                    {
                        response.StatusCode = "99";
                        response.StatusDescription = "Validación contiene errores en campos mandatorios.";
                    }
                }
                else
                {
                    response.StatusCode = "98";
                    response.StatusDescription = "En Proceso";
                }
            }
            else
            {
                response.StatusCode = "66";
                response.StatusDescription = "TrackId no existe en los registros de la DIAN.";
            }

            var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
            var finish = new GlobalLogger("GetStatus", trackId) { Message = globalEnd.ToString() };
            TableManagerGlobalLogger.InsertOrUpdate(finish);

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        public DianResponse SendEventUpdateStatus(byte[] contentFile, string authCode)
        {
            var start = DateTime.UtcNow;
            var globalStart = DateTime.UtcNow;
            var contentFileList = contentFile.ExtractMultipleZip();
            List<Task> arrayTasks = new List<Task>();
            var unzip = new GlobalLogger(string.Empty, Properties.Settings.Default.Param_GlobalLogger)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "Unzip"
            };

            // ZONE 1
            start = DateTime.UtcNow;
            DianResponse dianResponse = new DianResponse
            {
                IsValid = false,
                XmlFileName = contentFileList.First().XmlFileName,
                ErrorMessage = new List<string>()
            };

            if (contentFileList.Any(f => f.MaxQuantityAllowedFailed || f.UnzipError))
            {
                var countZeroMess = Properties.Settings.Default.Msg_Error_XmlInvalid;
                var countOverOneMess = Properties.Settings.Default.Msg_Error_XmlOnlyFile;

                dianResponse.StatusCode = Properties.Settings.Default.Code_89;
                dianResponse.StatusMessage = contentFileList.Any(f => f.MaxQuantityAllowedFailed) ? countOverOneMess : countZeroMess;
                dianResponse.XmlFileName = Properties.Settings.Default.Param_ApplicationResponse;
                return dianResponse;
            }

            if (contentFileList.First().HasError || contentFileList.Count > 1)
            {
                dianResponse.XmlFileName = contentFileList.First().XmlFileName;
                dianResponse.StatusMessage = contentFileList.First().XmlErrorMessage;

                if (contentFileList.Count > 1)
                    dianResponse.StatusMessage = Properties.Settings.Default.Msg_Error_EventUpdateOnlyDocument;

                dianResponse.StatusCode = Properties.Settings.Default.Code_89;
                return dianResponse;
            }
            var zone1 = new GlobalLogger(string.Empty, Properties.Settings.Default.Param_Zone1)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = contentFileList.First().XmlFileName
            };
            // ZONE 1

            // ZONE 2
            start = DateTime.UtcNow;
            var xmlBase64 = Convert.ToBase64String(contentFileList.First().XmlBytes);
            var zone2 = new GlobalLogger(string.Empty, Properties.Settings.Default.Param_Zone2)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "Convert.ToBase64String"
            };
            // ZONE 2

            // Parser
            start = DateTime.UtcNow;
            var xmlBytes = contentFileList.First().XmlBytes;
            XmlParser xmlParser = new XmlParser();
            try
            {
                xmlParser = new XmlParser(xmlBytes);
                if (!xmlParser.Parser())
                    throw new Exception(xmlParser.ParserError);
            }
            catch (Exception ex)
            {
                var failedList = new List<string> { $"Regla: ZB01, Rechazo: Fallo en el esquema XML del archivo" };
                dianResponse.IsValid = false;
                dianResponse.StatusCode = "99";
                dianResponse.StatusMessage = "Validación contiene errores en campos mandatorios. " + ex.Message;
                dianResponse.StatusDescription = "Documento con errores en campos mandatorios.";
                dianResponse.ErrorMessage.AddRange(failedList);
                return dianResponse;
            }

            //var xmlParser = new XmlParser(xmlBytes);
            //if (!xmlParser.Parser())
            //    throw new Exception(xmlParser.ParserError);

            var documentParsed = xmlParser.Fields.ToObject<DocumentParsed>();
            documentParsed.SigningTime = xmlParser.SigningTime;
            DocumentParsed.SetValues(ref documentParsed);
            var parser = new GlobalLogger(string.Empty, Properties.Settings.Default.Param_Parser)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "DocumentParsed.SetValues"
            };
            // Parser

            // ZONE 3
            start = DateTime.UtcNow;
            //Validar campos mandatorios basicos para el trabajo del WS      
            dianResponse.XmlDocumentKey = documentParsed.Cude;
            if (!DianServicesUtils.ValidateParserValuesSync(documentParsed, ref dianResponse)) return dianResponse;

            var senderCode = documentParsed.SenderCode;
            var docTypeCode = documentParsed.DocumentTypeId;
            var serie = documentParsed.Serie;
            var serieAndNumber = documentParsed.SerieAndNumber;
            var trackId = documentParsed.DocumentKey.ToLower();
            var eventCode = documentParsed.ResponseCode;
            var trackIdCude = documentParsed.Cude;
            var receiverCode = documentParsed.ReceiverCode;
            var signingTime = xmlParser.SigningTime;
            var customizationID = documentParsed.CustomizationId;
            var listId = documentParsed.listID == "" ? "1" : documentParsed.listID;
            var UBLVersionID = documentParsed.UBLVersionID;
            var documentTypeIdRef = documentParsed.DocumentTypeIdRef;
            var issuerPartyCode = documentParsed.IssuerPartyCode;
            var issuerPartyName = documentParsed.IssuerPartyName;
            var endDate = documentParsed.ValidityPeriodEndDate;
            bool validaMandatoListID = (Convert.ToInt32(eventCode) == 43 && listId == "3") ? false : true;
            var documentReferenceId = xmlParser.DocumentReferenceId;

            var zone3 = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_Zone3)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "DianServicesUtils.ValidateParserValuesSync"
            };

            unzip.PartitionKey = trackIdCude;
            parser.PartitionKey = trackIdCude;
            zone1.PartitionKey = trackIdCude;
            zone2.PartitionKey = trackIdCude;
            // ZONE 3          

            // Auth
            start = DateTime.UtcNow;
            //Si no es un endoso en blanco valida autorizacion
            //if (listId != "2")
            //{
            //    string listIdMessage = $"NIT {authCode} no autorizado a enviar documentos para emisor con NIT {senderCode}.";

            //    var authEntity = GetAuthorization(senderCode, authCode);
            //    if (authEntity == null)
            //    {
            //        dianResponse.XmlFileName = Properties.Settings.Default.Param_ApplicationResponse;
            //        dianResponse.StatusCode = Properties.Settings.Default.Code_89;
            //        dianResponse.StatusDescription = listIdMessage;
            //        var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
            //        if (globalEnd >= 10)
            //        {
            //            var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow:yyyyMMdd}", trackId + " - " + trackIdCude) { Message = globalEnd.ToString(CultureInfo.InvariantCulture), Action = Properties.Settings.Default.Param_Auth };
            //            TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
            //        }
            //        UpdateInTransactions(trackId, eventCode);

            //        return dianResponse;
            //    }
            //}

            var auth = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_Auth3)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "GetAuthorization"
            };
            // Auth

            // Duplicity
            start = DateTime.UtcNow;
            var response = CheckDocumentDuplicity(senderCode, docTypeCode, serie, serieAndNumber, trackIdCude, eventCode);
            var duplicity = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_Duplicity)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "CheckDocumentDuplicity",
                StackTrace = response != null ? response.StatusDescription : ""
            };
            if (response != null)
            {
                arrayTasks = new List<Task>
                {
                    TableManagerGlobalLogger.InsertOrUpdateAsync(unzip),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(parser),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(auth),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(duplicity),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone1),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone2),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone3)
                };

                Task.WhenAll(arrayTasks);
                return response;
            }

            // Duplicity     

            // ZONE MAPPER
            start = DateTime.UtcNow;
            if (contentFileList.First().XmlFileName.Split(Properties.Settings.Default.Symbol_Slash).Count() > 1 &&
                contentFileList.First().XmlFileName.Split(Properties.Settings.Default.Symbol_Slash).Last() != null)
                contentFileList.First().XmlFileName = contentFileList.First().XmlFileName.Split(Properties.Settings.Default.Symbol_Slash).Last();

            var trackIdMapperEntity = new GlobalOseTrackIdMapper(contentFileList[0].XmlFileName, trackIdCude);
            TableManagerDianFileMapper.InsertOrUpdate(trackIdMapperEntity);
            var mapper = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_Zone4Mapper)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "trackIdMapperEntity"
            };
            // ZONE MAPPER          

            // upload xml
            start = DateTime.UtcNow;
            trackId = trackIdCude;
            bool isEvent = true;
            bool sendTestSet = false;
            var uploadXmlRequest = new
            {
                xmlBase64,
                fileName = contentFileList[0].XmlFileName,
                documentTypeId = docTypeCode,
                trackId,
                isEvent,
                eventCode,
                customizationID,
                sendTestSet
            };
            var uploadXmlResponse = ApiHelpers.ExecuteRequest<ResponseUploadXml>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_UoloadXml), uploadXmlRequest);
            if (!uploadXmlResponse.Success)
            {
                dianResponse.XmlFileName = trackIdMapperEntity.PartitionKey;
                dianResponse.StatusCode = Properties.Settings.Default.Code_89;
                dianResponse.StatusDescription = uploadXmlResponse.Message;
                var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                if (globalEnd >= 10)
                {
                    var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow:yyyyMMdd}", trackIdCude) { Message = globalEnd.ToString(CultureInfo.InvariantCulture), Action = Properties.Settings.Default.Param_Uoload };
                    TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
                }
                return dianResponse;
            }
            var upload = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_Upload5)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "UploadXml"
            };
            // upload xml

            // send to validate document sync
            start = DateTime.UtcNow;
            trackId = trackIdCude;
            var requestObjTrackId = new { trackId, draft = Properties.Settings.Default.Param_False };
            var validations = ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_ValidateDocumentUrl), requestObjTrackId);
            var validate = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_Validate6)
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "ValidateDocument",
                StackTrace = "validations.Count => " + validations.Count
            };
            // send to validate document sync

            if (validations.Count == 0)
            {
                dianResponse.XmlFileName = contentFileList.First().XmlFileName;
                dianResponse.StatusDescription = string.Empty;
                dianResponse.StatusCode = Properties.Settings.Default.Code_66;
                var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                if (globalEnd >= 10)
                {
                    var globalTimeValidation = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_Validate)
                    {
                        Message = globalEnd.ToString(CultureInfo.InvariantCulture),
                        Action = "globalEnd",
                        StackTrace = $"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}"
                    };
                    TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
                }

                return dianResponse;
            }
            else
            {
                // ZONE APPLICATION
                start = DateTime.UtcNow;
                string message = string.Empty;
                bool existDocument = false;
                GlobalDocValidatorDocumentMeta documentMeta = null;

                Task secondLocalRun = Task.Run(() =>
                {
                    documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackIdCude.ToLower(), trackIdCude.ToLower());
                    message = $"La {documentMeta.DocumentTypeName} {serieAndNumber}, ha sido autorizada.";
                    existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
                });

                //Validaciones reglas Validador Xpath
                var errors = validations.Where(r => !r.IsValid && r.Mandatory).ToList();
                var notifications = validations.Where(r => r.IsNotification).ToList();

                if (!errors.Any() && !notifications.Any())
                {
                    dianResponse.IsValid = true;
                    dianResponse.StatusMessage = message;
                }

                if (errors.Any())
                {
                    var failedList = new List<string>();
                    foreach (var f in errors)
                        failedList.Add($"Regla: {f.ErrorCode}, Rechazo: {f.ErrorMessage}");

                    dianResponse.IsValid = false;
                    dianResponse.StatusMessage = Properties.Settings.Default.Msg_Error_FieldMandatori;
                    dianResponse.ErrorMessage.AddRange(failedList);
                    UpdateInTransactions(documentParsed.DocumentKey.ToLower(), eventCode);
                }

                if (notifications.Any())
                {
                    var notificationList = new List<string>();
                    foreach (var n in notifications)
                        notificationList.Add($"Regla: {n.ErrorCode}, Notificación: {n.ErrorMessage}");

                    dianResponse.IsValid = !errors.Any();
                    dianResponse.StatusMessage = errors.Any() ? Properties.Settings.Default.Msg_Error_FieldMandatori : message;
                    dianResponse.ErrorMessage.AddRange(notificationList);
                }


                arrayTasks.Add(secondLocalRun);
                Task.WhenAll(arrayTasks).Wait();

                var applicationResponse = XmlUtil.GetApplicationResponseIfExist(documentMeta);
                dianResponse.XmlBase64Bytes = applicationResponse ?? XmlUtil.GenerateApplicationResponseBytes(trackIdCude, documentMeta, validations);

                dianResponse.XmlDocumentKey = trackIdCude;
                GlobalDocValidatorDocument validatorDocument = null;

                if (dianResponse.IsValid)
                {
                    dianResponse.StatusCode = Properties.Settings.Default.Code_00;
                    dianResponse.StatusMessage = message;
                    dianResponse.StatusDescription = Properties.Settings.Default.Msg_Procees_Sucessfull;
                    validatorDocument = new GlobalDocValidatorDocument(documentMeta?.Identifier, documentMeta?.Identifier)
                    {
                        GlobalDocumentId = trackIdCude,
                        DocumentKey = trackIdCude,
                        EmissionDateNumber = documentMeta?.EmissionDate.ToString("yyyyMMdd")
                    };

                    var processRegistrateComplete = ApiHelpers.ExecuteRequest<EventResponse>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_RegistrateCompletedRadianUrl), new { TrackId = trackIdCude });
                    //var processRegistrateComplete = ApiHelpers.ExecuteRequest<EventResponse>("http://localhost:7071/api/RegistrateCompletedRadian", new { TrackId = trackIdCude });
                    if (processRegistrateComplete.Code != Properties.Settings.Default.Code_100)
                    {
                        dianResponse.IsValid = false;
                        dianResponse.XmlFileName = contentFileList.First().XmlFileName;
                        dianResponse.StatusCode = processRegistrateComplete.Code;
                        dianResponse.StatusDescription = processRegistrateComplete.Message;
                        UpdateInTransactions(documentParsed.DocumentKey.ToLower(), eventCode);
                        return dianResponse;
                    }

                    var processEventResponse = ApiHelpers.ExecuteRequest<EventResponse>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_ApplicationResponseProcessUrl), new { TrackId = documentParsed.DocumentKey, documentParsed.ResponseCode, trackIdCude, listId });
                    //var processEventResponse = ApiHelpers.ExecuteRequest<EventResponse>("http://localhost:7071/api/ApplicationResponseProcess", new { TrackId = documentParsed.DocumentKey, documentParsed.ResponseCode, trackIdCude, listId });
                    if (processEventResponse.Code != Properties.Settings.Default.Code_100)
                    {
                        dianResponse.IsValid = false;
                        dianResponse.XmlFileName = contentFileList.First().XmlFileName;
                        dianResponse.StatusCode = processEventResponse.Code;
                        dianResponse.StatusDescription = processEventResponse.Message;
                        UpdateInTransactions(documentParsed.DocumentKey.ToLower(), eventCode);
                        return dianResponse;
                    }                   
                }
                else
                {
                    dianResponse.IsValid = false;
                    dianResponse.StatusCode = Properties.Settings.Default.Code_99;
                    dianResponse.StatusDescription = Properties.Settings.Default.Msg_Error_FieldMandatori;
                    dianResponse.StatusMessage = "Validación contiene errores en campos mandatorios.";
                    dianResponse.XmlBase64Bytes = errors.Any() || notifications.Any() ? dianResponse.XmlBase64Bytes : null;
                }

                var application = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_7AplicattionSendEvent)
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "dianResponse.IsValid => " + dianResponse.IsValid
                };
                // ZONE APPLICATION

                // LAST ZONE
                start = DateTime.UtcNow;
                arrayTasks = new List<Task>
                {
                    TableManagerGlobalLogger.InsertOrUpdateAsync(unzip),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(parser),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(auth),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(duplicity),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(upload),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(validate),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(application),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone1),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone2),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(zone3),
                    TableManagerGlobalLogger.InsertOrUpdateAsync(mapper)
                };

                if (dianResponse.IsValid && !existDocument)
                    arrayTasks.Add(TableManagerGlobalDocValidatorDocument.InsertOrUpdateAsync(validatorDocument));

                Task.WhenAll(arrayTasks);

                var lastZone = new GlobalLogger(trackIdCude, Properties.Settings.Default.Param_LastZone)
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "existDocument => " + existDocument + " dianResponse.IsValid " + dianResponse.IsValid
                };

                UpdateInTransactions(documentParsed.DocumentKey.ToLower(), eventCode);
                TableManagerGlobalLogger.InsertOrUpdate(lastZone);
                // LAST ZONE

                return dianResponse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="accountCodeT"></param>
        /// <param name="softwareCode"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        public NumberRangeResponseList GetNumberingRange(string accountCode, string accountCodeT, string softwareCode, string authCode)
        {
            NumberRangeResponseList verificationResult = new NumberRangeResponseList();
            List<NumberRangeResponse> foliosResult = new List<NumberRangeResponse>();

            try
            {
                var authEntity = GetAuthorization(accountCode, authCode);
                if (authEntity != null && authEntity.PartitionKey != accountCodeT)
                {
                    verificationResult.OperationCode = "401";
                    verificationResult.OperationDescription = $"NIT: {authCode} del certificado no autorizado para consultar rangos de numeración asociados del NIT: {accountCodeT}";
                    return verificationResult;
                }


                if (authEntity == null)
                {
                    verificationResult.OperationCode = "401";
                    verificationResult.OperationDescription = $"NIT: {authCode} no autorizado para consultar rangos de numeración del NIT: {accountCode}";
                    return verificationResult;
                }

                var utcNowNumber = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
                var numberRanges = TableManagerGlobalNumberRange.FindByPartition<GlobalNumberRange>(accountCode);
                numberRanges = numberRanges.Where(r => utcNowNumber >= r.ValidDateNumberFrom && utcNowNumber <= r.ValidDateNumberTo).ToList();
                if (!numberRanges.Any(r => r.State == (long)NumberRangeState.Authorized))
                {
                    verificationResult.OperationCode = "301";
                    verificationResult.OperationDescription = $"No fue encontrado ningún rango de numeración para el NIT: {accountCode}.";
                    return verificationResult;
                }

                if (!numberRanges.Any(r => r.State == (long)NumberRangeState.Authorized && r.SoftwareId == softwareCode))
                {
                    verificationResult.OperationCode = "302";
                    verificationResult.OperationDescription = $"No registra prefijos asociados al código de software: {softwareCode}.";
                    return verificationResult;
                }

                if (!numberRanges.Any(r => r.State == (long)NumberRangeState.Authorized && r.SoftwareId == softwareCode && r.SoftwareOwnerCode == accountCodeT))
                {
                    verificationResult.OperationCode = "303";
                    verificationResult.OperationDescription = $"El código del software no corresponde al NIT: {accountCodeT}.";
                    return verificationResult;
                }

                numberRanges = numberRanges.Where(r => r.State == (long)NumberRangeState.Authorized && r.SoftwareOwnerCode == accountCodeT && r.SoftwareId == softwareCode).ToList();

                foliosResult = numberRanges.Select(n => new NumberRangeResponse()
                {
                    FromNumber = n.FromNumber,
                    ToNumber = n.ToNumber,
                    Prefix = n.Serie,
                    ResolutionNumber = n.ResolutionNumber,
                    ResolutionDate = n.ResolutionDate.ToString("yyyy-MM-dd"),
                    TechnicalKey = n.TechnicalKey,
                    ValidDateFrom = DateTime.ParseExact(n.ValidDateNumberFrom.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                    ValidDateTo = DateTime.ParseExact(n.ValidDateNumberTo.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                }).ToList();
                verificationResult.OperationCode = "100";
                verificationResult.OperationDescription = "Acción completada OK.";
                verificationResult.ResponseList = new List<NumberRangeResponse>();
                verificationResult.ResponseList.AddRange(foliosResult);
            }
            catch
            {
                verificationResult.OperationCode = "500";
                verificationResult.OperationDescription = "Ha ocurrido un error en el servicio solicitado, por favor intente mas tarde.";
                return verificationResult;
            }

            return verificationResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        public EventResponse GetXmlByDocumentKey(string trackId, string authCode)
        {
            var eventResponse = new EventResponse();
            //authCode = "9005089089";

            var documentKey = trackId.ToLower();
            var globalDocValidatorDocumentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(documentKey, documentKey);
            if (globalDocValidatorDocumentMeta == null)
            {
                eventResponse.Code = "404";
                eventResponse.Message = $"Xml con CUFE: '{documentKey}' no encontrado.";
                return eventResponse;
            }

            var identifier = globalDocValidatorDocumentMeta.Identifier;
            var globalDocValidatorDocument = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(identifier, identifier);
            if (globalDocValidatorDocument == null)
            {
                eventResponse.Code = "404";
                eventResponse.Message = $"Xml con CUFE: '{documentKey}' no encontrado.";
                return eventResponse;
            }

            if (globalDocValidatorDocument.DocumentKey != documentKey)
            {
                eventResponse.Code = "404";
                eventResponse.Message = $"Xml con CUFE: '{documentKey}' no encontrado.";
                return eventResponse;
            }

            var xmlBytes = GetXmlFromStorage(documentKey);
            if (xmlBytes == null)
            {
                eventResponse.Code = "404";
                eventResponse.Message = $"Xml con CUFE: '{documentKey}' no encontrado.";
                return eventResponse;
            }

            var xmlParser = new XmlParser(xmlBytes);
            if (!xmlParser.Parser())
            {
                eventResponse.Code = "206";
                eventResponse.Message = "Xml con errores en los campos número documento emisor o número documento receptor";
                return eventResponse;
            }

            var documentParsed = xmlParser.Fields.ToObject<DocumentParsed>();

            var senderCode = documentParsed.SenderCode;
            var receiverCode = documentParsed.ReceiverCode;

            if (!authCode.Contains(senderCode) && !authCode.Contains(receiverCode))
            {
                // pt con emisor
                var authEntity = GetAuthorization(senderCode, authCode);
                if (authEntity == null)
                {
                    // pt con receptor
                    authEntity = GetAuthorization(receiverCode, authCode);
                    if (authEntity == null)
                    {
                        eventResponse.Code = "401";
                        eventResponse.Message = $"NIT: {authCode} del certificado no autorizado para consultar xmls de emisor con NIT: {senderCode} y receptor con NIT: {receiverCode}";
                        return eventResponse;
                    }
                }
            }

            eventResponse.Code = "100";
            eventResponse.Message = $"Accion completada OK";
            eventResponse.XmlBytesBase64 = Convert.ToBase64String(xmlBytes);

            return eventResponse;
        }

        #region Private methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="senderCode"></param>
        /// <param name="documentType"></param>
        /// <param name="serie"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private DianResponse CheckDocumentDuplicity(string senderCode, string documentType, string serie, string serieAndNumber, string trackId, string eventCode)
        {
            var response = new DianResponse() { ErrorMessage = new List<string>() };
            // identifier
            if (new string[] { "01", "02", "04" }.Contains(documentType)) documentType = "01";
            var identifier = StringUtil.GenerateIdentifierSHA256($"{senderCode}{documentType}{serieAndNumber}");
            var document = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(identifier, identifier);

            // first check
            CheckDocument(ref response, document, documentType, eventCode);

            // Check if response has errors
            if (response.ErrorMessage.Any())
            {
                //
                var validations = TableManagerGlobalDocValidatorTracking.FindByPartition<GlobalDocValidatorTracking>(document.DocumentKey);
                if (validations.Any(v => !v.IsValid && v.Mandatory)) return null;

                //
                return response;
            }

            var number = StringUtil.TextAfter(serieAndNumber, serie).TrimStart('0');
            if (string.IsNullOrEmpty(number))
            {
                var failedList = new List<string> { $"" };
                response.IsValid = false;
                response.StatusCode = "99";
                response.StatusMessage = ".";
                response.StatusDescription = ".";
                response.ErrorMessage.AddRange(failedList);
                return response;
            }

            identifier = StringUtil.GenerateIdentifierSHA256($"{senderCode}{documentType}{serie}{number}");
            document = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(identifier, identifier);

            // second check
            CheckDocument(ref response, document, documentType, eventCode);

            // Check if response has errors
            if (response.ErrorMessage.Any())
            {
                //
                var validations = TableManagerGlobalDocValidatorTracking.FindByPartition<GlobalDocValidatorTracking>(document.DocumentKey);
                if (validations.Any(v => !v.IsValid && v.Mandatory)) return null;

                //
                return response;
            }

            // third check
            var meta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (meta != null)
            {
                document = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(meta?.Identifier, meta?.Identifier);

                CheckDocument(ref response, document, documentType, eventCode, meta);

                // Check if response has errors
                if (response.ErrorMessage.Any())
                {
                    //
                    var validations = TableManagerGlobalDocValidatorTracking.FindByPartition<GlobalDocValidatorTracking>(document.DocumentKey);
                    if (validations.Any(v => !v.IsValid && v.Mandatory)) return null;

                    //
                    return response;
                }
            }

            return null;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <param name="document"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        private DianResponse CheckDocument(ref DianResponse response, GlobalDocValidatorDocument document, string documentType, string eventCode, GlobalDocValidatorDocumentMeta meta = null)
        {
            List<string> failedList = new List<string>();
            if (document != null)
            {
                if (meta == null)
                    meta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(
                        document.DocumentKey, document.DocumentKey);

                failedList = new List<string>
                {
                    $"Regla: 90, Rechazo: Documento procesado anteriormente."
                };
                response.IsValid = false;
                response.StatusCode = "99";
                response.StatusMessage = "Documento con errores en campos mandatorios.";
                response.StatusDescription = "Validación contiene errores en campos mandatorios.";
                response.ErrorMessage.AddRange(failedList);
                var xmlBytes = XmlUtil.GetApplicationResponseIfExist(meta);
                response.XmlBase64Bytes = xmlBytes;
                response.XmlDocumentKey = document.DocumentKey;
                response.XmlFileName = meta.FileName;

            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        private GlobalAuthorization GetAuthorization(string code, string authCode)
        {

            var authorization = new GlobalAuthorization();
            var trimAuthCode = authCode?.Trim();
            var newAuthCode = trimAuthCode?.Substring(0, trimAuthCode.Length - 1);
            authorization = TableManagerGlobalAuthorization.Find<GlobalAuthorization>(trimAuthCode, code);
            if (authorization == null) authorization = TableManagerGlobalAuthorization.Find<GlobalAuthorization>(newAuthCode, code);
            if (authorization == null) return null;

            return authorization;
        }

        private GlobalContributor GetGlobalContributor(string authCode)
        {
            var globalContributor = new GlobalContributor();
            var trimAuthCode = authCode?.Trim();
            var newAuthCode = trimAuthCode?.Substring(0, trimAuthCode.Length - 1);
            globalContributor = TableManagerGlobalContributor.Find<GlobalContributor>(trimAuthCode, trimAuthCode);
            if (globalContributor == null) globalContributor = TableManagerGlobalContributor.Find<GlobalContributor>(newAuthCode, newAuthCode);

            return globalContributor;
        }

        public static byte[] GetXmlFromStorage(string trackId)
        {
            var tableManager = new TableManager("GlobalDocValidatorRuntime");
            var documentStatusValidation = tableManager.Find<GlobalDocValidatorRuntime>(trackId, "UPLOAD");
            if (documentStatusValidation == null)
                return null;

            var fileManager = new FileManager();
            var container = $"global";
            var fileName = $"docvalidator/{documentStatusValidation.Category}/{documentStatusValidation.Timestamp.Date.Year}/{documentStatusValidation.Timestamp.Date.Month.ToString().PadLeft(2, '0')}/{trackId}.xml";
            var xmlBytes = fileManager.GetBytes(container, fileName);

            tableManager = null;
            return xmlBytes;
        }

        private void UpdateInTransactions(string trackId, string eventCode)
        {
            //valida InTransaction Factura - eventos Endoso en propeidad, Garantia y procuración
            var arrayTasks = new List<Task>();
            if (Convert.ToInt32(eventCode) == (int)EventStatus.EndosoPropiedad
            || Convert.ToInt32(eventCode) == (int)EventStatus.EndosoGarantia
            || Convert.ToInt32(eventCode) == (int)EventStatus.EndosoProcuracion)
            {
                GlobalDocValidatorDocumentMeta validatorDocumentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                if (validatorDocumentMeta != null)
                {
                    validatorDocumentMeta.InTransaction = false;
                    arrayTasks.Add(TableManagerGlobalDocValidatorDocumentMeta.InsertOrUpdateAsync(validatorDocumentMeta));
                }
            }
        }

        private void UpdateIsInvoiceTV(string trackId, string eventCode)
        {
            //Actualiza factura electronica TV eventos fase 1 registrados
            var arrayTasks = new List<Task>();
            if (Convert.ToInt32(eventCode) == (int)EventStatus.Accepted
            || Convert.ToInt32(eventCode) == (int)EventStatus.AceptacionTacita)
            {
                GlobalDocValidatorDocumentMeta validatorDocumentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                if (validatorDocumentMeta != null)
                {
                    validatorDocumentMeta.IsInvoiceTV = true;
                    arrayTasks.Add(TableManagerGlobalDocValidatorDocumentMeta.InsertOrUpdateAsync(validatorDocumentMeta));

                }
            }
        }

        private void UpdateFinishAttorney(string trackId, string trackIdAttorney, string eventCode)
        {
            //validation if is an anulacion de mandato (Code 044)
            var arrayTasks = new List<Task>();
            if (Convert.ToInt32(eventCode) == (int)EventStatus.TerminacionMandato)
            {
                List<GlobalDocReferenceAttorney> documentsAttorney = TableManagerGlobalDocReferenceAttorney.FindAll<GlobalDocReferenceAttorney>(trackIdAttorney).ToList();
                foreach (var documentAttorney in documentsAttorney)
                {
                    documentAttorney.Active = false;
                    documentAttorney.DocReferencedEndAthorney = trackId;
                    arrayTasks.Add(TableManagerGlobalDocReferenceAttorney.InsertOrUpdateAsync(documentAttorney));
                }
            }
        }

        private void InsertGlobalDocRegisterProviderAR(GlobalDocRegisterProviderAR documentRegisterAR)
        {
            var arrayTasks = new List<Task>();
            arrayTasks.Add(TableManagerGlobalDocRegisterProviderAR.InsertOrUpdateAsync(documentRegisterAR));
        }

        private void UpdateEndoso(XmlParser xmlParser, DocumentParsed documentParsed)
        {
            //validation if is an Endoso en propiedad (Code 037)
            var arrayTasks = new List<Task>();
            string sender = string.Empty;
            string senderList = string.Empty;
            string valueStockAmountSender = string.Empty;
            string valueStockAmountSenderList = string.Empty;
            if (Convert.ToInt32(documentParsed.ResponseCode) == (int)EventStatus.EndosoPropiedad)
            {
                List<GlobalDocHolderExchange> documentsHolderExchange = TableManagerGlobalDocHolderExchange.FindpartitionKey<GlobalDocHolderExchange>(documentParsed.DocumentKey.ToLower()).ToList();
                foreach (var documentHolderExchange in documentsHolderExchange)
                {
                    documentHolderExchange.Active = false;
                    arrayTasks.Add(TableManagerGlobalDocHolderExchange.InsertOrUpdateAsync(documentHolderExchange));
                }
                //Lista de endosantes
                XmlNodeList valueListSender = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']");
                for (int i = 0; i < valueListSender.Count; i++)
                {
                    sender = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CompanyID']").Item(i)?.InnerText.ToString();
                    valueStockAmountSender = valueListSender.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                    if (i == 0)
                    {
                        senderList += sender;
                        valueStockAmountSenderList += valueStockAmountSender;
                    }
                    else
                    {
                        senderList += "|" + sender;
                        valueStockAmountSenderList += "|" + valueStockAmountSender;
                    }
                }

                //Lista de endosatrios
                XmlNodeList valueListReceiver = xmlParser.XmlDocument.DocumentElement.SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']");
                for (int i = 0; i < valueListReceiver.Count; i++)
                {
                    string companyId = valueListReceiver.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CompanyID']").Item(i)?.InnerText.ToString();
                    string valueStockAmount = valueListReceiver.Item(i).SelectNodes("//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyLegalEntity']/*[local-name()='CorporateStockAmount']").Item(i)?.InnerText.ToString();
                    string rowKey = senderList + "|" + companyId;
                    GlobalDocHolderExchange globalDocHolderExchange = new GlobalDocHolderExchange(documentParsed.DocumentKey.ToLower(), rowKey)
                    {
                        Timestamp = DateTime.Now,
                        Active = true,
                        CorporateStockAmount = valueStockAmount,
                        GlobalDocumentId = documentParsed.Cude,
                        PartyLegalEntity = companyId,
                        SenderCode = senderList,
                        CorporateStockAmountSender = valueStockAmountSenderList
                    };
                    arrayTasks.Add(TableManagerGlobalDocHolderExchange.InsertOrUpdateAsync(globalDocHolderExchange));
                }
            }
        }

    }
}