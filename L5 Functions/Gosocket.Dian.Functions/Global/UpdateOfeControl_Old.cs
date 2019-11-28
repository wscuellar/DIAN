// This is the default URL for triggering event grid function in the local environment.
// http://localhost:7071/admin/extensions/EventGridExtensionConfig?functionName={functionname} 

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Domain.Entity;
using System;

namespace Gosocket.Dian.Functions.Global
{
    public static class UpdateOfeControl_Old
    {
        //[FunctionName("UpdateOfeControl")]
        public static void Run([EventGridTrigger]JObject eventGridEvent, TraceWriter log)
        {
            log.Info(eventGridEvent.ToString(Formatting.Indented));

            try
            {
                var eventGridEventObject = JsonConvert.DeserializeObject<Microsoft.Azure.EventGrid.Models.EventGridEvent>(eventGridEvent.ToString());
                var document = JsonConvert.DeserializeObject<GlobalDataDocument>(eventGridEventObject.Data.ToString());

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
            }
            catch (Exception ex)
            {
                log.Error("Error updating ofe control", ex, "UpdateOfeControl");
            }

            log.Info("Update successfully completed.");
        }
    }
}
