using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Exchange
{
    public static class UpdateContributorExchangeEmailFile
    {
        private static readonly TableManager exchangeEmailTableManager = new TableManager("GlobalExchangeEmail");
        private static readonly FileManager fileManager = new FileManager();

        private static readonly string blobContainer = "dian";
        private static readonly string blobContainerFolder = "exchange";

        [FunctionName("UpdateContributorExchangeEmailFile")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                var exchangeEmails = exchangeEmailTableManager.FindAll<GlobalExchangeEmail>();
                BlockingCollection<ExchangeEmail> items = new BlockingCollection<ExchangeEmail>();
                Parallel.ForEach(exchangeEmails, new ParallelOptions { MaxDegreeOfParallelism = 100 }, item =>
                {
                    items.Add(new ExchangeEmail { Code = item.PartitionKey, Email = item.Email, LastDateUpdate = item.Timestamp.ToString("yyyy-MM-dd") });
                });

                var csv = Services.Utils.StringUtil.ToCSV(items);
                var csvBytes = Encoding.UTF8.GetBytes(csv);

                var result = await fileManager.UploadAsync(blobContainer, $"{blobContainerFolder}/emails.csv", csvBytes);
                if (!result) return req.CreateResponse(HttpStatusCode.InternalServerError, "Error uploading csv file to blob.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            log.Info("Csv file saved successfully.");
            return req.CreateResponse(HttpStatusCode.OK, "Csv file saved successfully.");
        }

        public class ExchangeEmail
        {
            public string Code { get; set; }
            public string Email { get; set; }
            public string LastDateUpdate { get; set; }
        }
    }
}
