using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Data.Entity.Core;
using System.Globalization;

namespace Gosocket.Dian.Functions.Others
{
    public static class ActivateOtherDocumentContributor
    {
        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly SoftwareService softwareService = new SoftwareService();
        private static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager TableManagerOtherDocElecOperation = new TableManager("GlobalOtherDocElecOperation");
        private static readonly TableManager TableManagerTestSetOthersDocumentsResult = new TableManager("GlobalTestSetOthersDocumentsResult");


        // Set queue name
        private const string queueName = "activate-otherdocument-operation-input%Slot%";

        [FunctionName("ActivateOtherDocumentContributor")]

        public static void Run([QueueTrigger(queueName, Connection = "GlobalStorage")] string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            var start = DateTime.UtcNow;

            SetLogger(null, "Step OtherDocument-00", " Ingresamos a ActivateOtherDocumentContributor ", "ACTSEND-01");

            //Variable de validacion para registro informacion del piloto
            bool.TryParse(Environment.GetEnvironmentVariable("IsProduction"), out bool IsProduction);

            SetLogger(null, "Step OtherDoc-00", " Ingresamos a OtherDocumentContributor " + IsProduction, "Step Other-100");
            SetLogger(null, "Step OtherDoc-00", " Ingresamos a OtherDocumentContributor Entorno" + ConfigurationManager.GetValue("Environment"), "Step Other-101");

            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                SetLogger(null, "Step OtherDocument-01", " Ingresamos a ActivateOtherDocumentContributor Ambiente Prod " + IsProduction, "ACTSEND-02");

                OtherDocElecContributor otherDocElecContributor = null;
                GlobalContributorActivation contributorActivation = null;
                OtherDocumentActivateContributorRequestObject requestObject = null;

                try
                {
                    var contributor = contributorService.Get(requestObject.ContributorId);

                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found contributor with given id {requestObject.ContributorId}");

                    // Step 1  Validate OtherDocumentActivateContributor
                    EventGridEvent eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                    requestObject = JsonConvert.DeserializeObject<OtherDocumentActivateContributorRequestObject>(eventGridEvent.Data.ToString());

                    SetLogger(requestObject, "Step OtherDocument-1", "ActivateOtherContributor SoftwareId " + requestObject.SoftwareId, "ACT-01");
                    start = DateTime.UtcNow;
                    var requestObjectACT = new GlobalLogger(requestObject.SoftwareId, "ACTSEND-02.1")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = " SoftwareID " + requestObject.SoftwareId + " SoftwareType " + requestObject.SoftwareType 
                    };
                    var resultRequestObjectACT = TableManagerGlobalLogger.InsertOrUpdateAsync(requestObjectACT);

                    //Contributorid = OtherDocumentActivateContributorID
                    int otherDocContributorId = 0;
                    otherDocElecContributor = contributorService.GetOtherDocElecContributorID(requestObject.ContributorId, requestObject.OtherDocContributorTypeId, requestObject.OtherDocOperationModeId);
                    SetLogger(null, "Step ActOther-1", otherDocElecContributor == null ? "vacio" : "OtherDocContributor no es null", "ACT-02");                    

                    if (otherDocElecContributor != null)
                    {
                        // Step 3 Activo Otros Documentos
                        otherDocElecContributor.Step = 3;
                        otherDocElecContributor.ElectronicDocumentId = requestObject.ElectronicDocumentId;
                        otherDocElecContributor.OtherDocElecContributorTypeId = requestObject.OtherDocContributorTypeId;
                        if(IsProduction)
                            contributorService.ActivateOtherDocument(otherDocElecContributor);
                        otherDocContributorId = otherDocElecContributor.Id;
                        SetLogger(null, "Step ActOther-3", "ActivateOtherDocument -->  ", "ACT-03");
                    }
                    else
                    {
                        // Step 4 Actualizo otherDocElecContributor en SQL 
                        otherDocElecContributor = new OtherDocElecContributor()
                        {
                            CreatedBy = requestObject.CreatedBy,
                            ContributorId = requestObject.ContributorId,
                            OtherDocElecContributorTypeId = requestObject.OtherDocContributorTypeId,
                            OtherDocElecOperationModeId = requestObject.OtherDocOperationModeId,
                            ElectronicDocumentId = requestObject.ElectronicDocumentId,
                            State = Domain.Common.OtherDocElecState.Habilitado.GetDescription(),
                            CreatedDate = System.DateTime.Now,
                            Update = System.DateTime.Now,
                            Step = 3
                        };
                        SetLogger(otherDocElecContributor, "Step OtherDoc-4", " -- contributorService.AddOrUpdateOtherDocContributor -- ", "ACT-04");
                        
                        if (IsProduction)
                            otherDocContributorId = contributorService.AddOrUpdateOtherDocContributor(otherDocElecContributor);

                        otherDocContributorId = otherDocElecContributor.Id;
                        SetLogger(null, "Step OtherDoc-5", " -- contributorService.AddOrUpdateOtherDocContributor -- ", "ACT-05");
                    }

                    try
                    {
                        contributorService.Activate(contributor);

                        var guid = Guid.NewGuid().ToString();
                        contributorActivation = new GlobalContributorActivation(requestObject.Code.ToString(), guid)
                        {
                            Success = true,
                            Message = "Contribuyente se activó en producción con éxito.",
                            TestSetId = requestObject.TestSetOthersDocumentsResultObj.Id,
                            ContributorCode = requestObject.Code,
                            ContributorTypeId = requestObject.OtherDocContributorTypeId,
                            OperationModeId = requestObject.OtherDocOperationModeId,
                            SentToActivateBy = "SendToActivateOtherDocumentContributor",
                            Request = myQueueItem,
                            SendDate = DateTime.UtcNow,
                            SoftwareId = requestObject.SoftwareProvider,
                            OperationModeName = requestObject.TestSetOthersDocumentsResultObj.OperationModeName,
                            OtherDocContributorId = otherDocContributorId
                        };
                    }
                    catch { }

                    start = DateTime.UtcNow;
                    var OtherDocSW = new GlobalLogger(requestObject.SoftwareId, "ACTSEND-05.1")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = " otherDocContributorId " + otherDocContributorId
                    };
                    var resultOtherDocSW = TableManagerGlobalLogger.InsertOrUpdateAsync(OtherDocSW);

                    OtherDocElecSoftware otherDocElecSoftware = softwareService.GetOtherDocSoftware(new Guid(requestObject.SoftwareId));

                    if (otherDocElecSoftware == null)
                    {
                        OtherDocElecSoftware newSoftware = new OtherDocElecSoftware()
                        {
                            Id = new Guid(requestObject.SoftwareId),
                            Deleted = false,
                            Name = requestObject.SoftwareName,
                            Pin = requestObject.Pin,
                            SoftwareDate = DateTime.Now,
                            SoftwareUser = requestObject.SoftwareUser,
                            SoftwarePassword = requestObject.SoftwarePassword,
                            Status = true,
                            OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted,
                            Url = ConfigurationManager.GetValue("URLServiceProd"),
                            CreatedBy = requestObject.CreatedBy,
                            OtherDocElecContributorId = otherDocContributorId,
                            SoftwareId = new Guid(requestObject.SoftwareProvider),
                            ProviderId = requestObject.ProviderId
                        };

                        if (IsProduction)
                            softwareService.AddOrUpdateOtherDocSoftware(newSoftware);

                        SetLogger(newSoftware, "Step OtherDoc-5", " -- softwareService.AddOrUpdateOtherDocSoftware -- ", "ACT-06");
                    }
                    else
                    {
                        otherDocElecSoftware.Deleted = false;
                        otherDocElecSoftware.Name = requestObject.SoftwareName;
                        otherDocElecSoftware.Pin = requestObject.Pin;
                        otherDocElecSoftware.SoftwareDate = DateTime.Now;
                        otherDocElecSoftware.SoftwareUser = requestObject.SoftwareUser;
                        otherDocElecSoftware.SoftwarePassword = requestObject.SoftwarePassword;
                        otherDocElecSoftware.Status = true;
                        otherDocElecSoftware.OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted;
                        otherDocElecSoftware.Url = ConfigurationManager.GetValue("URLServiceProd");
                        otherDocElecSoftware.OtherDocElecContributorId = otherDocContributorId;
                        otherDocElecSoftware.SoftwareId = new Guid(requestObject.SoftwareProvider);
                        otherDocElecSoftware.ProviderId = requestObject.ProviderId;                        

                        if (IsProduction)
                            softwareService.AddOrUpdateOtherDocSoftware(otherDocElecSoftware);

                        SetLogger(null, "Step OtherDoc-5", " -- softwareService.AddOrUpdateOtherDocSoftware -- ", "ACT-06.1");
                    }

                    // Crear Software en TableSTorage
                    var software = softwareService.GetOtherDocSoftware(Guid.Parse(requestObject.SoftwareId));
                    GlobalSoftware globalSoftware = new GlobalSoftware(software.SoftwareId.ToString(), software.SoftwareId.ToString())
                    {
                        Id = new Guid(requestObject.SoftwareId),
                        Deleted = false,
                        Pin = requestObject.Pin,
                        StatusId = (int)Domain.Common.SoftwareStatus.Production
                    };

                    if (IsProduction)
                        softwareTableManager.InsertOrUpdateAsync(globalSoftware).Wait();

                    SetLogger(globalSoftware, "Step OtherDoc-6", " -- softwareTableManager.InsertOrUpdateAsync -- ", "ACT-07");
                    
                    //--1. se busac operation por radiancontributorid y software 
                    OtherDocElecContributorOperations OtherDocOperation = contributorService.GetOtherDocOperations(otherDocContributorId, requestObject.SoftwareId);

                    //---2.. si no existe se crea  
                    if (OtherDocOperation == null)
                    {
                        OtherDocOperation = new OtherDocElecContributorOperations()
                        {
                            Deleted = false,
                            OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                            SoftwareId = new Guid(requestObject.SoftwareId),
                            OtherDocElecContributorId = otherDocContributorId,
                            Timestamp = System.DateTime.Now,
                            SoftwareType = Convert.ToInt32(requestObject.SoftwareType)
                        };
                        SetLogger(OtherDocOperation, "OtherDocOperation", string.Empty, "ACT-08");

                        if (IsProduction)
                            _ = contributorService.AddOtherDocOperation(OtherDocOperation);

                        SetLogger(null, "Step OtherDoc-7", "Cree la operacion", "ACT-09");
                    }
                    else //---3. si existe se actualiza.
                    {
                        OtherDocOperation.OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado;
                        OtherDocOperation.SoftwareId = new Guid(requestObject.SoftwareId);
                        OtherDocOperation.OtherDocElecContributorId = otherDocContributorId;
                        OtherDocOperation.SoftwareType = Convert.ToInt32(requestObject.SoftwareType);
                        OtherDocOperation.Timestamp = System.DateTime.Now;
                        if (IsProduction)
                            contributorService.UpdateOtherDocOperation(OtherDocOperation);
                        SetLogger(null, "Step OtherDoc-8", " -- contributorService.UpdateOtherDocOperation -- ", "ACT-10");
                    }

                    //--1. COnsultar previo por si ya existe
                    GlobalOtherDocElecOperation globalOtherDocElecOperation = TableManagerOtherDocElecOperation.FindSoftwareId<GlobalOtherDocElecOperation>(requestObject.Code, requestObject.SoftwareProvider);
                    if (globalOtherDocElecOperation == null)
                        globalOtherDocElecOperation = new GlobalOtherDocElecOperation(requestObject.Code, requestObject.SoftwareProvider);

                    //--2. si existe solo actualizar lo que llega
                    if (otherDocElecContributor.OtherDocElecContributorTypeId == (int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider)
                        globalOtherDocElecOperation.TecnologicalSupplier = globalOtherDocElecOperation.ContributorTypeId == (int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider;

                    globalOtherDocElecOperation.Deleted = false;
                    globalOtherDocElecOperation.ContributorTypeId = otherDocElecContributor.OtherDocElecContributorTypeId;
                    globalOtherDocElecOperation.State = Domain.Common.OtherDocElecState.Habilitado.GetDescription();
                    globalOtherDocElecOperation.OperationModeId = Convert.ToInt32(requestObject.SoftwareType);
                    globalOtherDocElecOperation.SoftwareId = requestObject.SoftwareId;
                    globalOtherDocElecOperation.OtherDocElecContributorId = otherDocContributorId;
                    globalOtherDocElecOperation.ElectronicDocumentId = requestObject.ElectronicDocumentId;

                    if (otherDocElecContributor.OtherDocElecContributorTypeId == (int)Domain.Common.OtherDocElecContributorType.Transmitter)
                        globalOtherDocElecOperation.Transmitter = true;
                    if (otherDocElecContributor.OtherDocElecContributorTypeId == (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                        globalOtherDocElecOperation.TecnologicalSupplier = true;

                    //--3. Si no existe si se crea.    //-----Razon los estados deben mantenerse en la actualizacion. mismo nit y software pueden usar diferentes modos.
                    if (IsProduction)
                        TableManagerOtherDocElecOperation.InsertOrUpdateAsync(globalOtherDocElecOperation).Wait();

                    //Se inserta en GlobalTestSetOthersDocumentsResult
                    if (IsProduction)
                    {
                        requestObject.TestSetOthersDocumentsResultObj.OtherDocElecContributorId = otherDocContributorId;
                        TableManagerTestSetOthersDocumentsResult.InsertOrUpdateAsync(requestObject.TestSetOthersDocumentsResultObj).Wait();
                    }
                        
                    SetLogger(null, "Step RA-9", " -- globalRadianOperations -- ", "ACT-11");

                    start = DateTime.UtcNow;
                    var endACT = new GlobalLogger(requestObject.SoftwareId, "ACT-12")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = " otherDocContributorId " + otherDocElecContributor.Id
                    };
                    var resultEndACT = TableManagerGlobalLogger.InsertOrUpdateAsync(endACT);


                    contributorActivationTableManager.InsertOrUpdate(contributorActivation);                    
                    log.Info($"Activation OtherDocument successfully completed. Contributor with given id: {otherDocElecContributor.Id}");

                }
                catch (Exception ex)
                {
                    SetLogger(null, "OtherDocument-Exception", ex.Message, "Excep-00");
                    SetLogger(null, "OtherDocument-Exception", ex.StackTrace, "Excep-01");

                    if (contributorActivation == null)
                        contributorActivation = new GlobalContributorActivation(requestObject.Code, Guid.NewGuid().ToString());

                    contributorActivation.Success = false;
                    contributorActivation.Message = "Error al activar contribuyente en producción.";
                    contributorActivation.Detail = ex.Message;
                    contributorActivation.Trace = ex.StackTrace;
                    contributorActivation.TestSetId = requestObject.TestSetOthersDocumentsResultObj.Id;
                    contributorActivation.ContributorCode = requestObject.Code;
                    contributorActivation.ContributorTypeId = requestObject.OtherDocContributorTypeId;
                    contributorActivation.OperationModeId = requestObject.OtherDocOperationModeId;
                    contributorActivation.SentToActivateBy = "SendToActivateOtherDocumentContributor";
                    contributorActivation.SoftwareId = requestObject.SoftwareProvider;
                    contributorActivation.OperationModeName = requestObject.TestSetOthersDocumentsResultObj.OperationModeName;                    
                    contributorActivationTableManager.InsertOrUpdate(contributorActivation);


                    log.Error($"Exception in ActivateOtherDocumentContributor. {ex.Message}", ex);
                }
            }
            else
                log.Error($"ActivateOtherDocumentContributor: Wrong enviroment {ConfigurationManager.GetValue("Environment")}. {myQueueItem}");
        }

        /// <summary>
        /// Metodo que permite registrar en el Log cualquier mensaje o evento que deeemos
        /// </summary>
        /// <param name="objData">Un Objeto que se serializara en Json a String y se mostrara en el Logger</param>
        /// <param name="Step">El paso del Log o de los mensajes</param>
        /// <param name="msg">Un mensaje adicional si no hay objdata, por ejemplo</param>
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

        class OtherDocumentActivateContributorRequestObject
        {
            [JsonProperty(PropertyName = "code")]
            public string Code { get; set; }

            [JsonProperty(PropertyName = "contributorId")]
            public int ContributorId { get; set; }

            [JsonProperty(PropertyName = "otherDocContributorTypeId")]
            public int OtherDocContributorTypeId { get; set; }

            [JsonProperty(PropertyName = "otherDocOperationModeId")]
            public int OtherDocOperationModeId { get; set; }

            [JsonProperty(PropertyName = "createdBy")]
            public string CreatedBy { get; set; }

            [JsonProperty(PropertyName = "softwareType")]
            public string SoftwareType { get; set; }

            [JsonProperty(PropertyName = "softwareId")]
            public string SoftwareId { get; set; }

            [JsonProperty(PropertyName = "softwareName")]
            public string SoftwareName { get; set; }

            [JsonProperty(PropertyName = "pin")]
            public string Pin { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "softwareUser")]
            public string SoftwareUser { get; set; }

            [JsonProperty(PropertyName = "softwarePassword")]
            public string SoftwarePassword { get; set; }

            [JsonProperty(PropertyName = "electronicDocumentId")]
            public int ElectronicDocumentId { get; set; }

            [JsonProperty(PropertyName = "softwareProvider")]
            public string SoftwareProvider { get; set; }

            [JsonProperty(PropertyName = "providerId")]
            public int ProviderId { get; set; }

            [JsonProperty(PropertyName = "testSetOthersDocumentsResult")]
            public GlobalTestSetOthersDocumentsResult TestSetOthersDocumentsResultObj { get; set; }
        }

    }
}
