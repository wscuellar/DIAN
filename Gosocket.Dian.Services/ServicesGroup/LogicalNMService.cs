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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gosocket.Dian.Logger.Logger;

namespace Gosocket.Dian.Services.ServicesGroup
{
    public class LogicalNMService : IDisposable
    {
        private static readonly TableManager TableManagerDianFileMapper = new TableManager("DianFileMapper");
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager TableManagerGlobalDocValidatorDocument = new TableManager("GlobalDocValidatorDocument");
        private static readonly TableManager TableManagerGlobalDocValidatorRuntime = new TableManager("GlobalDocValidatorRuntime");
        private static readonly TableManager TableManagerGlobalDocValidatorTracking = new TableManager("GlobalDocValidatorTracking");

        private static readonly TableManager TableManagerGlobalBatchFileMapper = new TableManager("GlobalBatchFileMapper");
        private static readonly TableManager TableManagerGlobalBatchFileRuntime = new TableManager("GlobalBatchFileRuntime");
        private static readonly TableManager TableManagerGlobalBatchFileResult = new TableManager("GlobalBatchFileResult");
        private static readonly TableManager TableManagerGlobalBatchFileStatus = new TableManager("GlobalBatchFileStatus");
        private static readonly TableManager TableManagerGlobalContributor = new TableManager("GlobalContributor");
        private static readonly TableManager TableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");

        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly TableManager tableManager = new TableManager("GlobalDocValidatorRuntime");

        private static readonly FileManager fileManager = new FileManager();

        private readonly string blobContainer = "global";
        private readonly string blobContainerFolder = "batchValidator";

        public LogicalNMService() { }

        public void Dispose()
        {
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
        }

        #region SendNomina

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentFile"></param>
        /// <returns></returns>
        //public async Task<DianResponse> SendNominaUpdateStatusAsync(byte[] contentFile, string authCode)
        public async Task<DianResponse> SendNominaUpdateStatusAsync(byte[] contentFile, string authCode)
        {
            var start = DateTime.UtcNow;
            var globalStart = DateTime.UtcNow;
            var contentFileList = contentFile.ExtractMultipleZip();
            var filename = contentFileList.First().XmlFileName;
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
                    dianResponse.StatusMessage = Properties.Settings.Default.Msg_Error_NominaOnlyDocument;

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
            XmlParseNomina xmlParser;
            try
            {
                xmlParser = new XmlParseNomina(xmlBytes);
                if (!xmlParser.Parser())
                    throw new Exception(xmlParser.ParserError);
            }
            catch (Exception ex)
            {
                var failedList = new List<string> { $"Regla: ZB01, Fallo en el esquema XML del archivo." };
                dianResponse.IsValid = false;
                dianResponse.StatusCode = "99";
                dianResponse.StatusMessage = "Rechazo: Fallo en el esquema XML del archivo. " + ex.Message;
                dianResponse.StatusDescription = "Documento con errores en campos mandatorios.";
                dianResponse.ErrorMessage.AddRange(failedList);
                return dianResponse;
            }

            var documentParsed = xmlParser.Fields.ToObject<DocumentParsedNomina>();
            DocumentParsedNomina.SetValues(ref documentParsed);
            var trackId = documentParsed.CUNE;
            sb.AppendLine($"{Properties.Settings.Default.Param_Parser} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} DocumentParsedNomina.SetValues");
            
            // Parser

            // ZONE 3
            //Validar campos mandatorios basicos para el trabajo del WS
            if (!DianServicesUtils.ValidateParserNomina(documentParsed, xmlParser, ref dianResponse)) return dianResponse;

            var senderCode = documentParsed.EmpleadorNIT;
            var serieAndNumber = documentParsed.SerieAndNumber;
            var docTypeCode = documentParsed.DocumentTypeId;

     
            // Duplicity
            start = DateTime.UtcNow;
            var response = CheckDocumentDuplicity(senderCode, docTypeCode, serieAndNumber, trackId);
            if (!response.IsValid)
            {
                var log = new GlobalLogger(trackId, "CheckDocumentDuplicity") { Message = sb.ToString(), Action = $"CheckDocumentDuplicity" };
                TableManagerGlobalLogger.InsertOrUpdate(log);
                return response;
            }                              
            sb.AppendLine($"{Properties.Settings.Default.Param_Zone3} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} CheckDocumentDuplicity");

            // ZONE 3

            // ZONE MAPPER
            start = DateTime.UtcNow;
            if (contentFileList.First().XmlFileName.Split('/').Count() > 1 && contentFileList.First().XmlFileName.Split('/').Last() != null)
                contentFileList.First().XmlFileName = contentFileList.First().XmlFileName.Split('/').Last();

            var trackIdMapperEntity = new GlobalOseTrackIdMapper(contentFileList[0].XmlFileName, trackId);
            TableManagerDianFileMapper.InsertOrUpdate(trackIdMapperEntity);
            sb.AppendLine($"{Properties.Settings.Default.Param_Zone4Mapper} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} TrackIdMapperEntity");           

            // ZONE MAPPER

            // upload xml
            start = DateTime.UtcNow;
            bool sendTestSet = false;
            var uploadXmlRequest = new { xmlBase64, filename, documentTypeId = documentParsed.DocumentTypeId, trackId, eventNomina = true, sendTestSet };
            var uploadXmlResponse = await ApiHelpers.ExecuteRequestAsync<ResponseUploadXml>(ConfigurationManager.GetValue("UploadXmlUrl"), uploadXmlRequest);
            //var uploadXmlResponse = ApiHelpers.ExecuteRequest<ResponseUploadXml>("http://localhost:7071/api/UploadXml", uploadXmlRequest);
            if (!uploadXmlResponse.Success)
            {
                dianResponse.StatusCode = Properties.Settings.Default.Code_89;
                dianResponse.StatusDescription = uploadXmlResponse.Message;
                var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                if (globalEnd >= 10)
                {
                    var globalTimeValidation = new GlobalLogger(trackId, $"MORETHAN10SECONDS-{DateTime.UtcNow:yyyyMMdd}") { Message = globalEnd.ToString(CultureInfo.InvariantCulture), Action = Properties.Settings.Default.Param_Uoload };
                    TableManagerGlobalLogger.InsertOrUpdate(globalTimeValidation);
                }
                return dianResponse;
            }            
            sb.AppendLine($"{Properties.Settings.Default.Param_Uoload} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} UploadXml");
            // upload xml

            // send to validate document sync
            var requestObjTrackId = new { trackId, draft = Properties.Settings.Default.Param_False };
            var validations = await ApiHelpers.ExecuteRequestAsync<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("ValidateDocumentUrl"), requestObjTrackId);            
            sb.AppendLine($"{Properties.Settings.Default.Param_ValidateDocumentUrl} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} ValidateDocument ");
            // send to validate document sync

            if (validations.Count == 0)
            {
                dianResponse.XmlFileName = filename;
                dianResponse.StatusDescription = string.Empty;
                dianResponse.StatusCode = Properties.Settings.Default.Code_66;
                var globalEnd = DateTime.UtcNow.Subtract(globalStart).TotalSeconds;
                if (globalEnd >= 10)
                {
                    var globalTimeValidation = new GlobalLogger(trackId, $"MORETHAN10SECONDS-{DateTime.UtcNow:yyyyMMdd}") { Message = globalEnd.ToString(CultureInfo.InvariantCulture), Action = Properties.Settings.Default.Param_ValidateDocumentUrl };
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

               
               documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
               message = $"La {documentMeta.DocumentTypeName} {serieAndNumber}, ha sido autorizada.";
              
               

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

                
                

                var applicationResponse = XmlUtil.GetApplicationResponseIfExist(documentMeta);
                dianResponse.XmlBase64Bytes = applicationResponse ?? XmlUtil.GenerateApplicationResponseBytes(trackId, documentMeta, validations);
                dianResponse.XmlDocumentKey = trackId;

                GlobalDocValidatorDocument validatorDocument = null;

                if (dianResponse.IsValid)
                {
                    dianResponse.StatusCode = Properties.Settings.Default.Code_00;
                    dianResponse.StatusMessage = message;
                    dianResponse.StatusDescription = Properties.Settings.Default.Msg_Procees_Sucessfull;
                    validatorDocument = new GlobalDocValidatorDocument(documentMeta?.Identifier, documentMeta?.Identifier) { DocumentKey = trackId, EmissionDateNumber = documentMeta?.EmissionDate.ToString("yyyyMMdd") };


                }
                else
                {
                    dianResponse.IsValid = false;
                    dianResponse.StatusCode = Properties.Settings.Default.Code_99;
                    dianResponse.StatusDescription = Properties.Settings.Default.Msg_Error_FieldMandatori;
                }
                sb.AppendLine($"{Properties.Settings.Default.Param_7AplicattionSendEvent} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)} dianResponse.IsValid => {dianResponse.IsValid}");
                
                // ZONE APPLICATION

                // LAST ZONE
                start = DateTime.UtcNow;
                

                if (dianResponse.IsValid)
                {
                    existDocument = TableManagerGlobalDocValidatorDocument.Exist<GlobalDocValidatorDocument>(documentMeta?.Identifier, documentMeta?.Identifier);
                    if (!existDocument) 
                        TableManagerGlobalDocValidatorDocument.InsertOrUpdate(validatorDocument);
                  
                    var processRegistrateComplete = await ApiHelpers.ExecuteRequestAsync<EventResponse>(ConfigurationManager.GetValue(Properties.Settings.Default.Param_RegistrateCompletedPayrollUrl), new { TrackId = trackId, AuthCode = authCode });
                    if (processRegistrateComplete.Code != Properties.Settings.Default.Code_100)
                    {
                        dianResponse.IsValid = false;
                        dianResponse.XmlFileName = filename;
                        dianResponse.StatusCode = processRegistrateComplete.Code;
                        dianResponse.StatusDescription = processRegistrateComplete.Message;
                    }
                    sb.AppendLine($"{Properties.Settings.Default.Param_RegistrateCompletedPayrollUrl} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)}");
                    
                }


                sb.AppendLine($"{Properties.Settings.Default.Param_LastZone} {DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture)}");

                var log = new GlobalLogger(trackId, "timers") { Message = sb.ToString() };
                TableManagerGlobalLogger.InsertOrUpdate(log);

                return dianResponse;
            }
        }

        #endregion

        #region CheckDocumentDuplicity

        private DianResponse CheckDocumentDuplicity(string senderCode, string documentType, string serieAndNumber, string trackId)
        {
            GlobalDocValidatorDocumentMeta meta = new GlobalDocValidatorDocumentMeta();
            List<string> failedList = new List<string>();
            var response = new DianResponse() { ErrorMessage = new List<string>() };
            var identifier = StringUtil.GenerateIdentifierSHA256($"{senderCode}{documentType}{serieAndNumber}");
            var document = TableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(identifier, identifier);

            //Existe documento validado por la DIAN
            if (document != null)
            {
                meta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(
                       document.DocumentKey, document.DocumentKey);
                failedList = new List<string>
                {
                    $"Regla: 90, Rechazo: Documento procesado anteriormente."
                };
                response.IsValid = false;
                response.StatusCode = "99";
                response.StatusMessage = "Documento con errores en campos mandatorios.";
                response.StatusDescription = $"Documento {serieAndNumber} procesado anteriormente. CUNE {document.DocumentKey}";
                response.ErrorMessage.AddRange(failedList);
                var xmlBytes = XmlUtil.GetApplicationResponseIfExist(meta);
                response.XmlBase64Bytes = xmlBytes;
                response.XmlDocumentKey = trackId;
                response.XmlFileName = meta.FileName;
            }
            else
                response.IsValid = true;

            return response;
        }


        #endregion

    }
}