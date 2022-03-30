using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Utils;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.AR
{
    public static class GetApplicationResponse
    {
        private static readonly TableManager tableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager tableManagerGlobalDocValidatorDocument = new TableManager("GlobalDocValidatorDocument");
        private static readonly TableManager tableManagerDocumentTracking = new TableManager("GlobalDocValidatorTracking");
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");
        //private static readonly TableManager tableManagerDianNsuControl = new TableManager("DianNsuControl");

        [FunctionName("GetApplicationResponse")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            //var startFunction = DateTime.UtcNow;
            List<Task> logsArrayTasks = new List<Task>();
            Stopwatch stopwatch0 = new Stopwatch();
            stopwatch0.Start();

            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request body is empty");

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a trackId in the request body");

            var trackId = data.TrackId;

            var response = new ResponseGetApplicationResponse { Success = true, Message = "OK" };
            try
            {
                List<Task> arrayTasks = new List<Task>();

                var validations = new List<GlobalDocValidatorTracking>();
                Task firstLocalRun = Task.Run(() =>
                {
                    //var start = DateTime.UtcNow;
                    Stopwatch stopwatch1 = new Stopwatch();
                    stopwatch1.Start();
                    validations = tableManagerDocumentTracking.FindByPartition<GlobalDocValidatorTracking>(trackId);
                    stopwatch1.Stop();
                    double ms1 = stopwatch1.ElapsedMilliseconds;
                    double seconds1 = ms1 / 1000;
                    stopwatch1 = null;
                    var step1 = new GlobalLogger(trackId, "(1)GetApplicationResponse") { Message = seconds1.ToString(), Action = "(1)GetValidations" };
                    logsArrayTasks.Add(tableManagerGlobalLogger.InsertOrUpdateAsync(step1));
                    log.Info($"1. Time GlobalDocValidatorTracking: {seconds1}");
                });

                GlobalDocValidatorDocumentMeta documentMetaEntity = null;
                GlobalDocValidatorDocument docValidatorEntity = null;
                //DianNsuControl nsuSender = null;
                Task secondLocalRun = Task.Run(() =>
                {
                    //var start = DateTime.UtcNow;
                    Stopwatch stopwatch2 = new Stopwatch();
                    stopwatch2.Start();
                    documentMetaEntity = tableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
                    //nsuSender = tableManagerDianNsuControl.Find<DianNsuControl>("DianNsu", documentMetaEntity.DocumentKey);
                    var identifier = documentMetaEntity.Identifier;
                    docValidatorEntity = tableManagerGlobalDocValidatorDocument.Find<GlobalDocValidatorDocument>(identifier, identifier);
                    stopwatch2.Stop();
                    double ms2 = stopwatch2.ElapsedMilliseconds;
                    double seconds2 = ms2 / 1000;
                    stopwatch2 = null;
                    var step2 = new GlobalLogger(trackId, "(2)GetApplicationResponse") { Message = seconds2.ToString(), Action = "(2)Get 2 entities objects" };
                    logsArrayTasks.Add(tableManagerGlobalLogger.InsertOrUpdateAsync(step2));
                    log.Info($"2. Time secondLocalRun: {seconds2}");
                });

                arrayTasks.Add(firstLocalRun);
                arrayTasks.Add(secondLocalRun);
                await Task.WhenAll(arrayTasks);

                byte[] xmlBytes = null;

                Stopwatch stopwatch3 = new Stopwatch();
                stopwatch3.Start();
                var applicationResponse = await XmlUtil.GetApplicationResponseIfExist(documentMetaEntity, docValidatorEntity);
                stopwatch3.Stop();
                double ms3 = stopwatch3.ElapsedMilliseconds;
                double seconds3 = ms3 / 1000;
                stopwatch3 = null;
                var step3 = new GlobalLogger(trackId, "(3)GetApplicationResponse") { Message = seconds3.ToString(), Action = $"(3)GetApplicationResponseIfExist" };
                logsArrayTasks.Add(tableManagerGlobalLogger.InsertOrUpdateAsync(step3));
                log.Info($"3. Time GetApplicationResponseIfExist: {seconds3}");
                if (applicationResponse != null) xmlBytes = applicationResponse;
                else
                {
                    var requestObj = new { trackId };
                    var response1 = await Utils.Utils.DownloadXmlAsync(requestObj);
                    Dictionary<string, string> newXpathRequest = CreateGetXpathValidation(response1.XmlBase64, "InvoiceValidation");
                    string pathServiceData = ConfigurationManager.GetValue("GetXpathDataValuesUrl");
                    var tributaryValues = await ApiHelpers.ExecuteRequestAsync<Domain.Domain.ResponseXpathDataValue>(pathServiceData, newXpathRequest);

                    Stopwatch stopwatch4 = new Stopwatch();
                    stopwatch4.Start();
                    xmlBytes = XmlUtil.GenerateApplicationResponseBytes(trackId, documentMetaEntity, validations, tributaryValues.XpathsValues);
                    stopwatch4.Stop();
                    double ms4 = stopwatch4.ElapsedMilliseconds;
                    double seconds4 = ms4 / 1000;
                    stopwatch4 = null;
                    var step4 = new GlobalLogger(trackId, "(4)GetApplicationResponse") { Message = seconds4.ToString(), Action = "(4)GenerateApplicationResponseBytes" };
                    logsArrayTasks.Add(tableManagerGlobalLogger.InsertOrUpdateAsync(step4));
                    log.Info($"4. Time GenerateApplicationResponseBytes: {seconds4}");
                }

                if (xmlBytes == null)
                {
                    response.Success = false;
                    response.Message = "No se pudo generar application response.";
                }
                else
                {
                    response.Content = xmlBytes;
                    response.ContentBase64 = Convert.ToBase64String(xmlBytes);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                response.Success = false;
                response.Message = ex.Message;
            }

            stopwatch0.Stop();
            double ms0 = stopwatch0.ElapsedMilliseconds;
            double seconds0 = ms0 / 1000;
            stopwatch0 = null;
            var step0 = new GlobalLogger(trackId, "(5)GetApplicationResponse") { Message = seconds0.ToString(), Action = "(5)End" };
            logsArrayTasks.Add(tableManagerGlobalLogger.InsertOrUpdateAsync(step0));
            await Task.WhenAll(logsArrayTasks);

            log.Info($"5. End function: {seconds0}");
            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        private static Dictionary<string, string> CreateGetXpathValidation(string xmlBase64, string fileName)
        {
            var requestObj = new Dictionary<string, string>
            {
                { "XmlBase64", xmlBase64},
                { "FileName", fileName},
                { "SenderTributary", "//*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='TaxScheme']/*[local-name()='Name']" },
                { "SenderTributaryId", "//*[local-name()='AccountingSupplierParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='TaxScheme']/*[local-name()='ID']" },
                { "ReceiverTributary", "//*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='TaxScheme']/*[local-name()='Name']" },
                { "ReceiverTributaryId", "//*[local-name()='AccountingCustomerParty']/*[local-name()='Party']/*[local-name()='PartyTaxScheme']/*[local-name()='TaxScheme']/*[local-name()='ID']" }
            };
            return requestObj;
        }

        public class RequestObject
        {
            [JsonProperty(PropertyName = "trackId")]
            public string TrackId { get; set; }
        }
    }
}
