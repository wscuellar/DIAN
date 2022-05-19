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

namespace Gosocket.Dian.Plugin.Functions.SigningTime
{
    public static class ValidateSigningTime
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");

        [FunctionName("ValidateSigningTime")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var data = await req.Content.ReadAsAsync<RequestObjectSigningTime>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");
            if (string.IsNullOrEmpty(data.EventCode))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a EventCode in the request body");           
            if (string.IsNullOrEmpty(data.DocumentTypeId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a DocumentTypeId in the request body");
            if (string.IsNullOrEmpty(data.CustomizationID))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a Customization in the request body");
            if (string.IsNullOrEmpty(data.SigningTime))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a SigningTime in the request body");

            try
            {
                var validateResponses = await ValidatorEngine.Instance.StartValidateSigningTimeAsync(data);
                return req.CreateResponse(HttpStatusCode.OK, validateResponses);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var logger = new GlobalLogger($"VALIDATESIGNINGTIMEPLGNS -{DateTime.UtcNow:yyyyMMdd}-Evento {data.DocumentTypeId}", data.TrackId) { Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(logger);
                var error = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                var validateResponses = new List<ValidateListResponse>
                {
                    new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "VALDIATESIGNINGTIMEPLGNS ",
                        ErrorMessage = $"Error en la validación de fechaa en la seccion firmas, error: {error}"
                    }
                };
                return req.CreateResponse(HttpStatusCode.InternalServerError, validateResponses);
            }
        }       
    }

    public class RequestObjectSigningTime
    {
        [JsonProperty(PropertyName = "trackId")]
        public string TrackId { get; set; }
        [JsonProperty(PropertyName = "EventCode")]
        public string EventCode { get; set; }
        [JsonProperty(PropertyName = "SigningTime")]
        public string SigningTime { get; set; }
        [JsonProperty(PropertyName = "DocumentTypeId")]
        public string DocumentTypeId { get; set; }
        [JsonProperty(PropertyName = "customizationID")]
        public string CustomizationID { get; set; }
        [JsonProperty(PropertyName = "EndDate")]
        public string EndDate { get; set; }
        [JsonProperty(PropertyName = "trackIdCude")]
        public string TrackIdcude { get; set; }

        public RequestObjectSigningTime()
        {
                
        }

    }
}
