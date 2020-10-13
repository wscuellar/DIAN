using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Gosocket.Dian.Plugin.Functions.ValidateResponseCode
{
    public static class ValidateCodeResponse
    {
        [FunctionName("ValidateCodeResponse")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObject>();

            #region Validate Parameters
            // Validate parameters
            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");

            #endregion

            // Aqui va la Logica 


            return req.CreateResponse(HttpStatusCode.OK, "Mensaje ");
        }
    }

    public class RequestObject
    {
        [JsonProperty(PropertyName = "trackId")]
        public string TrackId { get; set; }
    }
}
