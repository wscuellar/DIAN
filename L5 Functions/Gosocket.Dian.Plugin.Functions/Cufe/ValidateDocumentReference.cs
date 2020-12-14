using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Common;
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
            if (string.IsNullOrEmpty(data.EventCode))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass an EventCode in the request body");
            if (string.IsNullOrEmpty(data.DocumentTypeIdRef))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass an DocumentTypeIdRef in the request body");

            //Campos obligatorios para endoso
            if ( (Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoGarantia) 
                || (Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoProcuracion)
                || (Convert.ToInt32(data.EventCode) == (int)EventStatus.EndosoPropiedad))
            {
                if (string.IsNullOrEmpty(data.IssuerPartyCode))
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass an IssuerPartyCode in the request body");
                if (string.IsNullOrEmpty(data.IssuerPartyName))
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass an IssuerPartyName in the request body");
            }


            try
            {
                var validateResponses = ValidatorEngine.Instance.StartValidateDocumentReference(data);
                return req.CreateResponse(HttpStatusCode.OK, validateResponses);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var logger = new GlobalLogger($"VALIDATEDOCUMENTREFERENCECUFEPLGNS-{DateTime.UtcNow:yyyyMMdd}-Cufe {data.TrackId}", data.TrackId) { Message = ex.Message, StackTrace = ex.StackTrace };
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
            [JsonProperty(PropertyName = "idDocumentReference")]
            public string IdDocumentReference { get; set; }
            [JsonProperty(PropertyName = "eventCode")]
            public string EventCode { get; set; }
            [JsonProperty(PropertyName = "documentTypeIdRef")]
            public string DocumentTypeIdRef { get; set; }
            [JsonProperty(PropertyName = "issuerPartyCode")]
            public string IssuerPartyCode { get; set; }
            [JsonProperty(PropertyName = "issuerPartyName")]
            public string IssuerPartyName { get; set; }
        }
    }
}