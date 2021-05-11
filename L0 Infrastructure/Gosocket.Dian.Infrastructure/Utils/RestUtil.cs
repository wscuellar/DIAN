using Newtonsoft.Json;
using RestSharp;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gosocket.Dian.Infrastructure.Utils
{
    public class RestUtil
    {
        private static HttpClient client = new HttpClient();

        public static RestResponse ExecuteRequest(string method, object requestData)
        {
            var client = new RestClient(ConfigurationManager.GetValue("FunctionsUrl"));
            var request = new RestRequest($"api/{method}", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };

            request.AddBody(requestData);
            return (RestResponse)client.Execute(request);
        }

        public static HttpResponseMessage ConsumeApi(string url, dynamic requestObj)
        {
            
            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return client.PostAsync(url, byteContent).Result;
            
        }
    }
}
