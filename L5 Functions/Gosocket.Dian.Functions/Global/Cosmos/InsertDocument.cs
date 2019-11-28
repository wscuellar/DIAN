using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Global.Common;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Global.Cosmos
{
    public static class InsertDocument
    {
        private static readonly TableManager tableManagerGlobalDocValidatorDocument = new TableManager("GlobalDocValidatorDocument");

        [FunctionName("InsertDocument")]
        public static async Task Run([QueueTrigger("global-document-input", Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            // parse query parameter
            var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
            var document = JsonConvert.DeserializeObject<GlobalDataDocument>(eventGridEvent.Data.ToString());
            document.id = $"{document.SenderCode}{document.DocumentTypeId}{document.SerieAndNumber}".ToGuid().ToString();
            string discriminator = document.DocumentKey.ToString().Substring(0, 2);
            document.PartitionKey = $"co|{document.EmissionDate.Day.ToString().PadLeft(2, '0')}|{discriminator}";
            GlobalDocValidatorDocument globalDocValidatorDocument = null;
            try
            {
                var tableManager = new TableManager("GlobalDocValidatorDocument");
                globalDocValidatorDocument = tableManager.Find<GlobalDocValidatorDocument>(document.Identifier, document.Identifier);
                if (globalDocValidatorDocument == null)
                {
                    // Create a instance of GlobalDocValidatorDocument
                    globalDocValidatorDocument = new GlobalDocValidatorDocument(document.Identifier, document.Identifier)
                    {
                        DocumentKey = document.DocumentKey.ToLower(),
                        DocumentTypeId = document.DocumentTypeId,
                        EmissionDateNumber = document.EmissionDateNumber.ToString(),
                        GlobalDocumentId = document.DocumentKey.ToLower(),
                        ValidationStatus = document.ValidationResultInfo.Status,
                        ValidationStatusName = document.ValidationResultInfo.StatusName
                    };
                }
                else
                {
                    globalDocValidatorDocument.DocumentKey = document.DocumentKey.ToLower();
                    globalDocValidatorDocument.DocumentTypeId = document.DocumentTypeId;
                    globalDocValidatorDocument.EmissionDateNumber = document.EmissionDateNumber.ToString();
                    globalDocValidatorDocument.GlobalDocumentId = document.DocumentKey.ToLower();
                    globalDocValidatorDocument.ValidationStatus = document.ValidationResultInfo.Status;
                    globalDocValidatorDocument.ValidationStatusName = document.ValidationResultInfo.StatusName;
                }

                await tableManagerGlobalDocValidatorDocument.InsertOrUpdateAsync(globalDocValidatorDocument);

                // Insert documento into cosmos db
                var cosmosService = CosmosDBService.Instance(document.EmissionDate);
                var cosmosDocument = await cosmosService.GetAsync(document.id, document.PartitionKey, document.EmissionDate);
                GlobalDataDocument result = null;
                if (cosmosDocument == null)
                    result = await cosmosService.CreateDocumentAsync(document);

                ApiHelpers.ExecuteRequest<ResponseGetApplicationResponse>(ConfigurationManager.GetValue("GetAppResponseUrl"), new { trackId = globalDocValidatorDocument.DocumentKey });

                log.Info($"Insertion successfully completed. DocumentKey: {result?.DocumentKey}");
            }
            catch (Exception ex)
            {
                log.Error($"Error inserting document into cosmos db.", ex, "InsertDocument");
                throw;
            }
        }
    }
}
