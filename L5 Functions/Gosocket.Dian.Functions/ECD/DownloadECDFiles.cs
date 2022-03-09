using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Models;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.ECD
{
    public static class DownloadECDFiles
    {
        static readonly FileManager fileManager = new FileManager();
        static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");

        static string Extension { get; set; }
        static readonly string[] extensionsAllowed = { "crl", "crt" };

        [FunctionName("DownloadECDFiles")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                string extension = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "extension", true) == 0).Value;

                dynamic data = await req.Content.ReadAsAsync<object>();

                Extension = extension ?? data?.extension;

                if (!extensionsAllowed.Contains(Extension))
                    return req.CreateResponse(HttpStatusCode.BadRequest, new { ok = false, message = "Extension not allowed." });

                await DonwloadFiles();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                return req.CreateResponse(HttpStatusCode.InternalServerError, new { ok = false, message = ex.Message });
            }

            return req.CreateResponse(HttpStatusCode.OK, new { ok = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        private static void DonwloadFile(ECDDownloadConfiguration configuration)
        {
            try
            {
                UriBuilder downloadUriBuilder = new UriBuilder(configuration.Url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUriBuilder.Uri);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream br = response.GetResponseStream();

                var buffer = new List<byte>();
                while (true) 
                { 
                    byte[] tmpBuffer = new byte[1024]; 
                    int bytesRead = br.Read(tmpBuffer, 0, tmpBuffer.Length);
                    if (bytesRead <= 0)
                        break;
                    buffer.AddRange(tmpBuffer.Take(bytesRead)); 
                    
                }               
                
                fileManager.Upload(configuration.Container, configuration.FileName, buffer.ToArray());
                
            }
            catch (Exception ex)
            {                
                var logger = new GlobalLogger("DonwloadECDFiles", configuration.Name) { Action = "DonwloadECDFiles", Message = ex.Message, RouteData = JsonConvert.SerializeObject(configuration) };
                tableManagerGlobalLogger.InsertOrUpdate(logger);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static async Task DonwloadFiles()
        {
            var configurations = await GetECDConfigurations();

            List<Task> arrayTasks = new List<Task>();

            Task firstTask = Task.Run(() =>
            {
                Parallel.ForEach(configurations, new ParallelOptions { MaxDegreeOfParallelism = 100 }, configuration =>
                {
                    DonwloadFile(configuration);
                });
            });

            arrayTasks.Add(firstTask);
            await Task.WhenAll(arrayTasks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static async Task<List<ECDDownloadConfiguration>> GetECDConfigurations()
        {
            var result = await fileManager.GetTextAsync("configurations", "ECDConfiguration.json");
            var configurations = JsonConvert.DeserializeObject<List<ECDDownloadConfiguration>>(result);
            configurations = configurations.Where(_ => _.Active && _.Extension == Extension && (_.Url.EndsWith(".crl") || _.Url.EndsWith(".crt"))).ToList();
            return configurations;
        }
    }
}
