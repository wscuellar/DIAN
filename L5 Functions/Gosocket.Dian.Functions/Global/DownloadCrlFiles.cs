using System;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Gosocket.Dian.Functions.Global
{
    public static class DownloadCrlFiles
    {
        [FunctionName("DownloadCrlFiles")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                var fileManager = new FileManager();
                var container = $"dian";

                // Download files from Andes
                DownloadAndesFiles(fileManager, container);

                // Reload files on Redis
                ReloadFiles();
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
                return req.CreateResponse(HttpStatusCode.InternalServerError, false);
            }

            return req.CreateResponse(HttpStatusCode.OK, true);
        }

        /// <summary>
        /// Donwnload crt files from Andes
        /// </summary>
        /// <param name="fileManager"></param>
        /// <param name="container"></param>
        private static void DownloadAndesFiles(FileManager fileManager, string container)
        {
            //Raiz
            var fileNameContainer = $"certificates/crls/AndesRaiz.crl";
            UriBuilder downloadUriBuilder = new UriBuilder("http://crl.andesscd.com.co/Raiz.crl");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            var bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            var result = fileManager.Upload(container, fileNameContainer, bytes);


            //Clase II
            fileNameContainer = $"certificates/crls/AndesClaseII.crl";
            downloadUriBuilder = new UriBuilder("http://crl.andesscd.com.co/ClaseII.crl");
            request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            response = (HttpWebResponse)request.GetResponse();

            responseStream = response.GetResponseStream();
            bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            result = fileManager.Upload(container, fileNameContainer, bytes);


            //Clase III
            fileNameContainer = $"certificates/crls/AndesClaseIII.crl";
            downloadUriBuilder = new UriBuilder("http://crl.andesscd.com.co/ClaseIII.crl");
            request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            response = (HttpWebResponse)request.GetResponse();
            responseStream = response.GetResponseStream();
            bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            result = fileManager.Upload(container, fileNameContainer, bytes);


            //Clase IIIESP
            fileNameContainer = $"certificates/crls/AndesClaseIIIESP.crl";
            downloadUriBuilder = new UriBuilder("http://crl.andesscd.com.co/ClaseIIIESP.crl");
            request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            response = (HttpWebResponse)request.GetResponse();
            responseStream = response.GetResponseStream();
            bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            result = fileManager.Upload(container, fileNameContainer, bytes);
        }

        /// <summary>
        /// Reload files on Redis
        /// </summary>
        private static void ReloadFiles()
        {
            dynamic requestObj = new ExpandoObject();
            requestObj.type = "CRL";
            Utils.Utils.ConsumeApi(ConfigurationManager.GetValue("ReloadFilesCO"), requestObj);
        }
    }
}
