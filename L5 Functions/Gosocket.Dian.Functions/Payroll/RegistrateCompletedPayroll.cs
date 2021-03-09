using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Payroll
{
    public static class RegistrateCompletedPayroll
    {
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager TableManagerGlobalDocPayroll = new TableManager("GlobalDocPayroll");
        private static readonly TableManager TableManagerGlobalDocPayrollHistoric = new TableManager("GlobalDocPayrollHistoric");

        [FunctionName("RegistrateCompletedPayroll")]
        public static async Task<EventResponse> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return new EventResponse { Code = "400", Message = "Request body is empty." };

            if (string.IsNullOrEmpty(data.TrackId))
                return new EventResponse { Code = "400", Message = "Please pass a trackId in the request body." };

            var trackIdCude = data.TrackId;
            var response = new EventResponse
            {
                Code = ((int)EventValidationMessage.Success).ToString(),
                Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success),
            };

            try
            {
                var xmlBytes = await Utils.Utils.GetXmlFromStorageAsync(trackIdCude);
                var xmlParser = new XmlParseNomina(xmlBytes);
                if (!xmlParser.Parser())
                    throw new Exception(xmlParser.ParserError);

                var documentParsed = xmlParser.Fields.ToObject<DocumentParsedNomina>();
                DocumentParsedNomina.SetValues(ref documentParsed);

                GlobalDocPayroll docGlobalPayroll = xmlParser.globalDocPayrolls;
                docGlobalPayroll.Timestamp = DateTime.Now;

                var arrayTasks = new List<Task>();
                arrayTasks.Add(TableManagerGlobalDocPayroll.InsertOrUpdateAsync(docGlobalPayroll));

                // Nómina Individual de Ajuste...
                if (Convert.ToInt32(documentParsed.DocumentTypeId) == (int)DocumentType.IndividualPayrollAdjustments)
                {
                    var trackId = documentParsed.CUNE;
                    var trackIdPred = documentParsed.CUNEPred;
                    var docGlobalPayrollHistoric = new GlobalDocPayrollHistoric(trackIdPred, trackId);
                    
                    arrayTasks.Add(TableManagerGlobalDocPayrollHistoric.InsertOrUpdateAsync(docGlobalPayrollHistoric));
                    // se actualiza en la Meta el DocumentReferenceKey con el ID del último ajuste...
                    var documentMetaAdjustment = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackIdPred, trackIdPred);
                    documentMetaAdjustment.DocumentReferencedKey = trackId;
                    arrayTasks.Add(TableManagerGlobalDocValidatorDocumentMeta.InsertOrUpdateAsync(documentMetaAdjustment));
                }
                // ...
                Task.WhenAll(arrayTasks).Wait();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                response.Code = ((int)EventValidationMessage.Error).ToString();
                response.Message = ex.Message;
            }

            return response;
        }

        public class RequestObject
        {
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
        }
    }
}
