using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Cosmos;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Cosmos
{
    public static class RegisterDocumentTag
    {
        // Set queue name
        private const string queueName = "global-document-tag-input%Slot%";

        [FunctionName("RegisterDocumentTag")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            try
            {
                var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                var documentTagMessage = JsonConvert.DeserializeObject<DocumentTagMessage>(eventGridEvent.Data.ToString());
                await CosmosDBService.Instance(documentTagMessage.Date).UpdateDocumentAsync(documentTagMessage);
                log.Info($"Register document tag success. {JsonConvert.SerializeObject(documentTagMessage)}");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                throw;
            }
        }
    }
}
