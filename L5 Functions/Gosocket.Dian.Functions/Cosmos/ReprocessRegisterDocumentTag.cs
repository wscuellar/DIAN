using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Cosmos
{
    public static class ReprocessRegisterDocumentTag
    {
        //[FunctionName("ReprocessRegisterDocumentTag")]
        public static async Task Run([QueueTrigger("global-document-tag-input-poison", Connection = "GlobalQueue")]string myQueueItem, TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");

                var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);

                List<EventGridEvent> eventsList = new List<EventGridEvent>
                {
                    eventGridEvent
                };
                await EventGridManager.Instance("EventGridKey", "EventGridTopicEndpoint").SendMessagesToEventGridAsync(eventsList);
            }
            catch
            {
                throw;
            }
        }
    }
}
