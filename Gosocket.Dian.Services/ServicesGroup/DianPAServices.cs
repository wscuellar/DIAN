using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gosocket.Dian.Logger.Logger;

namespace Gosocket.Dian.Services.ServicesGroup
{
    public class DianPAServices : IDisposable
    {
        private static readonly TableManager TableManagerDianFileMapper = new TableManager("DianFileMapper");
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager TableManagerGlobalDocAssociate = new TableManager("GlobalDocAssociate");
        private static readonly TableManager TableManagerGlobalDocValidatorDocument = new TableManager("GlobalDocValidatorDocument");
        private static readonly TableManager TableManagerGlobalDocValidatorRuntime = new TableManager("GlobalDocValidatorRuntime");
        private static readonly TableManager TableManagerGlobalDocValidatorTracking = new TableManager("GlobalDocValidatorTracking");

        private static readonly TableManager TableManagerGlobalBatchFileMapper = new TableManager("GlobalBatchFileMapper");
        private static readonly TableManager TableManagerGlobalBatchFileRuntime = new TableManager("GlobalBatchFileRuntime");
        private static readonly TableManager TableManagerGlobalBatchFileResult = new TableManager("GlobalBatchFileResult");
        private static readonly TableManager TableManagerGlobalBatchFileStatus = new TableManager("GlobalBatchFileStatus");
        private static readonly TableManager TableManagerGlobalContributor = new TableManager("GlobalContributor");

        private static readonly TableManager TableManagerGlobalNumberRange = new TableManager("GlobalNumberRange");
        private static readonly TableManager TableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");

        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");

        private static readonly TableManager TableManagerGlobalDocEvent = new TableManager("GlobalDocEvent");
        private static readonly TableManager TableManagerGlobalDocumentWithEventRegistered = new TableManager("GlobalDocumentWithEventRegistered");

        private static readonly FileManager fileManager = new FileManager();

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
        public async Task<UploadDocumentResponse> ProcessBatchZipFile(string fileName, byte[] contentFile, string authCode = null, string testSetId = null)
        {
            var zipKey = Guid.NewGuid().ToString();
            UploadDocumentResponse responseMessages = new UploadDocumentResponse
            {
                ZipKey = zipKey
            };
            var utcNow = DateTime.UtcNow;
            var blobPath = $"{utcNow.Year}/{utcNow.Month.ToString().PadLeft(2, '0')}/{utcNow.Day.ToString().PadLeft(2, '0')}";
            var result = await fileManager.UploadAsync(blobContainer, $"{blobContainerFolder}/{blobPath}/{zipKey}.zip", contentFile);
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
        public async Task<DianResponse> UploadDocumentSync(string fileName, byte[] contentFile, string authCode = null)
        {
            StringBuilder sb = new StringBuilder();
            var start = DateTime.UtcNow;
            var globalStart = DateTime.UtcNow;
            var contentFileList = contentFile.ExtractMultipleZip();

            sb.AppendLine($"1 Unzip{DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");

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


            sb.AppendLine($"Zone 1 {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");

            // ZONE 1

            // ZONE 2
            start = DateTime.UtcNow;
            var xmlBase64 = Convert.ToBase64String(contentFileList.First().XmlBytes);


            sb.AppendLine($"Zone 2 {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");

            // ZONE 2

            start = DateTime.UtcNow;
            var xmlBytes = contentFileList.First().XmlBytes;
            XmlParser xmlParser;
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


            sb.AppendLine($"Parser {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");

            // Parser

            // ZONE 3
            start = DateTime.UtcNow;
            //Validar campos mandatorios basicos para el trabajo del WS
            if(documentParsed.DocumentTypeId == "50")
            {
                DianServicesUtils.ValidateParserValuesSync(documentParsed, ref dianResponse);
            }
            else if (!DianServicesUtils.ValidateParserValuesSync(documentParsed, ref dianResponse))
            {
                return dianResponse;
            }

            

            var senderCode = documentParsed.SenderCode;
            var docTypeCode = documentParsed.DocumentTypeId;
            var serie = documentParsed.Serie;
            var serieAndNumber = documentParsed.SerieAndNumber;
            var trackId = documentParsed.DocumentKey.ToLower();
            var eventCode = documentParsed.ResponseCode;

            sb.AppendLine($"Zone 3 {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");

            // ZONE 3

            // Auth
            start = DateTime.UtcNow;

            if (senderCode != "01" && !String.IsNullOrWhiteSpace(senderCode))
            {
                var authEntity = GetAuthorization(senderCode, authCode);
                if (authEntity == null)
                {
                    dianResponse.XmlFileName = $"{fileName}";
                    dianResponse.StatusCode = Properties.Settings.Default.Code_89;
                    dianResponse.StatusDescription = $"NIT {authCode} no autorizado a enviar documentos para emisor con NIT {senderCode}.";
                    var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                    if (globalEnd >= 10)
                    {
                        var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow.ToString("yyyyMMdd")}", trackId) { Message = globalEnd.ToString(), Action = "Auth" };
                        TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
                    }
                    return dianResponse;
                }
            }


            sb.AppendLine($"3 Auth {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");

            // Auth

            // Duplicity
            start = DateTime.UtcNow;
            var response = CheckDocumentDuplicity(senderCode, docTypeCode, serie, serieAndNumber, trackId, eventCode);
            if (response != null) return response;


            sb.AppendLine($"Duplicity {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");





            // ZONE MAPPER
            start = DateTime.UtcNow;
            if (contentFileList.First().XmlFileName.Split('/').Count() > 1 && contentFileList.First().XmlFileName.Split('/').Last() != null)
                contentFileList.First().XmlFileName = contentFileList.First().XmlFileName.Split('/').Last();

            var trackIdMapperEntity = new GlobalOseTrackIdMapper(contentFileList[0].XmlFileName, trackId);
            TableManagerDianFileMapper.InsertOrUpdate(trackIdMapperEntity);


            sb.AppendLine($"Zone 4 Mapper {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");


            // ZONE MAPPER

            // upload xml
            start = DateTime.UtcNow;
            var uploadXmlRequest = new { xmlBase64, fileName = contentFileList[0].XmlFileName, documentTypeId = docTypeCode, trackId };
            var uploadXmlResponse = await ApiHelpers.ExecuteRequestAsync<ResponseUploadXml>(ConfigurationManager.GetValue("UploadXmlUrl"), uploadXmlRequest);
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
            sb.AppendLine($"5 Upload {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");

            // upload xml

            // send to validate document sync
            start = DateTime.UtcNow;
            var requestObjTrackId = new { trackId, draft = "false" };
            var validations = await ApiHelpers.ExecuteRequestAsync<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("ValidateDocumentUrl"), requestObjTrackId);


            sb.AppendLine($"6 Validate {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");

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

                GlobalDocValidatorDocumentMeta documentMeta = null;



                documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                //var prefix = !string.IsNullOrEmpty(serie) ? serie : string.Empty;
                message = $"La {documentMeta.DocumentTypeName} {serieAndNumber}, ha sido autorizada."; // (string.IsNullOrEmpty(prefix)) ? $"La {documentMeta.DocumentTypeName} {serieAndNumber}, ha sido autorizada." : $"La {documentMeta.DocumentTypeName} {prefix}-{number}, ha sido autorizada.";



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

                    dianResponse.IsValid = !errors.Any();
                    dianResponse.StatusMessage = errors.Any() ? "Documento con errores en campos mandatorios." : message;
                    dianResponse.ErrorMessage.AddRange(notificationList);
                }



                var applicationResponse = XmlUtil.GetApplicationResponseIfExist(documentMeta);
                dianResponse.XmlBase64Bytes = applicationResponse ?? XmlUtil.GenerateApplicationResponseBytes(trackId, documentMeta, validations);

                dianResponse.XmlDocumentKey = trackId;

                GlobalDocValidatorDocument validatorDocument = null;
                if (dianResponse.IsValid)
                {
                    dianResponse.StatusCode = "00";
                    dianResponse.StatusMessage = message;
                    dianResponse.StatusDescription = "Procesado Correctamente.";
                    validatorDocument = new GlobalDocValidatorDocument(documentMeta?.Identifier, documentMeta?.Identifier)
                    {
                        DocumentKey = trackId,
                        EmissionDateNumber = documentMeta?.EmissionDate.ToString("yyyyMMdd")
                    };
                }
                else
                {
                    dianResponse.IsValid = false;
                    dianResponse.StatusCode = "99";
                    dianResponse.StatusDescription = "Validación contiene errores en campos mandatorios.";
                }


                sb.AppendLine($"7 Aplication SendBillSync {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");


                // LAST ZONE
                start = DateTime.UtcNow;




                bool existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);

                if (dianResponse.IsValid && !existDocument)
                    TableManagerGlobalDocValidatorDocument.InsertOrUpdate(validatorDocument);

                //Task.WhenAll(arrayTasks);
                sb.AppendLine($"Last Zone {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString()}");


                // ZONE APPLICATION
                var application = new GlobalLogger(trackId, "timers") { Message = sb.ToString() };
                TableManagerGlobalLogger.InsertOrUpdate(application);



                return dianResponse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task <ExchangeEmailResponse> GetExchangeEmails(string authCode)
        {
            var contributor = GetGlobalContributor(authCode);
            if (contributor == null)
                return new ExchangeEmailResponse { StatusCode = "89", Success = false, Message = $"NIT {authCode} no autorizado para consultar correos de recepción de facturas.", CsvBase64Bytes = null };

            var bytes = await fileManager.GetBytesAsync("dian", $"exchange/emails.csv");
            var response = new ExchangeEmailResponse { StatusCode = "0", Success = true, CsvBase64Bytes = bytes };
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        public async Task<List<DianResponse>> GetBatchStatus(string trackId)
        {
            var batchStatus = TableManagerGlobalBatchFileStatus.Find<GlobalBatchFileStatus>(trackId, trackId);
            if (batchStatus == null) return await GetStatusZip(trackId);

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
            if (resultsEntities.Count == 1) return await GetStatusZip (trackId);

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
                var zipBytes = await fileManager.GetBytesAsync(blobContainer, $"{blobContainerFolder}/applicationResponses/{trackId}.zip");
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
        public async  Task<DianResponse> GetStatus(string trackId)
        {
            var globalStart = DateTime.UtcNow;
            DateTime start;

            var response = new DianResponse() { ErrorMessage = new List<string>() };
            var validatorRuntimes = TableManagerGlobalDocValidatorRuntime.FindByPartition(trackId);

            //if (validatorRuntimes.Any(v => v.RowKey == "UPLOAD")) isUploaded = true;
            if (validatorRuntimes.Any(v => v.RowKey == "UPLOAD"))
            {
                //var isFinished = DianServicesUtils.CheckIfDocumentValidationsIsFinished(trackId);
                if (validatorRuntimes.Any(v => v.RowKey == "END"))
                {
                    string messageDescription = string.Empty;
                    start = DateTime.UtcNow;
                    GlobalDocValidatorDocumentMeta documentMeta = null;
                    bool applicationResponseExist = false;
                    bool existDocument = false;
                    //bool existNsu = false;
                    var validations = new List<GlobalDocValidatorTracking>();
                    List<Task> arrayTasks = new List<Task>();

                    
                    var applicationResponse = await ApiHelpers.ExecuteRequestAsync<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), new { trackId });
                    response.XmlBase64Bytes = !applicationResponse.Success ? null : applicationResponse.Content;
                    if (!applicationResponse.Success)
                        Debug.WriteLine(applicationResponse.Message);
                    

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

                    
                    arrayTasks.Add(secondLocalRun);
                    arrayTasks.Add(fourLocalRun);
                    Task.WhenAll(arrayTasks).Wait();

                    //var applicationResponse = XmlUtil.GetApplicationResponseIfExist(documentMeta);
                    //response.XmlBase64Bytes = (applicationResponse != null) ? XmlUtil.GenerateApplicationResponseBytes(trackId, documentMeta, validations) : null;

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
                    var document = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);


                    if (!failed.Any() && !notifications.Any())
                    {
                        response.IsValid = true;
                        response.StatusMessage = message;
                    }

                    if (failed.Any() && !applicationResponseExist)
                    //if (failed.Any())
                    {                        
                        var failedList = new List<string>();
                        foreach (var f in failed)
                        {
                            failedList.Add($"Regla: {f.ErrorCode}, Rechazo: {f.ErrorMessage}");
                            if (f.ErrorCode == "90")
                            {
                                switch (f.DocumentTypeCode)
                                {
                                    case "01":
                                    case "91":
                                    case "92":
                                        {
                                            messageDescription = document != null ? $"Documento {documentMeta.SerieAndNumber} procesado anteriormente. CUFE {document.DocumentKey} " : null ;
                                            break;
                                        }
                                    case "96":
                                        {
                                            messageDescription = document != null ? $"Documento {documentMeta.SerieAndNumber} procesado anteriormente. CUDE {document.DocumentKey} " : null;
                                            break;
                                        }
                                    case "102":
                                    case "103":
                                        {
                                            messageDescription = document != null ?  $"Documento {documentMeta.SerieAndNumber} procesado anteriormente. CUNE  {document.DocumentKey} " : null;
                                            break;
                                        }
                                    default:
                                        break;
                                }
                                response.StatusDescription = messageDescription;
                            }
                        }
                                                   
                        response.IsValid = false;
                        response.StatusMessage = "Documento con errores en campos mandatorios.";
                        response.ErrorMessage.AddRange(failedList);
                    }

                    if (notifications.Any())
                    {
                        var notificationList = new List<string>();
                        foreach (var n in notifications)
                            notificationList.Add($"Regla: {n.ErrorCode}, Notificación: {n.ErrorMessage}");

                        response.IsValid = !failed.Any() || response.IsValid;
                        response.StatusMessage = failed.Any() ? response.StatusMessage : message;
                        response.ErrorMessage.AddRange(notificationList);
                    }

                    if (response.IsValid || applicationResponseExist)
                    //if (response.IsValid)
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
        public async Task<List<DianResponse>> GetStatusZip(string trackId)
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
                        response = await GetStatus(item.RowKey);
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

        private GlobalDocValidatorDocumentMeta OperationProcess(List<GlobalDocAssociate> associations, List<GlobalDocValidatorDocumentMeta> meta, string cufe)
        {
            List<Task> arrayTasks = new List<Task>();
            GlobalDocValidatorDocumentMeta invoice = new GlobalDocValidatorDocumentMeta();

            //Consulta documentos en la meta Factura
            Task operation1 = Task.Run(() =>
            {
                invoice = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(cufe, cufe);
            });

            //Consulta documentos en la meta
            Task operation2 = Task.Run(() =>
            {
                for (int i = 0; i < associations.Count; i++)
                {
                    meta.Add(TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(associations[i].RowKey, associations[i].RowKey));
                }
            });

            arrayTasks.Add(operation1);
            arrayTasks.Add(operation2);

            Task.WhenAll(arrayTasks).Wait();

            return invoice;
        }

        private List<InvoiceWrapper> GetEventsByTrackId(string trackId)
        {
            //Traemos las asociaciones de la factura = Eventos
            List<GlobalDocAssociate> associateDocumentList = TableManagerGlobalDocAssociate.FindpartitionKey<GlobalDocAssociate>(trackId.ToLower()).ToList();
            if (!associateDocumentList.Any())
                return new List<InvoiceWrapper>();

            //Organiza grupos por factura
            var groups = associateDocumentList.Where(t => t.Active && !string.IsNullOrEmpty(t.Identifier)).GroupBy(t => t.PartitionKey);
            List<InvoiceWrapper> responses = groups.Aggregate(new List<InvoiceWrapper>(), (list, source) =>
            {
                //obtenemos el cufe
                string cufe = source.Key;
                List<GlobalDocAssociate> events = source.ToList();

                //calcula items del proceso
                List<GlobalDocValidatorDocumentMeta> meta = new List<GlobalDocValidatorDocumentMeta>();
                GlobalDocValidatorDocumentMeta invoice = OperationProcess(events, meta, cufe);

                //Unifica la data
                var eventDoc = from associate in events
                               join docMeta in meta on associate.RowKey equals docMeta.PartitionKey
                               select new EventDocument()
                               {
                                   Cufe = cufe,
                                   Associate = associate,
                                   DocumentMeta = docMeta,
                               };

                InvoiceWrapper invoiceWrapper = new InvoiceWrapper()
                {
                    Cufe = cufe,
                    Invoice = invoice,
                    Documents = eventDoc.OrderByDescending(t => t.DocumentMeta.SigningTimeStamp).ToList()
                };

                list.Add(invoiceWrapper);

                return list;
            });

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

            var response = new DianResponse() { ErrorMessage = new List<string>() };
            var validatorRuntimes = TableManagerGlobalDocValidatorRuntime.FindByPartition(trackId);

            if (validatorRuntimes.Any(v => v.RowKey == "UPLOAD"))
            {
                if (validatorRuntimes.Any(v => v.RowKey == "END"))
                {
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

                    arrayTasks.Add(firstLocalRun);
                    Task.WhenAll(arrayTasks).Wait();

                    if (int.Parse(documentMeta.DocumentTypeId) != (int)DocumentType.Invoice)
                    {
                        response.StatusCode = "68";
                        response.StatusDescription = "TrackId no corresponde a un CUFE.";
                        return response;
                    }

                    arrayTasks.Clear();

                    Task secondLocalRun = Task.Run(() =>
                    {
                        //Servicio
                        List<InvoiceWrapper> InvoiceWrapper = GetEventsByTrackId(trackId.ToLower());

                        events = (InvoiceWrapper.Any()) ? InvoiceWrapper[0].Documents.Select(x => x.DocumentMeta).ToList() : null;

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
                                    var docEvent = TableManagerGlobalDocEvent.FindGlobalEvent<GlobalDocEvent>(e?.EventCode, e?.CustomizationID, "96");
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

                    arrayTasks.Add(secondLocalRun);
                    arrayTasks.Add(thirdLocalRun);
                    Task.WhenAll(arrayTasks).Wait();

                    response.XmlBase64Bytes = XmlUtilEvents.GenerateApplicationResponseBytes(trackId, documentMeta, validations, events, originalEvents, originalEventsValidations);

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

                        response.IsValid = !failed.Any() || response.IsValid;
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
        public async Task<DianResponse> SendEventUpdateStatus(byte[] contentFile, string authCode)
        {
            var start = DateTime.UtcNow;
            var globalStart = DateTime.UtcNow;
            var contentFileList = contentFile.ExtractMultipleZip();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{Properties.Settings.Default.Param_GlobalLogger} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)}");



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

            sb.AppendLine($"{Properties.Settings.Default.Param_Zone1} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} {contentFileList.First().XmlFileName}");
            // ZONE 1

            // ZONE 2
            start = DateTime.UtcNow;
            var xmlBase64 = Convert.ToBase64String(contentFileList.First().XmlBytes);
            sb.AppendLine($"{Properties.Settings.Default.Param_Zone2} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} Convert.ToBase64String");
            // ZONE 2

            // Parser
            start = DateTime.UtcNow;
            var xmlBytes = contentFileList.First().XmlBytes;

            try
            {
                var documentMetaManager = new DocumentMetaManager(xmlBytes);
                var category = documentMetaManager.GetCategory()?.RowKey;
            }
            catch (Exception ex)
            {
                var failedList = new List<string> { $"Regla: ZB01, Rechazo: Fallo en el esquema XML del archivo" };
                dianResponse.IsValid = false;
                dianResponse.StatusCode = "99";
                dianResponse.StatusMessage = "Error al subir xml. " + ex.Message;
                dianResponse.StatusDescription = "Documento con errores en campos mandatorios.";
                dianResponse.ErrorMessage.AddRange(failedList);
                return dianResponse;
            }

            XmlParser xmlParser;
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
            documentParsed.SigningTime = xmlParser.SigningTime;
            DocumentParsed.SetValues(ref documentParsed);
            sb.AppendLine($"{Properties.Settings.Default.Param_Parser} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} DocumentParsed.SetValues");
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
            var trackIdCude = documentParsed.Cude.ToLower();
            var customizationID = documentParsed.CustomizationId;
            var listId = documentParsed.listID == "" ? "1" : documentParsed.listID;

            sb.AppendLine($"{Properties.Settings.Default.Param_Zone3} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} DianServicesUtils.ValidateParserValuesSync");

            // ZONE 3          

            // Auth
            start = DateTime.UtcNow;
            //Mandato sin CUFES referenciados
            //bool mandato = (eventCode == "043" && listId != "3");
            //bool validaAutho = ((eventCode == "037" || eventCode == "038" || eventCode == "039") && listId != "2" || mandato);

            //Si no es un endoso en blanco valida autorizacion            
            //if (validaAutho && senderCode != "01")
            if (senderCode != "01" && !String.IsNullOrWhiteSpace(senderCode))
            {

                string listIdMessage = $"NIT {authCode} no autorizado a enviar documentos para emisor con NIT {senderCode}.";

                var authEntity = GetAuthorization(senderCode, authCode);
                if (authEntity == null)
                {
                    dianResponse.XmlFileName = Properties.Settings.Default.Param_ApplicationResponse;
                    dianResponse.StatusCode = Properties.Settings.Default.Code_89;
                    dianResponse.StatusDescription = listIdMessage;
                    var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                    if (globalEnd >= 10)
                    {
                        var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow:yyyyMMdd}", trackId + " - " + trackIdCude) { Message = globalEnd.ToString(CultureInfo.InvariantCulture), Action = Properties.Settings.Default.Param_Auth };
                        TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
                    }
                    UpdateInTransactions(trackId, eventCode);

                    return dianResponse;
                }

            }

            sb.AppendLine($"{Properties.Settings.Default.Param_Auth3} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} GetAuthorization");


            // Auth

            // Duplicity
            start = DateTime.UtcNow;
            var response = CheckDocumentDuplicity(senderCode, docTypeCode, serie, serieAndNumber, trackIdCude, eventCode);
            var StackTrace = response != null ? response.StatusDescription : "";
            sb.AppendLine($"{Properties.Settings.Default.Param_Duplicity} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} CheckDocumentDuplicity StackTrace: {StackTrace}");
            if (response != null)
            {
                var log = new GlobalLogger(trackIdCude, "timers") { Message = sb.ToString() };
                TableManagerGlobalLogger.InsertOrUpdate(log);
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

            sb.AppendLine($"{Properties.Settings.Default.Param_Zone4Mapper} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} trackIdMapperEntity");

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
            var uploadXmlResponse = await ApiHelpers.ExecuteRequestAsync<ResponseUploadXml>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_UoloadXml), uploadXmlRequest);
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
            sb.AppendLine($"{Properties.Settings.Default.Param_Upload5} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} UploadXml");

            // upload xml

            // send to validate document sync
            start = DateTime.UtcNow;
            trackId = trackIdCude;
            var requestObjTrackId = new { trackId, draft = Properties.Settings.Default.Param_False };
            var validations = await ApiHelpers.ExecuteRequestAsync<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_ValidateDocumentUrl), requestObjTrackId);
            StackTrace = "validations.Count => " + validations.Count;
            sb.AppendLine($"{Properties.Settings.Default.Param_Validate6} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} ValidateDocument StackTrace: {StackTrace}");

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

                GlobalDocValidatorDocumentMeta documentMeta = null;


                documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackIdCude.ToLower(), trackIdCude.ToLower());
                message = $"La {documentMeta.DocumentTypeName} {serieAndNumber}, ha sido autorizada.";



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




                dianResponse.XmlBase64Bytes = XmlUtil.GenerateApplicationResponseBytes(trackIdCude, documentMeta, validations);

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

                    var processRegistrateComplete = await ApiHelpers.ExecuteRequestAsync<EventResponse>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_RegistrateCompletedRadianUrl), new { TrackId = trackIdCude, AuthCode = authCode });

                    if (processRegistrateComplete.Code != Properties.Settings.Default.Code_100)
                    {
                        dianResponse.IsValid = false;
                        dianResponse.XmlFileName = contentFileList.First().XmlFileName;
                        dianResponse.StatusCode = processRegistrateComplete.Code;
                        dianResponse.StatusDescription = processRegistrateComplete.Message;
                        UpdateInTransactions(documentParsed.DocumentKey.ToLower(), eventCode);
                        return dianResponse;
                    }


                    trackId = documentParsed.DocumentKey;
                    var responseCode = documentParsed.ResponseCode;
                    var processEventResponse = await ApiHelpers.ExecuteRequestAsync<EventResponse>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_ApplicationResponseProcessUrl), new { trackId, responseCode, trackIdCude, listId });

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
                sb.AppendLine($"{Properties.Settings.Default.Param_7AplicattionSendEvent} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} dianResponse.IsValid => {dianResponse.IsValid}");

                // ZONE APPLICATION

                // LAST ZONE
                start = DateTime.UtcNow;



                bool existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
                if (dianResponse.IsValid && !existDocument)
                    TableManagerGlobalDocValidatorDocument.InsertOrUpdate(validatorDocument);


                var action = "existDocument => " + existDocument + " dianResponse.IsValid " + dianResponse.IsValid;
                sb.AppendLine($"{Properties.Settings.Default.Param_LastZone} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} Action:{action}");




                UpdateInTransactions(documentParsed.DocumentKey.ToLower(), eventCode);

                var log = new GlobalLogger(trackIdCude, "timers") { Message = sb.ToString() };
                TableManagerGlobalLogger.InsertOrUpdate(log);

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
                var authCodeNew = authCode?.Trim().Substring(0, authCode.Trim().Length - 1);
                if (authEntity != null && authEntity.PartitionKey != accountCodeT && authCodeNew != accountCodeT)
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
        public async Task<EventResponse> GetXmlByDocumentKey(string trackId, string authCode)
        {
            var eventResponse = new EventResponse();
            XmlParser xmlParser = new XmlParser();
            DocumentParsed documentParsed = new DocumentParsed();
            DocumentParsedNomina documentParsedNomina = new DocumentParsedNomina();
            XmlParseNomina xmlParserNomina = new XmlParseNomina();
            string senderCode = string.Empty;
            string receiverCode = string.Empty;

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

            var xmlBytes = await GetXmlFromStorage(documentKey);
            if (xmlBytes == null)
            {
                eventResponse.Code = "404";
                eventResponse.Message = $"Xml con CUFE: '{documentKey}' no encontrado.";
                return eventResponse;
            }

            if (Convert.ToInt32(globalDocValidatorDocumentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayroll
                || Convert.ToInt32(globalDocValidatorDocumentMeta.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
            {

                xmlParserNomina = new XmlParseNomina(xmlBytes);
                if (!xmlParserNomina.Parser())
                {
                    eventResponse.Code = "206";
                    eventResponse.Message = "Xml con errores en los campos número documento emisor o número documento receptor";
                    return eventResponse;
                }

                documentParsedNomina = xmlParserNomina.Fields.ToObject<DocumentParsedNomina>();

                senderCode = documentParsedNomina.EmpleadorNIT;
                receiverCode = documentParsedNomina.NumeroDocumento;
            }
            else
            {

                xmlParser = new XmlParser(xmlBytes);
                if (!xmlParser.Parser())
                {
                    eventResponse.Code = "206";
                    eventResponse.Message = "Xml con errores en los campos número documento emisor o número documento receptor";
                    return eventResponse;
                }

                documentParsed = xmlParser.Fields.ToObject<DocumentParsed>();

                senderCode = documentParsed.SenderCode;
                receiverCode = documentParsed.ReceiverCode;
            }



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

        public DocIdentifierWithEventsResponse GetDocIdentifierWithEvents(string authCode, string contributorCode, string dateNumber)
        {
            var response = new DocIdentifierWithEventsResponse { StatusCode = "0", Success = true, CsvBase64Bytes = null };
            bool validateAuthorization = Convert.ToBoolean(ConfigurationManager.GetValue("ValidateAuthorizationDocIdentifierWithEvents"));
            if (validateAuthorization)
            {
                // La variable authCode obtienes el NIT que viene dentro del certificado y el contributorCode es el Nit enviado por parametro 
                var authEntity = GetAuthorization(contributorCode, authCode);
                if (authEntity == null)
                {
                    response.StatusCode = "401";
                    response.Success = false;
                    response.Message = $"NIT {authCode} no autorizado para consultar documentos con eventos del Nit { contributorCode }";
                    return response;
                }
            }

            if (string.IsNullOrWhiteSpace(dateNumber)) // Si no llega fecha, se establece la actual...
            {
                var now = DateTime.Now;
                dateNumber = $"{now.Year}{now.Month.ToString().PadLeft(2, char.Parse("0"))}{now.Day.ToString().PadLeft(2, char.Parse("0"))}";
            }

            var partitionKey = $"{dateNumber}|{contributorCode}"; // Se arma el PartitionKey compuesto (fecha|NIT)
            var eventsList = TableManagerGlobalDocumentWithEventRegistered.FindAll<GlobalDocumentWithEventRegistered>(partitionKey).ToList();
            if (eventsList == null || eventsList.Count <= 0)
            {
                response.Message = $"No existe información con los criterios de búsqueda recibidos";
                return response;
            }

            var cufesList = eventsList.Select(item => new Models.CufeModel { CUFE = item.RowKey }).ToList();

            var csv = StringUtil.ToCSV(cufesList);
            response.CsvBase64Bytes = Encoding.UTF8.GetBytes(csv);

            return response;
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

            var number = StringUtil.TextAfter(serieAndNumber, serie) == null ? string.Empty : StringUtil.TextAfter(serieAndNumber, serie).TrimStart('0');
            if (string.IsNullOrEmpty(number))
            {
                response.IsValid = false;
                response.StatusCode = "99";
                response.StatusMessage = "Documento con errores en campos mandatorios.";
                response.StatusDescription = "Validación contiene errores en campos mandatorios.";
                response.XmlDocumentKey = trackId;
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
                    ////
                    //var validations = TableManagerGlobalDocValidatorTracking.FindByPartition<GlobalDocValidatorTracking>(document.DocumentKey);
                    //if (validations.Any(v => !v.IsValid && v.Mandatory)) return null;

                    ////
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
        private void CheckDocument(ref DianResponse response, GlobalDocValidatorDocument document, string documentType, string eventCode, GlobalDocValidatorDocumentMeta meta = null)
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

        public async static Task<byte[]> GetXmlFromStorage(string trackId)
        {
            var tableManager = new TableManager("GlobalDocValidatorRuntime");
            var documentStatusValidation = tableManager.Find<GlobalDocValidatorRuntime>(trackId, "UPLOAD");
            if (documentStatusValidation == null)
                return null;

            var fileManager = new FileManager();
            var container = $"global";
            var fileName = $"docvalidator/{documentStatusValidation.Category}/{documentStatusValidation.Timestamp.Date.Year}/{documentStatusValidation.Timestamp.Date.Month.ToString().PadLeft(2, '0')}/{trackId}.xml";
            var xmlBytes = await fileManager.GetBytesAsync(container, fileName);

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

        public DianResponse SendRequestBulkDocumentsDownload(string authCode, string email, string nit, DateTime startDate, DateTime endDate, string documentGroup)
        {
            var timer = new Stopwatch();
            DianResponse dianResponse = new DianResponse
            {
                StatusCode = Properties.Settings.Default.Msg_Procees_Sucessfull,
                StatusDescription = "",
                ErrorMessage = new List<string>()
            };

            timer.Start();

            var user = GetGlobalContributor(authCode);

            var authorization = GetAuthorization(nit, authCode);

            if(authorization is null)
            {
                dianResponse.StatusCode = "89";
                dianResponse.StatusDescription = "Usted no está autorizado para realizar esta operación.";
                return dianResponse;
            }

            var request = new BulkDocumentDownloadRequest(user.Code, email, nit, startDate, endDate, documentGroup);
            var response = ApiHelpers.ExecuteRequest<BulkDocumentDownloadResponse>(ConfigurationManager.GetValue("BulkDocumentsDownloadUrl"), request);
            timer.Stop();
            if (!response.IsCorrect)
            {
                dianResponse.StatusCode = "89";
            }
            dianResponse.IsValid = response.IsCorrect;
            dianResponse.StatusDescription = response.Message;
            
            var globalEnd = timer.ElapsedMilliseconds / 1000;
            if (globalEnd >= 10)
            {
                var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow:yyyyMMdd}", Guid.NewGuid().ToString()) { Message = globalEnd.ToString(), Action = "Download" };
                TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
            }

            return dianResponse;
        }

        public DianResponseBulkDocumentDownload GetStatusBulkDocumentsDownload(string trackId)
        {
            var timer = new Stopwatch();
            DianResponseBulkDocumentDownload dianResponse = new DianResponseBulkDocumentDownload
            {
                StatusCode = Properties.Settings.Default.Msg_Procees_Sucessfull,
                StatusDescription = "",
                ErrorMessage = new List<string>()
            };

            timer.Start();
            var response = ApiHelpers.ExecuteRequest<GetStatusBulkDocumentDownloadResponse>(ConfigurationManager.GetValue("GetStatusBulkDocumentsDownloadUrl"), new { trackId });
            timer.Stop();
            if (response.IsCorrect)
            {
                dianResponse.StatusDescription = $"La solicitud {trackId} se encuentra en estado: {response.State}, {response.Response}";
                dianResponse.UrlDescarga = response.UrlFileCsv;
            }
            else
            {
                dianResponse.StatusCode = "89";
                dianResponse.StatusDescription = response.Message;
            }
            dianResponse.IsValid = response.IsCorrect;

            var globalEnd = timer.ElapsedMilliseconds / 1000;
            if (globalEnd >= 10)
            {
                var globalTimeValidation = new GlobalLogger($"MORETHAN10SECONDS-{DateTime.UtcNow:yyyyMMdd}", Guid.NewGuid().ToString()) { Message = globalEnd.ToString(), Action = "Download" };
                TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
            }

            return dianResponse;
        }
    }

    public class BulkDocumentDownloadRequest
    {
        public BulkDocumentDownloadRequest(string userId, string email, string nit, DateTime initialDate, DateTime endDate, string groupDocument)
        {
            UserId = userId;
            Email = email;
            Nit = nit;
            InitialDate = initialDate;
            EndDate = endDate;
            GroupDocument = groupDocument;
        }

        public string UserId { get; set; }
        public string Email { get; set; }
        public string Nit { get; set; }
        public DateTime InitialDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GroupDocument { get; set; }
    }
    public class BulkDocumentDownloadResponse
    {
        public bool IsCorrect { get; set; }
        public string Message { get; set; }
        public string LogDownloadWsTrackId { get; set; }
    }

    public class GetStatusBulkDocumentDownloadResponse
    {
        public bool IsCorrect { get; set; }
        public string Message { get; set; }
        public string Nit { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string logDownloadWs { get; set; }
        public string State { get; set; }
        public string Response { get; set; }
        public string TotalRecords { get; set; }
        public string UrlFileCsv { get; set; }
    }
}