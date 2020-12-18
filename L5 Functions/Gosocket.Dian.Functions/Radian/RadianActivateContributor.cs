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
        private static readonly ContributorOperationsService contributorOperationService = new ContributorOperationsService();
        private static readonly TableManager tableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");


        // Set queue name
        private const string queueName = "radian-activate-contributor-input%Slot%";



        [FunctionName("ActivateRadianOperation")]
        public static void Run([QueueTrigger(queueName, Connection = "GlobalStorage")] string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            SetLogger(null, "Step 00", " Ingresamos a ActivateRadianOperation ");

            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                SetLogger(null, "Step 01", " Ingresamos a ActivateRadianOperation Ambiente Prod ");

                RadianContributor radianContributor = null;
                GlobalContributorActivation contributorActivation = null;
                RadianaActivateContributorRequestObject requestObject = null;
                try
                {
                    // Stepp 1  Validate RadianContributor
                    EventGridEvent eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                    requestObject = JsonConvert.DeserializeObject<RadianaActivateContributorRequestObject>(eventGridEvent.Data.ToString());

                    //Contributorid = RadiancontributoriD
                    radianContributor = contributorService.GetRadian(requestObject.ContributorId, requestObject.RadianContributorTypeId);

                    SetLogger(requestObject, "Step 1", "RadianContributor ");

                    // Step 3 Activo RadianContributor

                    radianContributor.RadianContributorTypeId = requestObject.RadianContributorTypeId;
                    contributorService.ActivateRadian(radianContributor);

                    SetLogger(radianContributor, "Step 3", "ActivateRadian -->  ");

                    // Step 4 Actualizo RadianSoftware en SQL 

                    RadianContributor newRadianContributor = new RadianContributor()
                    {
                        CreatedBy = requestObject.CreatedBy,
                        ContributorId = requestObject.ContributorId,
                        RadianContributorTypeId = requestObject.RadianContributorTypeId,
                        RadianOperationModeId = requestObject.RadianOperationModeId,
                        RadianState = Domain.Common.RadianState.Habilitado.GetDescription(),
                        Step = 4
                    };

                    int radianContributorId = contributorService.AddOrUpdateRadianContributor(newRadianContributor);
                    SetLogger(radianContributorId, "Step 4", " -- contributorService.AddOrUpdateRadianContributor -- ");

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
                            RadianContributorId = radianContributorId
                        };

                        softwareService.AddOrUpdateRadianSoftware(newSoftware);

                        SetLogger(newSoftware, "Step 5", " -- softwareService.AddOrUpdateRadianSoftware -- ");

                        // Crear Software en TableSTorage
                        GlobalSoftware globalSoftware = new GlobalSoftware(requestObject.SoftwareId, requestObject.SoftwareId)
                        {
                            Id = new Guid(requestObject.SoftwareId),
                            Deleted = false,
                            Pin = requestObject.Pin,
                            StatusId = (int)Domain.Common.SoftwareStatus.Production
                        };
                        softwareTableManager.InsertOrUpdateAsync(globalSoftware);

                        SetLogger(globalSoftware, "Step 6", " -- softwareTableManager.InsertOrUpdateAsync -- ");
                    }

                    //  Insertamos la operacion
                    RadianContributorOperation operation = new RadianContributorOperation()
                    {
                        Deleted = false,
                        OperationStatusId = (int)Domain.Common.RadianState.Habilitado,
                        SoftwareId = new Guid(requestObject.SoftwareId),
                        RadianContributorId = radianContributorId,
                        SoftwareType = Convert.ToInt32(requestObject.SoftwareType)
                    };

                    int oper = contributorService.AddRadianOperation(operation);

                    SetLogger(operation, "Step 7", " -- contributorService.AddRadianOperation -- ");

                    GlobalRadianOperations globalRadianOperations =
                                new GlobalRadianOperations(requestObject.Code, requestObject.SoftwareId)
                                {
                                    Deleted = false,
                                    RadianContributorTypeId = radianContributor.RadianContributorTypeId,
                                    RadianState = Domain.Common.RadianState.Habilitado.GetDescription(),
                                    SoftwareType = Convert.ToInt32(requestObject.SoftwareType)
                                };

                    SetLogger(globalRadianOperations, "Step 8", " -- globalRadianOperations -- ");


                    log.Info($"Activation Radian successfully completed. Contributor with given id: {radianContributor.Id}");

                }
                catch (Exception ex)
                {
                    if (contributorActivation == null)
                        contributorActivation = new GlobalContributorActivation(requestObject.ContributorId.ToString(), Guid.NewGuid().ToString());

                    contributorActivation.Success = false;
                    contributorActivation.Message = "Error al activar contribuyente en producción.";
                    contributorActivation.Detail = ex.Message;
                    contributorActivation.Trace = ex.StackTrace;
                    contributorActivationTableManager.InsertOrUpdate(contributorActivation);

                    SetLogger(contributorActivation, "Exception", ex.Message + " -- -- " + ex);

                    log.Error($"Exception in RadianActivateContributor. {ex.Message}", ex);
                    throw;
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
        }

        /// <summary>
        /// Metodo que permite registrar en el Log cualquier mensaje o evento que deeemos
        /// </summary>
        /// <param name="objData">Un Objeto que se serializara en Json a String y se mostrara en el Logger</param>
        /// <param name="Step">El paso del Log o de los mensajes</param>
        /// <param name="msg">Un mensaje adicional si no hay objdata, por ejemplo</param>
        private static void SetLogger(object objData, string Step, string msg)
        {
            object resultJson;

            if (objData != null)
                resultJson = JsonConvert.SerializeObject(objData);
            else
                resultJson = String.Empty;

            var lastZone = new GlobalLogger("202012", "202012") { Message = Step + " --> " + resultJson + " -- Msg --" + msg };
            TableManagerGlobalLogger.InsertOrUpdate(lastZone);
        }
    }
}
