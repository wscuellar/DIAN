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

namespace Gosocket.Dian.Plugin.Functions.ValidateReferenceAttorney
{
    public static class ValidateReferenceAttorney
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");

        [FunctionName("ValidateReferenceAttorney")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObjectReferenceAttorney>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");

            try
            {
                var validateResponses = await ValidatorEngine.Instance.StartValidateReferenceAttorney(data);
                return req.CreateResponse(HttpStatusCode.OK, validateResponses);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var logger = new GlobalLogger($"VALIDATEREFERENCEATTORNEYPLGNS-{DateTime.UtcNow.ToString("yyyyMMdd")}", data.TrackId) { Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(logger);

                var validateResponses = new List<ValidateListResponse>
                {
                    new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "VALIDATEREFERENCEATTORNEYPLGNS",
                        ErrorMessage = $"No se pudo obtener lista de documentos referenciados."
                    }
                };
                return req.CreateResponse(HttpStatusCode.InternalServerError, validateResponses);
            }

        }
    }
    public class RequestObjectReferenceAttorney
    {
        [JsonProperty(PropertyName = "trackId")]        
        public string TrackId { get; set; }
    }

}
