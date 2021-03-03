using Gosocket.Dian.Application;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Batch
{
    public static class ProcessBatchDocumentsZipFile
    {
        private static readonly string blobContainer = "global";
        private static readonly string blobContainerFolder = "batchValidator";
        private static readonly FileManager fileManager = new FileManager();
        private static readonly TableManager tableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");
        private static readonly TableManager tableManagerGlobalBatchFileFailed = new TableManager("GlobalBatchFileFailed");
        private static readonly TableManager tableManagerbatchFileResult = new TableManager("GlobalBatchFileResult");
        private static readonly TableManager tableManagerGlobalBatchFileStatus = new TableManager("GlobalBatchFileStatus");
        private static readonly TableManager tableManagerGlobalBatchFileRuntime = new TableManager("GlobalBatchFileRuntime");
        private static readonly TableManager tableManagerGlobalBigContributorRequestAuthorization = new TableManager("GlobalBigContributorRequestAuthorization");
        private static readonly TableManager tableManagerGlobalTestSetResult = new TableManager("GlobalTestSetResult");
        private static readonly TableManager tableManagerRadianTestSetResult = new TableManager("RadianTestSetResult");
        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly TableManager tableManagerGlobalTestSetOthersDocumentResult = new TableManager("GlobalTestSetOthersDocumentsResult");
        private static readonly TableManager tableMaganerGlobalTestSetOthersDocuments = new TableManager("GlobalTestSetOthersDocuments");
        private static readonly GlobalRadianOperationService globalRadianOperationService = new GlobalRadianOperationService();
        // Set queue name
        private const string queueName = "global-process-batch-zip-input%Slot%";

        [FunctionName("ProcessBatchDocumentsZipFile")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")] string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            var testSetId = string.Empty;            
            var zipKey = string.Empty;
            string nitNomina = string.Empty;
            string softwareIdNomina = string.Empty;
            XmlParseNomina xmlParser = null;
            GlobalBatchFileStatus batchFileStatus = null;
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
                var xpathResponse = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), xpathRequest);

                Boolean flagApplicationResponse = !string.IsNullOrWhiteSpace(xpathResponse.XpathsValues["AppResDocumentTypeXpath"]);

                var setResultOther = tableMaganerGlobalTestSetOthersDocuments.FindGlobalTestOtherDocumentId<GlobalTestSetOthersDocuments>(testSetId);

                SetLogger(null, "Step prueba nomina", " validando consulta " + flagApplicationResponse, "PROC-01");

                var xmlBytes = contentFileList.First().XmlBytes;               

                if (setResultOther != null)
                {
                    xmlParser = new XmlParseNomina();
                    SetLogger(null, "Step prueba nomina", " Trajo datos setResultOther ", "BATCH-01.0");
                    xmlParser = new XmlParseNomina(xmlBytes);
                    nitNomina = Convert.ToString(xmlParser.globalDocPayrolls.NIT);
                    softwareIdNomina = xmlParser.globalDocPayrolls.SoftwareID;
                    SetLogger(null, "Step prueba nomina", " Trajo datos testSetId" + testSetId + " nitNomina " + nitNomina, "BATCH-01");
                }

                // Check big contributor
                if (setResultOther == null && string.IsNullOrEmpty(testSetId))
                {
                    xpathRequest = requestObjects.FirstOrDefault();
                    xpathResponse = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), xpathRequest);
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

                SetLogger(null, "Step prueba nomina", " Paso el segundo If ");
                SetLogger(null, "Step" , ConfigurationManager.GetValue("BatchThreads"), "PN-02");
                var threads = int.Parse(ConfigurationManager.GetValue("BatchThreads"));

                BlockingCollection<ResponseXpathDataValue> xPathDataValueResponses = new BlockingCollection<ResponseXpathDataValue>();
                Parallel.ForEach(requestObjects, new ParallelOptions { MaxDegreeOfParallelism = threads }, request =>
                {
                    var xpathDataValueResponse = ApiHelpers.ExecuteRequest<ResponseXpathDataValue>(ConfigurationManager.GetValue("GetXpathDataValuesUrl"), request);
                    xpathDataValueResponse.XpathsValues.Add("XmlBase64", request["XmlBase64"]);
                    xPathDataValueResponses.Add(xpathDataValueResponse);
                });

                SetLogger(null, "Step prueba nomina", " Paso servicio");

                var multipleResponsesXpathDataValue = xPathDataValueResponses.ToList();

                // filer by success
                multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.Where(c => c.Success).ToList();

                // check if unique nits
                var nits = multipleResponsesXpathDataValue.GroupBy(x => x.XpathsValues[flagApplicationResponse ? "AppResSenderCodeXpath" : "SenderCodeXpath"]).Distinct();
               

                SetLogger(null, "Step prueba nomina", " Paso nitNomina " + nitNomina, "BATCH-02");
                if (nits.Count() > 1)
                {
                    batchFileStatus.StatusCode = "2";
                    batchFileStatus.StatusDescription = "Lote de documentos contenidos en el archivo zip deben pertenecer todos a un mismo emisor.";
                    await tableManagerGlobalBatchFileStatus.InsertOrUpdateAsync(batchFileStatus);
                    return;
                }

                SetLogger(null, "Step prueba nomina", " nits mayores a 1 paso ");

                if (setResultOther == null)
                {
                    // Check xpaths
                    var xpathValuesValidationResult = ValidateXpathValues(multipleResponsesXpathDataValue, flagApplicationResponse);

                    multipleResponsesXpathDataValue = multipleResponsesXpathDataValue.Where(c => xpathValuesValidationResult.Where(v => v.Success).Select(v => v.DocumentKey).Contains(c.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"])).ToList();
                    foreach (var responseXpathValues in multipleResponsesXpathDataValue)
                    {
                        if (!string.IsNullOrEmpty(responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"]) && responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"].Length > responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"].Length)
                            responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"] = responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"].Substring(responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"].Length, responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"].Length - responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"].Length);

                        responseXpathValues.XpathsValues["SeriesAndNumberXpath"] = $"{responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResSeriesXpath" : "SeriesXpath"]}-{responseXpathValues.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"]}";
                    }
                }

                // Check permissions
                var result = CheckPermissions(multipleResponsesXpathDataValue, obj.AuthCode, testSetId, nitNomina, softwareIdNomina, flagApplicationResponse);
                SetLogger(null, "Step prueba nomina", " Paso permisos " + result.Count.ToString(), "PROC-01");
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
                        }

                        //Registra tablas Nomina
                        if (setResultOther != null)
                        {
                            if (validateDocumentUrl)
                            {
                                SetLogger(null, "Step prueba nomina", " Ingresa cargue de documento NOMINA ", "PROC-04");
                                try
                                {
                                    byte[] xmlBytesEvent = null;
                                    var processRegistrateComplete = ApiHelpers.ExecuteRequest<EventResponse>(ConfigurationManager.GetValue("RegistrateCompletedPayrollUrl"), new { TrackId = trackId });
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
                                SetLogger(null, "Step prueba nomina", " Ingresa cargue de documento RADIAN ", "PROC-05");
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
                                SetLogger(null, "Step prueba nomina", " Salida cargue de documento RADIAN ", "PROC-05.1");
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
                await ProcessBatchFileResults(batchFileResults);
                SetLogger(null, "Step prueba nomina", " Paso update documento status ", "PROC-06");

                var successAppResponses = appResponses.Where(x => x.Success && x.Content != null).ToList();
                log.Info($"{successAppResponses.Count()} application responses generated.");
                if (successAppResponses.Any())
                {
                    var multipleZipBytes = ZipExtensions.CreateMultipleZip(zipKey, successAppResponses);
                    var uploadResult = new FileManager().Upload(blobContainer, $"{blobContainerFolder}/applicationResponses/{zipKey}.zip", multipleZipBytes);
                    log.Info($"Upload applition responses zip OK.");
                }
                tableManagerGlobalBatchFileRuntime.InsertOrUpdate(new GlobalBatchFileRuntime(zipKey, "END", xpathResponse.XpathsValues["FileName"]));
                SetLogger(null, "Step prueba nomina", " proceso terminado " + flagApplicationResponse, "PROC-07");
                log.Info($"End.");
            }
            catch (Exception ex)
            {
                SetLogger(null, "Step prueba nomina", " Error " + ex.StackTrace, "Err-PROCBATCH-trace");
                SetLogger(null, "Step prueba nomina", " Error " + ex.Message,"Err-PROCBATCH");
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
                { "AppResEventCodeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "AppResDocumentTypeXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='Response']/*[local-name()='ResponseCode']" },
                { "AppResNumberXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='ID']" },
                { "AppResSeriesXpath", "//*[local-name()='ApplicationResponse']/*[local-name()='ID']"},
                { "AppResDocumentKeyXpath","//*[local-name()='ApplicationResponse']/*[local-name()='UUID']"},
                { "AppResDocumentReferenceKeyXpath","//*[local-name()='ApplicationResponse']/*[local-name()='DocumentResponse']/*[local-name()='DocumentReference']/*[local-name()='UUID']"},
                { "AppResCustomizationIDXpath","//*[local-name()='ApplicationResponse']/*[local-name()='CustomizationID']"},

                //Xpath Nomina Individual
                { "NominaCUNE", "//*[local-name()='NominaIndividual']/*[local-name()='InformacionGeneral']/@CUNE"},
                { "NominaReceiverCodeXpath","//*[local-name()='NominaIndividual']/*[local-name()='Trabajador']/@NumeroDocumento" },
                { "NominaSenderCodeXpath","//*[local-name()='NominaIndividual']/*[local-name()='Empleador']/@NIT" },

                //Xpath Nomina Individual de Ajustes
                { "NominaAjusteCUNE", "//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='InformacionGeneral']/@CUNE"},
                { "NominaAjusteCUNEPred", "//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='ReemplazandoPredecesor']/@CUNEPred"},
                { "NominaAjusteReceiverCodeXpath","//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Trabajador']/@NumeroDocumento" },
                { "NominaAjusteSenderCodeXpath","//*[local-name()='NominaIndividualDeAjuste']/*[local-name()='Empleador']/@NIT" },

            };

            return requestObj;
        }

        private static List<XmlParamsResponseTrackId> CheckPermissions(List<ResponseXpathDataValue> responseXpathDataValue, string authCode, string testSetId = null, string nitNomina = null, string softwareIdNomina = null, Boolean flagApplicationResponse = false)
        {
            SetLogger(null, "Step-Checkpermission 1", responseXpathDataValue.Count().ToString(), "CHECK-01");
            SetLogger(null, "Step-Checkpermission 1", authCode, "CHECK-02");
            SetLogger(null, "Step-Checkpermission 2", testSetId, "CHECK-03");          
            SetLogger(null, "Step-Checkpermission 4", nitNomina, "CHECK-05");
            SetLogger(null, "Step-Checkpermission 5", flagApplicationResponse.ToString(), "CHECK-06");
            
            var result = new List<XmlParamsResponseTrackId>();
            var codes = responseXpathDataValue.Select(x => x.XpathsValues[flagApplicationResponse ? "AppResProviderIdXpath" : "SenderCodeXpath"]).Distinct();
            SetLogger(null, "Step-Checkpermission 5", flagApplicationResponse.ToString(), "CHECK-06.1");
            var softwareIds = responseXpathDataValue.Select(x => x.XpathsValues["SoftwareIdXpath"]).Distinct();

            SetLogger(null, "Step-Checkpermission 5", flagApplicationResponse.ToString(), "CHECK-06.2");
            foreach (var code in codes.ToList())
            {
                SetLogger(null, "Step code", "NIT RADIAN: " + code + " NIT NOMINA: " + nitNomina, "CHECK-07");
                var trimAuthCode = authCode.Trim();
                var newAuthCode = trimAuthCode.Substring(0, trimAuthCode.Length - 1);
                GlobalAuthorization authEntity = null;

                if (string.IsNullOrEmpty(trimAuthCode))
                    result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"NIT de la empresa no encontrado en el certificado." });
                else
                {
                    SetLogger(null, "Step code", "Ingrese a validar testSetId " + testSetId + " y nitNomina " + nitNomina, "CHECK-08");
                    if (!string.IsNullOrEmpty(testSetId))
                    {
                        //Consulta exista testSetID FE GlobalTestSetResult
                        List<GlobalTestSetResult> lstResulGlobalTestSetResult = tableManagerGlobalTestSetResult.FindByPartition<GlobalTestSetResult>(code);
                        GlobalTestSetResult objGlobalTestSetResult = lstResulGlobalTestSetResult.FirstOrDefault(t => t.Id.Trim().Equals(testSetId.Trim(), StringComparison.OrdinalIgnoreCase));

                        SetLogger(null, "Step code", "tengo set pruebas ni nit de nomina --- RADIAN", "CHECK-09");

                        //Consulta exista testSetID registros RADIAN RadianTestSetResult
                        List<RadianTestSetResult> lstResult = tableManagerRadianTestSetResult.FindByPartition<RadianTestSetResult>(code);                       
                        RadianTestSetResult objRadianTestSetResult = lstResult.FirstOrDefault(t => t.Id.Trim().Equals(testSetId.Trim(), StringComparison.OrdinalIgnoreCase));
                        var softwareId = softwareIds.Last();

                        //Validaciones exista testSetID GlobalTestSetOthersDocumentsResult
                        SetLogger(null, "Step code", "Estoy verificando nomina", "CHECK-10.2");
                        List<GlobalTestSetOthersDocumentsResult> lstOtherDocResult = tableManagerGlobalTestSetOthersDocumentResult.FindByPartition<GlobalTestSetOthersDocumentsResult>(nitNomina);
                        GlobalTestSetOthersDocumentsResult objGlobalTestSetOthersDocumentResult = lstOtherDocResult.FirstOrDefault(t => t.Id.Trim().Equals(testSetId.Trim(), StringComparison.OrdinalIgnoreCase));


                        if (objGlobalTestSetResult != null)
                        {
                            //Factura Electronica
                            SetLogger(null, "Step code", "Estoy verificando Factrua", "CHECK-10.1");
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
                            //Validaciones GlobalTestSetOthersDocumentsResult documento de Nomina
                            SetLogger(null, "Step code", "Estoy verificando nomina", "CHECK-10.2");
                          
                            SetLogger(null, "Step code", "Estoy verificando nomina idSoftware " + softwareIdNomina, "CHECK-10.2.1");
                            
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
                        else if(objRadianTestSetResult != null)
                        {
                            SetLogger(null, "Step code", "Estoy verificando RADIAN", "CHECK-10.4");
                            // Validations to RADIAN  
                         
                            RadianTestSetResult radianTestSetResultEntity = null;
                            if (objRadianTestSetResult != null &&
                                (objRadianTestSetResult.Status == (int)TestSetStatus.InProcess ||
                                 objRadianTestSetResult.Status == (int)TestSetStatus.Accepted ||
                                 objRadianTestSetResult.Status == (int)TestSetStatus.Rejected))
                                radianTestSetResultEntity = objRadianTestSetResult;

                            if (radianTestSetResultEntity == null)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"NIT {code} no tiene habilitado set de prueba RADIAN para software con id {softwareId}" });
                            else if (radianTestSetResultEntity.Id != testSetId)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba RADIAN con identificador {testSetId} es incorrecto." });
                            else if (radianTestSetResultEntity.Status == (int)TestSetStatus.Accepted)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba RADIAN con identificador {testSetId} se encuentra {EnumHelper.GetEnumDescription(TestSetStatus.Accepted)}." });
                            else if (radianTestSetResultEntity.Status == (int)TestSetStatus.Rejected)
                                result.Add(new XmlParamsResponseTrackId { Success = false, SenderCode = code, ProcessedMessage = $"Set de prueba RADIAN con identificador {testSetId} se encuentra {EnumHelper.GetEnumDescription(TestSetStatus.Rejected)}." });
                        }
                        else
                        {
                            SetLogger(result, "Step code", "No existe TestSetID registrado", "CHECK-10.5");
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

        private static List<XmlParamsResponseTrackId> ValidateXpathValues(List<ResponseXpathDataValue> responses, Boolean flagApplicationResponse = false)
        {

            string[] noteCodes = { "7", "07", "8", "08", "91", "92", "96" };
            var result = new List<XmlParamsResponseTrackId>();

            foreach (var response in responses)
            {
                bool isValid = true;
                var documentTypeCode = flagApplicationResponse ? "96" : response.XpathsValues["DocumentTypeXpath"];

                if (string.IsNullOrEmpty(documentTypeCode))
                    documentTypeCode = response.XpathsValues["DocumentTypeId"];

                if (string.IsNullOrEmpty(response.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"])
                    && !noteCodes.Contains(documentTypeCode))
                    isValid = false;

                if (string.IsNullOrEmpty(response.XpathsValues["EmissionDateXpath"]))
                    isValid = false;
                if (string.IsNullOrEmpty(response.XpathsValues[flagApplicationResponse ? "AppResNumberXpath" : "NumberXpath"]))
                    isValid = false;
                if (string.IsNullOrEmpty(response.XpathsValues[flagApplicationResponse ? "AppResSenderCodeXpath" : "SenderCodeXpath"]))
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

                if (isValid)
                    result.Add(new XmlParamsResponseTrackId
                    {
                        Success = isValid,
                        XmlFileName = response.XpathsValues["FileName"],
                        DocumentKey = response.XpathsValues[flagApplicationResponse ? "AppResDocumentKeyXpath" : "DocumentKeyXpath"],
                        SenderCode = response.XpathsValues[flagApplicationResponse ? "AppResSenderCodeXpath" : "SenderCodeXpath"]
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
