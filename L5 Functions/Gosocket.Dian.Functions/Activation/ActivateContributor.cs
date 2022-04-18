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
using System.Data.Entity.Core;

namespace Gosocket.Dian.Functions.Activation
{
    public static class ActivateContributor
    {
        private static readonly ContributorOperationsService contributorOperationService = new ContributorOperationsService();
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly SoftwareService softwareService = new SoftwareService();
        private static readonly TableManager tableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");
        private static readonly TableManager contributorTableManager = new TableManager("GlobalContributor");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        private static readonly TableManager exchangeEmailTableManager = new TableManager("GlobalExchangeEmail");

        // Set queue name
        private const string queueName = "activate-contributor-input%Slot%";

        [FunctionName("ActivateContributor")]
        public static void Run([QueueTrigger(queueName, Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                Contributor contributor = null;
                GlobalContributorActivation contributorActivation = null;
                ActivateContributorRequestObject requestObject = null;
                try
                {
                    var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                    requestObject = JsonConvert.DeserializeObject<ActivateContributorRequestObject>(eventGridEvent.Data.ToString());

                    contributor = contributorService.Get(requestObject.ContributorId);

                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found contributor with given id {requestObject.ContributorId}");

                    try
                    {
                        var guid = Guid.NewGuid().ToString();
                        contributorActivation = new GlobalContributorActivation(contributor.Code, guid)
                        {
                            Success = true,
                            ContributorCode = contributor.Code,
                            ContributorTypeId = requestObject.ContributorTypeId,
                            OperationModeId = requestObject.OperationModeId,
                            SentToActivateBy = "Function",
                            SoftwareId = requestObject.Software?.Id.ToString(),
                            SendDate = DateTime.UtcNow,
                            Message = "Contribuyente se activ� en producci�n con �xito.",
                            Request = myQueueItem
                        };
                    }
                    catch { }

                    contributor.ContributorTypeId = requestObject.ContributorTypeId;
                    contributorService.Activate(contributor);

                    var globalContributor = new GlobalContributor(contributor.Code, contributor.Code) { Code = contributor.Code, StatusId = (int)ContributorStatus.Enabled, TypeId = contributor.ContributorTypeId };
                    contributorTableManager.InsertOrUpdate(globalContributor);

                    if (!string.IsNullOrEmpty(contributor.ExchangeEmail))
                    {
                        var globalExchangeEmail = new GlobalExchangeEmail(contributor.Code, contributor.Code) { Email = contributor.ExchangeEmail };
                        exchangeEmailTableManager.InsertOrUpdate(globalExchangeEmail);
                    }

                    var utcNow = DateTime.UtcNow;
                    var software = softwareService.Get(requestObject.Software.Id);
                    if (software == null)
                    {
                        var ownSoftware = contributorService.GetByCode(requestObject.Software.ContributorCode);
                        software = new Software
                        {
                            Id = requestObject.Software.Id,
                            ContributorId = ownSoftware.Id, //requestObject.Software.ContributorId,
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
                            AcceptanceStatusSoftwareId = (int)SoftwareStatus.Production
                        };
                        softwareService.AddOrUpdate(software);

                        var softwareId = software.Id.ToString();
                        var globalSoftware = new GlobalSoftware(softwareId, softwareId) { Id = software.Id, Deleted = software.Deleted, Pin = software.Pin, StatusId = software.AcceptanceStatusSoftwareId };
                        softwareTableManager.InsertOrUpdate(globalSoftware);
                    }

                    var contributorOperation = new ContributorOperations
                    {
                        ContributorId = contributor.Id,
                        ContributorTypeId = requestObject.ContributorTypeId,
                        OperationModeId = requestObject.OperationModeId,
                        ProviderId = requestObject.ProviderId != 0 ? requestObject.ProviderId : null,
                        SoftwareId = requestObject.Software.Id,
                        Deleted = false,
                        Timestamp = utcNow
                    };

                    if (contributorOperation.OperationModeId == (int)Domain.Common.OperationMode.Free && contributorOperation.SoftwareId == null)
                        contributorOperation.SoftwareId = Guid.Parse(ConfigurationManager.GetValue("BillerSoftwareId"));

                    var contributorOperationSearch = contributorOperationService.Get(contributor.Id, contributorOperation.OperationModeId, contributorOperation.ProviderId, contributorOperation.SoftwareId);
                    if (contributorOperationSearch == null)
                        contributorOperationService.AddOrUpdate(contributorOperation);

                    var auth = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(contributor.Code, contributor.Code);
                    if (auth == null)
                        tableManagerGlobalAuthorization.InsertOrUpdate(new GlobalAuthorization(contributor.Code, contributor.Code));
                    if (contributorOperation.ProviderId != null)
                    {
                        var provider = contributorService.Get(contributorOperation.ProviderId.Value);
                        if (provider != null)
                        {
                            var authorization = new GlobalAuthorization(provider.Code, contributor.Code);
                            tableManagerGlobalAuthorization.InsertOrUpdate(authorization);
                        }
                    }

                    contributorActivationTableManager.InsertOrUpdate(contributorActivation);
                    log.Info($"Activation successfully completed. Contributor with given id: {contributor.Id}");
                }
                catch (Exception ex)
                {
                    if (contributorActivation == null)
                        contributorActivation = new GlobalContributorActivation(requestObject.ContributorId.ToString(), Guid.NewGuid().ToString());

                    contributorActivation.Success = false;
                    contributorActivation.Message = "Error al activar contribuyente en producci�n.";
                    contributorActivation.Detail = ex.Message;
                    contributorActivation.Trace = ex.StackTrace;
                    contributorActivationTableManager.InsertOrUpdate(contributorActivation);
                    log.Error($"Error al activar contribuyente con id '{contributor?.Id}' en producci�n _________ queueItem: '{myQueueItem}'_____________{ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                    throw;
                }
            }
            else
                log.Error($"Wrong enviroment {ConfigurationManager.GetValue("Environment")}. {myQueueItem}");
        }

        class ActivateContributorRequestObject
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
            public ActivateSoftwareContributorRequestObject Software { get; set; }
        }

        class ActivateSoftwareContributorRequestObject
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