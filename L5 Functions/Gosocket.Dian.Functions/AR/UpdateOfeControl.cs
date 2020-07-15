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
    public static class UpdateOfeControl
    {
        //[FunctionName("UpdateOfeControl")]
        public static void Run([QueueTrigger("global-ofe-input", Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
            var document = JsonConvert.DeserializeObject<GlobalDataDocument>(eventGridEvent.Data.ToString());

            var tableManager = new TableManager("DianOfeControl");

            DianOfeControl entity = tableManager.Find<DianOfeControl>(document.SenderCode, document.SenderCode);

            if (entity != null)
            {
                if (document.EmissionDate != null && document.EmissionDateNumber != int.Parse(DateTime.UtcNow.ToString("yyyyMMdd")))
                {
                    entity.SenderCode = document.SenderCode;
                    entity.SenderName = document.SenderName;
                    entity.LastDocumentDate = document.EmissionDate;
                    entity.LastDocumentDateNumber = document.EmissionDateNumber;
                }
            }
            else
            {
                entity = new DianOfeControl(document.SenderCode, document.SenderCode)
                {
                    SenderCode = document.SenderCode,
                    SenderName = document.SenderName,
                    StartDate = null,
                    LastDocumentDate = document.EmissionDate,
                    LastDocumentDateNumber = document.EmissionDateNumber
                };
            }
            tableManager.InsertOrUpdate(entity);

            log.Info("Update successfully completed.");
        }
    }
}
