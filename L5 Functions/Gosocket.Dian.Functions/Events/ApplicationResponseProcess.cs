using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Functions.Common;
using Gosocket.Dian.Functions.Utils;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Events
{
    public static class ApplicationResponseProcess
    {
        [FunctionName("ApplicationResponseProcess")]
        public static async Task<EventResponse> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return new EventResponse { Code = "400", Message = "Request body is empty." };

            if (string.IsNullOrEmpty(data.ResponseCode))
                return new EventResponse { Code = "400", Message = "Please pass a responseCode in the request body." };

            if (string.IsNullOrEmpty(data.TrackId))
                return new EventResponse { Code = "400", Message = "Please pass a trackId in the request body." };

            var trackId = data.TrackId;
            var responseCode = data.ResponseCode;

            if(!StringUtils.HasOnlyNumbers(responseCode))
                return new EventResponse { Code = ((int)EventValidationMessage.InvalidResponseCode).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.InvalidResponseCode) };

            string[] eventCodesImplemented =
                    {
                        ((int)EventStatus.Received).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.Rejected).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.Receipt).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.Accepted).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.AceptacionTacita).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.Avales).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.SolicitudDisponibilizacion).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.EndosoPropiedad).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.EndosoGarantia).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.EndosoProcuracion).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.InvoiceOfferedForNegotiation).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.NegotiatedInvoice).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.AnulacionLimitacionCirculacion).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.Mandato).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.TerminacionMandato).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.NotificacionPagoTotalParcial).ToString().PadLeft(3, '0'),
                    };
            //Validate response code is implemented
            if (!eventCodesImplemented.Contains(responseCode))
            {
                var message = EnumHelper.GetEnumDescription(EventValidationMessage.NotImplemented);
                message = string.Format(message, responseCode, EnumHelper.GetEnumDescription((EventStatus)int.Parse(responseCode)));
                return new EventResponse { Code = ((int)EventValidationMessage.NotImplemented).ToString(), Message = message };
            }

            TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");

            var documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            if (documentMeta == null)
                return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };

            var partitionKey = $"co|{documentMeta.EmissionDate.Day.ToString().PadLeft(2, '0')}|{documentMeta.DocumentKey.Substring(0, 2)}";

            var globalDataDocument = await CosmosDBService.Instance(documentMeta.EmissionDate).ReadDocumentAsync(documentMeta.DocumentKey, partitionKey, documentMeta.EmissionDate);

            if (globalDataDocument == null)
                return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };

            // Validate reception date
            var receptionDateValidation = Validator.ValidateReceptionDate(globalDataDocument);
            if (!receptionDateValidation.Item1)
                return receptionDateValidation.Item2;

            // Validate event
            var eventValidation = Validator.ValidateEvent(globalDataDocument, responseCode);
            if (!eventValidation.Item1)
                return eventValidation.Item2;
            else if (globalDataDocument.Events.Count == 0)
            {
                globalDataDocument.Events = new List<Event>()
                {
                    InstanceEventObject(globalDataDocument, responseCode)
                };
            }
            else
                globalDataDocument.Events.Add(InstanceEventObject(globalDataDocument, responseCode));

            // upsert document in cosmos
            var result = CosmosDBService.Instance(documentMeta.EmissionDate).UpdateDocument(globalDataDocument);
            if (result == null)
                return new EventResponse { Code = ((int)EventValidationMessage.Error).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.Error) };

            var response = new EventResponse { Code = ((int)EventValidationMessage.Success).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success) };
            return response;
        }

        private static Event InstanceEventObject(GlobalDataDocument globalDataDocument, string code)
        {
            return new Event
            {
                Date = DateTime.UtcNow,
                DocumentKey = globalDataDocument.DocumentKey,
                DateNumber = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd")),
                TimeStamp = DateTime.UtcNow,
                Code = code,
                Description = EnumHelper.GetEnumDescription((EventStatus)int.Parse(code)),
                SenderCode = globalDataDocument.SenderCode,
                SenderName = globalDataDocument.SenderName,
                ReceiverCode = globalDataDocument.ReceiverCode,
                ReceiverName = globalDataDocument.ReceiverName
            };
        }

        public class RequestObject
        {
            [JsonProperty(PropertyName = "responseCode")]
            public string ResponseCode { get; set; }
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
        }
    }
}

