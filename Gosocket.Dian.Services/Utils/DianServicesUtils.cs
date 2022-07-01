using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Services.Utils.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Gosocket.Dian.Services.Utils
{
    public class DianServicesUtils
    {
        private const string CategoryContainerName = "dian";
        private static readonly TableManager tableManagerDianFileMapper = new TableManager("DianFileMapper");
        private static readonly TableManager tableManagerDianProcessResult = new TableManager("DianProcessResult");
        private static readonly TableManager tableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager tableManagerGlobalDocumentType = new TableManager("GlobalDocumentType");
        private static readonly TableManager tableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");
        private static readonly TableManager tableManagerGlobalTestSet = new TableManager("GlobalTestSet");
        private static readonly TableManager tableManagerGlobalTestSetResult = new TableManager("GlobalTestSetResult");
        private static int _ublVersion = 20;
        private static readonly string[] _equivalentDocumentTypes = new string[] {"20", "25", "27", "32", "35", "40", "45", "50", "55", "60"};

        public static object ApiHelepers { get; private set; }

        //public static DianResponse GenerateApplicationResponse(long nsu, string trackId, GlobalDocValidatorDocumentMeta docMetadataEntity, List<GlobalDocValidatorTracking> validatorTrackings = null)
        //{
        //    var response = new DianResponse();
        //    var fileManager = new FileManager();

        //    var docTypeCode = docMetadataEntity.DocumentTypeId;

        //    var messageIdNode = XmlUtil.SerieNumberMessageFromDocType(docMetadataEntity);
        //    var series = messageIdNode.Item1;
        //    var number = messageIdNode.Item2;
        //    var messDocType = messageIdNode.Item3;

        //    var results = ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("GetValidationsByTrackIdUrl"), new { trackId });
        //    var rulesFound = validatorTrackings ?? results;

        //    if (rulesFound == null || (rulesFound != null && !rulesFound.Any())) return null;

        //    var listMandatoryFailedRules = new List<GlobalDocValidatorTracking>();
        //    var listNotificationRules = new List<GlobalDocValidatorTracking>();

        //    listMandatoryFailedRules = rulesFound.Where(r => r.Mandatory && !r.IsValid).ToList();
        //    listNotificationRules = rulesFound.Where(r => r.IsNotification).ToList();

        //    bool mandatoryInvalid = false;
        //    bool priorityInvalid = false;

        //    if (listMandatoryFailedRules.Count > 0) mandatoryInvalid = true;

        //    if (!mandatoryInvalid && listNotificationRules.Count > 0) priorityInvalid = true;

        //    XElement root = XmlUtil.BuildRootNode(docMetadataEntity);
        //    root.Add(XmlUtil.BuildSenderNode(docMetadataEntity));
        //    root.Add(XmlUtil.BuildReceiverNode(docMetadataEntity));

        //    XElement docResponse = new XElement("DocumentResponse");

        //    List<XElement> notesListObservations = new List<XElement>();
        //    List<XElement> notesListErrors = new List<XElement>();

        //    int lineId = 1;

        //    #region CONSTRUYENDO NODO APROBADO SIN OBSERVACIONES
        //    if (!mandatoryInvalid && !priorityInvalid)
        //    {
        //        docResponse = XmlUtil.BuildDocumentResponseNode(lineId, docMetadataEntity, false, false);
        //        var firstLineResponse = XmlUtil.BuildResponseLineResponse(lineId, nsu);
        //        docResponse.Add(firstLineResponse);

        //        lineId++;

        //        response = new DianResponse
        //        {
        //            IsValid = true,
        //            StatusCode = "0",
        //            StatusMessage = messDocType
        //        };

        //        var lineResponse = XmlUtil.BuildResponseNode(lineId, string.Empty, string.Empty, false, false, docMetadataEntity);
        //        docResponse.Add(lineResponse);
        //        root.Add(docResponse);
        //    }
        //    #endregion

        //    if (!mandatoryInvalid && priorityInvalid)
        //    {
        //        List<string> obsMessageList = new List<string>();

        //        #region CONSTRUYENDO NODO CON OBSERVACIONES
        //        docResponse = XmlUtil.BuildDocumentResponseNode(lineId, docMetadataEntity, true, false);
        //        var lineResponse = XmlUtil.BuildResponseLineResponse(lineId, nsu);
        //        docResponse.Add(lineResponse);

        //        foreach (var i in listNotificationRules)
        //        {
        //            lineId++;
        //            obsMessageList.Add($"Regla: {i.ErrorCode}, Notificación: {i.ErrorMessage}");
        //            notesListObservations.Add(XmlUtil.BuildResponseNode(lineId, i.ErrorCode, i.ErrorMessage, true, false, docMetadataEntity));
        //        }

        //        #endregion

        //        response = new DianResponse
        //        {
        //            IsValid = true,
        //            StatusCode = "0",
        //            StatusMessage = messDocType,
        //            ErrorMessage = obsMessageList
        //        };

        //        docResponse.Add(notesListObservations);
        //        root.Add(docResponse);
        //    }
        //    if (mandatoryInvalid)
        //    {
        //        docResponse = XmlUtil.BuildDocumentResponseNode(lineId, docMetadataEntity, false, true);
        //        var lineResponse = XmlUtil.BuildResponseLineResponse(lineId, nsu);
        //        docResponse.Add(lineResponse);

        //        List<string> errorsList = new List<string>();
        //        List<string> errorsMessageList = new List<string>();
        //        foreach (var ruleItem in listMandatoryFailedRules)
        //        {
        //            lineId++;

        //            errorsList.Add(ruleItem.ErrorCode);
        //            errorsMessageList.Add($"Regla: {ruleItem.ErrorCode}, Rechazo: {ruleItem.ErrorMessage}");

        //            #region CONSTRUYENDO NODO CON ERRORES

        //            notesListErrors.Add(XmlUtil.BuildResponseNode(lineId, ruleItem.ErrorCode, ruleItem.ErrorMessage, false, true, docMetadataEntity));

        //            #endregion
        //        }

        //        docResponse.Add(notesListErrors);
        //        root.Add(docResponse);

        //        response = new DianResponse
        //        {
        //            IsValid = false,
        //            StatusDescription = "Documento con errores en campos mandatorios.",
        //            ErrorMessage = errorsMessageList
        //        };
        //    }

        //    var xml = XmlUtil.FormatterXml(root);
        //    response.XmlBase64Bytes = Encoding.UTF8.GetBytes(xml);

        //    var cdrSigned = Encoding.UTF8.GetString(response.XmlBase64Bytes);

        //    response.XmlDocumentKey = trackId;
        //    response.XmlFileName = docMetadataEntity.FileName;

        //    response = SendCdrToSign(response, cdrSigned);

        //    if (response.XmlBase64Bytes == null) return response;

        //    return response;
        //}

        public static GlobalOseProcessResult InstanceGlobalOseProcessResultObject(string fileName, dynamic trackId,
            string docTypeCode, ResponseXpathDataValue responseXpathValues)
        {
            var trackIdProcessResultEntity =
                new GlobalOseProcessResult(trackId, trackId)
                {
                    DocumentType = string.IsNullOrEmpty(docTypeCode) ? 0 : int.Parse(docTypeCode),
                    EmisionDate = string.IsNullOrEmpty(docTypeCode) ? DateTime.Parse("2001-01-01") : DateTime.Parse(responseXpathValues.XpathsValues["EmissionDateXpath"].Split('|')[0]),
                    ProcessDate = DateTime.Now,
                    FileName = fileName,
                    OutputRetry = 0,
                    ReceiverCode = responseXpathValues.XpathsValues["ReceiverCodeXpath"],
                    SenderCode = responseXpathValues.XpathsValues["SenderCodeXpath"],
                    SerieNumber = responseXpathValues.XpathsValues["SeriesAndNumberXpath"],
                    TransactionId = responseXpathValues.XpathsValues["TransactionIdXpath"],
                    TrackId = trackId,
                    XmlUploaded = true
                };
            return trackIdProcessResultEntity;
        }

        //public static void GetUploadXmlRequestObject(XmlBytesArrayParams contentXmlFile, string newTrackId, string xmlBase64, out ResponseXpathDataValue responseXpathValues, out string docTypeCode, out dynamic sendRequestObj)
        //{
        //    var requestObj = new Dictionary<string, string>
        //    {
        //        { "XmlBase64", xmlBase64},
        //        { "UblVersionXpath", "//cbc:UBLVersionID" },
        //        { "EmissionDateXpath", "//cbc:IssueDate" },
        //        { "SenderCodeXpath", "//fe:AccountingSupplierParty/fe:Party/cac:PartyIdentification/cbc:ID" },
        //        { "ReceiverCodeXpath", "//fe:AccountingCustomerParty/fe:Party/cac:PartyIdentification/cbc:ID" },
        //        { "DocumentTypeXpath", "//cbc:InvoiceTypeCode" },
        //        { "NumberXpath", "//campoString[@name='FOLIO'][1]|//campoString[@name='Folio'][1]" },
        //        { "SeriesXpath", "//campoString[@name='SERIE'][1]|//campoString[@name='Serie'][1]" },
        //        { "TransactionIdXpath", "fe:Invoice/cbc:UUID" }
        //    };
        //    responseXpathValues = DianServicesUtils.GetXpathDataValues(requestObj);
        //    responseXpathValues = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestObj);
        //    _ublVersion = int.Parse(StractUblVersion(responseXpathValues.XpathsValues["UblVersionXpath"]));
        //    responseXpathValues.XpathsValues["SeriesAndNumberXpath"] = $"{responseXpathValues.XpathsValues["SeriesXpath"]}-{responseXpathValues.XpathsValues["NumberXpath"]}";
        //    docTypeCode = responseXpathValues.XpathsValues["DocumentTypeXpath"];
        //    sendRequestObj = new ExpandoObject();
        //    sendRequestObj.category = GetEnumDescription((Category)_ublVersion);
        //    sendRequestObj.xmlBase64 = xmlBase64;
        //    sendRequestObj.fileName = contentXmlFile.XmlFileName;
        //    sendRequestObj.documentTypeId = docTypeCode;
        //    sendRequestObj.trackId = newTrackId;
        //    sendRequestObj.fileMapperTable = "DianFileMapper";
        //}

        //public static DianResponse SendCdrToSign(DianResponse response, string xml)
        //{
        //    var requestObj = new { trackId = response.XmlDocumentKey };

        //    var getAppResponse = ApiHelpers.ExecuteRequest<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), requestObj);

        //    response.XmlBase64Bytes = !getAppResponse.Success ? null : getAppResponse.Content;

        //    return response;
        //}

        //[Obsolete]
        //public static ResponseXpathDataValue GetXpathDataValues<T>(T requestXpathData)
        //{
        //    var responseXpathData = new ResponseXpathDataValue();
        //    try
        //    {
        //        var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), requestXpathData);
        //        var result = response.Content.ReadAsStringAsync().Result;
        //        return JsonConvert.DeserializeObject<ResponseXpathDataValue>(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        responseXpathData.Success = false;
        //        responseXpathData.Message = $"Error al obtener xpath data values. {ex.Message}";
        //    }

        //    return responseXpathData;
        //}

        //public static List<GlobalDocValidatorTracking> ValidateDocument<T>(T requestObj)
        //{
        //    var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("ValidateDocumentUrl"), requestObj);
        //    var result = response.Content.ReadAsStringAsync().Result;
        //    var validations = JsonConvert.DeserializeObject<List<GlobalDocValidatorTracking>>(result);
        //    return validations;
        //}

        //[Obsolete]
        //public static bool CheckIfDocumentValidationsIsFinished(string trackId)
        //{
        //    try
        //    {
        //        dynamic requestObj = new ExpandoObject();
        //        requestObj.trackId = trackId;
        //        var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("CheckIfDocumentValidationsIsFinishedUrl"), requestObj);
        //        var result = response.Content.ReadAsStringAsync().Result;
        //        return bool.Parse(result);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return false;
        //    }
        //}

        //[Obsolete]
        //public static List<GlobalDocValidatorTracking> GetValidationsByTrackId(string trackId)
        //{
        //    try
        //    {
        //        dynamic requestObj = new ExpandoObject();
        //        requestObj.trackId = trackId;
        //        var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("GetValidationsByTrackIdUrl"), requestObj);
        //        var result = response.Content.ReadAsStringAsync().Result;
        //        var validations = (List<GlobalDocValidatorTracking>)JsonConvert.DeserializeObject<List<GlobalDocValidatorTracking>>(result);
        //        return validations;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        //[Obsolete]
        //public static string DownloadXmlBase64(string trackId)
        //{
        //    dynamic requestObj = new ExpandoObject();
        //    requestObj.trackId = trackId;
        //    var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("DownloadXmlUrl"), requestObj);
        //    var xmlBase64 = response.Content.ReadAsStringAsync().Result.Replace("\"", string.Empty); ;
        //    return xmlBase64;
        //}

        public static Tuple<string, string> GetCdrPath(string senderCode, int documentType, string series, long number)
        {
            string container = $"{CategoryContainerName}";

            var fileName = string.Empty;

            if (documentType == 11)
            {
                fileName = $"{senderCode}-RC-{series}-{number}";
            }
            else
            {
                fileName = $"{senderCode}-{documentType.ToString().PadLeft(2, '0')}-{series}-{number}";
            }

            var entityFileMapper = tableManagerDianFileMapper.FindByPartition<GlobalOseTrackIdMapper>(fileName);

            if (entityFileMapper.Count == 0)
            {
                return null;
            }

            var trackId = entityFileMapper.OrderByDescending(d => d.Timestamp).FirstOrDefault().RowKey;

            var entityProcessResult = tableManagerDianProcessResult.Find<GlobalOseProcessResult>(trackId, trackId);

            if (entityProcessResult == null || entityProcessResult.SuccessValidator == null)
            {
                return null;
            }

            string folder = entityProcessResult.SuccessValidator.Value ? "Success" : "Error";

            var serieFolder = string.IsNullOrEmpty(series) ? "Sin Serie" : series;

            string cdrFileName = $"responses/{entityProcessResult.Timestamp.Year}/{entityProcessResult.Timestamp.Month.ToString().PadLeft(2, '0')}/{entityProcessResult.Timestamp.Day.ToString().PadLeft(2, '0')}/{folder}/{senderCode}/{documentType.ToString()}/{serieFolder}/{number}.xml";
            if (new FileManager().Exists(container, cdrFileName))
            {
                return new Tuple<string, string>(container, cdrFileName);
            }

            return null;
        }

        //Consulta si todos los parametros para construir la ruta al CDR existe
        //public static DianResponse ParamsExistsFilenameInBlob(GlobalDocValidatorDocumentMeta documentMeta)
        //{
        //    var dianResponse = new DianResponse();
        //    var fileManager = new FileManager();

        //    byte[] xmlBytes = null;
        //    bool existsFile = false;

        //    var processDate = documentMeta.Timestamp;

        //    var serieFolder = string.IsNullOrEmpty(documentMeta.Serie) ? "NOTSERIE" : documentMeta.Serie;

        //    var isValidFolder = "Success";

        //    var container = CategoryContainerName;
        //    var fileName = $"responses/{documentMeta.Timestamp.Year}/{documentMeta.Timestamp.Month.ToString().PadLeft(2, '0')}/{documentMeta.Timestamp.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";

        //    dianResponse.XmlBase64Bytes = null;

        //    existsFile = fileManager.Exists(container, fileName);

        //    if (existsFile)
        //    {
        //        xmlBytes = fileManager.GetBytes(container, fileName);
        //    }

        //    if (!existsFile)
        //    {
        //        isValidFolder = "Error";
        //        fileName = $"responses/{documentMeta.Timestamp.Year}/{documentMeta.Timestamp.Month.ToString().PadLeft(2, '0')}/{documentMeta.Timestamp.Day.ToString().PadLeft(2, '0')}/{isValidFolder}/{documentMeta.SenderCode}/{documentMeta.DocumentTypeId}/{serieFolder}/{documentMeta.Number}/{documentMeta.PartitionKey}.xml";

        //        existsFile = fileManager.Exists(container, fileName);

        //        if (existsFile)
        //        {
        //            xmlBytes = fileManager.GetBytes(container, fileName);
        //        }
        //    }

        //    if (xmlBytes != null)
        //    {
        //        var listErrors = new List<string>();

        //        var requestObj = new { trackId = documentMeta.PartitionKey };
        //        var rulesFound = ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("GetValidationsByTrackIdUrl"), requestObj);

        //        foreach (var item in rulesFound)
        //            if (!item.IsValid)
        //                listErrors.Add($"Regla: {item.ErrorCode} {item.ErrorMessage}");

        //        var trackId = documentMeta.PartitionKey;
        //        dianResponse.IsValid = isValidFolder.ToLower().Contains("error") ? false : true;
        //        dianResponse.StatusCode = isValidFolder.ToLower().Contains("error") ? "99" : "0";
        //        dianResponse.StatusMessage = isValidFolder.ToLower().Contains("error") ? "Validación contiene errores en campos mandatorios." : "Procesado Correctamente.";
        //        dianResponse.XmlBase64Bytes = xmlBytes;
        //        dianResponse.XmlDocumentKey = documentMeta.DocumentKey;
        //        dianResponse.XmlFileName = documentMeta.FileName;
        //        dianResponse.ErrorMessage = listErrors;
        //    }

        //    return dianResponse;
        //}

        public static bool ValidateXpathValuesResponse(ResponseXpathDataValue responseXpathValues, string fileName, ref List<XmlParamsResponseTrackId> trackIdList)
        {
            bool response = true;

            if (responseXpathValues == null || !responseXpathValues.Success)
            {
                trackIdList.Add(new XmlParamsResponseTrackId
                {
                    XmlFileName = fileName,
                    ProcessedMessage = responseXpathValues.Message
                });

                response = false;
            }

            return response;
        }


        public static List<XmlParamsResponseTrackId> ValidateXpathValues(List<ResponseXpathDataValue> responses)
        {
            string[] noteCodes = { "7", "07", "8", "08", "91", "92" };
            var result = new List<XmlParamsResponseTrackId>();

            foreach (var response in responses)
            {
                bool isValid = true;
                var stringBuilder = new StringBuilder();
                var docTypeCode = response.XpathsValues["DocumentTypeXpath"];
                if (string.IsNullOrEmpty(docTypeCode))
                {
                    docTypeCode = response.XpathsValues["DocumentTypeId"];
                }

                if (string.IsNullOrEmpty(response.XpathsValues["DocumentKeyXpath"]) && !noteCodes.Contains(docTypeCode))
                {
                    stringBuilder.AppendLine("Campo cufe del documento es obligatorio");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(response.XpathsValues["EmissionDateXpath"]))
                {
                    stringBuilder.AppendLine("Campo fecha emisión es obligatoria");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(response.XpathsValues["NumberXpath"]))
                {
                    stringBuilder.AppendLine("Campo folio del documento es obligatorio");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(response.XpathsValues["SenderCodeXpath"]))
                {
                    stringBuilder.AppendLine("Campo nit del emisor es obligatorio");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(response.XpathsValues["ReceiverCodeXpath"]))
                {
                    stringBuilder.AppendLine("Campo nit del receptor es obligatorio");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(docTypeCode))
                {
                    stringBuilder.AppendLine("Campo tipo del documento es obligatorio");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(response.XpathsValues["UblVersionXpath"]))
                {
                    stringBuilder.AppendLine("Campo versión del UBL es obligatorio");
                    isValid = false;
                }
                if (!response.XpathsValues["UblVersionXpath"].Equals("UBL 2.0") && !response.XpathsValues["UblVersionXpath"].Equals("UBL 2.1"))
                {
                    stringBuilder.AppendLine("Versión del UBL debe ser 2.0 o 2.1");
                    isValid = false;
                }
                if (string.IsNullOrEmpty(response.XpathsValues["SoftwareIdXpath"]))
                {
                    stringBuilder.AppendLine("Campo SoftwareID es obligatorio");
                    isValid = false;
                }

                if (!isValid)
                {
                    if (string.IsNullOrEmpty(response.XpathsValues["DocumentKeyXpath"]) && noteCodes.Contains(docTypeCode))
                    {
                        var documentKeyData = $"{docTypeCode}{response.XpathsValues["SenderCodeXpath"]}{response.XpathsValues["ReceiverCodeXpath"]}{response.XpathsValues["NumberXpath"]}";
                        var documentKey = CreateCufeId(documentKeyData);
                        response.XpathsValues["DocumentKeyXpath"] = documentKey;
                    }
                }

                result.Add(new XmlParamsResponseTrackId { Success = isValid, XmlFileName = response.XpathsValues["FileName"], DocumentKey = response.XpathsValues["DocumentKeyXpath"], SenderCode = response.XpathsValues["SenderCodeXpath"], ProcessedMessage = stringBuilder.ToString() });
            }


            return result;
        }

        public static bool ValidateXpathValues(ResponseXpathDataValue responseXpathValues, string fileName, ref List<XmlParamsResponseTrackId> response)
        {
            bool isValid = true;
            StringBuilder stringBuilder = new StringBuilder();

            var docTypeCode = responseXpathValues.XpathsValues["DocumentTypeXpath"];
            if (string.IsNullOrEmpty(docTypeCode))
            {
                docTypeCode = responseXpathValues.XpathsValues["DocumentTypeId"];
            }

            string[] noteCodes = { "7", "8" };

            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["DocumentKeyXpath"]) && !noteCodes.Contains(docTypeCode))
            {
                stringBuilder.AppendLine("Campo cufe del documento es obligatorio");
                isValid = false;
            }
            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["EmissionDateXpath"]))
            {
                stringBuilder.AppendLine("Campo fecha emisión es obligatoria");
                isValid = false;
            }
            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["NumberXpath"]))
            {
                stringBuilder.AppendLine("Campo folio del documento es obligatorio");
                isValid = false;
            }
            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["SenderCodeXpath"]))
            {
                stringBuilder.AppendLine("Campo nit del emisor es obligatorio");
                isValid = false;
            }
            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["ReceiverCodeXpath"]))
            {
                stringBuilder.AppendLine("Campo nit del receptor es obligatorio");
                isValid = false;
            }
            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["DocumentTypeXpath"]) && string.IsNullOrEmpty(responseXpathValues.XpathsValues["DocumentTypeId"]))
            {
                stringBuilder.AppendLine("Campo tipo del documento es obligatorio");
                isValid = false;
            }
            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["UblVersionXpath"]))
            {
                stringBuilder.AppendLine("Campo versión del UBL es obligatorio");
                isValid = false;
            }
            if (!responseXpathValues.XpathsValues["UblVersionXpath"].Equals("UBL 2.0") && !responseXpathValues.XpathsValues["UblVersionXpath"].Equals("UBL 2.1"))
            {
                stringBuilder.AppendLine("Versión del UBL debe ser 2.0 o 2.1");
                isValid = false;
            }

            if (!isValid)
            {
                response.Add(new XmlParamsResponseTrackId
                {
                    XmlFileName = fileName,
                    ProcessedMessage = stringBuilder.ToString()
                });
            }
            else
            {
                if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["DocumentKeyXpath"]) && noteCodes.Contains(docTypeCode))
                {
                    var documentKeyData = $"{docTypeCode}{responseXpathValues.XpathsValues["SenderCodeXpath"]}{responseXpathValues.XpathsValues["ReceiverCodeXpath"]}{responseXpathValues.XpathsValues["NumberXpath"]}";
                    var documentKey = CreateCufeId(documentKeyData);
                    responseXpathValues.XpathsValues["DocumentKeyXpath"] = documentKey;
                }
            }

            return isValid;
        }

        public static bool ValidateXpathValuesSync(ResponseXpathDataValue responseXpathValues, ref DianResponse dianResponse)
        {
            string codeMessage = string.Empty;
            bool isValid = true;
            StringBuilder stringBuilder = new StringBuilder();

            List<string> errors = new List<string>();
            var docTypeCode = responseXpathValues.XpathsValues.ContainsKey("DocumentTypeXpath") ? responseXpathValues.XpathsValues["DocumentTypeXpath"] : null;
            if (string.IsNullOrEmpty(docTypeCode))
                docTypeCode = responseXpathValues.XpathsValues.ContainsKey("DocumentTypeId") ? responseXpathValues.XpathsValues["DocumentTypeId"] : null;

            switch (docTypeCode)
            {
                case "1":
                case "01":
                case "2":
                case "02":
                case "3":
                case "03":
                    {
                        codeMessage = "FA";
                        break;
                    }
                case "05":
                    {
                        codeMessage = "DSA";
                        break;
                    }
                case "7":
                case "91":
                    {
                        codeMessage = "CA";
                        break;
                    }
                case "8":
                case "92":
                    {
                        codeMessage = "DA";
                        break;
                    }
                case "20":
                case "25":
                case "27":
                case "32":
                case "35":
                case "40":
                case "45":
                case "50":
                case "55":
                case "60":
                    {
                        codeMessage = "DEA";
                        break;
                    }
                case "94":
                    {
                        codeMessage = "NAA";
                        break;
                    }
                case "95":
                    {
                        codeMessage = "NSA";
                        break;
                    }
                default:
                    {
                        codeMessage = "GEN";
                        break;
                    }
            }

            string[] noteCodes = { "7", "8" };

            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["DocumentKeyXpath"]) && !noteCodes.Contains(docTypeCode))
            {
                stringBuilder.AppendLine($"{codeMessage}D06: El valor UUID no está correctamente calculado.");
                errors.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                isValid = false;
            }

            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["NumberXpath"]))
            {
                string codeEnd = _equivalentDocumentTypes.Contains(docTypeCode) ? "b" : "";
                stringBuilder.AppendLine($"{codeMessage}D05{codeEnd}: El ID del Documento no puede estar vacío.");
                errors.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["SenderCodeXpath"]) && docTypeCode != "50" && codeMessage != "DEA")
            {
                stringBuilder.AppendLine($"{codeMessage}J21: El NIT del Emisor no puede estar vacío.");
                errors.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                isValid = false;
            }
        
            if (string.IsNullOrEmpty(docTypeCode))
            {
                stringBuilder.AppendLine("ZB05: XML informado no corresponde a un tipo de documento valido: Facturas (nodo padre y tipos de documemto no son validos) o Nota de Crédito (nodo padre y tipo de documemto no es valido) o Nota de Débito (nodo padre no es valido) o Application Response (nodo padre no es valido)");
                errors.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                isValid = false;
            }

            if (!isValid)
            {
                dianResponse.StatusCode = "66";
                dianResponse.ErrorMessage = errors;
            }
            else
            {
                if (string.IsNullOrEmpty(responseXpathValues.XpathsValues["DocumentKeyXpath"]) && noteCodes.Contains(docTypeCode))
                {
                    var documentKeyData = $"{docTypeCode}{responseXpathValues.XpathsValues["SenderCodeXpath"]}{responseXpathValues.XpathsValues["NumberXpath"]}";
                    var documentKey = CreateCufeId(documentKeyData);
                    responseXpathValues.XpathsValues["DocumentKeyXpath"] = documentKey;
                }
            }

            return isValid;
        }

        public static bool ValidateParserNomina(DocumentParsedNomina documentParsed, XmlParseNomina xmlParser, ref DianResponse dianResponse)
        {
            string codeMessage = string.Empty;
            string txtRegla = string.Empty;
            string txtRechazo = string.Empty;
            bool isValid = true;
            bool isValidTipoNota = true;
            StringBuilder stringBuilder = new StringBuilder();
            txtRegla = "Regla: ";
            txtRechazo = ", Rechazo: ";

            List<string> errors = new List<string>();

            var docTypeCode = documentParsed.DocumentTypeId;
            var cune = documentParsed.CUNE;           
            var tipoNota = documentParsed.TipoNota;
         
            switch (docTypeCode)
            {
                case "102":               
                    {
                        codeMessage = "NIE";
                        break;
                    }
                case "103":          
                    {
                        codeMessage = "NIAE";
                        break;
                    }                       
                default:
                    {
                        codeMessage = "GEN";
                        break;
                    }
            }

            if (string.IsNullOrEmpty(cune))
            {                
                string errorCode = tipoNota == "2" && docTypeCode == "103"
                    ? "NIAE238"
                    : docTypeCode == "102" ? "NIE024" : "NIAE024";

                stringBuilder.AppendLine($"{txtRegla + errorCode + txtRechazo} Se debe indicar el CUNE según la definición establecida.");
                errors.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                isValid = false;
            }

            if ((string.IsNullOrEmpty(tipoNota) || (tipoNota != "1" && tipoNota != "2") )  && docTypeCode == "103")
            {
                isValidTipoNota = false;
                stringBuilder.AppendLine(txtRegla + $"{codeMessage}214" + txtRechazo + "Se debe colocar el Codigo correspondiente");
                errors.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                isValid = false;
            }

            if ($"{xmlParser.globalDocPayrolls.Prefijo}{xmlParser.SequenceConsecutive}" != xmlParser.globalDocPayrolls.Numero)
            {
                string errorCode = tipoNota == "2" && docTypeCode == "103"
                    ? "NIAE220"
                    : docTypeCode == "102" ? "NIE012" : "NIAE012";

                stringBuilder.AppendLine($"{txtRegla + errorCode + txtRechazo} No se permiten caracteres adicionales como espacios o guiones. Debe corresponder a Prefijo + Número consecutivo del documento");
                errors.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                isValid = false;
            }

            if (isValidTipoNota && docTypeCode == "103")
            {
                var tipoNotaEliminar = xmlParser.xmlDocument.DocumentElement.SelectNodes("//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Eliminar']");
                var tipoNotaReemplazar = xmlParser.xmlDocument.DocumentElement.SelectNodes("//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Reemplazar']");

                if (tipoNota == "1" && tipoNotaReemplazar.Count == 0)
                {
                    stringBuilder.AppendLine(txtRegla + $"{codeMessage}214" + txtRechazo + "Se debe colocar el Codigo correspondiente");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

                if (tipoNota == "2" && tipoNotaEliminar.Count == 0)
                {                   
                    stringBuilder.AppendLine(txtRegla + $"{codeMessage}214" + txtRechazo + "Se debe colocar el Codigo correspondiente");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }              
            }

            if (!isValid)
            {
                dianResponse.StatusCode = "66";
                dianResponse.ErrorMessage = errors;
                dianResponse.StatusDescription = Properties.Settings.Default.Msg_Error_FieldMandatori;
                dianResponse.StatusMessage = "Validación contiene errores en campos mandatorios.";
            }

            

            return isValid;
        }


        public static bool ValidateParserValuesSync(DocumentParsed documentParsed, ref DianResponse dianResponse)
        {
            string codeMessage = string.Empty;
            string txtRegla = string.Empty;
            string txtRechazo = string.Empty;

            bool isValid = true;
            StringBuilder stringBuilder = new StringBuilder();

            List<string> errors = new List<string>();
            var docTypeCode = documentParsed.DocumentTypeId;
            var documentKey = documentParsed.DocumentKey;
            var senderCode = documentParsed.SenderCode;
            var eventCode = documentParsed.ResponseCode;
            var documentCude = documentParsed.Cude;
            var customizationId = documentParsed.CustomizationId; 
            var serieAndNumber = documentParsed.SerieAndNumber;
            var listID = documentParsed.listID;
            var UBLVersionID = documentParsed.UBLVersionID; 
            var providerCode = documentParsed.ProviderCode;
            txtRegla = "Regla: ";
            txtRechazo = ", Rechazo: ";

            switch (docTypeCode)
            {
                case "1":
                case "01":
                case "2":
                case "02":
                case "3":
                case "03":
                    {
                        codeMessage = "FA";
                        break;
                    }
                case "7":
                case "91":
                    {
                        codeMessage = "CA";
                        break;
                    }
                case "8":
                case "92":
                    {
                        codeMessage = "DA";
                        break;
                    }
                case "96":
                    {
                        codeMessage = "AA";                      
                        break;
                    }
                case "05":
                    {
                        codeMessage = "DSA";
                        break;
                    }
                case "101":
                    {
                        codeMessage = "DIA";
                        break;
                    }
                case "20":
                case "25":
                case "27":
                case "32":
                case "35":
                case "40":
                case "45":
                case "50":
                    codeMessage = "DEA";
                    break;
                case "55":
                case "60":
                    {
                        codeMessage = "DEA";
                        break;
                    }
                case "94":
                    {
                        codeMessage = "NAA";
                        break;
                    }
                case "95":
                    {
                        codeMessage = "NSA";
                        break;
                    }
                default:
                    {
                        codeMessage = "GEN";
                        break;
                    }
            }

            //string[] noteCodes = { "7", "8" };

            bool flagEvento = true;
            

            if (docTypeCode == "96")
            {
                if (providerCode == "800197268")
                {
                    stringBuilder.AppendLine(txtRegla + "AAB19b" + txtRechazo + ConfigurationManager.GetValue("ErrorMessage_AAB19b"));
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(UBLVersionID) || !UBLVersionID.Equals("UBL 2.1"))
                {
                    stringBuilder.AppendLine(txtRegla + "AAD01" + txtRechazo + ConfigurationManager.GetValue("ErrorMessage_AAD01"));
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }                    

                if (string.IsNullOrEmpty(eventCode))
                {
                    stringBuilder.AppendLine(txtRegla + "AAH03" + txtRechazo + ConfigurationManager.GetValue("ErrorMessage_AAH03"));
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                    flagEvento = false;
                }
                else if (!(eventCode == "030" || eventCode == "031" || eventCode == "032" || eventCode == "033" || eventCode == "034"
                   || eventCode == "035" || eventCode == "036" || eventCode == "037" || eventCode == "038" || eventCode == "039"
                   || eventCode == "040" || eventCode == "041" || eventCode == "042" || eventCode == "043" || eventCode == "044"
                   || eventCode == "045" || eventCode == "046" || eventCode == "047" || eventCode == "048" || eventCode == "049" 
                   || eventCode == "050" || eventCode == "051"))
                {
                    stringBuilder.AppendLine(txtRegla + "AAH03" + txtRechazo + ConfigurationManager.GetValue("ErrorMessage_AAH03"));
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                    flagEvento = false;
                }

                if (flagEvento)
                {

                    if (string.IsNullOrEmpty(serieAndNumber))
                    {
                        if (Convert.ToInt32(eventCode) >= 30 && Convert.ToInt32(eventCode) <= 34 && flagEvento)
                        {
                            stringBuilder.AppendLine(txtRegla + "AAD05" + txtRechazo + ConfigurationManager.GetValue("ErrorMessage_AAD05"));
                            errors.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                            isValid = false;
                        }
                        if (Convert.ToInt32(eventCode) >= 35 && Convert.ToInt32(eventCode) <= 51)
                        {
                            stringBuilder.AppendLine(txtRegla + "AAD05a" + txtRechazo + ConfigurationManager.GetValue("ErrorMessage_AAD05a"));
                            errors.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                            isValid = false;
                        }
                    }

                  
                    if (listID == "1" && (Convert.ToInt32(eventCode) >= 37 && Convert.ToInt32(eventCode) <= 40) && string.IsNullOrEmpty(senderCode) && flagEvento)
                    {
                        stringBuilder.AppendLine(txtRegla + $"{codeMessage}F04" + txtRechazo + "No fue informado el Nit.");
                        errors.Add(stringBuilder.ToString());
                        stringBuilder.Clear();
                        isValid = false;
                    }
                    else if ((Convert.ToInt32(eventCode) >= 30 && Convert.ToInt32(eventCode) <= 46) && string.IsNullOrEmpty(senderCode) && flagEvento)
                    {
                        if (Convert.ToInt32(eventCode) >= 30 && Convert.ToInt32(eventCode) <= 34)
                        {

                            stringBuilder.AppendLine(txtRegla + $"{codeMessage}F04" + txtRechazo + "El ID de emisor del evento no es Valido..");
                            errors.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                            isValid = false;
                        }
                        else if (!(listID == "2" && (eventCode == "037" || eventCode == "038" || eventCode == "039")))
                        {
                            stringBuilder.AppendLine(txtRegla + $"{codeMessage}F04" + txtRechazo + "No fue informado el Nit.");
                            errors.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                            isValid = false;
                        }
                    }
                }
               

                if (string.IsNullOrEmpty(documentCude))
                {
                    stringBuilder.AppendLine(txtRegla + "AAD06" + txtRechazo + ConfigurationManager.GetValue("ErrorMessage_AAD06"));
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

                if (string.IsNullOrEmpty(customizationId))
                {
                    stringBuilder.AppendLine(txtRegla + $"{codeMessage}D02" + txtRechazo + "No corresponde a un código valido.");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

            }
            else if ( docTypeCode == "05")
            {
                if (string.IsNullOrEmpty(documentKey))
                {
                    stringBuilder.AppendLine(txtRegla + $"{codeMessage}D06" + txtRechazo + "Valor del CUDS no está calculado correctamente.");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

                if (string.IsNullOrEmpty(documentParsed.SerieAndNumber))
                {
                    stringBuilder.AppendLine(txtRegla + $"{codeMessage}D05d" + txtRechazo + "Número de documento soporte en adquisiciones efectuadas a sujetos no obligados a expedir factura o documento equivalente no está contenido en el rango de numeración autorizado.");                                      
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

                if (string.IsNullOrEmpty(documentParsed.SerieAndNumber))
                {                   
                    stringBuilder.AppendLine(txtRegla + $"{codeMessage}D05e" + txtRechazo + "Número de documento soporte en adquisiciones efectuadas a sujetos no obligados a expedir factura o documento equivalente no existe para el número de autorización.");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

            }
            else if (docTypeCode == "95")
            {
                if (string.IsNullOrEmpty(documentKey))
                {
                    stringBuilder.AppendLine(txtRegla + $"{codeMessage}D06" + txtRechazo + "Valor del CUDS no está calculado correctamente.");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }
            }
            else if (docTypeCode == "101")
            {
                if (string.IsNullOrEmpty(documentKey))
                {
                    stringBuilder.AppendLine(txtRegla + $"{codeMessage}D06" + txtRechazo + "Valor del CUDI no está calculado correctamente.");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(documentKey))
                {
                    stringBuilder.AppendLine($"{codeMessage}D06: el valor UUID no está correctamente calculado.");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

                if (string.IsNullOrEmpty(documentParsed.SerieAndNumber))
                {
                    string codeEnd = _equivalentDocumentTypes.Contains(docTypeCode) ? "b" : "";
                    stringBuilder.AppendLine($"{codeMessage}D05{codeEnd}: El ID del Documento no puede estar vacío.");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }

                if (string.IsNullOrEmpty(senderCode) && codeMessage != "DEA")
                {
                    stringBuilder.AppendLine($"{codeMessage}J21: El NIT del Emisor no puede estar vacío.");
                    errors.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    isValid = false;
                }
            }

            if (string.IsNullOrEmpty(docTypeCode))
            {
                stringBuilder.AppendLine("ZB05: XML informado no corresponde a un tipo de documento valido: Facturas (nodo padre y tipos de documemto no son validos) o Nota de Crédito (nodo padre y tipo de documemto no es valido) o Nota de Débito (nodo padre no es valido) o Application Response (nodo padre no es valido)");
                errors.Add(stringBuilder.ToString());
                stringBuilder.Clear();
                isValid = false;
            }
          
            if (!isValid)
            {
                dianResponse.StatusCode = "66";
                dianResponse.ErrorMessage = errors;
                dianResponse.StatusDescription = Properties.Settings.Default.Msg_Error_FieldMandatori;
                dianResponse.StatusMessage = "Validación contiene errores en campos mandatorios.";
            }
            else
            {
                //if (string.IsNullOrEmpty(documentKey) && noteCodes.Contains(docTypeCode))
                //{
                //    var documentKeyData = $"{docTypeCode}{senderCode}{documentParsed.Number}";
                //    documentParsed.DocumentKey = CreateCufeId(documentKeyData);
                //}
            }

            return isValid;
        }

        public static bool ValidateIfDocumentTypeExistInCategory(string category, string documentType)
        {
            bool exist = true;

            var globalDocumentTypeEntity = tableManagerGlobalDocumentType.Find<GlobalDocumentType>(category, documentType);

            if (globalDocumentTypeEntity == null)
            {
                exist = false;
            }
            return exist;
        }

        public static bool ValidateIfDocumentTypeExistInCategory(string category, string documentType, string fileName, ref List<XmlParamsResponseTrackId> response)
        {
            bool exist = true;

            var globalDocumentTypeEntity = tableManagerGlobalDocumentType.Find<GlobalDocumentType>(category, documentType);

            if (globalDocumentTypeEntity == null)
            {
                exist = false;

                response.Add(new XmlParamsResponseTrackId
                {
                    XmlFileName = fileName,
                    ProcessedMessage = $"Tipo de documento {documentType} no implementado."
                });
            }

            return exist;
        }

        public static bool ValidateIfDocumentSentAlready(GlobalDocValidatorDocument documentEntity, string fileName, ref List<XmlParamsResponseTrackId> response)
        {
            bool found = false;

            if (documentEntity != null)
            {
                found = true;

                response.Add(new XmlParamsResponseTrackId
                {
                    XmlFileName = fileName,
                    ProcessedMessage = $"Documento procesado anteriormente con trackId: {documentEntity.DocumentKey}"
                });
            }

            return found;
        }

        public static bool ValidateIfZipHasFlagsErrors(string fileName, List<XmlBytesArrayParams> contentFileList, List<XmlParamsResponseTrackId> trackIdList, ref UploadDocumentResponse responseMessages)
        {
            XmlParamsResponseTrackId respTrackId = new XmlParamsResponseTrackId();
            bool hasError = false;

            if (contentFileList.Any(f => f.MaxQuantityAllowedFailed || f.UnzipError))
            {
                hasError = true;

                respTrackId.XmlFileName = $"{fileName}";
                respTrackId.ProcessedMessage = $"{contentFileList[0].XmlErrorMessage}.";
                trackIdList.Add(respTrackId);

                responseMessages.ZipKey = string.Empty;
                responseMessages.ErrorMessageList = trackIdList;
            }

            return hasError;
        }

        public static bool ValidateIfZipParamHasError(string fileName, XmlBytesArrayParams contentXmlFile, ref List<XmlParamsResponseTrackId> trackIdList)
        {
            XmlParamsResponseTrackId respTrackId = new XmlParamsResponseTrackId();
            bool hasError = false;

            if (contentXmlFile.HasError)
            {
                hasError = true;

                respTrackId.XmlFileName = $"{fileName}";
                respTrackId.ProcessedMessage = contentXmlFile.XmlErrorMessage;
                trackIdList.Add(respTrackId);
            }

            return hasError;
        }

        public static List<XmlParamsResponseTrackId> ValidateIfIsAllowedToSend(List<ResponseXpathDataValue> responseXpathDataValue, string authCode, string testSetId)
        {
            //authCode = "9005089089";
            var result = new List<XmlParamsResponseTrackId>();
            var codes = responseXpathDataValue.Select(x => x.XpathsValues["SenderCodeXpath"]).Distinct();
            var softwareIds = responseXpathDataValue.Select(x => x.XpathsValues["SoftwareIdXpath"]).Distinct();
            foreach (var code in codes.ToList())
            {
                var trimAuthCode = authCode?.Trim();
                var newAuthCode = trimAuthCode.Substring(0, trimAuthCode.Length - 1);
                GlobalAuthorization authEntity = null;

                if (string.IsNullOrEmpty(trimAuthCode))
                    result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"NIT de la empresa no encontrado en el certificado." });
                else
                {
                    authEntity = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(trimAuthCode, code);
                    if (authEntity == null)
                        authEntity = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(newAuthCode, code);
                    if (authEntity == null)
                        result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"NIT {trimAuthCode} no autorizado a enviar documentos para emisor con NIT {code}." });

                    if (!string.IsNullOrEmpty(testSetId))
                    {
                        var softwareId = softwareIds.Last();
                        GlobalTestSetResult testSetResultEntity = null;
                        var testSetResults = tableManagerGlobalTestSetResult.FindByPartition<GlobalTestSetResult>(code);

                        if (testSetResults.Any(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Biller}|{softwareId}" && t.Status == (int)TestSetStatus.InProcess))
                            testSetResultEntity = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Biller}|{softwareId}" && t.Status == (int)TestSetStatus.InProcess);

                        else if (testSetResults.Any(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Provider}|{softwareId}" && t.Status == (int)TestSetStatus.InProcess))
                            testSetResultEntity = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Provider}|{softwareId}" && t.Status == (int)TestSetStatus.InProcess);

                        else if (testSetResults.Any(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Biller}|{softwareId}" && t.Status == (int)TestSetStatus.Accepted))
                            testSetResultEntity = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Biller}|{softwareId}" && t.Status == (int)TestSetStatus.Accepted);

                        else if (testSetResults.Any(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Provider}|{softwareId}" && t.Status == (int)TestSetStatus.Accepted))
                            testSetResultEntity = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Provider}|{softwareId}" && t.Status == (int)TestSetStatus.Accepted);

                        else if (testSetResults.Any(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Biller}|{softwareId}" && t.Status == (int)TestSetStatus.Rejected))
                            testSetResultEntity = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Biller}|{softwareId}" && t.Status == (int)TestSetStatus.Rejected);

                        else if (testSetResults.Any(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Provider}|{softwareId}" && t.Status == (int)TestSetStatus.Rejected))
                            testSetResultEntity = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)ContributorType.Provider}|{softwareId}" && t.Status == (int)TestSetStatus.Rejected);

                        if (testSetResultEntity == null)
                            result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"NIT {code} no tiene habilitado set de prueba para software con id {softwareId}" });
                        else if (testSetResultEntity.Id != testSetId)
                            result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba con identificador {testSetId} es incorrecto." });
                        else if (testSetResultEntity.Status == (int)TestSetStatus.Accepted)
                            result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba con identificador {testSetId} se encuentra {EnumHelper.GetEnumDescription(TestSetStatus.Accepted)}." });
                        else if (testSetResultEntity.Status == (int)TestSetStatus.Rejected)
                            result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba con identificador {testSetId} se encuentra {EnumHelper.GetEnumDescription(TestSetStatus.Rejected)}." });
                    }
                }
            }

            return result;
        }

        public static bool ValidateIfIsAllowedToSend(string fileName, string senderCode, string nitPa, string email, ref List<XmlParamsResponseTrackId> trackIdList, string testSetId = null)
        {
            XmlParamsResponseTrackId respTrackId = new XmlParamsResponseTrackId();
            bool isAllowed = true;

            var authEntity = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(nitPa, senderCode);

            if (authEntity == null)
            {
                isAllowed = false;

                if (trackIdList != null)
                {
                    respTrackId.XmlFileName = $"{fileName}";
                    respTrackId.ProcessedMessage = "Empresa emisora no autorizada para emitir documentos";
                    trackIdList.Add(respTrackId);
                }
            }

            if (!string.IsNullOrEmpty(testSetId))
            {
                var testSetEntity = tableManagerGlobalAuthorization.Find<GlobalTestSet>(senderCode, testSetId);

                if (isAllowed && testSetEntity == null)
                {
                    isAllowed = false;

                    if (trackIdList != null)
                    {
                        respTrackId.XmlFileName = $"{fileName}";
                        respTrackId.ProcessedMessage = "Set de pruebas no encontrado";
                        trackIdList.Add(respTrackId);
                    }
                }

            }

            return isAllowed;
        }

        public static void ValidateResultValidatorFunction(string trackId, dynamic result, string fileName, XmlParamsResponseTrackId respTrackId, ref List<XmlParamsResponseTrackId> trackIdList)
        {
            if (string.IsNullOrEmpty(result) || result.ToLower().Contains("false"))
            {
                respTrackId.XmlFileName = fileName;
                trackIdList.Add(respTrackId);
            }
            //else
            //{
            //    respTrackId.xmlFileName = fileName;
            //    respTrackId.trackId = trackId;
            //    trackIdList.Add(respTrackId);
            //}
        }

        public static bool ValidateIfEmbeddedAttachmentHasItems(Tuple<byte[], byte[]> tupleEmbeddedElements, string fileName, ref List<XmlParamsResponseTrackId> trackIdList)
        {
            bool existItems = true;

            if (tupleEmbeddedElements == null || !tupleEmbeddedElements.Item1.Any() || !tupleEmbeddedElements.Item2.Any())
            {
                existItems = false;

                trackIdList.Add(new XmlParamsResponseTrackId
                {
                    XmlFileName = fileName,
                    ProcessedMessage = $"AttachedDocument con estructura incorrecta en los nodos DocumentoFiscalElectronico y/o Attachment"
                });
            }

            return existItems;
        }

        //public static List<ResponseUploadMultipleXml> UploadMultipleXml<T>(List<T> sendRequestObjects)
        //{
        //    var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("UploadMultipleXmlUrl"), sendRequestObjects);
        //    var result = response.Content.ReadAsStringAsync().Result;
        //    return JsonConvert.DeserializeObject<List<ResponseUploadMultipleXml>>(result);
        //}

        //public static ResponseUploadXml UploadXml<T>(T requestObj)
        //{
        //    var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("UploadXmlUrl"), requestObj);
        //    var result = response.Content.ReadAsStringAsync().Result;
        //    return JsonConvert.DeserializeObject<ResponseUploadXml>(result);
        //}

        public static void UploadXml<T>(string fileName, string trackId, T sendRequestObj)
        {
            if (fileName.Split('/').Count() > 1 && fileName.Split('/').Last() != null)
                fileName = fileName.Split('/').Last();

            var trackIdMapperEntity = new GlobalOseTrackIdMapper(fileName, trackId);
            tableManagerDianFileMapper.InsertOrUpdate(trackIdMapperEntity);
            RestUtil.ConsumeApi(ConfigurationManager.GetValue("UploadXmlUrl"), sendRequestObj);
        }

        public static Dictionary<string, string> CreateRequestObject(string xmlBase64, string fileName = null)
        {
            var requestObj = new Dictionary<string, string>
            {
                { "XmlBase64", xmlBase64},
                { "FileName", fileName},
                { "UblVersionXpath", "//*[local-name()='UBLVersionID']" },
                { "EmissionDateXpath", "//*[local-name()='IssueDate']" },
                { "SenderCodeXpath", "//*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "ReceiverCodeXpath", "//*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "DocumentTypeXpath", "//*[local-name()='InvoiceTypeCode']" },
                { "NumberXpath", "/*[local-name()='Invoice']/*[local-name()='ID']|/*[local-name()='CreditNote']/*[local-name()='ID']|/*[local-name()='DebitNote']/*[local-name()='ID']" },
                { "SeriesXpath", "//*[local-name()='InvoiceControl']/*[local-name()='AuthorizedInvoices']/*[local-name()='Prefix']"},
                { "DocumentKeyXpath","//*[local-name()='Invoice']/*[local-name()='UUID']|//*[local-name()='CreditNote']/*[local-name()='UUID']|//*[local-name()='DebitNote']/*[local-name()='UUID']"},
                { "AdditionalAccountIdXpath","//*[local-name()='AccountingCustomerParty']/*[local-name()='AdditionalAccountID']"},
                { "PartyIdentificationSchemeIdXpath","//*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']/@schemeID|/*[local-name()='Invoice']/*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyIdentification']/*[local-name()='ID']/@schemeID"},
                { "DocumentReferenceKeyXpath","//*[local-name()='BillingReference']/*[local-name()='InvoiceDocumentReference']/*[local-name()='UUID']"},
                { "DocumentTypeId", "" },
                { "SoftwareIdXpath", "//sts:SoftwareID" }
            };
            return requestObj;
        }

        public static Dictionary<string, string> CreateGetXpathRequestObject(string xmlBase64, string fileName = null)
        {
            var requestObj = new Dictionary<string, string>
            {
                { "XmlBase64", xmlBase64},
                { "FileName", fileName},
                { "SenderCodeXpath", "//*[local-name()='Invoice']/*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']|//*[local-name()='CreditNote']/*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']|//*[local-name()='DebitNote']/*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "DocumentTypeXpath", "//*[local-name()='Invoice']/*[local-name()='InvoiceTypeCode']" },
                { "NumberXpath", "/*[local-name()='Invoice']/*[local-name()='ID']|/*[local-name()='CreditNote']/*[local-name()='ID']|/*[local-name()='DebitNote']/*[local-name()='ID']" },
                { "SeriesXpath", "//*[local-name()='InvoiceControl']/*[local-name()='AuthorizedInvoices']/*[local-name()='Prefix']"},
                { "DocumentKeyXpath","//*[local-name()='Invoice']/*[local-name()='UUID']|//*[local-name()='CreditNote']/*[local-name()='UUID']|//*[local-name()='DebitNote']/*[local-name()='UUID']"},
                { "DocumentTypeId", "" },
            };

            return requestObj;
        }

        //public static EventResponse SendEventApplicationResponseProcess<T>(T requestObj)
        //{
        //    var errorResponse = new EventResponse();

        //    try
        //    {
        //        var response = RestUtil.ConsumeApi(ConfigurationManager.GetValue("ApplicationResponseProcessUrl"), requestObj);
        //        var result = response.Content.ReadAsStringAsync().Result;

        //        return JsonConvert.DeserializeObject<EventResponse>(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        errorResponse.Code = "Error";
        //        errorResponse.Message = $"{ex.Message}";
        //    }

        //    return errorResponse;
        //}

        public enum Category
        {
            [Description("dian-ubl20")]
            UBL20 = 20,
            [Description("new-dian-ubl21")]
            UBL21 = 21
        }

        public static string StractUblVersion(string input)
        {
            var output = input.Replace("UBL", string.Empty).Replace(".", string.Empty).Trim();
            if (string.IsNullOrEmpty(output))
            {
                return "20";
            }

            return output;
        }

        public static string GetEnumDescription<T>(T value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
              (DescriptionAttribute[])fi.GetCustomAttributes
              (typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static string CreateCufeId(string joinedCombination)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA384.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(joinedCombination));

                foreach (Byte b in result)
                {
                    Sb.Append(b.ToString("x2"));
                }
            }

            return Sb.ToString();
        }

        public static void EventRuleNotification(double processTime, int documentType, string ubl, int group, string accountCode, bool hasZipErrors, bool sendToValidator)
        {
            //dynamic documentEvent = new ExpandoObject();

            //documentEvent.EventTimeStamp = DateTime.Now;

            //documentEvent.ProcessTime = processTime;
            //documentEvent.Entorno = System.Configuration.ConfigurationManager.AppSettings["Environment"].ToString();
            //documentEvent.Slot = System.Configuration.ConfigurationManager.AppSettings["Slot"].ToString();
            //documentEvent.Component = "Documento";
            //documentEvent.Ubl = ubl;
            //documentEvent.DocumentType = documentType;
            //documentEvent.Group = group;
            //documentEvent.AccountCode = accountCode;
            //documentEvent.HasZipErrors = hasZipErrors;
            //documentEvent.SendToValidator = sendToValidator;

            //EventHubHandler.SendMessagesToEventHub("Colombia-EventHub-WebServicesDIAN", documentEvent);
        }
    }
}