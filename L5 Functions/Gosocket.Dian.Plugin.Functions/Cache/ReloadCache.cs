using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;

namespace Gosocket.Dian.Plugin.Functions.Cache
{
    public static class ReloadCache
    {
        [FunctionName("ReloadCache")]
        public static void Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            StartUp.Initialize();
        }
    }
}
