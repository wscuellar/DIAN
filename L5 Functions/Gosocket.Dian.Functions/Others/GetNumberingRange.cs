using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Utils;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Others
{
    public static class GetNumberingRange
    {
        private static readonly TableManager tableManagerGlobalDocFolioManager = new TableManager("FolioManager");
        [FunctionName("GetNumberingRange")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string senderCode = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "senderCode", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            senderCode = senderCode ?? data?.senderCode;

            if (senderCode == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a senderCode on the query string or in the request body");

            

            var folios = tableManagerGlobalDocFolioManager.FindByPartition<GlobalDocModelFolio>(senderCode).ToList();

            List<NumberRangeResponse> foliosResult = new List<NumberRangeResponse>();

            if (folios.Count == 0)
            {
                var responseObjError = new { Code = "Error", Message = "No fue encontrado ningún rango de numeración con los datos proporcionados" };

                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(responseObjError),
                                                    System.Text.Encoding.UTF8, "application/json")
                };
            }

            else
            {
                foreach (var item in folios)
                {
                    NumberRangeResponse numberItem = new NumberRangeResponse();

                    numberItem.AccountCode = senderCode;
                    numberItem.DocumentType = int.Parse(item.RowKey.Split('_').First());
                    numberItem.FromNumber = item.FromNumber;
                    numberItem.ToNumber = item.ToNumber;
                    numberItem.Serie = string.IsNullOrEmpty(item.RowKey.Split('_')[1]) ? "NOTSERIE" : item.RowKey.Split('_')[1];
                    numberItem.ResolutionNumber = item.ResolutionNumber;
                    numberItem.ResolutionDateTime = item.ResolutionDateTime;
                    numberItem.TechnicalKey = item.TechnicalKey;
                    numberItem.ValidDateTimeFrom = item.ValidDateTimeFrom;
                    numberItem.ValidDateTimeTo = item.ValidDateTimeTo;

                    foliosResult.Add(numberItem);
                }
            }

            var responseObj = new { Code = "OK", Message = "La operación se ejecutó satisfactoriamente", Range = foliosResult };

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(responseObj),
                                            System.Text.Encoding.UTF8, "application/json")
            };

            return result;
        }
    }
}
