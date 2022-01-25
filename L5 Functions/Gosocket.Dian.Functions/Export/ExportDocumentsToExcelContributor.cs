using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Models;
using Gosocket.Dian.Functions.Office;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Export
{
    public static class ExportDocumentsContributor
    {
        private static readonly FileManager fileManager = new FileManager();
        private static readonly TableManager globalTaskTableMabager = new TableManager("GlobalTask");

        // Set queue name
        private const string queueName = "global-export-documents-to-excel-contributor-input%Slot%";

        [FunctionName("ExportDocumentsToExcelContributor")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
            var task = JsonConvert.DeserializeObject<GlobalTask>(eventGridEvent.Data.ToString());

            try
            {
                #region Filter documents
                string continuationToken = null;
                var exported = new List<DocumentExported>();

                var resultForEmitted = new Tuple<bool, string, List<GlobalDataDocument>>(false, null, null);
                if (new string[] { "0", "1" }.Contains(task.FilterGroupCode))
                {
                    resultForEmitted = await CosmosDBService.Instance(task.EndDate).ReadDocumentsAsync(continuationToken, task.StartDate, task.EndDate, 0, "00", task.SenderCode, null, task.ReceiverCode, null, Int32.Parse(ConfigurationManager.GetValue("ContributorsDocsToExport1")), null, "00");
                    var emitted = resultForEmitted.Item3.Select(d => new DocumentExported
                    {
                        DocumentTypeName = d.DocumentTypeName,
                        DocumentKey = d.DocumentKey,
                        Number = d.Number,
                        Serie = d.Serie,
                        EmissionDate = d.EmissionDate.ToString("dd-MM-yyyy"),
                        ReceptionDate = d.ReceptionTimeStamp.ToString("dd-MM-yyyy HH:mm:ss"),
                        SenderCode = d.SenderCode,
                        SenderName = d.SenderName,
                        ReceiverCode = d.ReceiverCode,
                        ReceiverName = d.ReceiverName,
                        TotalIVA = d.TaxAmountIva,
                        TotalICA = d.TaxAmountIca,
                        TotalIPC = d.TaxAmountIpc,
                        TotalAmount = d.TotalAmount,
                        Status = d.ValidationResultInfo.StatusName,
                        Group = "Emitido"
                    });
                    exported.AddRange(emitted);
                }

                var resultForReceived = new Tuple<bool, string, List<GlobalDataDocument>>(false, null, null);
                if (new string[] { "0", "2" }.Contains(task.FilterGroupCode))
                {
                    resultForReceived = await CosmosDBService.Instance(task.EndDate).ReadDocumentsAsync(continuationToken, task.StartDate, task.EndDate, 0, "00", null, null, task.SenderCode, null, Int32.Parse(ConfigurationManager.GetValue("ContributorsDocsToExport2")), null, "00");
                    var received = resultForReceived.Item3.Select(d => new DocumentExported
                    {
                        DocumentTypeName = d.DocumentTypeName,
                        DocumentKey = d.DocumentKey,
                        Number = d.Number,
                        Serie = d.Serie,
                        EmissionDate = d.EmissionDate.ToString("dd-MM-yyyy"),
                        ReceptionDate = d.ReceptionTimeStamp.ToString("dd-MM-yyyy HH:mm:ss"),
                        SenderCode = d.SenderCode,
                        SenderName = d.SenderName,
                        ReceiverCode = d.ReceiverCode,
                        ReceiverName = d.ReceiverName,
                        TotalIVA = d.TaxAmountIva,
                        TotalICA = d.TaxAmountIca,
                        TotalIPC = d.TaxAmountIpc,
                        TotalAmount = d.TotalAmount,
                        Status = d.ValidationResultInfo.StatusName,
                        Group = "Recibido"

                    });
                    exported.AddRange(received);
                }
                #endregion

                #region Generate excel file
                var columns = new Dictionary<string, string>
                {
                    {"DocumentTypeName", "Tipo de documento"},
                    {"DocumentKey", "CUFE/CUDE"},
                    {"Number", "Folio"},
                    {"Serie", "Prefijo"},
                    {"EmissionDate", "Fecha Emisión"},
                    {"ReceptionDate", "Fecha Recepción"},
                    {"SenderCode", "NIT Emisor"},
                    {"SenderName", "Nombre Emisor"},
                    {"ReceiverCode", "NIT Receptor"},
                    {"ReceiverName", "Nombre Receptor"},
                    {"TotalIVA", "IVA"},
                    {"TotalICA", "ICA"},
                    {"TotalIPC", "IPC"},
                    {"TotalAmount", "Total"},
                    {"Status", "Estado"},
                    {"Group", "Grupo"}
                };

                var headerColorCode = "#348441";
                int argb = int.Parse(headerColorCode.Replace("#", ""), NumberStyles.HexNumber);
                Color headerBgColor = Color.FromArgb(argb);

                Excel excel = new Excel(headerColorCode)
                {
                    TableName = $"Reporte_Documentos_{DateTime.UtcNow.ToString("yyyyMMdd_HHmm")}",
                    ColumnNames = columns,
                    HeaderBgColor = headerBgColor
                };
                var hiddenColumns = new List<bool> { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                var fileBytes = excel.Generate(exported.ToList(), null, hiddenColumns);
                #endregion

                #region Upload file to storage
                var upload = fileManager.Upload("global", $"export/{task.PartitionKey}/{task.RowKey}.xlsx", fileBytes);
                #endregion

                #region Update task
                task.Status = (int)ExportStatus.OK;
                task.TotalResult = exported.Count().ToString();
                var update = globalTaskTableMabager.InsertOrUpdate(task);

                if (!upload || !update)
                    throw new Exception();
                #endregion

                #region Only debug
                // Only debug
                //FileStream file = new FileStream($@"D:\DianExport\{task.PartitionKey}_{task.RowKey}.xlsx", FileMode.Create, FileAccess.Write);
                //file.Write(fileBytes, 0, fileBytes.Length);
                //file.Close(); 
                #endregion
            }
            catch (Exception ex)
            {
                task.Status = (int)ExportStatus.Error;
                globalTaskTableMabager.InsertOrUpdate(task);
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                throw;
            }
        }
    }
}
