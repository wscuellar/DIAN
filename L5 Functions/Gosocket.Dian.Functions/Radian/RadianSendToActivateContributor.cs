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
using System.Globalization;
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
        private static readonly GlobalRadianOperationService globalRadianOperationService = new GlobalRadianOperationService();

        [FunctionName("SendToActivateRadianOperation")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            SetLogger(null, "Step STA-00", ConfigurationManager.GetValue("Environment"));

            //Solo en ambiente de habilitacion.
            if (ConfigurationManager.GetValue("Environment") == "Hab")
            {
                //Se obtiene la informacion para habilitar
                RadianActivationRequest data = await req.Content.ReadAsAsync<RadianActivationRequest>();
                if (data == null)
                    throw new Exception("Request body is empty.");
                SetLogger(data, "Step STA-1.1 " + data.TestSetId, "Data");

                var start = DateTime.UtcNow;
                var startSendToActivateRadian = new GlobalLogger(data.TestSetId, "1 Start SendToActivateRadianOperation")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Start SendToActivateRadianOperation"
                };
                await TableManagerGlobalLogger.InsertOrUpdateAsync(startSendToActivateRadian);


                //Se obtiene el participante radian para habilitacion.
                RadianContributor radianContributor = contributorService.GetRadian(data.ContributorId, data.ContributorTypeId);
                SetLogger(null, "Step STA-4", radianContributor != null ? radianContributor.Id.ToString() : "no hay radian contributor");
                if (radianContributor == null)
                    throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                try
                {
                    start = DateTime.UtcNow;
                    var startSendContributorId = new GlobalLogger(data.TestSetId, "2 sendContributorId")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "startSendContributorId ContributorId: " + data.ContributorId
                    };
                    await TableManagerGlobalLogger.InsertOrUpdateAsync(startSendContributorId);

                    if (data.ContributorId == 0)
                        throw new Exception("Please pass a contributor ud in the request body.");
                    SetLogger(null, "Step STA-2", " -- Validaciones OK-- ");

                    //Se busca el contribuyente en el ambiente de habilitacion.
                    Contributor contributor = contributorService.Get(data.ContributorId);
                    SetLogger(null, "Step STA-2.1", contributor != null ? contributor.Id.ToString() : "no tiene");
                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                    //Se obtiene cadena de conexion de prod, para buscar la existencia del contribuyente en ese ambiente.
                    string sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");
                    SetLogger(null, "Step STA-1", sqlConnectionStringProd);

                    // Se busca el contribuyente en prod
                    Contributor contributorProd = contributorService.GetByCode(data.Code, sqlConnectionStringProd);
                    SetLogger(null, "Step STA-3", contributorProd != null ? contributorProd.Id.ToString() : "no tiene en prod");
                    if (contributorProd == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Prod with given code {data.Code}.");

                    // Se obtiene el set de pruebas par el cliente
                    string key = data.ContributorTypeId.ToString() + '|' + data.SoftwareId;
                    SetLogger(null, "Step STA-4.1", data.Code, "code123");
                    SetLogger(null, "Step STA-4.2", key, "key123");
                    RadianTestSetResult results = globalTestSetResultTableManager.Find<RadianTestSetResult>(data.Code, key);
                    SetLogger(null, "Step STA-5", results == null ? "result nullo" : "Pase " + results.Status.ToString(), "sta5-2020");

                    //Se valida que pase el set de pruebas.
                    if (results.Status != (int)Domain.Common.TestSetStatus.Accepted || results.Deleted)
                        throw new Exception("Contribuyente no a pasado set de pruebas.");

                    SetLogger(results, "Step STA-5.1", " -- RadianSendToActivateContributor -- ");
                    SetLogger(null, "Step STA-6", " -- RadianSendToActivateContributor -- " +
                                            radianContributor.ContributorId + " "
                                            + radianContributor.RadianContributorTypeId + " "
                                            + data.SoftwareId + " "
                                            + data.SoftwareType
                                            , "Step STA-6");

                    start = DateTime.UtcNow;
                    var startRadianContributor = new GlobalLogger(data.TestSetId, "3 startRadianContributor")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "startRadianContributor ContributorId:" + data.ContributorId +
                        " RadianContributorTypeId: " + radianContributor.RadianContributorTypeId +
                        " SoftwareId: " + data.SoftwareId +
                        " SoftwareType: " + data.SoftwareType
                    };
                    await TableManagerGlobalLogger.InsertOrUpdateAsync(startRadianContributor);

                    //Se habilita el contribuyente en BD
                    contributorService.SetToEnabledRadian(
                        radianContributor.ContributorId,
                        radianContributor.RadianContributorTypeId,
                        data.SoftwareId,
                        Convert.ToInt32(data.SoftwareType));

                    //Se habilita el contribuyente en la table Storage
                    globalRadianOperationService.EnableParticipantRadian(data.Code, data.SoftwareId, radianContributor);
                    
                    //Se recolecta la informacion para la creacion en prod.
                    RadianaActivateContributorRequestObject activateRadianContributorRequestObject = new RadianaActivateContributorRequestObject()
                    {
                        Code = data.Code,
                        ContributorId = contributorProd.Id,
                        RadianContributorTypeId = radianContributor.RadianContributorTypeId,
                        CreatedBy = radianContributor.CreatedBy,
                        RadianOperationModeId = (int)(data.SoftwareType == "1" ? Domain.Common.RadianOperationMode.Direct : Domain.Common.RadianOperationMode.Indirect),
                        SoftwarePassword = data.SoftwarePassword,
                        SoftwareUser = data.SoftwareUser,
                        Pin = data.Pin,
                        SoftwareName = data.SoftwareName,
                        SoftwareId = data.SoftwareId,
                        SoftwareType = data.SoftwareType,
                        Url = data.Url,
                        RadianTestSetObj = results

                    };
                    //Se envia para la creacion en prod.
                    await SendToActivateRadianContributorToProduction(activateRadianContributorRequestObject);
                    SetLogger(activateRadianContributorRequestObject, "Step STA-7", " -- SendToActivateRadianContributorToProduction -- ", "STA-7");

                    start = DateTime.UtcNow;
                    var finishSendContributorId = new GlobalLogger(data.TestSetId, "4 finishSendContributorId")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "finish SendToActivateRadianOperation"
                    };
                    await TableManagerGlobalLogger.InsertOrUpdateAsync(finishSendContributorId);

                }
                catch (Exception ex)
                {
                    log.Error($"Error al enviar a activar contribuyente con id {radianContributor?.Id} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                    var failResponse = new { success = false, message = "Error al enviar a activar contribuyente a producción.", detail = ex.Message, trace = ex.StackTrace };

                    SetLogger(failResponse, "STA-Exception", " ---------------------------------------- " + ex.Message + " ---> " + ex);

                    return req.CreateResponse(HttpStatusCode.InternalServerError, failResponse);
                }


                var response = new { success = true, message = "Contribuyente RADIAN se envió a activar a producción con éxito." };
                return req.CreateResponse(HttpStatusCode.OK, response);

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

            [JsonProperty(PropertyName = "testSetId")]
            public string TestSetId { get; set; }

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
