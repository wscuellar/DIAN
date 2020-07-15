using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Functions.Cryptography.Models;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Gosocket.Dian.Functions.Cryptography.KeyVault
{
    public static class DeleteCertificate
    {
        [FunctionName("DeleteCertificate")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var name = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "name", StringComparison.OrdinalIgnoreCase) == 0).Value;
            var data = await req.Content.ReadAsAsync<DeleteCertificateRequest>();
            name = name ?? data?.Name;

            var vaultUrl = CloudConfigurationManager.GetSetting("VaultUrl");
            var clientId = CloudConfigurationManager.GetSetting("AuthClientId");
            var clientSecret = CloudConfigurationManager.GetSetting("AuthClientSecret");
            var client = new KeyVaultClient(vaultUrl, clientId, clientSecret);
            var result = client.DeleteCertificate(name);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result, Encoding.UTF8, "application/json")
            };
        }
    }
}
