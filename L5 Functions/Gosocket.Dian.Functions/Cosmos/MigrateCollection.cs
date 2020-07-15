using Gosocket.Dian.Application.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Cosmos
{
    public static class MigrateCollection
    {
        [FunctionName("MigrateCollection")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                var data = await req.Content.ReadAsAsync<RequestObject>();

                await CosmosDBService.Instance(DateTime.UtcNow).MigrateCollection(data.CollectionName, data.OfferThroughput);
            }
            catch (Exception ex)
            {
                log.Error($"{ex.Message}____{ex.StackTrace}");
                throw;
            }
        }

        public class RequestObject
        {
            [JsonProperty(PropertyName = "collectionName")]
            public string CollectionName { get; set; }

            [JsonProperty(PropertyName = "offerThroughput")]
            public int OfferThroughput { get; set; }
        }
    }
}
