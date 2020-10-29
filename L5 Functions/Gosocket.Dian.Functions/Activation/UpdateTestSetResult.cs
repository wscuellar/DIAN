using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Activation
{
    public static class UpdateTestSetResult
    {
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly ContributorOperationsService contributorOperationService = new ContributorOperationsService();
        private static readonly SoftwareService softwareService = new SoftwareService();
        private static readonly TableManager globalTestSetTableManager = new TableManager("GlobalTestSet");
        private static readonly TableManager globalTestSetResultTableManager = new TableManager("GlobalTestSetResult");
        private static readonly TableManager globalTestSetTrackingTableManager = new TableManager("GlobalTestSetTracking");
        private static readonly TableManager contributorTableManager = new TableManager("GlobalContributor");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");

        // Set queue name
        private const string queueName = "global-test-set-tracking-input%Slot%";

        [FunctionName("UpdateTestSetResult")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            try
            {
                var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                var globalTestSetTracking = JsonConvert.DeserializeObject<GlobalTestSetTracking>(eventGridEvent.Data.ToString());
                await globalTestSetTrackingTableManager.InsertOrUpdateAsync(globalTestSetTracking);

                var allGlobalTestSetTracking = globalTestSetTrackingTableManager.FindByPartition<GlobalTestSetTracking>(globalTestSetTracking.TestSetId);

                var testSetResults = globalTestSetResultTableManager.FindByPartition<GlobalTestSetResult>(globalTestSetTracking.SenderCode);

                var globalTesSetResult = testSetResults.SingleOrDefault(t => !t.Deleted && t.Id == globalTestSetTracking.TestSetId && t.Status == (int)TestSetStatus.InProcess);

                if (globalTesSetResult == null)
                    return;

                var globalTestSet = globalTestSetTableManager.Find<GlobalTestSet>(globalTesSetResult.TestSetReference, globalTesSetResult.TestSetReference);

                string[] invoiceCodes = { "1", "01", "02", "03" };
                string[] creditNoteCodes = { "7", "91" };
                string[] debitNoteCodes = { "8", "92" };

                globalTesSetResult.Id = globalTestSetTracking.TestSetId;

                globalTesSetResult.TotalDocumentSent = allGlobalTestSetTracking.Count;
                globalTesSetResult.TotalDocumentAccepted = allGlobalTestSetTracking.Count(a => a.IsValid);
                globalTesSetResult.TotalDocumentsRejected = allGlobalTestSetTracking.Count(a => !a.IsValid);


                globalTesSetResult.InvoicesTotalSent = allGlobalTestSetTracking.Count(a => invoiceCodes.Contains(a.DocumentTypeId));
                globalTesSetResult.TotalInvoicesAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && invoiceCodes.Contains(a.DocumentTypeId));
                globalTesSetResult.TotalInvoicesRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && invoiceCodes.Contains(a.DocumentTypeId));

                globalTesSetResult.TotalCreditNotesSent = allGlobalTestSetTracking.Count(a => creditNoteCodes.Contains(a.DocumentTypeId));
                globalTesSetResult.TotalCreditNotesAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && creditNoteCodes.Contains(a.DocumentTypeId));
                globalTesSetResult.TotalCreditNotesRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && creditNoteCodes.Contains(a.DocumentTypeId));

                globalTesSetResult.TotalDebitNotesSent = allGlobalTestSetTracking.Count(a => debitNoteCodes.Contains(a.DocumentTypeId));
                globalTesSetResult.TotalDebitNotesAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && debitNoteCodes.Contains(a.DocumentTypeId));
                globalTesSetResult.TotalDebitNotesRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && debitNoteCodes.Contains(a.DocumentTypeId));

                if (globalTesSetResult.TotalInvoicesAccepted >= globalTesSetResult.TotalInvoicesAcceptedRequired && globalTesSetResult.TotalCreditNotesAccepted >= globalTesSetResult.TotalCreditNotesAcceptedRequired && globalTesSetResult.TotalDebitNotesAccepted >= globalTesSetResult.TotalDebitNotesAcceptedRequired && globalTesSetResult.Status == (int)TestSetStatus.InProcess)
                    globalTesSetResult.Status = (int)TestSetStatus.Accepted;

                if (globalTesSetResult.TotalDocumentsRejected > (globalTesSetResult.TotalDocumentRequired - globalTesSetResult.TotalDocumentAcceptedRequired) && globalTesSetResult.Status == (int)TestSetStatus.InProcess)
                    globalTesSetResult.Status = (int)TestSetStatus.Rejected;

                await globalTestSetResultTableManager.InsertOrUpdateAsync(globalTesSetResult);

                if (globalTesSetResult.Status == (int)TestSetStatus.Accepted)
                {
                    // partition key are sender code.
                    var contributor = contributorService.GetByCode(globalTesSetResult.PartitionKey);
                    if (contributor.AcceptanceStatusId == (int)ContributorStatus.Registered)
                    {
                        contributorService.SetToEnabled(contributor);
                        var globalContributor = new GlobalContributor(contributor.Code, contributor.Code) { Code = contributor.Code, StatusId = contributor.AcceptanceStatusId, TypeId = contributor.ContributorTypeId };
                        await contributorTableManager.InsertOrUpdateAsync(globalContributor);
                    }

                    var software = softwareService.Get(Guid.Parse(globalTesSetResult.SoftwareId));
                    if (software.AcceptanceStatusSoftwareId == (int)SoftwareStatus.Test && globalTesSetResult.OperationModeId != (int)OperationMode.Free)
                    {
                        softwareService.SetToProduction(software);
                        var softwareId = software.Id.ToString();
                        var globalSoftware = new GlobalSoftware(softwareId, softwareId) { Id = software.Id, Deleted = software.Deleted, Pin = software.Pin, StatusId = software.AcceptanceStatusSoftwareId };
                        await softwareTableManager.InsertOrUpdateAsync(globalSoftware);
                    }

                    // Send to activate contributor in production
                    if (ConfigurationManager.GetValue("Environment") == "Hab")
                    {
                        try
                        {
                            var requestObject = new { contributorId = contributor.Id };
                            var activation = await ApiHelpers.ExecuteRequestAsync<SendToActivateContributorResponse>(ConfigurationManager.GetValue("SendToActivateContributorUrl"), requestObject);

                            var guid = Guid.NewGuid().ToString();
                            var contributorActivation = new GlobalContributorActivation(contributor.Code, guid)
                            {
                                Success = activation.Success,
                                ContributorCode = contributor.Code,
                                ContributorTypeId = contributor.ContributorTypeId,
                                OperationModeId = globalTesSetResult.OperationModeId,
                                OperationModeName = globalTesSetResult.OperationModeName,
                                SentToActivateBy = "Function",
                                SoftwareId = globalTesSetResult.SoftwareId,
                                SendDate = DateTime.UtcNow,
                                TestSetId = globalTesSetResult.Id,
                                Trace = activation.Trace,
                                Message = activation.Message,
                                Detail = activation.Detail,
                                Request = JsonConvert.SerializeObject(requestObject)
                            };
                            await contributorActivationTableManager.InsertOrUpdateAsync(contributorActivation);

                            if (globalTesSetResult.OperationModeId == (int)OperationMode.Free)
                                await MigrateCertificate(contributor.Code);

                            //var activateContributorRequestObject = new ActivateContributorRequestObject();
                            //var sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");
                            //var contributorProd = contributorService.GetByCode(contributor.Code, sqlConnectionStringProd);

                            //activateContributorRequestObject.ContributorId = contributorProd.Id;
                            //activateContributorRequestObject.ContributorTypeId = contributor.ContributorTypeId.Value;
                            //activateContributorRequestObject.OperationModeId = globalTesSetResult.OperationModeId;
                            //var ownSoftware = contributorService.Get(software.ContributorId);
                            //activateContributorRequestObject.Software = new ActivateSoftwareContributorRequestObject
                            //{
                            //    Id = software.Id,
                            //    ContributorId = software.ContributorId,
                            //    ContributorCode = ownSoftware.Code,
                            //    Pin = software.Pin,
                            //    Name = software.Name,
                            //    SoftwareDate = software.SoftwareDate,
                            //    SoftwareUser = software.SoftwareUser,
                            //    SoftwarePassword = software.SoftwarePassword,
                            //    Url = software.Url,
                            //    AcceptanceStatusSoftwareId = (int)SoftwareStatus.Production
                            //};

                            //if (globalTesSetResult.ProviderId != null)
                            //{
                            //    var provider = contributorService.Get(globalTesSetResult.ProviderId.Value);
                            //    var providerInProd = contributorService.GetByCode(provider.Code, sqlConnectionStringProd);
                            //    if (providerInProd != null)
                            //        activateContributorRequestObject.ProviderId = providerInProd.Id;
                            //}

                            //await SendToActivateContributorToProduction(activateContributorRequestObject);
                        }
                        catch (Exception ex)
                        {
                            log.Error($"Error al enviar a activar contribuyente con id {contributor.Id} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                throw;
            }
        }

        private static async Task MigrateCertificate(string contributorCode)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "Migrate.Certificate.Event",
                    Data = JsonConvert.SerializeObject(contributorCode),
                    EventTime = DateTime.UtcNow,
                    Subject = $"|MigrateCertificate|",
                    DataVersion = "2.0"
                }
            };
            await EventGridManager.Instance("EventGridKeyProd", "EventGridTopicEndpointProd").SendMessagesToEventGridAsync(eventsList);
        }

        class SendToActivateContributorResponse
        {
            [JsonProperty(PropertyName = "success")]
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }

            [JsonProperty(PropertyName = "detail")]
            public string Detail { get; set; }

            [JsonProperty(PropertyName = "trace")]
            public string Trace { get; set; }
        }

        //private static async Task SendToActivateContributorToProduction(ActivateContributorRequestObject activateContributorRequestObject)
        //{
        //    List<EventGridEvent> eventsList = new List<EventGridEvent>
        //    {
        //        new EventGridEvent()
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            EventType = "Activate.Contributor.Event",
        //            Data = JsonConvert.SerializeObject(activateContributorRequestObject),
        //            EventTime = DateTime.UtcNow,
        //            Subject = $"|PRIORITY:1|",
        //            DataVersion = "2.0"
        //        }
        //    };
        //    await EventGridManager.Instance("EventGridKeyProd", "EventGridTopicEndpointProd").SendMessagesToEventGridAsync(eventsList);
        //}

        //class ActivateContributorRequestObject
        //{
        //    [JsonProperty(PropertyName = "contributorId")]
        //    public int ContributorId { get; set; }
        //    [JsonProperty(PropertyName = "contributorTypeId")]
        //    public int ContributorTypeId { get; set; }
        //    [JsonProperty(PropertyName = "operationModeId")]
        //    public int OperationModeId { get; set; }
        //    [JsonProperty(PropertyName = "providerId")]
        //    public int ProviderId { get; set; }
        //    [JsonProperty(PropertyName = "software")]
        //    public ActivateSoftwareContributorRequestObject Software { get; set; }
        //}

        //class ActivateSoftwareContributorRequestObject
        //{
        //    public Guid Id { get; set; }

        //    public int ContributorId { get; set; }

        //    public string ContributorCode { get; set; }

        //    public string Pin { get; set; }

        //    public string Name { get; set; }

        //    public DateTime? SoftwareDate { get; set; }

        //    public string SoftwareUser { get; set; }

        //    public string SoftwarePassword { get; set; }

        //    public string Url { get; set; }

        //    public bool Status { get; set; }

        //    public int AcceptanceStatusSoftwareId { get; set; }
        //}
    }
}
