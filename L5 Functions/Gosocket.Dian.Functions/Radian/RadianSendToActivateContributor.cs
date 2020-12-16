using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Radian
{
    public static class RadianSendToActivateContributor
    {

        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly TableManager globalTestSetResultTableManager = new TableManager("RadianTestSetResult");



        [FunctionName("RadianSendToActivateContributor")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (ConfigurationManager.GetValue("Environment") == "Hab")
            {
                RadianContributor contributor = null;
                var activateContributorRequestObject = new ActivateContributorRequestObject();
                var sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");

                try
                {
                    var data = await req.Content.ReadAsAsync<ActivationRequest>();
                    if (data == null)
                        throw new Exception("Request body is empty.");

                    if (data.ContributorId == 0)
                        throw new Exception("Please pass a contributor ud in the request body.");

                    // Step 1 Get RadianContributor
                    contributor = contributorService.GetRadian(data.ContributorId);
                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                    string resultJson = JsonConvert.SerializeObject(contributor);
                    var lastZone = new GlobalLogger("RadianSendToActivateContributor", "Step 1") { Message = resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 2 Contributor Production
                    var contributorProd = contributorService.GetByCode(contributor.ContributorId.ToString(), sqlConnectionStringProd);
                    if (contributorProd == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Prod with given code {contributor.ContributorId}.");

                    resultJson = JsonConvert.SerializeObject(contributorProd);
                    lastZone = new GlobalLogger("RadianSendToActivateContributor", "Step 2") { Message = resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 3 GlobalTestSetResult
                    var results = globalTestSetResultTableManager.FindByPartition<GlobalTestSetResult>(contributor.ContributorId.ToString());
                    results = results.Where(r => !r.Deleted && r.Status == (int)Domain.Common.TestSetStatus.Accepted).ToList();
                    if (!results.Any()) throw new Exception("Contribuyente no a pasado set de pruebas.");

                    resultJson = JsonConvert.SerializeObject(results);
                    lastZone = new GlobalLogger("RadianSendToActivateContributor", "Step 3") { Message = resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 4  Enable Contributor
                    RadianSoftware radianSoftware = new RadianSoftware();
                    if (contributor.RadianSoftwares != null)
                    {
                        radianSoftware = contributor.RadianSoftwares.FirstOrDefault();
                    }

                    contributorService.SetToEnabledRadian(
                        contributor.ContributorId,
                        contributor.RadianContributorTypeId,
                        radianSoftware.Id.ToString(),
                        contributor.RadianContributorTypeId);

                    // resultJson = JsonConvert.SerializeObject(results);
                    lastZone = new GlobalLogger("RadianSendToActivateContributor", "Step 4") { Message =  
                        contributor.ContributorId + " " 
                        + contributor.RadianContributorTypeId + " " 
                        + radianSoftware.Id.ToString() + " " 
                        +contributor.RadianContributorTypeId
                    };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 5 Contributor Operations

                }
                catch (Exception ex)
                {
                    log.Error($"Error al enviar a activar contribuyente con id {contributor?.Id} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                    var failResponse = new { success = false, message = "Error al enviar a activar contribuyente a producción.", detail = ex.Message, trace = ex.StackTrace };

                    string resultJson = JsonConvert.SerializeObject(failResponse);
                    var lastZone = new GlobalLogger("RadianSendToActivateContributor", "Exception")
                    {
                        Message = resultJson + " ---------------------------------------- "
                                + ex.Message + " ---> " + ex
                    };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    return req.CreateResponse(HttpStatusCode.InternalServerError, failResponse);
                }
            }

            var fail = new { success = false, message = $"Wrong enviroment {ConfigurationManager.GetValue("Environment")}." };
            return req.CreateResponse(HttpStatusCode.BadRequest, fail);
        }

        private static async Task SendToActivateContributorToProduction(ActivateContributorRequestObject activateContributorRequestObject)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "Activate.Contributor.Event",
                    Data = JsonConvert.SerializeObject(activateContributorRequestObject),
                    EventTime = DateTime.UtcNow,
                    Subject = $"|PRIORITY:1|",
                    DataVersion = "2.0"
                }
            };
            await EventGridManager.Instance("EventGridKeyProd", "EventGridTopicEndpointProd").SendMessagesToEventGridAsync(eventsList);
        }

        class ActivationRequest
        {
            [JsonProperty(PropertyName = "contributorId")]
            public int ContributorId { get; set; }
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
