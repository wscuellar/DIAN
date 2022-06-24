using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Documents
{
    public static class CheckDocumentValidations
    {
        private static readonly TableManager documentValidatorTableManager = new TableManager("GlobalDocValidatorDocument");
        private static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager documentValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");
        private static readonly TableManager documentReferenceTableManager = new TableManager("GlobalDocReference");

        // Set queue name
        private const string queueName = "global-check-document-validation-input%Slot%";

        [FunctionName("CheckDocumentValidations")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            var trackId = string.Empty;
            try
            {
                var data = string.Empty;
                try
                {
                    var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                    data = eventGridEvent.Data.ToString();
                }
                catch
                {
                    data = myQueueItem;
                }

                var obj = JsonConvert.DeserializeObject<RequestObject>(data);
                trackId = obj.TrackId;
                var globalDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                if (globalDocumentMeta == null)
                    throw new Exception("globalDocumentMeta not found.");

                var documentValidationTrackings = documentValidatorTrackingTableManager.FindByPartition<GlobalDocValidatorTracking>(trackId);

                await SendDocumentToCosmosDB(globalDocumentMeta, obj.Category, obj.ProcessTimeDocument, trackId, documentValidationTrackings, obj.SoftwareId);
            }
            catch (Exception ex)
            {
                log.Error($"Error al procesar validación del documento con trackId {trackId}. Ex: {ex.StackTrace}");
                throw;
            }
        }

        public static async Task SendDocumentToCosmosDB(GlobalDocValidatorDocumentMeta globalDocValidatordocumentMeta, string category, double processTimeDocument, string trackId, IEnumerable<GlobalDocValidatorTracking> globalDocValidatorList, string softwareId = null)
        {
            var globalDataDocument = new GlobalDataDocument
            {
                Identifier = globalDocValidatordocumentMeta.Identifier,
                DocumentKey = globalDocValidatordocumentMeta.DocumentKey,
                EmissionDate = globalDocValidatordocumentMeta.EmissionDate,
                EmissionDateNumber = int.Parse(globalDocValidatordocumentMeta.EmissionDate.ToString("yyyyMMdd")),
                GenerationTimeStamp = DateTime.UtcNow,
                ReceptionTimeStamp = DateTime.UtcNow,
                GlobalDocumentId = globalDocValidatordocumentMeta.DocumentKey,
                GlobalNumberRangeId = new Guid(),
                DocumentTypeId = globalDocValidatordocumentMeta.DocumentTypeId,
                DocumentTypeName = globalDocValidatordocumentMeta.DocumentTypeName,
                Number = globalDocValidatordocumentMeta.Number,
                Serie = globalDocValidatordocumentMeta.Serie,
                SerieAndNumber = globalDocValidatordocumentMeta.SerieAndNumber,
                SenderCode = globalDocValidatordocumentMeta.SenderCode,
                SenderName = globalDocValidatordocumentMeta.SenderName,
                ReceiverCode = globalDocValidatordocumentMeta.ReceiverCode,
                ReceiverName = globalDocValidatordocumentMeta.ReceiverName,
                TotalAmount = globalDocValidatordocumentMeta.TotalAmount,
                TaxAmountIva = globalDocValidatordocumentMeta.TaxAmountIva,
                TaxAmountIpc = globalDocValidatordocumentMeta.TaxAmountIpc,
                TaxAmountIca = globalDocValidatordocumentMeta.TaxAmountIca,
                SoftwareId = globalDocValidatordocumentMeta.SoftwareId,
                CustomizationID = globalDocValidatordocumentMeta.CustomizationID,
                DocumentCurrencyCode=globalDocValidatordocumentMeta.DocumentCurrencyCode
                
            };

            globalDataDocument.TaxesDetail = new TaxesDetail
            {
                TaxAmountIva5Percent = globalDocValidatordocumentMeta.TaxAmountIva5Percent,
                TaxAmountIva14Percent = globalDocValidatordocumentMeta.TaxAmountIva14Percent,
                TaxAmountIva16Percent = globalDocValidatordocumentMeta.TaxAmountIva16Percent,
                TaxAmountIva19Percent = globalDocValidatordocumentMeta.TaxAmountIva19Percent,
                TaxAmountIva = globalDocValidatordocumentMeta.TaxAmountIva,
                TaxAmountIpc = globalDocValidatordocumentMeta.TaxAmountIpc,
                TaxAmountIca = globalDocValidatordocumentMeta.TaxAmountIca,
            };

            globalDataDocument.PartitionKey = $"{globalDocValidatordocumentMeta.EmissionDate.Day.ToString().PadLeft(2, '0')}|{globalDocValidatordocumentMeta.DocumentKey.Substring(0, 2)}";

            globalDataDocument.TechProviderInfo = new TechProvider()
            {
                TechProviderCode = globalDocValidatordocumentMeta.TechProviderCode,
                //TechProviderName = globalDataDocument.SenderName,
                //TechProviderNSUCode = null
            };

            globalDataDocument.ValidationResultInfo = new ValidationResult()
            {
                CategoryCode = category,
                TotalCheckedRules = globalDocValidatorList.Count(),
                MandatoryOk = globalDocValidatorList.Count(v => v.IsValid && v.Mandatory),
                MandatoryFails = globalDocValidatorList.Count(v => !v.IsValid && v.Mandatory),
                NoMandatoryOk = globalDocValidatorList.Count(v => v.IsValid && !v.Mandatory),
                NoMandatoryFails = globalDocValidatorList.Count(v => v.IsNotification),
                ProcessTime = processTimeDocument,
                ValidationTimeStamp = DateTime.UtcNow,
            };
            if (globalDataDocument.ValidationResultInfo.MandatoryFails == 0 && globalDataDocument.ValidationResultInfo.NoMandatoryFails == 0)
            {
                globalDataDocument.ValidationResultInfo.Status = (int)DocumentStatus.Approved;
                globalDataDocument.ValidationResultInfo.StatusName = EnumHelper.GetEnumDescription(DocumentStatus.Approved);
            }
            if (globalDataDocument.ValidationResultInfo.MandatoryFails == 0 && globalDataDocument.ValidationResultInfo.NoMandatoryFails > 0)
            {
                globalDataDocument.ValidationResultInfo.Status = (int)DocumentStatus.Notification;
                globalDataDocument.ValidationResultInfo.StatusName = EnumHelper.GetEnumDescription(DocumentStatus.Notification);
            }
            if (globalDataDocument.ValidationResultInfo.MandatoryFails > 0)
            {
                globalDataDocument.ValidationResultInfo.Status = (int)DocumentStatus.Rejected;
                globalDataDocument.ValidationResultInfo.StatusName = EnumHelper.GetEnumDescription(DocumentStatus.Rejected);
            }

            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "Document.New.Event",
                    Data = JsonConvert.SerializeObject(globalDataDocument),
                    EventTime = DateTime.UtcNow,
                    Subject = $"|ST:{globalDataDocument.ValidationResultInfo.StatusName}|",
                    DataVersion = "2.0"
                }
            };
            await EventGridManager.Instance().SendMessagesToEventGridAsync(eventsList);

            if (globalDataDocument.ValidationResultInfo.Status == (int)DocumentStatus.Rejected) return;

            // Register reference
            if (!string.IsNullOrEmpty(globalDocValidatordocumentMeta.DocumentReferencedKey))
            {
                var referencedDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(globalDocValidatordocumentMeta.DocumentReferencedKey, globalDocValidatordocumentMeta.DocumentReferencedKey);
                if (referencedDocumentMeta != null)
                {
                    globalDataDocument.References.Add(new Reference
                    {
                        DocumentTypeId = referencedDocumentMeta.DocumentTypeId,
                        DocumenTypeName = referencedDocumentMeta.DocumentTypeName,
                        Date = referencedDocumentMeta.EmissionDate,
                        DateNumber = int.Parse(referencedDocumentMeta.EmissionDate.ToString("yyyyMMdd")),
                        DocumentKey = referencedDocumentMeta.DocumentKey,
                        SenderCode = referencedDocumentMeta.SenderCode,
                        SenderName = referencedDocumentMeta.SenderName,
                        ReceiverCode = referencedDocumentMeta.ReceiverCode,
                        ReceiverName = referencedDocumentMeta.ReceiverName,
                        Description = ""
                    });

                    await RegisterReference(globalDataDocument, referencedDocumentMeta);
                }
            }

            // register nsu
            //await RegisterNsu(globalDocValidatordocumentMeta);
        }

        private static async Task RegisterReference(GlobalDataDocument globalDataDocument, GlobalDocValidatorDocumentMeta referencedDocumentMeta)
        {
            GlobalDocReference globalDocReference;
            DocumentTagMessage documentTagMessage = null;

            switch (globalDataDocument.DocumentTypeId)
            {
                case "1":
                case "01":
                    globalDocReference = new GlobalDocReference(globalDataDocument.DocumentKey, "INVOICE") { AccountCode = referencedDocumentMeta.SenderCode, DocumentKey = referencedDocumentMeta.DocumentKey, DocumentTypeName = "Factura electrónica", DateNumber = int.Parse(referencedDocumentMeta.EmissionDate.ToString("yyyyMMdd")) };
                    await documentReferenceTableManager.InsertOrUpdateAsync(globalDocReference);
                    break;
                case "7":
                case "91":
                    documentTagMessage = new DocumentTagMessage { Code = "REFCN", Date = referencedDocumentMeta.EmissionDate, Description = "Nota de crédito electrónica", DocumentKey = referencedDocumentMeta.DocumentKey, Value = globalDataDocument.DocumentKey, Timestamp = DateTime.UtcNow };
                    SendMessage(documentTagMessage);
                    break;
                case "8":
                case "92":
                    documentTagMessage = new DocumentTagMessage { Code = "REFDN", Date = referencedDocumentMeta.EmissionDate, Description = "Nota de débito electrónica", DocumentKey = referencedDocumentMeta.DocumentKey, Value = globalDataDocument.DocumentKey, Timestamp = DateTime.UtcNow };
                    SendMessage(documentTagMessage);
                    break;
                default:
                    break;
            }

            
        }


        private static async void SendMessage(DocumentTagMessage documentTagMessage)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "DocumentTag.New.Event",
                    Data = JsonConvert.SerializeObject(documentTagMessage),
                    EventTime = DateTime.UtcNow,
                    Subject = "DocumentTag.New.Event",
                    DataVersion = "2.0"
                }
            };
            await EventGridManager.Instance().SendMessagesToEventGridAsync(eventsList);
        }

        public class RequestObject
        {

            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
            [JsonProperty(PropertyName = "category")]
            public string Category { get; set; }
            [JsonProperty(PropertyName = "processTimeDocument")]
            public double ProcessTimeDocument { get; set; }
            [JsonProperty(PropertyName = "softwareId")]
            public string SoftwareId { get; set; }
        }
    }
}
