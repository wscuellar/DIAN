﻿using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Plugin.Functions.Common;
using Gosocket.Dian.Plugin.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Plugin.Functions.EventApproveCufe
{
    public static class EventApproveCufe
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");

        [FunctionName("EventApproveCufe")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<EventApproveCufeObjectParty>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");

            if (string.IsNullOrEmpty(data.ResponseCode))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a ResponseCode in the request body");

            if (string.IsNullOrEmpty(data.DocumentTypeId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a CustomizationID in the request body");

            try
            {
                var validateResponses = await ValidatorEngine.Instance.StartEventApproveCufe(data);
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
                        ErrorMessage = $"No se pudo validar referencia."
                    }
                };
                return req.CreateResponse(HttpStatusCode.InternalServerError, validateResponses);
            }
        }

        public class EventApproveCufeObjectParty
        {
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
            [JsonProperty(PropertyName = "ResponseCode")]
            public string ResponseCode { get; set; }
            [JsonProperty(PropertyName = "DocumentTypeId")]
            public string DocumentTypeId { get; set; }
        }
    }
}