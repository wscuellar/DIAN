using Gosocket.Dian.Functions.Cryptography.Models;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Cryptography.KeyVault
{
    public static class ImportCertificate
    {
        [FunctionName("ImportCertificate")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");
                var pairs = req.GetQueryNameValuePairs().ToList();
                var name = pairs
                    .FirstOrDefault(q => string.Compare(q.Key, "name", StringComparison.OrdinalIgnoreCase) == 0).Value;
                var content = pairs.FirstOrDefault(q =>
                    string.Compare(q.Key, "content", StringComparison.OrdinalIgnoreCase) == 0).Value;
                var password = pairs.FirstOrDefault(q =>
                    string.Compare(q.Key, "password", StringComparison.OrdinalIgnoreCase) == 0).Value;

                // Get request body
                var data = await req.Content.ReadAsAsync<ImportCertificateRequest>();
                name = name ?? data?.Name;
                content = content ?? data?.Content;
                password = password ?? data?.Password;
                var contentBytes = Convert.FromBase64String(content ?? throw new InvalidOperationException());

                var vaultUrl = CloudConfigurationManager.GetSetting("VaultUrl");
                var clientId = CloudConfigurationManager.GetSetting("AuthClientId");
                var clientSecret = CloudConfigurationManager.GetSetting("AuthClientSecret");
                var client = new KeyVaultClient(vaultUrl, clientId, clientSecret);
                var result = client.ImportCertificate(contentBytes, name, password);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(result, Encoding.UTF8, "application/json")
                };
            }
            catch (Exception e)
            {
                log.Error($"Error on function -> {e.StackTrace}", e);
                var result = new { Success = false, Name = e.Message };
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json")
                };
            }
        }
    }
}
