using Gosocket.Dian.Application.FreeBillerSoftwares;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Common;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Batch
{
    public static class ProcessBatchDocumentsZipFile
    {
        private static readonly string blobContainer = "global";
        private static readonly string blobContainerFolder = "batchValidator";
        private static readonly FileManager fileManager = new FileManager();
        private static readonly TableManager tableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");       
        private static readonly TableManager tableManagerbatchFileResult = new TableManager("GlobalBatchFileResult");
        private static readonly TableManager tableManagerGlobalBatchFileStatus = new TableManager("GlobalBatchFileStatus");
        private static readonly TableManager tableManagerGlobalBatchFileRuntime = new TableManager("GlobalBatchFileRuntime");
        private static readonly TableManager tableManagerGlobalBigContributorRequestAuthorization = new TableManager("GlobalBigContributorRequestAuthorization");
        private static readonly TableManager tableManagerGlobalTestSetResult = new TableManager("GlobalTestSetResult");
        private static readonly TableManager tableManagerRadianTestSetResult = new TableManager("RadianTestSetResult");
        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly TableManager tableManagerGlobalTestSetOthersDocumentResult = new TableManager("GlobalTestSetOthersDocumentsResult");
        private static readonly TableManager tableManagerGlobalRadianOperations = new TableManager("GlobalRadianOperations");
        private static readonly TableManager tableManagerGlobalOtherDocElecOperation = new TableManager("GlobalOtherDocElecOperation");
        private static readonly TableManager tableManagerGlobalDocEvent = new TableManager("GlobalDocEvent");

        // Set queue name
        private const string queueName = "global-process-batch-zip-input%Slot%";

        [FunctionName("ProcessBatchDocumentsZipFile")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")] string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            var testSetId = string.Empty;
            var start = DateTime.UtcNow;
            var zipKey = string.Empty;
            string nitNominaProv = string.Empty;
            string nitNomina = string.Empty;
            string nitNominaEmp = string.Empty;
            string softwareIdNomina = string.Empty;
            XmlParseNomina xmlParser = null;
            GlobalBatchFileStatus batchFileStatus = null;
            var eventCodeRadian = string.Empty;
            var trackIdReferenceRadian = string.Empty;
            var trackIdCude = string.Empty;
            var listId = string.Empty;
            var payrollTypeXml = string.Empty;
            var payrollProvider = string.Empty;
            var payrollEmp = string.Empty;


            try
            {                
                var data = string.Empty;
                try
                {
                    var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                    data = eventGridEvent.Data.ToString();
                }
                catch
                {
                    data = myQueueItem;
                }

                var obj = JsonConvert.DeserializeObject<RequestObject>(data);
                testSetId = obj.TestSetId;
                zipKey = obj.ZipKey;
                log.Info($"Init batch process for zipKey {zipKey}.");
                tableManagerGlobalBatchFileRuntime.InsertOrUpdate(new GlobalBatchFileRuntime(zipKey, "START", ""));

                var startBatch = new GlobalLogger(zipKey, "1 Start ProcessBatchDocumentsZipFile")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Start ProcessBatchZip"
                };              
                await TableManagerGlobalLogger.InsertOrUpdateAsync(startBatch);

                // Get zip from storgae
                var zipBytes = await fileManager.GetBytesAsync(blobContainer, $"{blobContainerFolder}/{obj.BlobPath}/{zipKey}.zip");
                // Unzip files
                var maxBatch = string.IsNullOrEmpty(testSetId) ? 500 : 50;
                var contentFileList = zipBytes.ExtractMultipleZip(maxBatch);
                // Get batch file status object
                batchFileStatus = tableManagerGlobalBatchFileStatus.Find<GlobalBatchFileStatus>(zipKey, zipKey);
                // Check unzip has errors
                if (contentFileList.Any(f => f.MaxQuantityAllowedFailed || f.UnzipError))
                {
                    // if has errors return
                    batchFileStatus.StatusCode = "2";
                    batchFileStatus.StatusDescription = contentFileList[0].XmlErrorMessage;
                    await tableManagerGlobalBatchFileStatus.InsertOrUpdateAsync(batchFileStatus);
                    return;
                }
                // Create get xpath data values request object
                var requestObjects = contentFileList.Where(c => !c.HasError).Select(x => CreateGetXpathDataValuesRequestObject(Convert.ToBase64String(x.XmlBytes), x.XmlFileName));

                //Obtener xpath tipo documento
                var xpathRequest = requestObjects.FirstOrDefault();
                var xpathResponse = await ApiHelpers.ExecuteRequestAsync<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), xpathRequest);

                Boolean flagApplicationResponse = !string.IsNullOrWhiteSpace(xpathResponse.XpathsValues["AppResDocumentTypeXpath"]);
                Boolean flagInvoice = !string.IsNullOrWhiteSpace(xpathResponse.XpathsValues["DocumentKeyXpath"]);
                eventCodeRadian = xpathResponse.XpathsValues["AppResEventCodeXpath"];
                trackIdReferenceRadian = xpathResponse.XpathsValues["AppResDocumentReferenceKeyXpath"];
                listId = string.IsNullOrWhiteSpace(xpathResponse.XpathsValues["AppResListIDXpath"]) ? "1" : xpathResponse.XpathsValues["AppResListIDXpath"];
                trackIdCude = xpathResponse.XpathsValues["AppResDocumentKeyXpath"];

                //Informacion documento Nomina Individual / Ajuste
                if(!flagApplicationResponse && !flagInvoice)
                {
                    payrollTypeXml = !string.IsNullOrWhiteSpace(xpathResponse.XpathsValues["NominaTipoXML"]) ? xpathResponse.XpathsValues["NominaTipoXML"] : xpathResponse.XpathsValues["NominaAjusteTipoXML"];
                    payrollProvider = payrollTypeXml == "102" ? xpathResponse.XpathsValues["NominaProviderCodeXpath"] : xpathResponse.XpathsValues["NominaAjusteProviderCodeXpath"];
                    payrollEmp = payrollTypeXml == "102" ? xpathResponse.XpathsValues["NominaSenderCodeXpath"] : xpathResponse.XpathsValues["NominaAjusteSenderCodeXpath"];

                    payrollProvider = payrollProvider != payrollEmp ? payrollEmp : payrollProvider;

                }

                start = DateTime.UtcNow;
                var flagAppResponse = new GlobalLogger(zipKey, "2 flagApplicationResponse")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "validando consulta - flagInvoice " + flagInvoice + " flagApplicationResponse " + flagApplicationResponse + " eventCodeRadian " + eventCodeRadian + " trackIdReferenceRadian " + trackIdReferenceRadian 
                    + " trackIdCude " + trackIdCude + " listId " + listId + " payrollTypeXml " + payrollTypeXml + " payrollProvider " + payrollProvider + " testSetId " + testSetId
                    + " payrollEmp " + payrollEmp
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(flagAppResponse);

                var setResultOther = tableManagerGlobalTestSetOthersDocumentResult.FindGlobalTestOtherDocumentId<GlobalTestSetOthersDocumentsResult>(payrollProvider, testSetId);

                var xmlBytes = contentFileList.First().XmlBytes;               

                //Si retorna información otros documentos Nomina
                if (setResultOther != null)
                {
                    xmlParser = new XmlParseNomina();                   
                    xmlParser = new XmlParseNomina(xmlBytes);
                    nitNominaProv = Convert.ToString(xmlParser.globalDocPayrolls.NIT);
                    nitNominaEmp = Convert.ToString(xmlParser.globalDocPayrolls.Emp_NIT);
                    softwareIdNomina = xmlParser.globalDocPayrolls.SoftwareID;

                    nitNomina = nitNominaProv != nitNominaEmp ? nitNominaEmp : nitNominaProv;

                    start = DateTime.UtcNow;
                    var flagNomina = new GlobalLogger(zipKey, "3 flagNomina")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "Step prueba nomina Trajo datos testSetId " + testSetId + " nitNomina " + nitNomina + " nitNominaProv " + nitNominaProv + " nitNominaEmp " + nitNominaEmp
                    };
                    await TableManagerGlobalLogger.InsertOrUpdateAsync(flagNomina);
                }

                // Check big contributor
                if (setResultOther == null && string.IsNullOrEmpty(testSetId))
                {
                    xpathRequest = requestObjects.FirstOrDefault();
                    xpathResponse = await ApiHelpers.ExecuteRequestAsync<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), xpathRequest);
                    var nitBigContributor = xpathResponse.XpathsValues[flagApplicationResponse ? "AppResSenderCodeXpath" : "SenderCodeXpath"];                  

                    var bigContributorRequestAuthorization = tableManagerGlobalBigContributorRequestAuthorization.Find<GlobalBigContributorRequestAuthorization>(nitBigContributor, nitBigContributor);
                    if (bigContributorRequestAuthorization?.StatusCode != (int)BigContributorAuthorizationStatus.Authorized)
                    {
                        batchFileStatus.StatusCode = "2";
                        batchFileStatus.StatusDescription = $"Empresa emisora con NIT {nitBigContributor} no se encuentra autorizada para enviar documentos por los lotes.";
                        await tableManagerGlobalBatchFileStatus.InsertOrUpdateAsync(batchFileStatus);
                        return;
                    }
                }

                start = DateTime.UtcNow;
                var batchThreads = new GlobalLogger(zipKey, "4 batchThreads")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step BatchThreads " + ConfigurationManager.GetValue("BatchThreads")
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(batchThreads);

                var threads = int.Parse(ConfigurationManager.GetValue("BatchThreads"));

                BlockingCollection<ResponseXpathDataValue> xPathDataValueResponses = new BlockingCollection<ResponseXpathDataValue>();
                Parallel.ForEach(requestObjects, new ParallelOptions { MaxDegreeOfParallelism = threads }, request =>
                {
                    var xpathDataValueResponse = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), request);
                    xpathDataValueResponse.XpathsValues.Add("XmlBase64", request["XmlBase64"]);
                    xPathDataValueResponses.Add(xpathDataValueResponse);
                });

                var multipleResponsesXpathDataValue = xPathDataValueResponses.ToList();

                // filer by success
                multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.Where(c => c.Success).ToList();

                // check if unique nits
                var nits = multipleResponsesXpathDataValue.GroupBy(x => x.XpathsValues[flagApplicationResponse ? "AppResSenderCodeXpath" : "SenderCodeXpath"]).Distinct();               
                if (nits.Count() > 1)
                {
                    batchFileStatus.StatusCode = "2";
                    batchFileStatus.StatusDescription = "Lote de documentos contenidos en el archivo zip deben pertenecer todos a un mismo emisor.";
                    await tableManagerGlobalBatchFileStatus.InsertOrUpdateAsync(batchFileStatus);
                    return;
                }

                start = DateTime.UtcNow;
                var checkNits = new GlobalLogger(zipKey, "5 checkNits")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step nits.Count() " + nits.Count()
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(checkNits);

                //Informacion documento ApplicaionResponse, FE, NC y ND
                if (flagApplicationResponse || flagInvoice)
                {
                    start = DateTime.UtcNow;
                    var checksetResultOther = new GlobalLogger(zipKey, "5.1 checksetResultOther")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "Step flagApplicationResponse " + flagApplicationResponse + " multipleResponsesXpathDataValue " + multipleResponsesXpathDataValue.Count().ToString()
                    };
                    await TableManagerGlobalLogger.InsertOrUpdateAsync(checksetResultOther);

                    // Check xpaths
                    var xpathValuesValidationResult = ValidateXpathValues(multipleResponsesXpathDataValue, flagApplicationResponse);

                    start = DateTime.UtcNow;
                    var checkxpathValuesValidationResult = new GlobalLogger(zipKey, "5.2 checkxpathValuesValidationResult")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "Step xpathValuesValidationResult " + xpathValuesValidationResult.Count().ToString() 
                    };
                    await TableManagerGlobalLogger.InsertOrUpdateAsync(checkxpathValuesValidationResult);


                    multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.Where(c => xpathValuesValidationResult.Where(v => v.Success).Select(v => v.DocumentKey).Contains(c.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"])).ToList();
                    foreach (var responseXpathValues in multipleResponsesXpathDataValue)
                    {
                        if (!string.IsNullOrEmpty(responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"]) && responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"].Length > responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"].Length)
                            responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"] = responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"].Substring(responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"].Length, responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"].Length - responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"].Length);

                        responseXpathValues.XpathsValues["SeriesAndNumberXpath"] = $"{responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"]}-{responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"]}";
                    }

                    start = DateTime.UtcNow;
                    var checkXpath = new GlobalLogger(zipKey, "6 checkXpath")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "Step checkXpath "
                    };
                    await TableManagerGlobalLogger.InsertOrUpdateAsync(checkXpath);
                }

                // Check permissions
                var result = CheckPermissions(multipleResponsesXpathDataValue, obj.AuthCode, zipKey, testSetId, nitNomina, softwareIdNomina, flagApplicationResponse, flagInvoice, nitNominaProv);

                start = DateTime.UtcNow;
                var checkPermissions = new GlobalLogger(zipKey, "8 checkPermissions")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step checkPermissions Paso permisos " + result.Count.ToString()
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(checkPermissions);
               
                if (result.Count > 0)
                {
                    batchFileStatus.StatusCode = "2";
                    batchFileStatus.StatusDescription = result[0].ProcessedMessage;
                    SetLogger(null, " Reject Description", batchFileStatus.StatusDescription, "RD-Description");
                    await tableManagerGlobalBatchFileStatus.InsertOrUpdateAsync(batchFileStatus);
                    return;
                }

                // Select unique elements grouping by document key
                multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.GroupBy(x => x.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"]).Select(y => y.First()).ToList();

                //var arrayTasks = multipleResponsesXpathDataValue.Select(response => UploadXmlsAsync(testSetId, zipKey, response, uploadResponses));
                //await Task.WhenAll(arrayTasks);

                // Upload all xml's
                log.Info($"Init upload xml�s.");
                BlockingCollection<ResponseUploadXml> uploadResponses = new BlockingCollection<ResponseUploadXml>();
                SetLogger(null, "Step prueba nomina", " Paso multipleResponsesXpathDataValue " + multipleResponsesXpathDataValue.Count, "PROC-02");

                start = DateTime.UtcNow;
                var upload = new GlobalLogger(zipKey, "9 Upload")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step  multipleResponsesXpathDataValue.Count " + multipleResponsesXpathDataValue.Count
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(upload);

                bool sendTestSet = !string.IsNullOrWhiteSpace(testSetId);
                Parallel.ForEach(multipleResponsesXpathDataValue, new ParallelOptions { MaxDegreeOfParallelism = threads }, response =>
                {
                    SetLogger(null, "Step prueba nomina", " INICIO Paso upload ", "UPLOAD-01");
                    Boolean isEvent = flagApplicationResponse;
                    Boolean eventNomina = false;
                    var xmlBase64 = "";
                    var fileName = "";
                    var documentTypeId = "";
                    var trackId = "";
                    var softwareId = "";

                    if (setResultOther != null)
                    {
                        SetLogger(null, "Step prueba nomina", " Paso setResultOther nomina ", "PROC-02.1");
                        xmlBase64 = response.XpathsValues["XmlBase64"];
                        fileName = response.XpathsValues["FileName"];
                        documentTypeId = !string.IsNullOrWhiteSpace(xmlParser.globalDocPayrolls.CUNEPred)
                        ? "103" : "102";
                        trackId = xmlParser.globalDocPayrolls.CUNE;
                        eventNomina = true;
                        SetLogger(null, "Step prueba nomina", " Paso setResultOther documentTypeId " + documentTypeId, "PROC-02.1");
                    }
                    else
                    {
                        isEvent = flagApplicationResponse;
                        xmlBase64 = response.XpathsValues["XmlBase64"];
                        fileName = response.XpathsValues["FileName"];
                        documentTypeId = flagApplicationResponse ? "96" : response.XpathsValues["DocumentTypeXpath"];
                        trackId = response.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"];
                        trackId = trackId?.ToLower();
                        softwareId = response.XpathsValues["SoftwareIdXpath"];
                        eventNomina = false;
                    }

                    SetLogger(null, "Step prueba nomina", " Paso el setResult diferente null ");

                    if (isEvent)
                    {
                        var eventCode = response.XpathsValues["AppResEventCodeXpath"];
                        var customizationID = response.XpathsValues["AppResCustomizationIDXpath"];
                        var uploadXmlRequest = new { xmlBase64, fileName, documentTypeId, softwareId, trackId, zipKey, testSetId, isEvent, eventCode, customizationID, eventNomina, sendTestSet };
                        var uploadXmlResponse = ApiHelpers.ExecuteRequest<ResponseUploadXml>(ConfigurationManager.GetValue("UploadXmlUrl"), uploadXmlRequest);
                        uploadResponses.Add(uploadXmlResponse);
                    }
                    else
                    {
                        var uploadXmlRequest = new { xmlBase64, fileName, documentTypeId, softwareId, trackId, zipKey, testSetId, eventNomina, sendTestSet };
                        var uploadXmlResponse = ApiHelpers.ExecuteRequest<ResponseUploadXml>(ConfigurationManager.GetValue("UploadXmlUrl"), uploadXmlRequest);
                        uploadResponses.Add(uploadXmlResponse);
                    }
                    SetLogger(null, "Step prueba nomina", " Paso upload " +  trackId + "**" +zipKey + "**" + testSetId + "**" + eventNomina, "UPLOAD-02");

                });

                var uploadFailed = uploadResponses.Where(m => !m.Success && multipleResponsesXpathDataValue.Select(d => d.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"]).Contains(m.DocumentKey));

                var failed = uploadFailed.Count();
                await ProcessUploadFailed(zipKey, uploadFailed);

                SetLogger(null, "Step prueba nomina", " Paso cargue de documento ","PROC-03");
                // Get success upload
                multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.Where(x => !uploadFailed.Select(e => e.DocumentKey).Contains(x.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"])).ToList();

                start = DateTime.UtcNow;
                var validador = new GlobalLogger(zipKey, "10 Validations")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step  multipleResponsesXpathDataValue.Count " + multipleResponsesXpathDataValue.Count
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(validador);


                log.Info($"Init validation xml�s.");
                BlockingCollection<GlobalBatchFileResult> batchFileResults = new BlockingCollection<GlobalBatchFileResult>();
                BlockingCollection<ResponseApplicationResponse> appResponses = new BlockingCollection<ResponseApplicationResponse>();
                Parallel.ForEach(multipleResponsesXpathDataValue, new ParallelOptions { MaxDegreeOfParallelism = threads }, response =>
                {
                    var draft = false;
                    var eventNomina = false;
                    var trackId = response.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"].ToLower();
                    try
                    {
                        bool validateDocumentUrl = true;
                        if (setResultOther != null)
                        {
                            eventNomina = true;
                            trackId = xmlParser.globalDocPayrolls.CUNE;
                        }
                       
                        var request = new { trackId, draft, testSetId, eventNomina };
                        var validations = ApiHelpers.ExecuteRequest<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("ValidateDocumentUrl"), request);
                        if (validations.Count == 0)
                        {
                            appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                        }
                        else
                        {
                            //Validaciones reglas Validador Xpath
                            var errors = validations.Where(r => !r.IsValid && r.Mandatory).ToList();
                            var notifications = validations.Where(r => r.IsNotification).ToList();

                            if (!errors.Any() && !notifications.Any()) { validateDocumentUrl = true; }

                            if (errors.Any()) { validateDocumentUrl = false; }

                            if (notifications.Any()) { validateDocumentUrl = !errors.Any(); }

                            SetLogger(null, "Step prueba AR", " validateDocumentUrl " + validateDocumentUrl, "PROC-4.1");
                        }

                        //Registra tablas Nomina
                        if (setResultOther != null)
                        {
                            if (validateDocumentUrl)
                            {
                                var documentTypeId = !string.IsNullOrWhiteSpace(xmlParser.globalDocPayrolls.CUNEPred)
                                ? "103" : "102";
                                var freeBillerSoftwareId = FreeBillerSoftwareService.Get(documentTypeId);
                                if (softwareIdNomina == freeBillerSoftwareId)
                                {

                                    ApiHelpers.ExecuteRequest<EventResponse>(ConfigurationManager.GetValue("RegisterCompletedPayrollCosmosUrl"), new { TrackId = trackId });

                                }


                                SetLogger(null, "Step prueba nomina", " Ingresa cargue de documento NOMINA ", "PROC-04");
                                try
                                {
                                    byte[] xmlBytesEvent = null;
                                    var processRegistrateComplete = ApiHelpers.ExecuteRequest<EventResponse>(ConfigurationManager.GetValue("RegistrateCompletedPayrollUrl"), new { TrackId = trackId, AuthCode = obj.AuthCode });
                                    if (processRegistrateComplete.Code == "100")
                                    {
                                        xmlBytesEvent = Encoding.ASCII.GetBytes(processRegistrateComplete.XmlBytesBase64);
                                        appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = xmlBytesEvent, Success = true });
                                    }
                                    else
                                        appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                                }
                                catch (Exception ex)
                                {
                                    appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                                    log.Error($"Error al generar registro complemento de datos en NOMINA con trackId: {trackId} Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                                }

                                SetLogger(null, "Step prueba nomina", " Salida cargue de documento NOMINA ", "PROC-04.1");
                            }
                        }

                        //Registra tablas de negocio AR
                        if (flagApplicationResponse)
                        {                           
                            if (validateDocumentUrl)
                            {
                                SetLogger(null, "Step prueba AR", " Ingresa cargue de documento RADIAN ", "PROC-05");
                                try
                                {
                                    byte[] xmlBytesEvent = null;
                                    var processRegistrateComplete = ApiHelpers.ExecuteRequest<EventResponse>(ConfigurationManager.GetValue("RegistrateCompletedRadianUrl"), new { TrackId = trackId, AuthCode = obj.AuthCode });
                                    if (processRegistrateComplete.Code == "100")
                                    {
                                        xmlBytesEvent = Encoding.ASCII.GetBytes(processRegistrateComplete.XmlBytesBase64);
                                        appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = xmlBytesEvent, Success = true });
                                    }
                                    else
                                        appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                                }
                                catch (Exception ex)
                                {
                                    appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                                    log.Error($"Error al generar registro complemento de datos en RADIAN con trackId: {trackId} Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                                }
                                SetLogger(null, "Step prueba AR", " Salida cargue de documento RADIAN ", "PROC-05.1");

                                try
                                {
                                    SetLogger(null, "Step prueba AR", " inicio ApplicationResponseProcessUrl trackIdReferenceRadian " + trackIdReferenceRadian +
                                        " eventCodeRadian " + eventCodeRadian + " trackIdCude " + trackIdCude + " listId " + listId, "PROC-05.2");
                                    byte[] xmlBytesEvent = null;
                                    trackId = trackIdReferenceRadian;
                                    var responseCode = eventCodeRadian;
                                                                        
                                    var processEventResponse = ApiHelpers.ExecuteRequest<EventResponse>(ConfigurationManager.GetValue("ApplicationResponseProcessUrl"), new { trackId, responseCode, trackIdCude, listId });
                                    if (processEventResponse.Code == "100" || processEventResponse.Code == "201")
                                    {
                                        xmlBytesEvent = Encoding.ASCII.GetBytes(processEventResponse.XmlBytesBase64);
                                        appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = xmlBytesEvent, Success = true });
                                    }
                                    else
                                        appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                                }
                                catch (Exception ex)
                                {
                                    SetLogger(null, "Step prueba AR", " Exception " + ex.Message, "PROC-05.3");
                                    appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                                    log.Error($"Error al generar registro complemento de datos en Cosmos con trackId: {trackId} Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                                }
                                SetLogger(null, "Step prueba AR", " Salida cargue de documento Cosmos ", "PROC-05.4");

                            }                            
                        }                       

                        var batchFileResult = GetBatchFileResult(zipKey, trackId, validations);
                        if (batchFileResult != null)
                            batchFileResults.Add(batchFileResult);

                        try
                        {
                            var applicationResponse = ApiHelpers.ExecuteRequest<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), new { trackId });
                            if (applicationResponse.Content != null)
                                appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = applicationResponse.Content, Success = true });
                            else
                                appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                        }
                        catch (Exception ex)
                        {
                            appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
                            log.Error($"Error al generar application response del documento del batch con trackId: {trackId} Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error al validar documento del batch, trackId: {trackId} Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                    }
                });
                log.Info($"End validation xml�s.");

                // Update document status on batch
                start = DateTime.UtcNow;
                var batchUpdate = new GlobalLogger(zipKey, "11 batchUpdate")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step  ProcessBatchFileResults.Count "
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(batchUpdate);

                await ProcessBatchFileResults(batchFileResults);              

                var successAppResponses = appResponses.Where(x => x.Success && x.Content != null).ToList();
                log.Info($"{successAppResponses.Count()} application responses generated.");
                if (successAppResponses.Any())
                {                    
                    var thread = new Thread(() =>
                    {                      
                        try
                        {
                            SetLogger(null, "Step Hilo successAppResponses ", " Upload applition responses zip OK ", "PROC-04");
                            var multipleZipBytes = ZipExtensions.CreateMultipleZip(zipKey, successAppResponses);
                            var uploadResult = new FileManager().Upload(blobContainer, $"{blobContainerFolder}/applicationResponses/{zipKey}.zip", multipleZipBytes);
                            log.Info($"Upload applition responses zip OK.");
                        }
                        catch (Exception ex)
                        {
                            SetLogger(null, "Step Hilo successAppResponses ", " CreateMultipleZip " + ex.Message, "PROC-05");
                        }                       
                    });
                    // Iniciar el hilo
                    thread.Start();
                }

                tableManagerGlobalBatchFileRuntime.InsertOrUpdate(new GlobalBatchFileRuntime(zipKey, "END", xpathResponse.XpathsValues["FileName"]));               
                log.Info($"End.");

                start = DateTime.UtcNow;
                var batchFileRuntime = new GlobalLogger(zipKey, "12 GlobalBatchFileRuntime")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step proceso terminado"
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(batchFileRuntime);

            }
            catch (Exception ex)
            {
                start = DateTime.UtcNow;
                var batchException = new GlobalLogger(zipKey, "13 batchException")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step proceso Exception => " + ex.Message,
                    StackTrace = ex.StackTrace 
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(batchException);
              
                log.Error($"Error al procesar batch con trackId {zipKey}. Ex: {ex.StackTrace}");
                batchFileStatus.StatusCode = "ex";
                batchFileStatus.StatusDescription = $"Error al procesar batch. ZipKey: {zipKey}" + ex.StackTrace;
                await tableManagerGlobalBatchFileStatus.InsertOrUpdateAsync(batchFileStatus);
                throw;
            }
        }

        private static Dictionary<string, string> CreateGetXpathDataValuesRequestObject(string xmlBase64, string fileName = null)
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
                { "SoftwareIdXpath", "//sts:SoftwareID" },

                //ApplicationResponse
                { "AppResReceiverCodeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='ReceiverParty']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "AppResSenderCodeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='SenderParty']/*[local-name()='PartyTaxScheme']/*[local-name()='CompanyID']" },
                { "AppResProviderIdXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='UBLExtensions']/*[local-name()='UBLExtension']/*[local-name()='ExtensionContent']/*[local-name()='DianExtensions']/*[local-name()='SoftwareProvider']/*[local-name()='ProviderID']" },
                { "AppResEventCodeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse'][1]/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "AppResDocumentTypeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "AppResNumberXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='ID']" },
                { "AppResSeriesXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='ID']"},
                { "AppResDocumentKeyXpath","//*[local-name()='ApplicationResponse']/*[local-name()='UUID']"},
                { "AppResDocumentReferenceKeyXpath","//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='UUID']"},
                { "AppResCustomizationIDXpath","//*[local-name()='ApplicationResponse']/*[local-name()='CustomizationID']"},
                { "AppResListIDXpath","//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']/@listID"},

                //Xpath Nomina Individual
                { "NominaCUNE", "//*[local-name()='NominaIndividual']/*[local-name()='InformacionGeneral']/@CUNE"},
                { "NominaReceiverCodeXpath","//*[local-name()='NominaIndividual']/*[local-name()='Trabajador']/@NumeroDocumento" },
                { "NominaSenderCodeXpath","//*[local-name()='NominaIndividual']/*[local-name()='Empleador']/@NIT" },
                { "NominaProviderCodeXpath","//*[local-name()='NominaIndividual']/*[local-name()='ProveedorXML']/@NIT"},
                { "NominaTipoXML","//*[local-name()='NominaIndividual']/*[local-name()='InformacionGeneral']/@TipoXML"},

                //Xpath Nomina Individual de Ajustes
                { "NominaAjusteCUNE", "//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Reemplazar' or local-name()='Eliminar']/*[local-name()='InformacionGeneral']/@CUNE"},
                { "NominaAjusteCUNEPred", "//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Reemplazar' or local-name()='Eliminar']/*[local-name()='ReemplazandoPredecesor' or local-name()='EliminandoPredecesor']/@CUNEPred"},
                { "NominaAjusteReceiverCodeXpath","//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Reemplazar' or local-name()='Eliminar']/*[local-name()='Trabajador']/@NumeroDocumento" },
                { "NominaAjusteSenderCodeXpath","//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Reemplazar' or local-name()='Eliminar']/*[local-name()='Empleador']/@NIT" },
                { "NominaAjusteProviderCodeXpath","//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Reemplazar' or local-name()='Eliminar']/*[local-name()='ProveedorXML']/@NIT" },
                { "NominaAjusteTipoXML","//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Reemplazar' or local-name()='Eliminar']/*[local-name()='InformacionGeneral']/@TipoXML" },

            };

            return requestObj;
        }

        private static string validateReferenceAttorney(IEnumerable<string> codes, IEnumerable<string> codeProviders, IEnumerable<string> eventCodes, IEnumerable<string> responseListIds, string testSetId, IEnumerable<string> responseCustomizationID)
        {
            string senderCode = string.Empty;
            string issuerAttorney = string.Empty;
            string eventCode = string.Empty;
            string listId = string.Empty;
            string operationCode = string.Empty;

            foreach (var code in codes.ToList())
                senderCode = code;
            foreach (var codeProvider in codeProviders.ToList())
                issuerAttorney = codeProvider;
            foreach (var responseCode in eventCodes.ToList())
                eventCode = responseCode;
            foreach (var itemListId in responseListIds)
                listId = itemListId;
            foreach (var responseCustomization in responseCustomizationID.ToList())
                operationCode = responseCustomization;

            SetLogger(null, "Step-validateReferenceAttorney", " listId " + listId + " eventCode " + eventCode + " senderCode " +  senderCode + " IssuerAttorney " + issuerAttorney + " operationCode " + operationCode, "ATT-1");

            //Se busca el set de pruebas procesado para el testsetid en curso
            RadianTestSetResult radianTesSetResult = tableManagerRadianTestSetResult.FindByTestSetId<RadianTestSetResult>(issuerAttorney, testSetId);

            //Evento Mandato el provider es el mandatario
            if (Convert.ToInt32(eventCode) == (int)EventStatus.Mandato && listId == "3")
            {
                if (radianTesSetResult != null && radianTesSetResult.PartitionKey == issuerAttorney) 
                    return issuerAttorney;
                else
                    return senderCode;
            }

            //Evento Terminacion de Mandato X Revocatoria del Mandante (441)
            if (Convert.ToInt32(eventCode) == (int)EventStatus.TerminacionMandato 
                && (Convert.ToInt32(operationCode) == (int)SubEventStatus.TerminacionRevocatoria))
            {
                if (radianTesSetResult != null && radianTesSetResult.PartitionKey == issuerAttorney)
                    return issuerAttorney;
            }

            SetLogger(null, "Step-itemDocsReferenceAttorney", " codeMandato return null", "ATT-3");
            return null;
        }

        private static List<XmlParamsResponseTrackId> CheckPermissions(List<ResponseXpathDataValue> responseXpathDataValue, string authCode, string zipKey, string testSetId = null, string nitNomina = null, string softwareIdNomina = null, Boolean flagApplicationResponse = false, Boolean flagInvoice = false, string nitNominaProv = null)
        {
            var start = DateTime.UtcNow;
            var checkPermissions = new GlobalLogger(zipKey, "7 checkPermissions")
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "Step-responseXpathDataValue " + responseXpathDataValue.Count().ToString() +
                " Step-authCode " + authCode +
                " Step-testSetId " + testSetId +
                " Step-nitNomina " + nitNomina +
                " Step-flagApplicationResponse " + flagApplicationResponse.ToString() +
                " Step-flagInvoice " + flagInvoice.ToString()
            };
            var insertLog = TableManagerGlobalLogger.InsertOrUpdateAsync(checkPermissions);

            var result = new List<XmlParamsResponseTrackId>();
            List<RadianTestSetResult> lstResult = null;
            bool messageMandato = false;
            string blankEndorsement = string.Empty;
            string codeMandato = string.Empty;
            string eventCode = string.Empty;          

            var codes = responseXpathDataValue.Select(x => x.XpathsValues[flagApplicationResponse ? "AppResSenderCodeXpath" : "SenderCodeXpath"]).Distinct();
            var codeProviders = responseXpathDataValue.Select(x => x.XpathsValues["AppResProviderIdXpath"]).Distinct();
            var softwareIds = responseXpathDataValue.Select(x => x.XpathsValues["SoftwareIdXpath"]).Distinct();
            var eventCodes = responseXpathDataValue.Select(x => x.XpathsValues["AppResEventCodeXpath"]).Distinct();
            var responseListIds = responseXpathDataValue.Select(x => x.XpathsValues["AppResListIDXpath"]).Distinct();
            var responseCustomizationID = responseXpathDataValue.Select(x => x.XpathsValues["AppResCustomizationIDXpath"]).Distinct();
            string documentType = responseXpathDataValue.Select(t => t.XpathsValues["DocumentTypeXpath"]).FirstOrDefault();

            /*Si es un documento soporte o una una nota de ajuste del mismo, 
             * el emisor del documento está en el customerParty*/
            if (OtherDocumentsDocumentType.IsSupportDocument(documentType))
            {
                codes = responseXpathDataValue.Select(x => x.XpathsValues["ReceiverCodeXpath"]).Distinct();
            }
            
            var log = new GlobalLogger(zipKey, "7.0.1 Validate Data for update set test")
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = $@"documentType: {documentType} | senderCode: {string.Join(", ", codes)} | setTestId: {testSetId} | softwareId: {string.Join(", ",softwareIds)}"
            };
            TableManagerGlobalLogger.InsertOrUpdateAsync(log);

            //Valida si endoso es en blanco obtiene informacion NIT ProviderID - ApplicationResponse
            if (flagApplicationResponse)
            {
                foreach (var code in codes.ToList())
                    blankEndorsement = code;

                if (string.IsNullOrEmpty(blankEndorsement))
                    codes = responseXpathDataValue.Select(x => x.XpathsValues["AppResProviderIdXpath"]).Distinct();

                codeMandato = validateReferenceAttorney(codes, codeProviders, eventCodes, responseListIds, testSetId.Trim(), responseCustomizationID);

                //Valida si evento es Radian
                foreach (var responseCode in eventCodes.ToList())
                    eventCode = responseCode;
            }


            foreach (var code in codes.ToList())
            {
                var trimAuthCode = authCode.Trim();
                var newAuthCode = trimAuthCode.Substring(0, trimAuthCode.Length - 1);
                var softwareId = softwareIds.Last();

                start = DateTime.UtcNow;
                var checkVariables = new GlobalLogger(zipKey, "7.1 checkPermissions")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step-codeMandato " + codeMandato +                    
                    " Step-code " + code +
                    " Step-trimAuthCode " + trimAuthCode +
                    " Step-softwareId " + softwareId +
                    " Step-blankEndorsement " + blankEndorsement +
                    " Step-eventCode " + eventCode
                };
                var insertCheckVariables = TableManagerGlobalLogger.InsertOrUpdateAsync(checkVariables);
               
                GlobalAuthorization authEntity = null;
                RadianTestSetResult objRadianTestSetResult = null;

                if (string.IsNullOrEmpty(trimAuthCode))
                    result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"NIT de la empresa no encontrado en el certificado." });
                else
                {
                    if (!string.IsNullOrEmpty(testSetId))
                    {
                        //Consulta exista testSetID FE GlobalTestSetResult
                        List<GlobalTestSetResult> lstResulGlobalTestSetResult = tableManagerGlobalTestSetResult.FindByPartition<GlobalTestSetResult>(code);
                        GlobalTestSetResult objGlobalTestSetResult = lstResulGlobalTestSetResult.FirstOrDefault(t => t.Id.Trim().Equals(testSetId.Trim(), StringComparison.OrdinalIgnoreCase));                      

                        //Consulta exista testSetID registros RADIAN RadianTestSetResult
                        if (!string.IsNullOrWhiteSpace(codeMandato))
                        {
                            lstResult = tableManagerRadianTestSetResult.FindByPartition<RadianTestSetResult>(codeMandato);
                            objRadianTestSetResult = lstResult.FirstOrDefault(t => t.Id.Trim().Equals(testSetId.Trim(), StringComparison.OrdinalIgnoreCase));
                            messageMandato = true;
                        }
                        else
                        {
                            lstResult = tableManagerRadianTestSetResult.FindByPartition<RadianTestSetResult>(code);
                            objRadianTestSetResult = lstResult.FirstOrDefault(t => t.Id.Trim().Equals(testSetId.Trim(), StringComparison.OrdinalIgnoreCase));
                        }

                        //Consulta exista testSetID registros Otros Documentos           
                        nitNomina = !string.IsNullOrWhiteSpace(nitNomina) ? nitNomina : code;
                        nitNominaProv = !string.IsNullOrWhiteSpace(nitNominaProv) ? nitNominaProv : code;
                        softwareIdNomina = !string.IsNullOrWhiteSpace(softwareIdNomina) ? softwareIdNomina : softwareId;

                        List <GlobalTestSetOthersDocumentsResult> lstOtherDocResult = tableManagerGlobalTestSetOthersDocumentResult.FindByPartition<GlobalTestSetOthersDocumentsResult>(nitNomina);
                        GlobalTestSetOthersDocumentsResult objGlobalTestSetOthersDocumentResult = lstOtherDocResult.FirstOrDefault(t => t.Id.Trim().Equals(testSetId.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (objGlobalTestSetResult != null)
                        {
                            if (!objGlobalTestSetResult.SoftwareId.Equals(softwareIdNomina))
                            {
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = "", ProcessedMessage = String.Format("SoftwareID {0} no autorizado para enviar documentos con el set de pruebas {1}", softwareIdNomina, testSetId) });
                            }

                            //Factura Electronica
                            start = DateTime.UtcNow;
                            var checkFE = new GlobalLogger(zipKey, "7.2 checkPermissions")
                            {
                                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                                Action = "Step-checkFE Factura Electronica"
                            };
                            var insertCheckFE = TableManagerGlobalLogger.InsertOrUpdateAsync(checkFE);
                            
                            authEntity = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(trimAuthCode, code);
                            if (authEntity == null)
                                authEntity = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(newAuthCode, code);
                            if (authEntity == null)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"NIT {trimAuthCode} no autorizado a enviar documentos para emisor con NIT {code}." });

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
                        else if (objGlobalTestSetOthersDocumentResult != null)
                        {
                            if (!objGlobalTestSetOthersDocumentResult.RowKey.Split('|')[1].Equals(softwareIdNomina))
                            {
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = "", ProcessedMessage = String.Format("SoftwareID {0} no autorizado para enviar documentos con el set de pruebas {1}", softwareIdNomina, testSetId) });
                            }

                            //Otros Docuemntos Electronicos
                            start = DateTime.UtcNow;
                            var checkOtherDoc = new GlobalLogger(zipKey, "7.3 checkPermissions")
                            {
                                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                                Action = "Step-checkOtherDoc softwareIdNomina " + softwareIdNomina +
                                " Step-nitNomina " + nitNomina
                            };
                            var insertCheckOtherDoc = TableManagerGlobalLogger.InsertOrUpdateAsync(checkOtherDoc);

                            //Valida software asociado al NIT en GlobalOtherDocElecOperation
                            bool existOperationProv = tableManagerGlobalOtherDocElecOperation.Exist<GlobalOtherDocElecOperation>(nitNominaProv, softwareIdNomina);

                            bool existOperationEmp = tableManagerGlobalOtherDocElecOperation.Exist<GlobalOtherDocElecOperation>(nitNomina, softwareIdNomina);
                            if (!existOperationProv || !existOperationEmp)
                            {
                                nitNomina = !existOperationProv ? nitNominaProv : nitNomina;
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = nitNomina, ProcessedMessage = $"El NIT {nitNomina} no cuenta con el software con id {softwareIdNomina} asociado al proceso de habilitación Otros documentos" });
                            }

                            GlobalTestSetOthersDocumentsResult testSetOthersDocumentsResultEntity = null;
                            if (objGlobalTestSetOthersDocumentResult != null &&
                                (objGlobalTestSetOthersDocumentResult.Status == (int)TestSetStatus.InProcess ||
                                objGlobalTestSetOthersDocumentResult.Status == (int)TestSetStatus.Accepted ||
                                objGlobalTestSetOthersDocumentResult.Status == (int)TestSetStatus.Rejected))
                                testSetOthersDocumentsResultEntity = objGlobalTestSetOthersDocumentResult;

                            SetLogger(testSetOthersDocumentsResultEntity, "Step code", "comprueba validaciones Nomina", "CHECK-10.2.3");

                            if (testSetOthersDocumentsResultEntity == null)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = nitNomina, ProcessedMessage = $"NIT {nitNomina} no tiene habilitado set de prueba para software con id {softwareIdNomina}" });
                            else if (testSetOthersDocumentsResultEntity.Id != testSetId)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = nitNomina, ProcessedMessage = $"Set de prueba con identificador {testSetId} es incorrecto." });
                            else if (testSetOthersDocumentsResultEntity.Status == (int)TestSetStatus.Accepted)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = nitNomina, ProcessedMessage = $"Set de prueba con identificador {testSetId} se encuentra {EnumHelper.GetEnumDescription(TestSetStatus.Accepted)}." });
                            else if (testSetOthersDocumentsResultEntity.Status == (int)TestSetStatus.Rejected)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = nitNomina, ProcessedMessage = $"Set de prueba con identificador {testSetId} se encuentra {EnumHelper.GetEnumDescription(TestSetStatus.Rejected)}." });

                            SetLogger(result, "Step code", "Finaliza validaciones Nomina", "CHECK-10.2.4");

                        }
                        else if (objRadianTestSetResult != null)
                        {
                            if (!objRadianTestSetResult.SoftwareId.Equals(softwareIdNomina))
                            {
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = "", ProcessedMessage = String.Format("SoftwareID {0} no autorizado para enviar documentos con el set de pruebas {1}", softwareIdNomina, testSetId) });
                            }
                            // Is Radian
                            var isRadian = false;                          
                            var docEvent = tableManagerGlobalDocEvent.FindpartitionKey<GlobalDocEvent>(eventCode).FirstOrDefault();
                            isRadian = docEvent.IsRadian;
                            
                            // Validations to RADIAN 
                            start = DateTime.UtcNow;
                            var checkRadian = new GlobalLogger(zipKey, "7.3 checkPermissions")
                            {
                                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                                Action = "Step-checkRadian softwareIdNomina " + softwareId +
                                " Step-code " + code + " Step-isRadian " + isRadian +
                                " Step-codeMandato " + codeMandato
                            };
                            var insertRadian = TableManagerGlobalLogger.InsertOrUpdateAsync(checkRadian);

                            //Valida software asociado al NIT en GlobalRadianOperation
                            string codefinal = !string.IsNullOrWhiteSpace(codeMandato) ? codeMandato : code;
                            bool existOperation = tableManagerGlobalRadianOperations.Exist<GlobalRadianOperations>(codefinal, softwareId);
                            if (isRadian && !existOperation)
                            {
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = codefinal, ProcessedMessage = $"El NIT {codefinal} no cuenta con el software con id {softwareId} asociado al proceso de habilitación RADIAN" });
                            }

                            RadianTestSetResult radianTestSetResultEntity = null;
                            if (objRadianTestSetResult != null &&
                                (objRadianTestSetResult.Status == (int)TestSetStatus.InProcess ||
                                 objRadianTestSetResult.Status == (int)TestSetStatus.Accepted ||
                                 objRadianTestSetResult.Status == (int)TestSetStatus.Rejected))
                                radianTestSetResultEntity = objRadianTestSetResult;

                            if (radianTestSetResultEntity == null)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = codefinal, ProcessedMessage = $"NIT {codefinal} no tiene habilitado set de prueba RADIAN para software con id {softwareId}" });
                            else if (radianTestSetResultEntity.Id != testSetId)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = codefinal, ProcessedMessage = $"Set de prueba RADIAN con identificador {testSetId} es incorrecto." });
                            else if (radianTestSetResultEntity.Status == (int)TestSetStatus.Accepted)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = codefinal, ProcessedMessage = $"Set de prueba RADIAN con identificador {testSetId} se encuentra {EnumHelper.GetEnumDescription(TestSetStatus.Accepted)}." });
                            else if (radianTestSetResultEntity.Status == (int)TestSetStatus.Rejected)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = codefinal, ProcessedMessage = $"Set de prueba RADIAN con identificador {testSetId} se encuentra {EnumHelper.GetEnumDescription(TestSetStatus.Rejected)}." });
                        }
                        else
                        {
                            start = DateTime.UtcNow;
                            var checkElse = new GlobalLogger(zipKey, "7.7 checkPermissions")
                            {
                                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                                Action = "Step-messageMandato " + messageMandato.ToString()
                            };
                            var insertCheckElse = TableManagerGlobalLogger.InsertOrUpdateAsync(checkElse);

                            if (messageMandato)                                                            
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba con identificador {testSetId} no corresponde al proceso de mandato Abierto." });                            
                            else if(string.IsNullOrWhiteSpace(nitNomina) && !flagApplicationResponse && !flagInvoice)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba con identificador {testSetId} no corresponde al participante registrado en proceso de habilitación." });
                            else                                                            
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba con identificador {testSetId} no se encuentra registrado para realizar proceso de habilitación." });                                               
                        }
                    }
                }
            }

            return result;
        }       

        private static async Task ProcessBatchFileResults(IEnumerable<GlobalBatchFileResult> batchFileResults)
        {
            var table = AzureTableManager.GetTableRef("GlobalBatchFileResult");
            await AzureTableManager.InsertOrUpdateBatchAsync(batchFileResults, table);
        }

        private static async Task ProcessUploadFailed(string zipKey, IEnumerable<ResponseUploadXml> uploadFailed)
        {
            var list = uploadFailed.Select(f => new GlobalBatchFileFailed(zipKey, f.DocumentKey)
            {
                DocumentKey = f.DocumentKey,
                FileName = f.FileName,
                Message = f.Message,
                ZipKey = zipKey
            });
            var table = AzureTableManager.GetTableRef("GlobalBatchFileFailed");
            await AzureTableManager.InsertOrUpdateBatchAsync(list, table);
        }

        private static GlobalBatchFileResult GetBatchFileResult(string zipKey, string documentKey, IEnumerable<GlobalDocValidatorTracking> globalDocValidatorList)
        {
            var batchFileResult = tableManagerbatchFileResult.Find<GlobalBatchFileResult>(zipKey, documentKey);

            if (batchFileResult != null)
            {
                if (globalDocValidatorList.Count(v => !v.IsValid && v.Mandatory) == 0 && globalDocValidatorList.Count(v => v.IsNotification) == 0)
                {
                    batchFileResult.StatusCode = (int)BatchFileStatus.Accepted;
                    batchFileResult.StatusDescription = EnumHelper.GetEnumDescription(BatchFileStatus.Accepted);
                }
                if (globalDocValidatorList.Any(v => v.IsNotification))
                {
                    batchFileResult.StatusCode = (int)BatchFileStatus.Notification;
                    batchFileResult.StatusDescription = EnumHelper.GetEnumDescription(BatchFileStatus.Notification);
                }
                if (globalDocValidatorList.Count(v => !v.IsValid && v.Mandatory) > 0)
                {
                    batchFileResult.StatusCode = (int)BatchFileStatus.Rejected;
                    batchFileResult.StatusDescription = EnumHelper.GetEnumDescription(BatchFileStatus.Rejected);
                }
            }
            return batchFileResult;
        }

        private static async Task UploadXmlsAsync(string testSetId, string zipKey, ResponseXpathDataValue response, BlockingCollection<ResponseUploadXml> uploadResponses)
        {
            try
            {
                var xmlBase64 = response.XpathsValues["XmlBase64"];
                var fileName = response.XpathsValues["FileName"];
                var documentTypeId = response.XpathsValues["DocumentTypeXpath"];
                var trackId = response.XpathsValues["DocumentKeyXpath"];
                var softwareId = response.XpathsValues["SoftwareIdXpath"];
                var uploadXmlRequest = new { xmlBase64, fileName, documentTypeId, softwareId, trackId, zipKey, testSetId };
                var uploadXmlResponse = await ApiHelpers.ExecuteRequestAsync<ResponseUploadXml>(ConfigurationManager.GetValue("UploadXmlUrl"), uploadXmlRequest);
                uploadResponses.Add(uploadXmlResponse);
            }
            catch (Exception ex)
            {
                uploadResponses.Add(new ResponseUploadXml { Success = false, Message = ex.Message, DocumentKey = response.XpathsValues["DocumentTypeXpath"] });
            }
        }

        private static async Task ValidateDocumentsAsync(string zipKey, string trackId, BlockingCollection<ValidationResult> validationResults, BlockingCollection<GlobalBatchFileResult> batchFileResults)
        {
            try
            {
                var draft = false;
                var request = new { trackId, draft };
                var validations = await ApiHelpers.ExecuteRequestAsync<List<GlobalDocValidatorTracking>>(ConfigurationManager.GetValue("ValidateDocumentUrl"), request);

                var batchFileResult = GetBatchFileResult(zipKey, trackId, validations);
                if (batchFileResult != null)
                    batchFileResults.Add(batchFileResult);

                validationResults.Add(new ValidationResult { DocumentKey = trackId, Success = true, Message = "OK", Validations = validations });
            }
            catch (Exception ex)
            {
                validationResults.Add(new ValidationResult { DocumentKey = trackId, Success = false, Message = ex.Message, });
            }
        }

        private static async Task GetApplicationResponse(string trackId, BlockingCollection<ResponseApplicationResponse> appResponses)
        {
            try
            {
                var applicationResponse = await ApiHelpers.ExecuteRequestAsync<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), new { trackId });
                if (applicationResponse.Content != null)
                    appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = applicationResponse.Content, Success = true });
                else
                    appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false });
            }
            catch (Exception ex)
            {
                appResponses.Add(new ResponseApplicationResponse { DocumentKey = trackId, Content = null, Success = false, Message = ex.Message });
            }
        }

        private static List<XmlParamsResponseTrackId> ValidateXpathValues(List<ResponseXpathDataValue> responses,  Boolean flagApplicationResponse = false)
        {

            string[] noteCodes = { "7", "07", "8", "08", "91", "92", "96" };
            var result = new List<XmlParamsResponseTrackId>();
            string blankEndorsement = string.Empty;

            foreach (var response in responses)
            {
                SetLogger(null, "Step ValidateXpathValues", " Paso Ingresa a ValidateXpathValues foreach response", "PROC-5.3");

                bool isValid = true;
                var documentTypeCode = flagApplicationResponse ? "96" : response.XpathsValues["DocumentTypeXpath"];

                //Si es endoso en blanco obtiene informacion del providerID
                if (string.IsNullOrEmpty(response.XpathsValues["AppResSenderCodeXpath"]) && documentTypeCode == "96")
                    blankEndorsement = "AppResProviderIdXpath";
                else
                    blankEndorsement = "AppResSenderCodeXpath";


                SetLogger(null, "Step blankEndorsement ", blankEndorsement, "PROC-5.4");

                if (string.IsNullOrEmpty(documentTypeCode))
                    documentTypeCode = response.XpathsValues["DocumentTypeId"];

                if (string.IsNullOrEmpty(response.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"])
                    && !noteCodes.Contains(documentTypeCode))
                    isValid = false;

                if (string.IsNullOrEmpty(response.XpathsValues["EmissionDateXpath"]))
                    isValid = false;
                if (string.IsNullOrEmpty(response.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"]))
                    isValid = false;
                if (string.IsNullOrEmpty(response.XpathsValues[flagApplicationResponse ? blankEndorsement : "SenderCodeXpath"]))
                    isValid = false;
                if (string.IsNullOrEmpty(response.XpathsValues[flagApplicationResponse ? "AppResReceiverCodeXpath" : "ReceiverCodeXpath"]))
                    isValid = false;
                if (string.IsNullOrEmpty(documentTypeCode))
                    isValid = false;
                if (string.IsNullOrEmpty(response.XpathsValues["UblVersionXpath"]))
                    isValid = false;
                if (!response.XpathsValues["UblVersionXpath"].Equals("UBL 2.0") && !response.XpathsValues["UblVersionXpath"].Equals("UBL 2.1"))
                    isValid = false;
                if (string.IsNullOrEmpty(response.XpathsValues["SoftwareIdXpath"]))
                    isValid = false;

                SetLogger(null, "Step ValidateXpathValues", " Paso Ingresa a isValid " + isValid, "PROC-5.5");

                if (isValid)
                    result.Add(new XmlParamsResponseTrackId
                    {
                        Success = isValid,
                        XmlFileName = response.XpathsValues["FileName"],
                        DocumentKey = response.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"],
                        SenderCode = response.XpathsValues[flagApplicationResponse ? blankEndorsement : "SenderCodeXpath"]
                    });
            }


            return result;
        }
        private static void SetLogger(object objData, string Step, string msg, string keyUnique = "")
        {
            object resultJson;

            if (objData != null)
                resultJson = JsonConvert.SerializeObject(objData);
            else
                resultJson = String.Empty;

            GlobalLogger lastZone;
            if (string.IsNullOrEmpty(keyUnique))
                lastZone = new GlobalLogger("202015", "202015") { Message = Step + " --> " + resultJson + " -- Msg --" + msg };
            else
                lastZone = new GlobalLogger(keyUnique, keyUnique) { Message = Step + " --> " + resultJson + " -- Msg --" + msg };

            TableManagerGlobalLogger.InsertOrUpdate(lastZone);
        }

        public class RequestObject
        {
            [JsonProperty(PropertyName = "authCode")]
            public string AuthCode { get; set; }

            [JsonProperty(PropertyName = "blobPath")]
            public string BlobPath { get; set; }

            [JsonProperty(PropertyName = "testSetId")]
            public string TestSetId { get; set; }

            [JsonProperty(PropertyName = "zipKey")]
            public string ZipKey { get; set; }

        }

        public class ValidationResult
        {
            public string DocumentKey { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<GlobalDocValidatorTracking> Validations { get; set; }
        }
    }
}
