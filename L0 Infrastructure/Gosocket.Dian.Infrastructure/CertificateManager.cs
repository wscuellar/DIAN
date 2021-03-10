using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gosocket.Dian.Infrastructure
{
    public class CertificateManager
    {
        private static HttpClient client = new HttpClient();

        public Dictionary<string, string> GetCertificateFromHSM()
        {
            var apiUrl = "https://global-function-cryptography-sbx.azurewebsites.net/api/ExportCertificate?code=Z4iOposKBXigmFaORT76xQ9eqr5alqnR3zTu5haY8vdOS/2VWYBg/w==";//SANDBOX
            //var apiUrl = "https://global-function-cryptography-prd.azurewebsites.net/api/ExportCertificate?code=VeTDp9vmbzHIthSY/ZGF0QV1IOkHg50JXiiWFay6x5D0QmQFm3jb7g==";//PRODUCTIVO

            var result = GetPfxFromHSM(apiUrl, "peru-gosocket-cert");

            return result;
        }

        private static Dictionary<string, string> GetPfxFromHSM(string url, string name)
        {
            var responseDictionary = new Dictionary<string, string>();

            try
            {
                dynamic requestObj = new ExpandoObject();
                requestObj.Name = name;

                var response = ConsumeApi(url, requestObj);
                if (!response.IsSuccessStatusCode) return responseDictionary;

                var certificateResponse =
                    (ExportCertificateRepsonse)JsonConvert.DeserializeObject<ExportCertificateRepsonse>(response.Content
                        .ReadAsStringAsync().Result);
                if (!certificateResponse.Success) return responseDictionary;

                responseDictionary.Add("Content", certificateResponse.Base64Data);
                responseDictionary.Add("Password", certificateResponse.Password);
            }
            catch (Exception)
            {
                //ignore
            }

            return responseDictionary;
        }
        ///---
        private static HttpResponseMessage ConsumeApi(string url, dynamic requestObj)
        {
            
            
            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return client.PostAsync(url, byteContent).Result;
            
        }
    }

    public class ExportCertificateRepsonse : CertificateResponse
    {
        public string Base64Data { get; set; }
        public string Password { get; set; }
    }

    public abstract class CertificateResponse
    {
        public bool Success { get; set; }
        public string Name { get; set; }
        public string Error { get; set; }
    }
}
