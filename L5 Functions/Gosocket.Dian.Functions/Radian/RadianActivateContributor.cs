using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
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



        [FunctionName("RadianActivateContributor")]
        public static void Run([QueueTrigger(queueName, Connection = "GlobalStorage")] string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                RadianContributor contributor = null;
                GlobalContributorActivation contributorActivation = null;
                ActivateRadianContributorRequestObject requestObject = null;
                try
                {
                    // Stepp 1  Validate RadianContributor
                    EventGridEvent eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                    requestObject = JsonConvert.DeserializeObject<ActivateRadianContributorRequestObject>(eventGridEvent.Data.ToString());

                    contributor = contributorService.GetRadian(requestObject.ContributorId);

                    string resultJson = JsonConvert.SerializeObject(requestObject);
                    var lastZone = new GlobalLogger("RadianActivateContributor", "Step 1") { Message = resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found RADIAN contributor with given id {requestObject.ContributorId}");

                    // Step 2 Incluyo los datos en GlobalContributorActivation 
                    var guid = Guid.NewGuid().ToString();
                    contributorActivation = new GlobalContributorActivation(contributor.ContributorId.ToString(), guid)
                    {
                        Success = true,
                        ContributorCode = contributor.ContributorId.ToString(),
                        ContributorTypeId = requestObject.ContributorTypeId,
                        OperationModeId = requestObject.OperationModeId,
                        SentToActivateBy = "Function",
                        SoftwareId = requestObject.Software?.Id.ToString(),
                        SendDate = DateTime.UtcNow,
                        Message = "Contribuyente Radian se activó en producción con éxito.",
                        Request = myQueueItem
                    };

                    resultJson = JsonConvert.SerializeObject(contributorActivation);
                    lastZone = new GlobalLogger("RadianActivateContributor", "Step 2") { Message = resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 3 Activo RadianContributor

                    contributor.RadianContributorTypeId = requestObject.ContributorTypeId;
                    contributorService.ActivateRadian(contributor);

                    resultJson = JsonConvert.SerializeObject(contributor);
                    lastZone = new GlobalLogger("RadianActivateContributor", "Step 3") { Message = "ActivateRadian --> " + resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 4 Actualizo RadianSoftware en SQL 

                    var utcNow = DateTime.UtcNow;
                    var software = softwareService.GetRadianSoftware(requestObject.Software.Id);
                    if (software == null)
                    {
                        var ownSoftware = contributorService.GetByCode(requestObject.Software.ContributorCode);
                        software = new RadianSoftware
                        {
                            Id = requestObject.Software.Id,
                            RadianContributorId = ownSoftware.Id, // OJOOOOOOOOOOOO
                            Name = requestObject.Software.Name,
                            Pin = requestObject.Software.Pin,
                            SoftwareDate = utcNow,
                            SoftwareUser = requestObject.Software.SoftwareUser,
                            SoftwarePassword = requestObject.Software.SoftwarePassword,
                            Url = requestObject.Software.Url,
                            Status = true,
                            Deleted = false,
                            Timestamp = utcNow,
                            Updated = utcNow,
                            CreatedBy = "ActivateContributorFunction",
                            RadianSoftwareStatusId = (int)SoftwareStatus.Production
                        };
                        softwareService.AddOrUpdateRadianSoftware(software);

                        resultJson = JsonConvert.SerializeObject(software);
                        lastZone = new GlobalLogger("RadianActivateContributor", "Step 4") { Message = "AddOrUpdateRadianSoftware --> " + resultJson };
                        TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                        // Step 5  Actualizacion Software Table Storage

                        var softwareId = software.Id.ToString();
                        var globalSoftware = new GlobalSoftware(softwareId, softwareId)
                        {
                            Id = software.Id,
                            Deleted = software.Deleted,
                            Pin = software.Pin,
                            StatusId = software.RadianSoftwareStatusId
                        };
                        softwareTableManager.InsertOrUpdate(globalSoftware);

                        resultJson = JsonConvert.SerializeObject(software);
                        lastZone = new GlobalLogger("RadianActivateContributor", "Step 5") { Message = "AddOrUpdateRadianSoftware --> " + resultJson };
                        TableManagerGlobalLogger.InsertOrUpdate(lastZone);
                    }

                    // Step 6 

                    var contributorOperation = new RadianContributorOperations
                    {
                        ContributorId = contributor.Id,
                        ContributorTypeId = requestObject.ContributorTypeId,
                        OperationModeId = requestObject.OperationModeId,
                        ProviderId = requestObject.ProviderId != 0 ? requestObject.ProviderId : null,
                        SoftwareId = requestObject.Software.Id,
                        Deleted = false,
                        Timestamp = utcNow
                    };

                    /*********************************************************************************************/
                    // Porque esto ConfigurationManager.GetValue("BillerSoftwareId") ??????????????
                    /*********************************************************************************************/
                    if (contributorOperation.OperationModeId == (int)Domain.Common.OperationMode.Free &&
                                            contributorOperation.SoftwareId == null)
                        contributorOperation.SoftwareId = Guid.Parse(ConfigurationManager.GetValue("BillerSoftwareId"));

                    var contributorOperationSearch = contributorOperationService.Get(
                                            contributor.Id,
                                            contributorOperation.OperationModeId,
                                            contributorOperation.ProviderId,
                                            contributorOperation.SoftwareId);

                    if (contributorOperationSearch == null)
                        contributorOperationService.AddOrUpdateRadianContributorOperation(contributorOperation);

                    resultJson = JsonConvert.SerializeObject(contributorOperation);
                    lastZone = new GlobalLogger("RadianActivateContributor", "Step 6") { Message = "AddOrUpdateRadianContributorOperation --> " + resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 7  Global Authorization 

                    var auth = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(
                                                                            contributor.Id.ToString(),
                                                                            contributor.Id.ToString());
                    if (auth == null)
                        tableManagerGlobalAuthorization.InsertOrUpdate(new GlobalAuthorization(
                                                                contributor.Id.ToString(), contributor.Id.ToString()));

                    if (contributorOperation.ProviderId != null)
                    {
                        var provider = contributorService.Get(contributorOperation.ProviderId.Value);
                        if (provider != null)
                        {
                            var authorization = new GlobalAuthorization(provider.Code, contributor.ContributorId.ToString());
                            tableManagerGlobalAuthorization.InsertOrUpdate(authorization);

                            resultJson = JsonConvert.SerializeObject(contributorOperation);
                            lastZone = new GlobalLogger("RadianActivateContributor", "Step 7") { Message = "GlobalAuthorization --> " + resultJson };
                            TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                        }
                    }
                    // Step 8 Update GlobalContributor Activation 
                    contributorActivationTableManager.InsertOrUpdate(contributorActivation);

                    resultJson = JsonConvert.SerializeObject(contributorActivation);
                    lastZone = new GlobalLogger("RadianActivateContributor", "Step 8") { Message = " --> " + resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    log.Info($"Activation Radian successfully completed. Contributor with given id: {contributor.Id}");

                }
                catch (Exception ex)
                {
                    log.Error($"Exception in RadianActivateContributor. {ex.Message}", ex);
                    throw;
                }
            }
            else
                log.Error($"RadianActivateContributor: Wrong enviroment {ConfigurationManager.GetValue("Environment")}. {myQueueItem}");
        }

        class ActivateRadianContributorRequestObject
        {
            [JsonProperty(PropertyName = "contributorId")]
            public int ContributorId { get; set; }
            [JsonProperty(PropertyName = "exchangeEmail")]
            public string ExchangeEmail { get; set; }
            [JsonProperty(PropertyName = "contributorTypeId")]
            public int ContributorTypeId { get; set; }
            [JsonProperty(PropertyName = "operationModeId")]
            public int OperationModeId { get; set; }
            [JsonProperty(PropertyName = "providerId")]
            public int? ProviderId { get; set; }
            [JsonProperty(PropertyName = "software")]
            public ActivateRadianSoftwareContributorRequestObject Software { get; set; }
        }

        class ActivateRadianSoftwareContributorRequestObject
        {
            public Guid Id { get; set; }

            public int ContributorId { get; set; }
            public string ContributorCode { get; set; }
            public string Pin { get; set; }

            public string Name { get; set; }

            public DateTime? SoftwareDate { get; set; }

            public string SoftwareUser { get; set; }

            public string SoftwarePassword { get; set; }

            public string Url { get; set; }

            public bool Status { get; set; }

            public int AcceptanceStatusSoftwareId { get; set; }
        }
    }
}
