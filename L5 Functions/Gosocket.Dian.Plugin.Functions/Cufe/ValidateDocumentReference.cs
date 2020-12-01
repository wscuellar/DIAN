using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Common;
using Gosocket.Dian.Plugin.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Gosocket.Dian.Plugin.Functions.Cufe
{
    public static class ValidateDocumentReference
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");

        [FunctionName("ValidateDocumentReference")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");
            if (string.IsNullOrEmpty(data.IdDocumentReference))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass an IdDocumentReference in the request body");
          
            var trackId = data.TrackId;

            var idDocumentReference = data.IdDocumentReference;

            if (trackId == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId on the query string or in the request body");
            if (idDocumentReference == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass an IdDocumentReference on the query string or in the request body");
            
            try
            {
                var validateResponses = await ValidatorEngine.Instance.StartValidateDocumentReferenceAsync(trackId, idDocumentReference);
                return req.CreateResponse(HttpStatusCode.OK, validateResponses);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var logger = new GlobalLogger($"VALIDATEDOCUMENTREFERENCECUFEPLGNS-{DateTime.UtcNow:yyyyMMdd}-Cufe {trackId}", trackId) { Message = ex.Message, StackTrace = ex.StackTrace };
                await tableManagerGlobalLogger.InsertOrUpdateAsync(logger);
                
                var validateResponses = new List<ValidateListResponse>
                {
                    new ValidateListResponse
                    {
                        IsValid = false,
                        Mandatory = true,
                        ErrorCode = "VALIDATEDOCUMENTREFERENCECUFEPLGNS",
                        ErrorMessage = $"No se pudo validar la Sección DocumentReference - CUFE Informado"
                    }
                };
                return req.CreateResponse(HttpStatusCode.InternalServerError, validateResponses);
            }
        }

        public class RequestObject
        {
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
            [JsonProperty(PropertyName = "IdDocumentReference")]
            public string IdDocumentReference { get; set; }
        }
    }
}