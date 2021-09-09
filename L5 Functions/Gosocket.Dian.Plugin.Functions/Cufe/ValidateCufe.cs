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

namespace Gosocket.Dian.Plugin.Functions.Cufe
{
    public static class ValidateCufe
    {
        //private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly TableLoggerManager TableLoggerManagerFACELogger = new TableLoggerManager("FACELogger");
        

        [FunctionName("ValidateCufe")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");

            var trackId = data.TrackId;

            if (trackId == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId on the query string or in the request body");

            try
            {

                var validateResponses = await ValidatorEngine.Instance.StartCufeValidationAsync(trackId);
                return req.CreateResponse(HttpStatusCode.OK, validateResponses);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var logger = new GlobalLogger($"CUFEPLGNS-{DateTime.UtcNow.ToString("yyyyMMdd")}", trackId) { Message = ex.Message, StackTrace = ex.StackTrace };
                TableLoggerManagerFACELogger.InsertOrUpdate(logger);

                var validateResponses = new List<ValidateListResponse>
                {
                    new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "CUFEPLGNS",
                        ErrorMessage = $"No se pudo validar CUFE."
                    }
                };
                return req.CreateResponse(HttpStatusCode.InternalServerError, validateResponses);
            }
        }

        public class RequestObject
        {
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
        }
    }
}
