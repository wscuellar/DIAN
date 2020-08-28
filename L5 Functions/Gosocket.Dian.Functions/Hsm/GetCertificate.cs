using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CertificateResponse = Gosocket.Dian.Functions.Models.CertificateResponse;

namespace Gosocket.Dian.Functions.Hsm
{
    public static class GetCertificate
    {
        [FunctionName("GetCertificate")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var data = await req.Content.ReadAsAsync<GetRequest>();

            if (data.Name == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, new CertificateResponse { Ok = false, Message = "Please pass a name  in the request body" });

            try
            {
                var response = await ApiHelpers.ExecuteRequestAsync<CertificateResponse>(ConfigurationManager.GetValue("GetCertificateApiUrl"), new { name = data.Name });
                return req.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                return req.CreateResponse(HttpStatusCode.OK, new CertificateResponse { Ok = false, Message = $"Error getting certificate. {ex.Message}" });
            }
        }
    }
}
