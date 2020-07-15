using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Gosocket.Dian.Functions.ECD
{
    public static class DownloadCrtFiles
    {
        private static readonly FileManager fileManager = new FileManager();
        private static readonly string container = "dian";

        [FunctionName("DownloadCrtFiles")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                // Download files from Andes
                DownloadAndesFiles();

                // Download files from Certicámara
                //DownloadCertiCamaraFiles();

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
        private static void DownloadAndesFiles()
        {
            //Raiz
            var fileNameContainer = $"certificates/crts/AndesRaiz.crt";
            UriBuilder downloadUriBuilder = new UriBuilder("http://certs.andesscd.com.co/Raiz.crt");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            var bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            var result = fileManager.Upload(container, fileNameContainer, bytes);


            //Clase II
            fileNameContainer = $"certificates/crts/AndesClaseII.crt";
            downloadUriBuilder = downloadUriBuilder = new UriBuilder("http://certs.andesscd.com.co/ClaseII.crt");
            request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            response = (HttpWebResponse)request.GetResponse();
            responseStream = response.GetResponseStream();
            bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            result = fileManager.Upload(container, fileNameContainer, bytes);


            //Clase III
            fileNameContainer = $"certificates/crts/AndesClaseIII.crt";
            downloadUriBuilder = downloadUriBuilder = new UriBuilder("http://certs.andesscd.com.co/ClaseIII.crt");
            request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            response = (HttpWebResponse)request.GetResponse();
            responseStream = response.GetResponseStream();
            bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            result = fileManager.Upload(container, fileNameContainer, bytes);


            //Clase IIIESP
            fileNameContainer = $"certificates/crts/AndesClaseIIIESP.crt";
            downloadUriBuilder = downloadUriBuilder = new UriBuilder("http://certs.andesscd.com.co/ClaseIIIESP.crt");
            request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            response = (HttpWebResponse)request.GetResponse();
            responseStream = response.GetResponseStream();
            bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            result = fileManager.Upload(container, fileNameContainer, bytes);
        }

        /// <summary>
        /// Download cret files from Certicámara
        /// </summary>
        public static void DownloadCertiCamaraFiles()
        {
            //SafeLayer 4 Raiz
            var fileNameContainer = $"certificates/crts/SafeLayer4CerticamaraRaiz.crt";
            UriBuilder downloadUriBuilder = new UriBuilder("http://mirror.certicamara.com/ac_offline_raiz_certicamara_2016.crt");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            var bytes = Utils.Utils.ConvertStreamToBytes(responseStream);
            var result = fileManager.Upload(container, fileNameContainer, bytes);

            //SafeLayer 3 Raiz
            fileNameContainer = $"certificates/crts/SafeLayer3CerticamaraRaiz.crt";
            downloadUriBuilder = new UriBuilder("http://mirror.certicamara.com/ac_offline_raiz_certicamara.crt");
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
            requestObj.type = "CRT";
            Utils.Utils.ConsumeApi(ConfigurationManager.GetValue("ReloadFilesCO"), requestObj);
        }
    }
}
