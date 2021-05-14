using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Common;
using Gosocket.Dian.Plugin.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Plugin.Functions.ValidateParty
{
    public static class ValidateParty
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");

        [FunctionName("ValidateParty")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObjectParty>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");

            if (string.IsNullOrEmpty(data.ResponseCode))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a ResponseCode in the request body");

            //Valida eventos diferentes a Endosos en blanco, dado que evento no informa el emisor documento AR
            if (!((data.ResponseCode == "038" || data.ResponseCode == "037" || data.ResponseCode == "039") && data.ListId =="2"))
            {               
                if (string.IsNullOrEmpty(data.SenderParty))
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a SenderParty in the request body");

            }

            if (string.IsNullOrEmpty(data.ReceiverParty))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a ReceiverParty in the request body");

            if (string.IsNullOrEmpty(data.CustomizationID))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a CustomizationID in the request body");

            if (string.IsNullOrEmpty(data.TrackIdCude))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackIdCude in the request body");

            if (string.IsNullOrEmpty(data.ListId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a ListID in the request body");

            try
            {
                var validateResponses = await ValidatorEngine.Instance.StartValidateParty(data);
                return req.CreateResponse(HttpStatusCode.OK, validateResponses);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var logger = new GlobalLogger($"VALIDATEPARTYPLGNS-{DateTime.UtcNow.ToString("yyyyMMdd")}", data.TrackId) { Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(logger);

                var validateResponses = new List<ValidateListResponse>
                {
                    new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "VALIDATEPARTYPLGNS",
                        ErrorMessage = $"No se pudo validar referencia SenderParty / ReceiverParty."
                    }
                };
                return req.CreateResponse(HttpStatusCode.InternalServerError, validateResponses);
            }
        }
    }

    public class RequestObjectParty
    {
        [JsonProperty(PropertyName = "trackId")]
        public string TrackId { get; set; }
        [JsonProperty(PropertyName = "SenderParty")]
        public string SenderParty { get; set; }
        [JsonProperty(PropertyName = "ReceiverParty")]
        public string ReceiverParty { get; set; }
        [JsonProperty(PropertyName = "ResponseCode")]
        public string ResponseCode { get; set; }
        [JsonProperty(PropertyName = "CustomizationID")]
        public string CustomizationID { get; set; }
        [JsonProperty(PropertyName = "trackIdCude")]
        public string TrackIdCude { get; set; }
        [JsonProperty(PropertyName = "ListID")]
        public string ListId { get; set; }
        [JsonProperty(PropertyName = "SendTestSet")]
        public bool SendTestSet { get; set; }

        public RequestObjectParty()
        {

        }
    }
}
