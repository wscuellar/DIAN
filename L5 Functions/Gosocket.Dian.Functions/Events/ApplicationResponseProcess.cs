using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
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
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Events
{
    public static class ApplicationResponseProcess
    {
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager TableManagerGlobalDocReferenceAttorney = new TableManager("GlobalDocReferenceAttorney");
        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");

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

            if(!string.IsNullOrWhiteSpace(data.ListId) && data.ListId == "3")
            {
                data.TrackId = "01";
            }
            else
            {
                if (string.IsNullOrEmpty(data.TrackId))
                    return new EventResponse { Code = "400", Message = "Please pass a trackId in the request body." };                
            }

            if (string.IsNullOrEmpty(data.TrackIdCude))
                return new EventResponse { Code = "400", Message = "Please pass a trackIdTrackIdCude in the request body." };

            var trackId = data.TrackId;
            var responseCode = data.ResponseCode;
            var trackIdCude = data.TrackIdCude;
            var start = DateTime.UtcNow;

            var startBatch = new GlobalLogger(trackIdCude, "1 Start ApplicationResponseProcess")
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "Start ApplicationResponseProcess: trackIdCude " + trackIdCude + " responseCode " + responseCode + " trackId " + trackId + " listId " + data.ListId
            };
            await TableManagerGlobalLogger.InsertOrUpdateAsync(startBatch);

            if (!StringUtils.HasOnlyNumbers(responseCode))
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
                        ((int)EventStatus.ValInfoPago).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.EndorsementWithEffectOrdinaryAssignment).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.Objection).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.TransferEconomicRights).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.NotificationDebtorOfTransferEconomicRights).ToString().PadLeft(3, '0'),
                        ((int)EventStatus.PaymentOfTransferEconomicRights).ToString().PadLeft(3, '0'),

                    };
            //Validate response code is implemented
            if (!eventCodesImplemented.Contains(responseCode))
            {
                var message = EnumHelper.GetEnumDescription(EventValidationMessage.NotImplemented);
                message = string.Format(message, responseCode, EnumHelper.GetEnumDescription((EventStatus)int.Parse(responseCode)));
                return new EventResponse { Code = ((int)EventValidationMessage.NotImplemented).ToString(), Message = message };
            }           
           
            //Obtiene informacion del CUDE
            var documentMetaCUDE = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackIdCude, trackIdCude);
            if (documentMetaCUDE == null)
                return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };


            //Obtiene informacion del CUFE Anulacion Endoso, Terminacion Limitacion de Circulacion
            if (Convert.ToInt32(responseCode) == (int)EventStatus.InvoiceOfferedForNegotiation ||
                Convert.ToInt32(responseCode) == (int)EventStatus.AnulacionLimitacionCirculacion)
            {
                //Obtiene informacion del CUFE
                var documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                if (documentMeta == null)
                    return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };


                var documentMetaReferenced = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(documentMeta.DocumentReferencedKey, documentMeta.DocumentReferencedKey);
                if (documentMetaReferenced == null)
                    return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };

                var partitionKey = $"co|{documentMetaReferenced.EmissionDate.Day.ToString().PadLeft(2, '0')}|{documentMetaReferenced.DocumentKey.Substring(0, 2)}";

                var globalDataDocument = await CosmosDBService.Instance(documentMetaReferenced.EmissionDate).ReadDocumentAsync(documentMetaReferenced.DocumentKey, partitionKey, documentMetaReferenced.EmissionDate);
                if (globalDataDocument == null)
                    return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };

                // Validate event
                var eventValidation = Validator.ValidateEvent(globalDataDocument, responseCode);
                if (!eventValidation.Item1)
                    return eventValidation.Item2;
                else if (globalDataDocument.Events.Count == 0)
                {
                    globalDataDocument.Events = new List<Event>()
                {
                    InstanceEventObject(documentMetaCUDE, responseCode)
                };
                }
                else
                    globalDataDocument.Events.Add(InstanceEventObject(documentMetaCUDE, responseCode));

                // upsert document in cosmos
                var result = CosmosDBService.Instance(documentMetaReferenced.EmissionDate).UpdateDocument(globalDataDocument);
                if (result == null)
                    return new EventResponse { Code = ((int)EventValidationMessage.Error).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.Error) };
            }
            else if (Convert.ToInt32(responseCode) == (int)EventStatus.Mandato && data.ListId != "3")
            {
                List<GlobalDocReferenceAttorney> listAttorney = TableManagerGlobalDocReferenceAttorney.FindAll<GlobalDocReferenceAttorney>(trackIdCude).ToList();
                if (listAttorney != null)
                {
                    foreach (var documentAttorney in listAttorney)
                    {
                        var documentMetaAttorney = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(documentAttorney.RowKey, documentAttorney.RowKey);
                        if (documentMetaAttorney == null)
                            return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };

                        var partitionKey = $"co|{documentMetaAttorney.EmissionDate.Day.ToString().PadLeft(2, '0')}|{documentMetaAttorney.DocumentKey.Substring(0, 2)}";

                        var globalDataDocument = await CosmosDBService.Instance(documentMetaAttorney.EmissionDate).ReadDocumentAsync(documentMetaAttorney.DocumentKey, partitionKey, documentMetaAttorney.EmissionDate);
                        if (globalDataDocument == null)
                            return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };

                        // Validate event
                        var eventValidation = Validator.ValidateEvent(globalDataDocument, responseCode);
                        if (!eventValidation.Item1)
                            return eventValidation.Item2;
                        else if (globalDataDocument.Events.Count == 0)
                        {
                            globalDataDocument.Events = new List<Event>()
                            {
                                InstanceEventObject(documentMetaCUDE, responseCode)
                            };
                        }
                        else
                            globalDataDocument.Events.Add(InstanceEventObject(documentMetaCUDE, responseCode));

                        // upsert document in cosmos
                        var result = CosmosDBService.Instance(documentMetaAttorney.EmissionDate).UpdateDocument(globalDataDocument);
                        if (result == null)
                            return new EventResponse { Code = ((int)EventValidationMessage.Error).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.Error) };

                    }
                }
            }
            else if (data.TrackId != "01")
            {
                //Obtiene informacion del CUFE
                var documentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                if (documentMeta == null)
                    return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };

                var partitionKey = $"co|{documentMeta.EmissionDate.Day.ToString().PadLeft(2, '0')}|{documentMeta.DocumentKey.Substring(0, 2)}";

                var globalDataDocument = await CosmosDBService.Instance(documentMeta.EmissionDate).ReadDocumentAsync(documentMeta.DocumentKey, partitionKey, documentMeta.EmissionDate);
                if (globalDataDocument == null)
                    return new EventResponse { Code = ((int)EventValidationMessage.NotFound).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.NotFound) };

                // Validate event
                if (string.IsNullOrWhiteSpace(documentMetaCUDE.TestSetId))
                {
                    var eventValidation = Validator.ValidateEvent(globalDataDocument, responseCode);
                    if (!eventValidation.Item1)
                        return eventValidation.Item2;
                }
                
                if (globalDataDocument.Events.Count == 0)
                {
                    globalDataDocument.Events = new List<Event>()
                    {
                         InstanceEventObject(documentMetaCUDE, responseCode)
                    };
                }
                else
                    globalDataDocument.Events.Add(InstanceEventObject(documentMetaCUDE, responseCode));
                
               
                // upsert document in cosmos
                var result = CosmosDBService.Instance(documentMeta.EmissionDate).UpdateDocument(globalDataDocument);
                if (result == null)
                    return new EventResponse { Code = ((int)EventValidationMessage.Error).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.Error) };

            }

            var finishtBatch = new GlobalLogger(trackIdCude, "1 Finish ApplicationResponseProcess")
            {
                Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                Action = "Finish ApplicationResponseProcess"
            };
            await TableManagerGlobalLogger.InsertOrUpdateAsync(finishtBatch);

            var response = new EventResponse { Code = ((int)EventValidationMessage.Success).ToString(), Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success) };
            return response;
        }

        private static Event InstanceEventObject(GlobalDocValidatorDocumentMeta globalDataDocumentCude, string code)
        {
            return new Event
            {
                Date = DateTime.UtcNow,
                DocumentKey = globalDataDocumentCude.DocumentKey,
                DateNumber = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd")),
                TimeStamp = DateTime.UtcNow,
                Code = code,
                CustomizationID = globalDataDocumentCude.CustomizationID,
                Description = EnumHelper.GetEnumDescription((EventStatus)int.Parse(code)),
                SenderCode = globalDataDocumentCude.SenderCode,
                SenderName = globalDataDocumentCude.SenderName,
                ReceiverCode = globalDataDocumentCude.ReceiverCode,
                ReceiverName = globalDataDocumentCude.ReceiverName,
                CancelElectronicEvent = globalDataDocumentCude.CancelElectronicEvent,
                SendTestSet = globalDataDocumentCude.TestSetId
            };
        }

        public class RequestObject //listId
        {
            [JsonProperty(PropertyName = "responseCode")]
            public string ResponseCode { get; set; }
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
            [JsonProperty(PropertyName = "trackIdCude")]
            public string TrackIdCude { get; set; }
            [JsonProperty(PropertyName = "listId")]
            public string ListId { get; set; }

        }
    }
}

