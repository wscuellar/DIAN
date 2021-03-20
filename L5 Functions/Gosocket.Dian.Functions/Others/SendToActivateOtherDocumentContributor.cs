using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Entity.Core;
using Gosocket.Dian.Domain;


namespace Gosocket.Dian.Functions.Others
{
    public static class SendToActivateOtherDocumentContributor
    {        
        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly TableManager globalTestSetResultTableManager = new TableManager("GlobalTestSetOthersDocumentsResult");
        private static readonly TableManager globalTestSetOthersDocumentsResult = new TableManager("GlobalTestSetOthersDocumentsResult");
        private static readonly GlobalOtherDocElecOperationService globalOtherDocElecOperation = new GlobalOtherDocElecOperationService();
        


        [FunctionName("SendToActivateOtherDocumentContributor")]

        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            SetLogger(null, "Step STA-00", ConfigurationManager.GetValue("Environment"));

            //Solo en ambiente de habilitacion.
            if (ConfigurationManager.GetValue("Environment") == "Hab")
            {
                //Se obtiene la informacion para habilitar
                OtherDocumentActivationRequest data = await req.Content.ReadAsAsync<OtherDocumentActivationRequest>();
                if (data == null)
                    throw new Exception("Request body is empty.");
                SetLogger(data, "Step STA-1.1", "Data", "SEND-00");

                //Se obtiene participante otros documentos habilitacion
                OtherDocElecContributor otherDocElecContributor = contributorService.GetOtherDocElecContributor(data.ContributorId, data.ContributorTypeId);
                SetLogger(null, "Step STA-4", otherDocElecContributor != null ? otherDocElecContributor.Id.ToString() : "no hay otherDocElecContributor contributor", "SEND-01");
                if (otherDocElecContributor == null)
                    throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                try
                {
                    if (data.ContributorId == 0)
                        throw new Exception("Please pass a contributor ud in the request body.");
                    SetLogger(null, "Step STA-2", " -- Validaciones OK-- ", "SEND-02");

                    //Se busca el contribuyente en el ambiente de habilitacion.
                    Contributor contributor = contributorService.Get(data.ContributorId);
                    SetLogger(null, "Step STA-2.1", contributor != null ? contributor.Id.ToString() : "no tiene","SEND-03");
                    if (contributor == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Hab with given id {data.ContributorId}.");

                    //Se obtiene cadena de conexion de prod, para buscar la existencia del contribuyente en ese ambiente.
                    string sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");
                    SetLogger(null, "Step STA-1", sqlConnectionStringProd, "SEND-04");

                    // Se busca el contribuyente en prod
                    Contributor contributorProd = contributorService.GetByCode(data.Code, sqlConnectionStringProd);
                    SetLogger(null, "Step STA-3", contributorProd != null ? contributorProd.Id.ToString() : "no tiene en prod", "SEND-05");
                    if (contributorProd == null)
                        throw new ObjectNotFoundException($"Not found contributor in environment Prod with given code {data.Code}.");

                    // Se obtiene el set de pruebas par el cliente
                    string key = data.SoftwareType + '|' + data.SoftwareId;
                    SetLogger(null, "Step STA-4.1", data.Code,"SEND-06");
                    SetLogger(null, "Step STA-4.2", key,"SEND-07");
                    GlobalTestSetOthersDocumentsResult results = globalTestSetResultTableManager.Find<GlobalTestSetOthersDocumentsResult>(data.Code, key);
                    SetLogger(null, "Step STA-5", results == null ? "result nullo" : "Pase " + results.Status.ToString(), "SEND-08");

                    //Se valida que pase el set de pruebas.
                    if (results.Status != (int)Domain.Common.TestSetStatus.Accepted || results.Deleted)
                        throw new Exception("Contribuyente no a pasado set de pruebas.");

                    SetLogger(results, "Step STA-5.1", " -- OtherDocumentActivationRequest -- ", "SEND-09");

                    SetLogger(null, "Step STA-6", " -- OtherDocElecContributor -- " +
                                            otherDocElecContributor.ContributorId + " "
                                            + otherDocElecContributor.OtherDocElecContributorTypeId + " "
                                            + results.SoftwareId + " "
                                            + data.SoftwareType
                                            , "SEND-10");

                    //Se habilita el contribuyente en BD
                    contributorService.SetToEnabledOtherDocElecContributor(
                      otherDocElecContributor.ContributorId,
                      otherDocElecContributor.OtherDocElecContributorTypeId,
                      results.SoftwareId,
                      Convert.ToInt32(data.SoftwareType));

                    //Se habilita el contribuyente en la table Storage
                    globalOtherDocElecOperation.EnableParticipantOtherDocument(data.Code, results.SoftwareId, otherDocElecContributor);

                    //Obtiene informacion GlobalTestSetOthersDocumentsResult Hab
                    GlobalTestSetOthersDocumentsResult retrieveGlobalTestSetOthersDocumentsResult = globalTestSetOthersDocumentsResult.FindOthersDocumentsResult<GlobalTestSetOthersDocumentsResult>(data.Code, results.SoftwareId);

                    //Se recolecta la informacion para la creacion en prod.
                    OtherDocumentActivateContributorRequestObject activateOtherDocumentContributorRequestObject = new OtherDocumentActivateContributorRequestObject()
                    {
                        Code = data.Code,
                        ContributorId = otherDocElecContributor.ContributorId,
                        OtherDocContributorTypeId = otherDocElecContributor.OtherDocElecContributorTypeId,
                        CreatedBy = otherDocElecContributor.CreatedBy,
                        OtherDocOperationModeId = (int)(data.SoftwareType == "1" ? Domain.Common.RadianOperationMode.Direct : Domain.Common.RadianOperationMode.Indirect),
                        SoftwarePassword = data.SoftwarePassword,
                        SoftwareUser = data.SoftwareUser,
                        Pin = data.Pin,
                        SoftwareName = data.SoftwareName,
                        SoftwareId = results.SoftwareId,
                        SoftwareType = data.SoftwareType,
                        Url = data.Url,
                        ElectronicDocumentId = otherDocElecContributor.ElectronicDocumentId,
                        SoftwareProvider = data.SoftwareId,
                        ProviderId = results.ProviderId,
                        TestSetOthersDocumentsResultObj = retrieveGlobalTestSetOthersDocumentsResult
                    };

                    //Se envia para la creacion en prod.
                    await SendToActivateOtherDocumentContributorToProduction(activateOtherDocumentContributorRequestObject);

                    SetLogger(activateOtherDocumentContributorRequestObject, "Step STA-7", " -- SendToActivateOtherDocumentContributorToProduction -- ", "SEND-11");


                }
                catch (Exception ex)
                {
                    log.Error($"Error al enviar a activar contribuyente con id {otherDocElecContributor?.Id} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                    var failResponse = new { success = false, message = "Error al enviar a activar contribuyente a producción.", detail = ex.Message, trace = ex.StackTrace };

                    SetLogger(failResponse, "STA-Exception", " ---------------------------------------- " + ex.Message + " ---> " + ex);

                    return req.CreateResponse(HttpStatusCode.InternalServerError, failResponse);
                }
            }


            return null;
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
                resultJson = string.Empty;

            GlobalLogger lastZone;
            if (string.IsNullOrEmpty(keyUnique))
                lastZone = new GlobalLogger("202015", "202015") { Message = Step + " --> " + resultJson + " -- Msg --" + msg };
            else
                lastZone = new GlobalLogger(keyUnique, keyUnique) { Message = Step + " --> " + resultJson + " -- Msg --" + msg };

            TableManagerGlobalLogger.InsertOrUpdate(lastZone);
        }

        private static async Task SendToActivateOtherDocumentContributorToProduction(OtherDocumentActivateContributorRequestObject activateContributorRequestObject)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "Activate.OtherDocument.Operation.Event",
                    Data = JsonConvert.SerializeObject(activateContributorRequestObject),
                    EventTime = DateTime.UtcNow,
                    Subject = $"|PRIORITY:1|",
                    DataVersion = "2.0"
                }
            };
            await EventGridManager.Instance("EventGridKeyProd", "EventGridTopicEndpointProd").SendMessagesToEventGridAsync(eventsList);
        }


        class OtherDocumentActivationRequest
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

        class OtherDocumentActivateContributorRequestObject
        {
            [JsonProperty(PropertyName = "code")]
            public string Code { get; set; }
            [JsonProperty(PropertyName = "contributorId")]
            public int ContributorId { get; set; }

            [JsonProperty(PropertyName = "otherDocContributorTypeId")]
            public int OtherDocContributorTypeId { get; set; }

            [JsonProperty(PropertyName = "otherDocOperationModeId")]
            public int OtherDocOperationModeId { get; set; }

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

            [JsonProperty(PropertyName = "electronicDocumentId")]
            public int ElectronicDocumentId { get; set; }

            [JsonProperty(PropertyName = "softwareProvider")]
            public string SoftwareProvider { get; set; }

            [JsonProperty(PropertyName = "providerId")]
            public int ProviderId { get; set; }

            [JsonProperty(PropertyName = "testSetOthersDocumentsResult")]
            public GlobalTestSetOthersDocumentsResult TestSetOthersDocumentsResultObj { get; set; }
        }
    }
}
