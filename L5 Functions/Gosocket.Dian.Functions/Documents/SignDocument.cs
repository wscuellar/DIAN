using eFacturacionColombia_V2.Firma;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Documents
{
    public static class SignDocument
    {
        private static readonly FirmaElectronica signer = new FirmaElectronica();

        [FunctionName("SignDocument")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var startFunction = DateTime.UtcNow;
            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");


            if (data.DocumentType == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a adocument type in the request body");

            if (data.CertificateBase64 == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a certificate base64 in the request body");

            if (data.XmlBase64 == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a xml base64 in the request body");

            byte[] xmlBytes = null;
            try
            {
                var certificateBytes = Convert.FromBase64String(data.CertificateBase64);
                var certificate2 = new X509Certificate2(certificateBytes);
                signer.Certificate2 = certificate2;
                var xmlBytesToSign = Convert.FromBase64String(data.XmlBase64);

                //File.WriteAllBytes(@"D:\ValidarDianXmls\1XmlBytesToSign.xml", xmlBytesToSign);
                 
                xmlBytes = signer.SignDocument(xmlBytesToSign, data.DocumentType, DateTime.UtcNow.AddHours(-5));

                //File.WriteAllBytes(@"D:\ValidarDianXmls\1XmlSigned.xml", xmlBytes);

                log.Info($"Document signed in {DateTime.UtcNow.Subtract(startFunction).TotalSeconds} seconds.");
                var response = new { success = true, xmlBytes, message = "Documento firmado correctamente." };
                return req.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                var response = new { success = false, xmlBytes, message = "Error al firma documento.", detail = ex.Message };
                return req.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        public class RequestObject
        {
            [JsonProperty(PropertyName = "documentType")]
            public string DocumentType { get; set; }
            [JsonProperty(PropertyName = "certificateBase64")]
            public string CertificateBase64 { get; set; }
            [JsonProperty(PropertyName = "xmlBase64")]
            public string XmlBase64 { get; set; }
        }
    }
}
