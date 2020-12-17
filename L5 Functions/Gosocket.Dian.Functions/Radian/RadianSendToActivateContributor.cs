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
                RadianContributor radianContributor = null;
                Contributor contributor = null;
                //var activateContributorRequestObject = new ActivateContributorRequestObject();
                var sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");

                try
                {
                    var data = await req.Content.ReadAsAsync<RadianActivationRequest>();
                    if (data == null)
                        throw new Exception("Request body is empty.");

                    if (data.ContributorId == 0)
                        throw new Exception("Please pass a contributor ud in the request body.");


                    contributor = contributorService.Get(data.ContributorId);
                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                    // Step 1 Contributor Production
                    var contributorProd = contributorService.GetByCode(data.Code, sqlConnectionStringProd);
                    if (contributorProd == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Prod with given code {data.Code}.");

                    string resultJson = JsonConvert.SerializeObject(contributorProd);
                    var lastZone = new GlobalLogger("RadianSendToActivateContributor", "Step 1") { Message = resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 2 Get RadianContributor
                    radianContributor = contributorService.GetRadian(data.ContributorId, data.ContributorTypeId);
                    if (radianContributor == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                    resultJson = JsonConvert.SerializeObject(radianContributor);
                    lastZone = new GlobalLogger("RadianSendToActivateContributor", "Step 2") { Message = resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);


                    // Step 3 RadianTestSetResult
                    string key = data.SoftwareType + '|' + data.SoftwareId;
                    var results = globalTestSetResultTableManager.Find<RadianTestSetResult>(data.Code, key);
                    if(results.Status != (int)Domain.Common.TestSetStatus.Accepted || !results.Deleted)
                        throw new Exception("Contribuyente no a pasado set de pruebas.");
                    
                    resultJson = JsonConvert.SerializeObject(results);
                    lastZone = new GlobalLogger("RadianSendToActivateContributor", "Step 3") { Message = resultJson };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 4  Enable Contributor
                    contributorService.SetToEnabledRadian(
                        radianContributor.ContributorId,
                        radianContributor.RadianContributorTypeId,
                        data.SoftwareId,
                        Convert.ToInt32( data.SoftwareType));

                    // resultJson = JsonConvert.SerializeObject(results);
                    lastZone = new GlobalLogger("RadianSendToActivateContributor", "Step 4") { Message =  
                        radianContributor.ContributorId + " " 
                        + radianContributor.RadianContributorTypeId + " " 
                        + data.SoftwareId + " "
                        + data.SoftwareType
                    };
                    TableManagerGlobalLogger.InsertOrUpdate(lastZone);

                    // Step 5 Contributor Operations
                    RadianaActivateContributorRequestObject activateRadianContributorRequestObject = new RadianaActivateContributorRequestObject()
                    {
                        Code = data.Code,
                        ContributorId = radianContributor.ContributorId,
                        RadianContributorTypeId = radianContributor.RadianContributorTypeId,
                        CreatedBy = radianContributor.CreatedBy,
                        RadianOperationModeId = (int)(data.SoftwareType == "1" ? Domain.Common.RadianOperationMode.Direct : Domain.Common.RadianOperationMode.Indirect),
                        SoftwarePassword = data.SoftwarePassword,
                        SoftwareUser = data.SoftwareUser,
                        Pin = data.Pin,
                        SoftwareName = data.SoftwareName,
                        SoftwareId = data.SoftwareId,
                        SoftwareType=data.SoftwareType,
                        Url = data.Url
                    };

                    await SendToActivateRadianContributorToProduction(activateRadianContributorRequestObject);

                }
                catch (Exception ex)
                {
                    log.Error($"Error al enviar a activar contribuyente con id {radianContributor?.Id} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
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

        private static async Task SendToActivateRadianContributorToProduction(RadianaActivateContributorRequestObject activateContributorRequestObject)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "Activate.RadianContributor.Event", //andres proporciona este dato.
                    Data = JsonConvert.SerializeObject(activateContributorRequestObject),
                    EventTime = DateTime.UtcNow,
                    Subject = $"|PRIORITY:1|",
                    DataVersion = "2.0"
                }
            };
            await EventGridManager.Instance("EventGridKeyProd", "EventGridTopicEndpointProd").SendMessagesToEventGridAsync(eventsList);
        }


        class RadianActivationRequest
        {

            [JsonProperty(PropertyName = "code")]
            public string Code { get; set; }

            [JsonProperty(PropertyName = "contributorId")]
            public int ContributorId { get; set; }

            [JsonProperty(PropertyName = "contributorTypeId")]
            public int ContributorTypeId { get; set; }

            [JsonProperty(PropertyName = "softwareId")]
            public string SoftwareId { get; set; }

            [JsonProperty(PropertyName = "softwareType")]
            public string SoftwareType { get; set; }

            [JsonProperty(PropertyName = "softwareUser")]
            public string SoftwareUser { get; set; }

            [JsonProperty(PropertyName = "softwarePassword")]
            public string SoftwarePassword { get; set; }

            [JsonProperty(PropertyName = "pin")]
            public string Pin { get; set; }

            [JsonProperty(PropertyName = "softwareName")]
            public string SoftwareName { get; set; }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

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
            public string  SoftwareId { get; set; }

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

        

    }
}
