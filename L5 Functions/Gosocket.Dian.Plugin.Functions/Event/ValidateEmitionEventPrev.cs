using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Common;
using Gosocket.Dian.Plugin.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Gosocket.Dian.Plugin.Functions.Event
{
    public static  class ValidateEmitionEventPrev
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");

        [FunctionName("ValidateEmitionEventPrev")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var data = await req.Content.ReadAsAsync<RequestObjectEventPrev>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");
            if (string.IsNullOrEmpty(data.EventCode))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a EventCode in the request body");
            if (string.IsNullOrEmpty(data.DocumentTypeId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a DocumentTypeId in the request body");
            if (string.IsNullOrEmpty(data.TrackIdCude))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a TrackIdCude in the request body");
            if (string.IsNullOrEmpty(data.CustomizationID))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a CustomizationID in the request body");
            if (string.IsNullOrEmpty(data.ListId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a ListId in the request body");

            try
            {
                var validateResponses = await ValidatorEngine.Instance.StartValidateEmitionEventPrevAsync(data);
                return req.CreateResponse(HttpStatusCode.OK, validateResponses);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var logger = new GlobalLogger($"VALIDATEEMITIONEVENTPLGNS-{DateTime.UtcNow:yyyyMMdd}-Evento {data.EventCode}", data.TrackId) { Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(logger);

                var validateResponses = new List<ValidateListResponse>
                {
                    new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "VALIDATEEMITIONEVENTPLGNS",
                        ErrorMessage = $"No se pudo validar los eventos previos del documento."
                    }
                };
                return req.CreateResponse(HttpStatusCode.InternalServerError, validateResponses);
            }
        }       
    }
    public class RequestObjectEventPrev
    {
        [JsonProperty(PropertyName = "trackId")]
        public string TrackId { get; set; }
        [JsonProperty(PropertyName = "eventCode")]
        public string EventCode { get; set; }
        [JsonProperty(PropertyName = "documentTypeId")]
        public string DocumentTypeId { get; set; }
        [JsonProperty(PropertyName = "trackIdCude")]
        public string TrackIdCude { get; set; }
        [JsonProperty(PropertyName = "listID")]
        public string ListId { get; set; }
        [JsonProperty(PropertyName = "customizationID")]
        public string CustomizationID { get; set; }

        public RequestObjectEventPrev() { }

    }
}
