using Gosocket.Dian.Infrastructure;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Services.Utils
{
    public class RestUtil
    {
        private static HttpClient client = new HttpClient();

        public static RestResponse ExecuteRequest<T>(string method, T requestData)
        {
            var client = new RestClient(ConfigurationManager.GetValue("FunctionsUrl"));
            var request = new RestRequest($"api/{method}", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };

            request.AddBody(requestData);
            return (RestResponse)client.Execute(request);
        }

        public static async Task<HttpResponseMessage> ConsumeApiAsync<T>(string url, T requestObj)
        {
            
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await client.PostAsync(url, byteContent);
            
        }

        public static HttpResponseMessage ConsumeApi<T>(string url, T requestObj)
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return client.PostAsync(url, byteContent).Result;
            
        }
    }
}
