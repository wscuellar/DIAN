using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;

namespace Gosocket.Dian.Functions.Others
{
    public static class RegisterRejectedDocument
    {
        // Set queue name
        private const string queueName = "global-rejected-document-input%Slot%";
        private static readonly TableManager tableManager = new TableManager("GlobalRejectedDocument");

        [FunctionName("RegisterRejectedDocument")]
        [return: Table("GlobalRejectedDocument", Connection = "GlobalStorage")]
        public static GlobalRejectedDocument Run([QueueTrigger(queueName, Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
            var document = JsonConvert.DeserializeObject<GlobalDataDocument>(eventGridEvent.Data.ToString());

            GlobalRejectedDocument rejectedDocument = null;
            try
            {
                
                rejectedDocument = new GlobalRejectedDocument("REJECTED", Guid.NewGuid().ToString()) { SenderCode = document.SenderCode, SenderName = document.SenderName, GenerationTimeStamp = document.GenerationTimeStamp };
                tableManager.InsertOrUpdate(rejectedDocument);
            }
            catch (Exception ex)
            {
                log.Error($"Error registering rejected document.", ex, "RegisterRejectedDocument");
            }

            return rejectedDocument;
        }
    }
}
