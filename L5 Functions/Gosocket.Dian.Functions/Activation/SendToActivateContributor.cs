using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Activation
{
    public static class SendToActivateContributor
    {
        private static readonly TableManager globalTestSetResultTableManager = new TableManager("GlobalTestSetResult");
        private static readonly ContributorOperationsService contributorOperationService = new ContributorOperationsService();
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly SoftwareService softwareService = new SoftwareService();

        [FunctionName("SendToActivateContributor")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            if (ConfigurationManager.GetValue("Environment") == "Hab")
            {
                Contributor contributor = null;
                var activateContributorRequestObject = new ActivateContributorRequestObject();
                var sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");
                try
                {
                    var data = await req.Content.ReadAsAsync<ActivationRequest>();
                    if (data == null)
                        throw new Exception("Request body is empty."); //return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

                    if (data.ContributorId == 0)
                        throw new Exception("Please pass a contributor ud in the request body."); //return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a contributor ud in the request body");

                    contributor = contributorService.Get(data.ContributorId);
                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                    var contributorProd = contributorService.GetByCode(contributor.Code, sqlConnectionStringProd);
                    if (contributorProd == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Prod with given code {contributor.Code}.");

                    var results = globalTestSetResultTableManager.FindByPartition<GlobalTestSetResult>(contributor.Code);
                    results = results.Where(r => !r.Deleted && r.Status == (int)Domain.Common.TestSetStatus.Accepted).ToList();
                    if (!results.Any()) throw new Exception("Contribuyente no a pasado set de pruebas."); // return req.CreateResponse(HttpStatusCode.OK, a);

                    contributorService.SetToEnabled(contributor);

                    List<ContributorOperations> operationModes = contributorOperationService.GetContributorOperations(data.ContributorId);
                    foreach (var operation in operationModes.Where(o => !o.Deleted))
                    {
                        if (operation.OperationModeId == (int)Domain.Common.OperationMode.Free && operation.SoftwareId == null)
                            operation.SoftwareId = Guid.Parse(ConfigurationManager.GetValue("BillerSoftwareId"));

                        var testSetResult = globalTestSetResultTableManager.Find<GlobalTestSetResult>(contributor.Code, $"{contributor.ContributorTypeId}|{operation.SoftwareId}");
                        if (testSetResult == null || testSetResult.Status != (int)Domain.Common.TestSetStatus.Accepted) continue;

                        var software = softwareService.Get(operation.SoftwareId.Value);

                        activateContributorRequestObject.ContributorId = contributorProd.Id;
                        activateContributorRequestObject.ExchangeEmail = contributor.ExchangeEmail;
                        activateContributorRequestObject.ContributorTypeId = contributor.ContributorTypeId.Value;
                        activateContributorRequestObject.OperationModeId = operation.OperationModeId;
                        var ownSoftware = contributorService.Get(software.ContributorId);
                        activateContributorRequestObject.Software = new ActivateSoftwareContributorRequestObject
                        {
                            Id = software.Id,
                            Pin = software.Pin,
                            Name = software.Name,
                            ContributorId = software.ContributorId,
                            ContributorCode = ownSoftware.Code,
                            SoftwareDate = software.SoftwareDate,
                            SoftwareUser = software.SoftwareUser,
                            SoftwarePassword = software.SoftwarePassword,
                            Url = software.Url,
                            AcceptanceStatusSoftwareId = (int)Domain.Common.SoftwareStatus.Production
                        };

                        if (operation.ProviderId != null)
                        {
                            var provider = contributorService.Get(operation.ProviderId.Value);
                            var providerInProd = contributorService.GetByCode(provider.Code, sqlConnectionStringProd);
                            if (providerInProd != null)
                                activateContributorRequestObject.ProviderId = providerInProd.Id;
                        }

                        await SendToActivateContributorToProduction(activateContributorRequestObject);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error al enviar a activar contribuyente con id {contributor?.Id} en producci�n _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                    var failResponse = new { success = false, message = "Error al enviar a activar contribuyente a producci�n.", detail = ex.Message, trace = ex.StackTrace };
                    return req.CreateResponse(HttpStatusCode.InternalServerError, failResponse);
                }

                var response = new { success = true, message = "Contribuyente se envi� a activar a producci�n con �xito." };
                return req.CreateResponse(HttpStatusCode.OK, response);
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
