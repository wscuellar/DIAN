using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;

namespace Gosocket.Dian.Functions.Radian
{
    public static class RadianActivateContributor
    {
        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly SoftwareService softwareService = new SoftwareService();
        private static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager GlobalRadianOperationsTableManager = new TableManager("GlobalRadianOperations");
        private static readonly TableManager TableManagerRadianTestSetResult = new TableManager("RadianTestSetResult");
        private static readonly TableManager tableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");

        // Set queue name
        private const string queueName = "activate-radian-operation-input%Slot%";

        [FunctionName("ActivateRadianOperation")]
        public static void Run([QueueTrigger(queueName, Connection = "GlobalStorage")] string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            SetLogger(null, "Step RA-00", " Ingresamos a RadianActivateOperation ");

            //Variable de validacion para registro informacion del piloto           
            bool.TryParse(Environment.GetEnvironmentVariable("IsProduction"), out bool IsProduction);


            SetLogger(null, "Step RA-00", " Ingresamos a RadianActivateOperation " + IsProduction, "Step RA-100");
            SetLogger(null, "Step RA-00", " Ingresamos a RadianActivateOperation Entorno" + ConfigurationManager.GetValue("Environment"), "Step RA-101");

            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                SetLogger(null, "Step RA-01", " Ingresamos a ActivateRadianOperation Ambiente Prod ");

                Contributor contributor = null;
                RadianContributor radianContributor = null;
                GlobalContributorActivation contributorActivation = null;
                RadianaActivateContributorRequestObject requestObject = null;
                try
                {
                    //    // Step 1  Validate RadianContributor
                    EventGridEvent eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                    requestObject = JsonConvert.DeserializeObject<RadianaActivateContributorRequestObject>(eventGridEvent.Data.ToString());
                    SetLogger(requestObject, "Step RA-1", "RadianContributor ", "Paso1");

                    //Contributorid = RadiancontributoriD
                    int radianContributorId = 0;
                    radianContributor = contributorService.GetRadian(requestObject.ContributorId, requestObject.RadianContributorTypeId);
                    SetLogger(null, "Step RA-1", radianContributor == null ? "vacio" : "radiancontributor no es null", "Paso2");

                    if (radianContributor != null)
                    {
                        // Step 3 Activo RadianContributor
                        radianContributor.Step = 4;
                        radianContributor.RadianContributorTypeId = requestObject.RadianContributorTypeId;
                        contributorService.ActivateRadian(radianContributor);
                        radianContributorId = radianContributor.Id;
                        SetLogger(null, "Step RA-3", "ActivateRadian -->  ");

                    }
                    else  //si el sujeto en radian no existe
                    {
                        //        // Step 4 Actualizo RadianSoftware en SQL 
                        radianContributor = new RadianContributor()
                        {
                            CreatedBy = requestObject.CreatedBy,
                            ContributorId = requestObject.ContributorId,
                            RadianContributorTypeId = requestObject.RadianContributorTypeId,
                            RadianOperationModeId = requestObject.RadianOperationModeId,
                            RadianState = Domain.Common.RadianState.Habilitado.GetDescription(),
                            CreatedDate = System.DateTime.Now,
                            Update = System.DateTime.Now,
                            Step = 4
                        };
                        SetLogger(radianContributor, "Step RA-4", " -- contributorService.AddOrUpdateRadianContributor -- ", "Paso3");
                        
                        if(IsProduction)
                            radianContributorId = contributorService.AddOrUpdateRadianContributor(radianContributor);
                        SetLogger(null, "Step RA-4", " -- contributorService.AddOrUpdateRadianContributor -- ");
                    }



                    // si el software No Existe
                    if (Convert.ToInt32(requestObject.SoftwareType) == (int)Domain.Common.RadianOperationModeTestSet.OwnSoftware)
                    {
                        RadianSoftware newSoftware = new RadianSoftware()
                        {
                            Id = new Guid(requestObject.SoftwareId),
                            Deleted = false,
                            Name = requestObject.SoftwareName,
                            Pin = requestObject.Pin,
                            SoftwareDate = DateTime.Now,
                            SoftwareUser = requestObject.SoftwareUser,
                            SoftwarePassword = requestObject.SoftwarePassword,
                            Status = true,
                            RadianSoftwareStatusId = (int)Domain.Common.RadianSoftwareStatus.Accepted,
                            Url = requestObject.Url,
                            CreatedBy = requestObject.CreatedBy,
                            ContributorId = requestObject.ContributorId
                        };
                        if (IsProduction)
                            softwareService.AddOrUpdateRadianSoftware(newSoftware);

                        SetLogger(newSoftware, "Step RA-5", " -- softwareService.AddOrUpdateRadianSoftware -- ");

                        // Crear Software en TableSTorage
                        GlobalSoftware globalSoftware = new GlobalSoftware(requestObject.SoftwareId, requestObject.SoftwareId)
                        {
                            Id = new Guid(requestObject.SoftwareId),
                            Deleted = false,
                            Pin = requestObject.Pin,
                            StatusId = (int)Domain.Common.SoftwareStatus.Production
                        };
                        if (IsProduction)
                            softwareTableManager.InsertOrUpdateAsync(globalSoftware).Wait();

                        SetLogger(globalSoftware, "Step RA-6", " -- softwareTableManager.InsertOrUpdateAsync -- ");
                    }


                    //    //--1. se busac operation por radiancontributorid y software 

                    RadianContributorOperation radianOperation = contributorService.GetRadianOperations(radianContributorId, requestObject.SoftwareId);

                    //---2.. si no existe se crea  
                    if (radianOperation == null)
                    {
                        radianOperation = new RadianContributorOperation()
                        {
                            Deleted = false,
                            OperationStatusId = (int)Domain.Common.RadianState.Habilitado,
                            SoftwareId = new Guid(requestObject.SoftwareId),
                            RadianContributorId = radianContributorId,
                            Timestamp = System.DateTime.Now,
                            SoftwareType = Convert.ToInt32(requestObject.SoftwareType)
                        };
                        SetLogger(radianOperation, "RadianOperation", string.Empty, "paso8");

                        if (IsProduction)
                            _ = contributorService.AddRadianOperation(radianOperation);

                        SetLogger(null, "Step RA-7", "Cree la operacion", "paso9");
                    }
                    else //---3. si existe se actualiza.
                    {
                        radianOperation.OperationStatusId = (int)Domain.Common.RadianState.Habilitado;
                        radianOperation.SoftwareId = new Guid(requestObject.SoftwareId);
                        radianOperation.RadianContributorId = radianContributorId;
                        radianOperation.SoftwareType = Convert.ToInt32(requestObject.SoftwareType);
                        radianOperation.Timestamp = System.DateTime.Now;

                        if (IsProduction)
                            contributorService.UpdateRadianOperation(radianOperation);
                        SetLogger(null, "Step RA-8", " -- contributorService.UpdateRadianOperation -- ");
                    }



                    //    //--1. COnsultar previo por si ya existe
                    GlobalRadianOperations globalRadianOperations = GlobalRadianOperationsTableManager.Find<GlobalRadianOperations>(requestObject.Code, requestObject.SoftwareId);
                    if (globalRadianOperations == null)
                        globalRadianOperations = new GlobalRadianOperations(requestObject.Code, requestObject.SoftwareId);

                    //--2. si existe solo actualizar lo que llega
                    if (radianContributor.RadianOperationModeId == (int)Domain.Common.RadianOperationMode.Indirect)
                        globalRadianOperations.IndirectElectronicInvoicer = radianContributor.RadianOperationModeId == (int)Domain.Common.RadianOperationMode.Indirect;
                    if (radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.ElectronicInvoice)
                        globalRadianOperations.ElectronicInvoicer = radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.ElectronicInvoice;
                    if (radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.TechnologyProvider)
                        globalRadianOperations.TecnologicalSupplier = radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.TechnologyProvider;
                    if (radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.TradingSystem)
                        globalRadianOperations.NegotiationSystem = radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.TradingSystem;
                    if (radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.Factor)
                        globalRadianOperations.Factor = radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.Factor;

                    globalRadianOperations.Deleted = false;
                    globalRadianOperations.RadianContributorTypeId = radianContributor.RadianContributorTypeId;
                    globalRadianOperations.RadianState = Domain.Common.RadianState.Habilitado.GetDescription();
                    globalRadianOperations.SoftwareType = Convert.ToInt32(requestObject.SoftwareType);

                    //--3. Si no existe si se crea.    //-----Razon los estados deben mantenerse en la actualizacion. mismo nit y software pueden usar diferentes modos.
                    if (IsProduction)
                        GlobalRadianOperationsTableManager.InsertOrUpdateAsync(globalRadianOperations).Wait();

                    //Se inserta en RadianTestSetResult
                    if (IsProduction)
                        TableManagerRadianTestSetResult.InsertOrUpdateAsync(requestObject.RadianTestSetObj).Wait();
                    

                    //--Traemos la informacion del software
                    string softwareId = requestObject.SoftwareId;
                    RadianSoftware software = softwareService.GetByRadian(Guid.Parse(softwareId));
                    contributor = contributorService.Get(software.ContributorId);
                    if(contributor != null)
                    {
                        //Se inserta en GlobalAuthorization
                        var auth = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(contributor.Code, requestObject.Code);
                        if (auth == null)
                            tableManagerGlobalAuthorization.InsertOrUpdate(new GlobalAuthorization(contributor.Code, requestObject.Code));

                    }

                    SetLogger(null, "Step RA-9", " -- globalRadianOperations -- ");


                    log.Info($"Activation Radian successfully completed. Contributor with given id: {radianContributor.Id}");

                }
                catch (Exception ex)
                {


                    SetLogger(null, "RA-Exception", ex.Message, "Excep-00");
                    SetLogger(null, "RA-Exception", ex.StackTrace, "Excep-01");

                    if (contributorActivation == null)
                        contributorActivation = new GlobalContributorActivation(requestObject.ContributorId.ToString(), Guid.NewGuid().ToString());

                    contributorActivation.Success = false;
                    contributorActivation.Message = "Error al activar contribuyente en producción.";
                    contributorActivation.Detail = ex.Message;
                    contributorActivation.Trace = ex.StackTrace;
                    contributorActivationTableManager.InsertOrUpdate(contributorActivation);


                    log.Error($"Exception in RadianActivateContributor. {ex.Message}", ex);
                }
            }
            else
                log.Error($"RadianActivateContributor: Wrong enviroment {ConfigurationManager.GetValue("Environment")}. {myQueueItem}");
        }

        class RadianaActivateContributorRequestObject
        {
            [JsonProperty(PropertyName = "code")]
            public string Code { get; set; }

            [JsonProperty(PropertyName = "contributorId")]
            public int ContributorId { get; set; }

            [JsonProperty(PropertyName = "radianContributorTypeId")]
            public int RadianContributorTypeId { get; set; }

            [JsonProperty(PropertyName = "radianOperationModeId")]
            public int RadianOperationModeId { get; set; }

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

            [JsonProperty(PropertyName = "radianTestSetObj")]
            public RadianTestSetResult RadianTestSetObj { get; set; }
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

    }
}
