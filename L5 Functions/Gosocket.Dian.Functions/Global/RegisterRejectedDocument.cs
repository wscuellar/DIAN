using System;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Global
{
    public static class RegisterRejectedDocument
    {
        [FunctionName("RegisterRejectedDocument")]
        [return: Table("GlobalRejectedDocument", Connection = "GlobalStorage")]
        public static GlobalRejectedDocument Run([QueueTrigger("global-rejected-document-input", Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
            var document = JsonConvert.DeserializeObject<GlobalDataDocument>(eventGridEvent.Data.ToString());

            GlobalRejectedDocument rejectedDocument = null;
            try
            {
                var tableManager = new TableManager("GlobalRejectedDocument");
                rejectedDocument = new GlobalRejectedDocument("REJECTED", Guid.NewGuid().ToString()) { SenderCode = document.SenderCode,SenderName = document.SenderName, GenerationTimeStamp = document.GenerationTimeStamp};
            }
            catch (Exception ex)
            {
                log.Error($"Error registering rejected document.", ex, "RegisterRejectedDocument");
            }

            return rejectedDocument;
        }
    }
}
