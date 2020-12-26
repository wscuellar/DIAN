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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Radian
{
    public static class SendToActivateRadianOperation
    {

        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly TableManager globalTestSetResultTableManager = new TableManager("RadianTestSetResult");



        [FunctionName("SendToActivateRadianOperation")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            SetLogger(null, "Step STA-00", ConfigurationManager.GetValue("Environment"));

            if (ConfigurationManager.GetValue("Environment") == "Hab")
            {
                RadianContributor radianContributor = null;
                Contributor contributor = null;

                var sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");
                SetLogger(null, "Step STA-1", sqlConnectionStringProd);


                try
                {
                    var data = await req.Content.ReadAsAsync<RadianActivationRequest>();
                    if (data == null)
                        throw new Exception("Request body is empty.");

                    SetLogger(data, "Step STA-1.1", "Data");

                    if (data.ContributorId == 0)
                        throw new Exception("Please pass a contributor ud in the request body.");

                    SetLogger(null, "Step STA-2", " -- Validaciones OK-- ");

                    contributor = contributorService.Get(data.ContributorId);
                    SetLogger(null, "Step STA-2.1", contributor != null ? contributor.Id.ToString() : "no tiene");
                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                    // Step 1 Contributor Production
                    var contributorProd = contributorService.GetByCode(data.Code, sqlConnectionStringProd);
                    SetLogger(null, "Step STA-3", contributorProd != null ? contributorProd.Id.ToString() : "no tiene en prod");
                    if (contributorProd == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Prod with given code {data.Code}.");



                    //    // Step 2 Get RadianContributor
                    radianContributor = contributorService.GetRadian(data.ContributorId, data.ContributorTypeId);
                    SetLogger(null, "Step STA-4", radianContributor != null ? radianContributor.Id.ToString() : "no hay radian contributor");
                    if (radianContributor == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                    // Step 3 RadianTestSetResult
                    string key = data.SoftwareType + '|' + data.SoftwareId;
                    SetLogger(null, "Step STA-4.1", data.Code, "code123");
                    SetLogger(null, "Step STA-4.2", key, "key123");
                    var results = globalTestSetResultTableManager.Find<RadianTestSetResult>(data.Code, key);

                    SetLogger(null, "Step STA-5", results == null ? "result nullo" : "Pase " + results.Status.ToString(),"sta5-2020");

                    if (results.Status != (int)Domain.Common.TestSetStatus.Accepted || results.Deleted)
                        throw new Exception("Contribuyente no a pasado set de pruebas.");

                    SetLogger(results, "Step STA-5.1", " -- RadianSendToActivateContributor -- ");

                    // Step 4  Enable Contributor

                    SetLogger(null, "Step STA-5.2", " Radiancontributor", "Radiancontributor_01");
                    SetLogger(data, "Step STA-5.1", "data", "data_01");


                    SetLogger(null, "Step STA-6", " -- RadianSendToActivateContributor -- " +
                        radianContributor.ContributorId + " "
                        + radianContributor.RadianContributorTypeId + " "
                        + data.SoftwareId + " "
                        + data.SoftwareType
                        , "Step STA-6");


                    contributorService.SetToEnabledRadian(
                        radianContributor.ContributorId,
                        radianContributor.RadianContributorTypeId,
                        data.SoftwareId,
                        Convert.ToInt32(data.SoftwareType));


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
                        SoftwareType = data.SoftwareType,
                        Url = data.Url
                    };

                    await SendToActivateRadianContributorToProduction(activateRadianContributorRequestObject);

                    SetLogger(activateRadianContributorRequestObject, "Step STA-7", " -- SendToActivateRadianContributorToProduction -- ");

                }
                catch (Exception ex)
                {
                    log.Error($"Error al enviar a activar contribuyente con id {radianContributor?.Id} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                    var failResponse = new { success = false, message = "Error al enviar a activar contribuyente a producción.", detail = ex.Message, trace = ex.StackTrace };

                    SetLogger(failResponse, "STA-Exception", " ---------------------------------------- " + ex.Message + " ---> " + ex);

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
                    EventType = "Activate.Radian.Operation.Event", //andres proporciona este dato.
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
