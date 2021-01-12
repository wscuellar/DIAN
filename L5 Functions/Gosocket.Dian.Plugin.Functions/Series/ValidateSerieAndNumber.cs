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

namespace Gosocket.Dian.Plugin.Functions.Series
{
    public static class ValidateSerieAndNumber
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");

        [FunctionName("ValidateSerieAndNumber")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.Number))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a Number in the request body");
            if (string.IsNullOrEmpty(data.DocumentTypeId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a DocumentTypeId in the request body");
            if (string.IsNullOrEmpty(data.ProviderCode))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a ProviderCode in the request body");

            try
            {
                var validateResponses = ValidatorEngine.Instance.StartValidateSerieAndNumberAsync(data);
                return req.CreateResponse(HttpStatusCode.OK, validateResponses);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var logger = new GlobalLogger($"VALIDATESERIEPLGNS-{DateTime.UtcNow:yyyyMMdd}", data.Number) { Message = ex.Message, StackTrace = ex.StackTrace };
                tableManagerGlobalLogger.InsertOrUpdate(logger);

                var validateResponses = new List<ValidateListResponse>
                {
                    new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "VALIDATESERIEPLGNS",
                        ErrorMessage = $"No se pudo validar serie del ApplicationResponse con el Nit del emisor."
                    }
                };
                return req.CreateResponse(HttpStatusCode.InternalServerError, validateResponses);
            }
        }

        public class RequestObject
        {            
            [JsonProperty(PropertyName = "number")]
            public string Number { get; set; }
            [JsonProperty(PropertyName = "documentTypeId")]
            public string DocumentTypeId { get; set; }     
            [JsonProperty(PropertyName = "providerCode")]
            public string ProviderCode { get; set; }
        }
    }
}
