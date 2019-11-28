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
using System.Threading.Tasks;
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

        private TableManager TableManagerGlobalBatchFileMapper = new TableManager("GlobalBatchFileMapper");
        private TableManager TableManagerGlobalBatchFileRuntime = new TableManager("GlobalBatchFileRuntime");
        private TableManager TableManagerGlobalBatchFileResult = new TableManager("GlobalBatchFileResult");
        private TableManager TableManagerGlobalBatchFileStatus = new TableManager("GlobalBatchFileStatus");
        private TableManager TableManagerGlobalContributor = new TableManager("GlobalContributor");

        private TableManager TableManagerGlobalNumberRange = new TableManager("GlobalNumberRange");
        private TableManager TableManagerDianOfeControl = new TableManager("DianOfeControl");
        private TableManager TableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");

        private TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");

        private FileManager fileManager = new FileManager();

        private int _ublVersion = 20;

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
        /// <param name="testSetId"></param>
        /// <returns></returns>
        public UploadDocumentResponse UploadMultipleDocumentAsync(string fileName, byte[] contentFile, string authCode = null, string testSetId = null)
        {
            UploadDocumentResponse responseMessages = new UploadDocumentResponse();
            List<XmlParamsResponseTrackId> trackIdList = new List<XmlParamsResponseTrackId>();

            var contentFileList = contentFile.ExtractMultipleZip();

            //Validar si tiene problemas el Zip enviado
            if (DianServicesUtils.ValidateIfZipHasFlagsErrors(fileName, contentFileList, trackIdList, ref responseMessages)) return responseMessages;

            //TrackId para el zip
            var zipKey = Guid.NewGuid().ToString();

            trackIdList.AddRange(contentFileList.Where(c => c.HasError).Select(x => new XmlParamsResponseTrackId
            {
                XmlFileName = x.XmlFileName,
                ProcessedMessage = x.XmlErrorMessage,
            }));

            var requestObjects = contentFileList.Where(c => !c.HasError).Select(x => DianServicesUtils.CreateRequestObject(Convert.ToBase64String(x.XmlBytes), x.XmlFileName));

            //Llamar a function para traer los valores de los Xpath
            var multipleResponsesXpathDataValue = ApiHelpers.ExecuteRequest<List<ResponseXpathDataValue>>(ConfigurationManager.GetValue("GetMultipleXpathDataValuesUrl"), requestObjects);

            //Validar si respuesta de la function viene con errores
            trackIdList.AddRange(multipleResponsesXpathDataValue.Where(c => (!c.Success) && (c == null)).Select(x => new XmlParamsResponseTrackId
            {
                XmlFileName = x.XpathsValues["FileName"],
                ProcessedMessage = x.Message
            }));
            multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.Where(c => c.Success).ToList();

            //Validar que nits emisores sea único
            var nits = multipleResponsesXpathDataValue.GroupBy(x => x.XpathsValues["SenderCodeXpath"]).Distinct();
            if (nits.Count() > 1)
            {
                trackIdList.Add(new XmlParamsResponseTrackId
                {
                    Success = false,
                    ProcessedMessage = "Lote de documentos contenidos en el archivo zip deben pertenecer todos a un mismo emisor."
                });
                responseMessages.ErrorMessageList = trackIdList;
                return responseMessages;
            }

            //Validar grandes contribuyentes
            if (string.IsNullOrEmpty(testSetId))
            {
                var nitBigContributor = multipleResponsesXpathDataValue.FirstOrDefault().XpathsValues["SenderCodeXpath"];
                var bigContributorRequestAuthorization = TableManagerGlobalBigContributorRequestAuthorization.Find<GlobalBigContributorRequestAuthorization>(nitBigContributor, nitBigContributor);
                if (bigContributorRequestAuthorization?.StatusCode != (int)BigContributorAuthorizationStatus.Authorized)
                {
                    trackIdList.Add(new XmlParamsResponseTrackId
                    {
                        Success = false,
                        ProcessedMessage = $"Empresa emisora con NIT {nitBigContributor} no se encuentra autorizada para enviar documentos por los lotes.",
                        SenderCode = nitBigContributor
                    });
                    responseMessages.ErrorMessageList = trackIdList;
                    return responseMessages;
                }
            }

            //Validar campos mandatorios basicos para el trabajo del WS
            var xpathValuesValidationResult = DianServicesUtils.ValidateXpathValues(multipleResponsesXpathDataValue);
            trackIdList.AddRange(xpathValuesValidationResult.Where(v => !v.Success));

            multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.Where(c => xpathValuesValidationResult.Where(v => v.Success).Select(v => v.DocumentKey).Contains(c.XpathsValues["DocumentKeyXpath"])).ToList();

            foreach (var responseXpathValues in multipleResponsesXpathDataValue)
            {
                if (!string.IsNullOrEmpty(responseXpathValues.XpathsValues["SeriesXpath"]) && responseXpathValues.XpathsValues["NumberXpath"].Length > responseXpathValues.XpathsValues["SeriesXpath"].Length)
                    responseXpathValues.XpathsValues["NumberXpath"] = responseXpathValues.XpathsValues["NumberXpath"].Substring(responseXpathValues.XpathsValues["SeriesXpath"].Length, responseXpathValues.XpathsValues["NumberXpath"].Length - responseXpathValues.XpathsValues["SeriesXpath"].Length);

                responseXpathValues.XpathsValues["SeriesAndNumberXpath"] = $"{responseXpathValues.XpathsValues["SeriesXpath"]}{responseXpathValues.XpathsValues["NumberXpath"]}";
            }

            ////Validar que el SenderCode coincide con las empresas permitidas
            var result = DianServicesUtils.ValidateIfIsAllowedToSend(multipleResponsesXpathDataValue, authCode, testSetId);
            if (result.Count > 0)
            {
                trackIdList.AddRange(result);
                responseMessages.ErrorMessageList = trackIdList;
                return responseMessages;
            }

            if (multipleResponsesXpathDataValue.Count > 0)
            {
                //Crear los request Objects para upload
                List<RequestObject> uploadRequest = new List<RequestObject>();
                uploadRequest.AddRange(multipleResponsesXpathDataValue.Select(
                    c => new RequestObject
                    {
                        Category = DianServicesUtils.GetEnumDescription((DianServicesUtils.Category)int.Parse(DianServicesUtils.StractUblVersion(c.XpathsValues["UblVersionXpath"]))),
                        XmlBase64 = c.XpathsValues["XmlBase64"],
                        FileName = c.XpathsValues["FileName"],
                        DocumentTypeId = c.XpathsValues["DocumentTypeXpath"],
                        TrackId = c.XpathsValues["DocumentKeyXpath"],
                        ZipKey = zipKey,
                        TestSetId = testSetId,
                        SoftwareId = c.XpathsValues["SoftwareIdXpath"]
                    }
                ));

                // upload multiple xml's
                var responseUploadMultipleXml = ApiHelpers.ExecuteRequest<List<ResponseUploadMultipleXml>>(ConfigurationManager.GetValue("UploadMultipleXmlUrl"), uploadRequest);
                var uploadFailed = responseUploadMultipleXml.Where(m => !m.Success && multipleResponsesXpathDataValue.Select(d => d.XpathsValues["DocumentKeyXpath"]).Contains(m.DocumentKey));
                trackIdList.AddRange(uploadFailed.Select(x => new XmlParamsResponseTrackId
                {
                    DocumentKey = x.DocumentKey,
                    XmlFileName = x.FileName,
                    ProcessedMessage = x.Message

                }));

                multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.Where(x => !uploadFailed.Select(e => e.DocumentKey).Contains(x.XpathsValues["DocumentKeyXpath"])).ToList();

                if (multipleResponsesXpathDataValue.Count > 0)
                {
                    List<EventGridEvent> eventsList = multipleResponsesXpathDataValue.Select(x => new EventGridEvent
                    {
                        Id = Guid.NewGuid().ToString(),
                        EventType = "Validate.Document.Event",
                        Data = JsonConvert.SerializeObject(new Dictionary<string, string> { { "trackId", x.XpathsValues["DocumentKeyXpath"] } }),
                        EventTime = DateTime.UtcNow,
                        Subject = $"|DT:{x.XpathsValues["DocumentTypeXpath"]}|",
                        DataVersion = "2.0"
                    }).ToList();

                    EventGridManager.Instance().SendMessagesToEventGrid(eventsList);
                    responseMessages.ZipKey = zipKey;
                }
            }

            responseMessages.ErrorMessageList = trackIdList.Any() ? trackIdList : null;

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
                XmlFileName = contentFileList.First().XmlFileName,
                ErrorMessage = new List<string>()
            };

            if (contentFileList.Any(f => f.MaxQuantityAllowedFailed || f.UnzipError))
            {
                var countZeroMess = "Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML valido.";
                var countOverOneMess = "Encontrados mas de un XML descomprimiendo el archivo ZIP: Se debe enviar solo un fichero XML.";

                dianResponse.StatusCode = "90";
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

                dianResponse.StatusCode = "90";
                return dianResponse;
            }
            var zone1 = new GlobalLogger("", "Zone 1") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // ZONE 1

            // ZONE 2
            start = DateTime.UtcNow;
            var xmlBase64 = Convert.ToBase64String(contentFileList.First().XmlBytes);
            var zone2 = new GlobalLogger("", "Zone 2") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // ZONE 2

            start = DateTime.UtcNow;
            var requestObj = DianServicesUtils.CreateGetXpathRequestObject(xmlBase64);
            var responseXpathValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObj);
            var xpath = new GlobalLogger("", "2 Xpath") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };

            // ZONE 3
            start = DateTime.UtcNow;
            if (!responseXpathValues.Success)
            {
                dianResponse.StatusDescription = $"{responseXpathValues.Message}";
                dianResponse.StatusCode = "90";
                return dianResponse;
            }

            //Validar campos mandatorios basicos para el trabajo del WS
            if (!DianServicesUtils.ValidateXpathValuesSync(responseXpathValues, ref dianResponse))
                return dianResponse;

            if (!string.IsNullOrEmpty(responseXpathValues.XpathsValues["SeriesXpath"]) && responseXpathValues.XpathsValues["NumberXpath"].Length > responseXpathValues.XpathsValues["SeriesXpath"].Length)
                responseXpathValues.XpathsValues["NumberXpath"] = responseXpathValues.XpathsValues["NumberXpath"].Substring(responseXpathValues.XpathsValues["SeriesXpath"].Length, responseXpathValues.XpathsValues["NumberXpath"].Length - responseXpathValues.XpathsValues["SeriesXpath"].Length);

            responseXpathValues.XpathsValues["SeriesAndNumberXpath"] = $"{responseXpathValues.XpathsValues["SeriesXpath"]}{responseXpathValues.XpathsValues["NumberXpath"]}";

            var senderCode = responseXpathValues.XpathsValues["SenderCodeXpath"];
            var docTypeCode = responseXpathValues.XpathsValues["DocumentTypeXpath"];
            if (string.IsNullOrEmpty(docTypeCode))
                docTypeCode = responseXpathValues.XpathsValues["DocumentTypeId"];
            var serie = $"{responseXpathValues.XpathsValues["SeriesXpath"]}";
            var number = $"{responseXpathValues.XpathsValues["NumberXpath"]}";
            var trackId = responseXpathValues.XpathsValues["DocumentKeyXpath"].ToLower();
            var zone3 = new GlobalLogger("", "Zone 3") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            // ZONE 3

            start = DateTime.UtcNow;
            var authEntity = GetAuthorization(senderCode, authCode);
            if (authEntity == null)
            {
                dianResponse.XmlFileName = $"{fileName}";
                dianResponse.StatusCode = "90";
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

            // Check document duplicity
            start = DateTime.UtcNow;
            var response = CheckDocumentDuplicity(senderCode, docTypeCode, serie, number);
            var duplicity = new GlobalLogger(trackId, "Duplicity") { Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString() };
            if (response != null) return response;

            unzip.PartitionKey = trackId;
            xpath.PartitionKey = trackId;
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
                dianResponse.StatusCode = "90";
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

                Task firstLocalRun = Task.Run(() =>
                {
                    var applicationResponse = ApiHelpers.ExecuteRequest<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), new { trackId });
                    dianResponse.XmlBase64Bytes = !applicationResponse.Success ? null : applicationResponse.Content;
                });
                Task secondLocalRun = Task.Run(() =>
                {
                    documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                    var prefix = !string.IsNullOrEmpty(serie) ? serie : string.Empty;
                    message = (string.IsNullOrEmpty(prefix)) ? $"La {documentMeta.DocumentTypeName} {number}, ha sido autorizada." : $"La {documentMeta.DocumentTypeName} {prefix}-{number}, ha sido autorizada.";
                    existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                });

                var errors = validations.Where(r => r.Mandatory && !r.IsValid).ToList();
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

                    dianResponse.IsValid = errors.Any() ? dianResponse.IsValid : true;
                    dianResponse.StatusMessage = errors.Any() ? dianResponse.StatusMessage : message;
                    dianResponse.ErrorMessage.AddRange(notificationList);
                }

                arrayTasks.Add(firstLocalRun);
                arrayTasks.Add(secondLocalRun);
                Task.WhenAll(arrayTasks).Wait();

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
                    TableManagerGlobalLogger.InsertOrUpdateAsync(xpath),
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
                if (validatorDocument != null && !existDocument)
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
        /// <param name="fileName"></param>
        /// <param name="contentFile"></param>
        /// <param name="authCode"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public UploadDocumentResponse UploadDocumentAttachmentAsync(string fileName, byte[] contentFile, string authCode = null, string email = null)
        {
            var startDate = DateTime.Now;

            UploadDocumentResponse responseMessages = new UploadDocumentResponse();

            List<XmlParamsResponseTrackId> trackIdList = new List<XmlParamsResponseTrackId>();

            var contentFileList = contentFile.ExtractMultipleZip();

            //Validar si tiene problemas el Zip enviado
            if (DianServicesUtils.ValidateIfZipHasFlagsErrors(fileName, contentFileList, trackIdList, ref responseMessages))
                return responseMessages;

            var zipKey = Guid.NewGuid().ToString();

            Parallel.ForEach(contentFileList, new ParallelOptions { MaxDegreeOfParallelism = 1 }, contentXmlFile =>
            {
                XmlParamsResponseTrackId respTrackId = new XmlParamsResponseTrackId();

                //Validar si algun xml contenido del zip enviado contiene error
                if (DianServicesUtils.ValidateIfZipParamHasError(contentXmlFile.XmlFileName, contentXmlFile, ref trackIdList))
                    return;

                var tupleEmbeddedElements = XmlUtil.GetEmbeddedXElementFromAttachment(contentXmlFile.XmlBytes);

                if (!DianServicesUtils.ValidateIfEmbeddedAttachmentHasItems(tupleEmbeddedElements, contentXmlFile.XmlFileName, ref trackIdList))
                    return;

                if (!XmlUtil.ValidateIfAttachmentSameDocumentKey(tupleEmbeddedElements, contentXmlFile.XmlFileName, ref trackIdList))
                    return;

                var xmlBase64 = Convert.ToBase64String(tupleEmbeddedElements.Item1);

                var requestObj = DianServicesUtils.CreateRequestObject(xmlBase64);

                //ResponseXpathDataValue responseXpathValues = DianServicesUtils.GetXpathDataValues(requestObj);
                ResponseXpathDataValue responseXpathValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObj);

                //Validar si respuesta de la function viene con errores
                if (!DianServicesUtils.ValidateXpathValuesResponse(responseXpathValues, contentXmlFile.XmlFileName, ref trackIdList))
                    return;

                //Validar campos mandatorios basicos para el trabajo del WS
                if (!DianServicesUtils.ValidateXpathValues(responseXpathValues, contentXmlFile.XmlFileName, ref trackIdList))
                    return;

                var senderCode = responseXpathValues.XpathsValues["SenderCodeXpath"];
                var docKey = responseXpathValues.XpathsValues["DocumentKeyXpath"];

                if (senderCode.Contains('|'))
                    senderCode = senderCode.Split('|').Last();

                //Validar que el SenderCode coincide con las empresas permitidas de un PA
                if (!DianServicesUtils.ValidateIfIsAllowedToSend(contentXmlFile.XmlFileName, senderCode, authCode, email, ref trackIdList))
                    return;

                if (!string.IsNullOrEmpty(responseXpathValues.XpathsValues["SeriesXpath"]))
                    responseXpathValues.XpathsValues["NumberXpath"] = responseXpathValues.XpathsValues["NumberXpath"].Substring(responseXpathValues.XpathsValues["SeriesXpath"].Length, responseXpathValues.XpathsValues["NumberXpath"].Length - responseXpathValues.XpathsValues["SeriesXpath"].Length);

                responseXpathValues.XpathsValues["SeriesAndNumberXpath"] = $"{responseXpathValues.XpathsValues["SeriesXpath"]}{responseXpathValues.XpathsValues["NumberXpath"]}";

                _ublVersion = int.Parse(DianServicesUtils.StractUblVersion(responseXpathValues.XpathsValues["UblVersionXpath"]));

                var receiverCode = responseXpathValues.XpathsValues["ReceiverCodeXpath"];
                var docTypeCode = responseXpathValues.XpathsValues["DocumentTypeXpath"];
                if (string.IsNullOrEmpty(docTypeCode))
                    docTypeCode = responseXpathValues.XpathsValues["DocumentTypeId"];

                var trackId = responseXpathValues.XpathsValues["DocumentKeyXpath"];

                var identifier = StringUtil.EncryptSHA256($"{senderCode}{docTypeCode}{responseXpathValues.XpathsValues["SeriesAndNumberXpath"]}");
                ////Validar si documento fue insertado anteriormente con mismo CUFE
                var documentEntity = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(identifier, identifier);
                //Validar si documento fue insertado anteriormente con mismo CUFE
                if (DianServicesUtils.ValidateIfDocumentSentAlready(documentEntity, contentXmlFile.XmlFileName, ref trackIdList))
                    return;

                var category = DianServicesUtils.GetEnumDescription((DianServicesUtils.Category)_ublVersion);

                var sendRequestObj = new { category, xmlBase64, fileName = contentXmlFile.XmlFileName, documentTypeId = docTypeCode, trackId, zipKey };

                // Inserta en DianFileMapper
                DianServicesUtils.UploadXml(contentXmlFile.XmlFileName, trackId, sendRequestObj);

                var requestObjTrackId = new { trackId };

                // send to validate document async
                var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("ValidateDocumentAsyncUrl"), requestObjTrackId);
                var result = response.Content.ReadAsStringAsync().Result;

                DianServicesUtils.ValidateResultValidatorFunction(trackId, result, contentXmlFile.XmlFileName, respTrackId, ref trackIdList);

                var processTime = (DateTime.Now - startDate).TotalSeconds;

                DianServicesUtils.EventRuleNotification(processTime, int.Parse(docTypeCode), _ublVersion.ToString(), 1, senderCode, false, true);
            });

            responseMessages.ZipKey = zipKey;
            responseMessages.ErrorMessageList = trackIdList.Any() ? trackIdList : null;

            return responseMessages;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ExchangeEmailResponse GetExchangeEmails(string authCode)
        {
            var contributor = TableManagerGlobalContributor.Find<GlobalContributor>(authCode, authCode);
            if (contributor == null)
                return new ExchangeEmailResponse { StatusCode = "90", Success = false, Message = $"NIT {authCode} no autorizado para consultar correos de recepción de facturas.", CsvBase64Bytes = null };

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

                    Task firstLocalRun = Task.Run(() =>
                    {
                        var applicationResponse = ApiHelpers.ExecuteRequest<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), new { trackId });
                        response.XmlBase64Bytes = !applicationResponse.Success ? null : applicationResponse.Content;
                        if (!applicationResponse.Success)
                            Debug.WriteLine(applicationResponse.Message);
                    });
                    Task secondLocalRun = Task.Run(() =>
                    {
                        documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                        if (!string.IsNullOrEmpty(documentMeta.Identifier))
                            existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta.Identifier, documentMeta.Identifier);
                        applicationResponseExist = XmlUtil.ApplicationResponseExist(documentMeta);
                    });

                    Task fourLocalRun = Task.Run(() =>
                    {
                        validations = ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("GetValidationsByTrackIdUrl"), new { trackId });
                    });

                    arrayTasks.Add(firstLocalRun);
                    arrayTasks.Add(secondLocalRun);
                    arrayTasks.Add(fourLocalRun);
                    Task.WhenAll(arrayTasks).Wait();

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
        /// <param name="contentFile"></param>
        /// <returns></returns>
        public List<EventResponse> SendEventUpdateStatus(byte[] contentFile, string authCode)
        {
            var globalStart = DateTime.UtcNow;
            List<EventResponse> eventsResponse = new List<EventResponse>();

            var contentFileList = contentFile.ExtractMultipleZip(1);

            if (contentFileList.Any(f => f.MaxQuantityAllowedFailed || f.UnzipError))
            {
                var countZeroMess = "Error descomprimiendo el archivo ZIP: No fue encontrado ningun documento XML valido.";
                var countOverOneMess = "Encontrados mas de un XML descomprimiendo el archivo ZIP: Se debe enviar solo un fichero XML.";

                var eventResponse = new EventResponse()
                {
                    Code = "90",
                    Message = contentFileList.Any(f => f.MaxQuantityAllowedFailed) ? countOverOneMess : countZeroMess
                };
                eventsResponse.Add(eventResponse);
                return eventsResponse;
            }

            foreach (var contentItem in contentFileList)
            {
                var contentBytes = contentItem.XmlBytes;

                var xmlBase64 = Convert.ToBase64String(contentBytes);

                var requestObj = new Dictionary<string, string>
                {
                    { "XmlBase64", xmlBase64},
                    { "CUDEXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='UUID']" },
                    { "DocumentKeyXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='UUID']" },
                    { "ResponseCodeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" }
                };

                ResponseXpathDataValue responseXpathValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObj);

                if (responseXpathValues == null || !responseXpathValues.Success)
                {
                    var eventResponse = new EventResponse()
                    {
                        Code = "Error",
                        Message = "ApplicationResponse con errores"
                    };

                    eventsResponse.Add(eventResponse);
                    continue;
                }

                var appResponseKey = responseXpathValues.XpathsValues["CUDEXpath"];
                var documentKey = responseXpathValues.XpathsValues["DocumentKeyXpath"];
                var responseCode = responseXpathValues.XpathsValues["ResponseCodeXpath"];

                var documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(documentKey, documentKey);

                var documentEntity = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);

                if (documentEntity == null)
                {
                    var eventResponse = new EventResponse()
                    {
                        Code = "Error",
                        Message = "Documento no se encuentra en los registros de la DIAN"
                    };
                    eventsResponse.Add(eventResponse);
                    continue;
                }

                // check permissions
                //var authEntity = GetAuthorization(documentMeta?.SenderCode, authCode);
                //if (authEntity == null)
                //{
                //    var eventResponse = new EventResponse()
                //    {
                //        Code = "90",
                //        Message = $"NIT {authCode} no autorizado a enviar eventos para emisor con NIT {documentMeta.SenderCode}."
                //    };
                //    eventsResponse.Add(eventResponse);
                //    return eventsResponse;
                //}

                var uploadRequest = new
                {
                    documentTypeId = ((int)DocumentType.ApplicationResponse).ToString(),
                    xmlBase64,
                    fileName = contentItem.XmlFileName,
                    isEvent = true,
                    trackId = Guid.NewGuid().ToString(),
                };

                var responseUpload = ApiHelpers.ExecuteRequest<ResponseUploadXml>(ConfigurationManager.GetValue("UploadXmlUrl"), uploadRequest);
                if (!responseUpload.Success)
                {
                    var eventResponse = new EventResponse()
                    {
                        Code = "Error",
                        Message = responseUpload.Message
                    };
                    eventsResponse.Add(eventResponse);
                    continue;
                }

                // send to validate document
                var validationRequest = new { uploadRequest.trackId };

                // send to validate application response
                List<GlobalDocValidatorTracking> validations = ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("ValidateDocumentUrl"), validationRequest);
                if (validations.Any(v => !v.IsValid))
                {
                    var validationsFailed = validations.Where(v => !v.IsValid).ToList();
                    if (validations.Where(v => !v.IsValid && v.BreakOut).ToList().Any()) validationsFailed = validationsFailed.OrderByDescending(x => x.BreakOut).OrderBy(y => y.Priority).ToList();
                    else validationsFailed = validationsFailed.OrderBy(y => y.Priority).ToList();

                    var validation = validationsFailed.FirstOrDefault();

                    eventsResponse.Add(new EventResponse
                    {
                        Code = validation.ErrorCode,
                        Message = validation.ErrorMessage
                    });
                }
                else
                {
                    var trackId = documentEntity.DocumentKey;
                    var sendRequestObj = new { trackId, responseCode };

                    var response = ApiHelpers.ExecuteRequest<EventResponse>(ConfigurationManager.GetValue("ApplicationResponseProcessUrl"), sendRequestObj);

                    eventsResponse.Add(response);
                }
            }

            var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
            var finish = new GlobalLogger("SendEventUpdateStatus", Guid.NewGuid().ToString()) { Message = globalEnd.ToString() };
            TableManagerGlobalLogger.InsertOrUpdate(finish);

            return eventsResponse;
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

            //authCode = "900680844";

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

            var downloadXmlRequest = new { trackId };
            var responseDownloadXml = ApiHelpers.ExecuteRequest<ResponseDownloadXml>(ConfigurationManager.GetValue("DownloadXmlUrl"), downloadXmlRequest);

            if (!responseDownloadXml.Success)
            {
                eventResponse.Code = "205";
                eventResponse.Message = responseDownloadXml.Message;
                return eventResponse;
            }
            else
            {
                var getXpathDataValuesRequest = new Dictionary<string, string>
                {
                    { "XmlBase64", responseDownloadXml.XmlBase64},
                    { "SenderCodeXpath", "//*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']|/*[local-name()='Invoice']/*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyIdentification']/*[local-name()='ID']" },
                    { "ReceiverCodeXpath", "//*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']|/*[local-name()='Invoice']/*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyIdentification']/*[local-name()='ID']" }
                };

                var responseXpathValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), getXpathDataValuesRequest);
                if (responseXpathValues == null || !responseXpathValues.Success)
                {
                    eventResponse.Code = "206";
                    eventResponse.Message = "Xml con errores en los campos número documento emisor o número documento receptor";
                    return eventResponse;
                }

                var senderCode = responseXpathValues.XpathsValues["SenderCodeXpath"].Split('|').FirstOrDefault();
                var receiverCode = responseXpathValues.XpathsValues["ReceiverCodeXpath"].Split('|').FirstOrDefault();

                if (!authCode.Contains(senderCode) && !authCode.Contains(receiverCode))
                {
                    // pt con emisor
                    var authEntity = GetAuthorization(senderCode, authCode);
                    if (authEntity == null)
                    {
                        // pt con receptor
                        authEntity = GetAuthorization(senderCode, receiverCode);
                        if (authEntity == null)
                        {
                            eventResponse.Code = "401";
                            eventResponse.Message = $"NIT: {authCode} del certificado no autorizado para consultar xmls de emisor con NIT: {senderCode} y receptor con NIT: {receiverCode}";
                            return eventResponse;
                        }
                    }
                }
            }

            eventResponse.Code = "100";
            eventResponse.Message = $"Accion completada OK";
            eventResponse.XmlBytesBase64 = responseDownloadXml.XmlBase64;

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
        private DianResponse CheckDocumentDuplicity(string senderCode, string documentType, string serie, string number)
        {
            var response = new DianResponse(); ;
            // identifier
            if (new string[] { "01", "02", "04" }.Contains(documentType)) documentType = "01";
            var identifier = StringUtil.GenerateIdentifierSHA256($"{senderCode}{documentType}{serie}{number}");
            var document = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(identifier, identifier);
            if (document != null)
            {
                var failedList = new List<string>
                {
                    $"Regla: 90, Rechazo: Documento procesado anteriormente."
                };
                response.IsValid = false;
                response.StatusMessage = "Documento con errores en campos mandatorios.";
                response.StatusDescription = "Validación contiene errores en campos mandatorios.";
                response.ErrorMessage = new List<string>();
                response.ErrorMessage.AddRange(failedList);
                var documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(document.DocumentKey, document.DocumentKey);
                var xmlBytes = GetApplicationResponseIfExist(documentMeta);
                response.XmlBase64Bytes = xmlBytes;
                response.XmlDocumentKey = document.DocumentKey;
                return response;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentMeta"></param>
        /// <returns></returns>
        public byte[] GetApplicationResponseIfExist(GlobalDocValidatorDocumentMeta documentMeta)
        {
            if (documentMeta == null) return null;
            var processDate = documentMeta.Timestamp;
            var serieFolder = string.IsNullOrEmpty(documentMeta.Serie) ? "NOTSERIE" : documentMeta.Serie;
            var isValidFolder = "Success";
            var fileName = $"responses/{documentMeta.Timestamp.Year}/{documentMeta.Timestamp.Month.ToString().PadLeft(2, '0')}/{documentMeta.Timestamp.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";
            var xmlBytes = fileManager.GetBytes("dian", fileName);
            if (xmlBytes == null)
            {
                fileName = $"responses/{documentMeta.EmissionDate.Year}/{documentMeta.EmissionDate.Month.ToString().PadLeft(2, '0')}/{documentMeta.EmissionDate.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";
                xmlBytes = fileManager.GetBytes("dian", fileName);
            }
            if (xmlBytes == null)
            {
                var applicationResponse = ApiHelpers.ExecuteRequest<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), new { trackId = documentMeta.PartitionKey });
                xmlBytes = !applicationResponse.Success ? null : applicationResponse.Content;
            }
            return xmlBytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="senderCode"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        private GlobalAuthorization GetAuthorization(string senderCode, string authCode)
        {

            //if(!string.IsNullOrEmpty(senderCode) && !string.IsNullOrEmpty(authCode))
            //{

            //}

            var authorization = new GlobalAuthorization();
            var trimAuthCode = authCode?.Trim();
            var cacheItemKey = $"auth-{trimAuthCode}";
            //var cacheItem = InstanceCache.AuthorizationsInstanceCache.GetCacheItem(cacheItemKey);
            //if (cacheItem == null)
            {
                var newAuthCode = trimAuthCode?.Substring(0, trimAuthCode.Length - 1);
                authorization = TableManagerGlobalAuthorization.Find<GlobalAuthorization>(trimAuthCode, senderCode);
                if (authorization == null)
                    authorization = TableManagerGlobalAuthorization.Find<GlobalAuthorization>(newAuthCode, senderCode);
                if (authorization == null) return null;
                //CacheItemPolicy policy = new CacheItemPolicy
                //{
                //    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1)
                //};
                //InstanceCache.AuthorizationsInstanceCache.Set(new CacheItem($"auth-{authorization.PartitionKey}", authorization), policy);
            }
            //else
            //    authorization = (GlobalAuthorization)cacheItem.Value;

            return authorization;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class RequestObject
    {
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "xmlBase64")]
        public string XmlBase64 { get; set; }
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }
        [JsonProperty(PropertyName = "documentTypeId")]
        public string DocumentTypeId { get; set; }
        [JsonProperty(PropertyName = "trackId")]
        public string TrackId { get; set; }
        [JsonProperty(PropertyName = "zipKey")]
        public string ZipKey { get; set; }
        [JsonProperty(PropertyName = "isEvent")]
        public bool? IsEvent { get; set; }
        public string SoftwareId { get; set; }
        [JsonProperty(PropertyName = "testSetId")]
        public string TestSetId { get; set; }
    }
}
