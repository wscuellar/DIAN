using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;


namespace Gosocket.Dian.Functions.Global
{
    public static class UpdateOfeData
    {
        //[FunctionName("UpdateOfeData")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            dynamic data = await req.Content.ReadAsAsync<object>();

            try
            {
                List<DianOfeControl> ofesControl = JsonConvert.DeserializeObject<List<DianOfeControl>>(data.ToString());
                var tableManager = new TableManager("DianOfeControl");
                foreach (var item in ofesControl)
                {
                    DianOfeControl entity = tableManager.Find<DianOfeControl>(item.SenderCode, item.SenderCode);
                    if (entity != null)
                    {
                        if (item.LastDocumentDate != null && int.Parse(item.LastDocumentDate.Value.ToString("yyyyMMdd")) != int.Parse(DateTime.UtcNow.ToString("yyyyMMdd")))
                        {
                            entity.SenderCode = item.SenderCode;
                            entity.SenderName = item.SenderName;
                            entity.LastDocumentDate = item.LastDocumentDate;
                            entity.LastDocumentDateNumber = int.Parse(entity.LastDocumentDate.Value.ToString("yyyyMMdd"));
                        }
                    }
                    else
                    {
                        entity = new DianOfeControl(item.SenderCode, item.SenderCode)
                        {
                            SenderCode = item.SenderCode,
                            SenderName = item.SenderName,
                            StartDate = null,
                            LastDocumentDate = item.LastDocumentDate,
                            LastDocumentDateNumber = int.Parse(item.LastDocumentDate.Value.ToString("yyyyMMdd"))
                        };
                    }
                    tableManager.InsertOrUpdate(entity);
                }

                return req.CreateResponse(HttpStatusCode.OK, "Update successfully completed.");
            }
            catch (Exception ex)
            {
                log.Error($"Error updating ofe control. {ex.StackTrace}");
                return req.CreateResponse(HttpStatusCode.BadRequest, "Error updating ofe control.");
            }
        }
    }
}
