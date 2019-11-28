using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Global.Cosmos
{
    public static class InsertDocumentTest
    {
        //[FunctionName("InsertDocumentTest")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            dynamic data = await req.Content.ReadAsAsync<object>();

            try
            {
                GlobalDataDocument document = JsonConvert.DeserializeObject<GlobalDataDocument>(data.ToString());
                CosmosDBService.Instance(document.EmissionDate).CreateDocument(document);
                return req.CreateResponse(HttpStatusCode.OK, "Insertion successfully completed.");
            }
            catch (Exception ex)
            {
                log.Error($"Error inserting document. {ex.StackTrace}");
                return req.CreateResponse(HttpStatusCode.BadRequest, "Error inserting document.");
            }
        }
    }
}
