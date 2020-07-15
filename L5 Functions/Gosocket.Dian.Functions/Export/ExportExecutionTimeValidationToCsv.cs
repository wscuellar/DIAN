using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Export
{
    public static class ExportExecutionTimeValidationToCsv
    {
        private static readonly string blobContainer = "global";
        private static readonly string blobContainerFolder = "export";
        private static readonly FileManager fileManager = new FileManager();
        private static readonly TableManager globalDocValidatorRuntimeTableManager = new TableManager("GlobalDocValidatorRuntime");

        [FunctionName("ExportExecutionTimeValidationToCsv")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                DateTime startDate = DateTime.UtcNow;
                var list = new List<ExecutionTimeValidation>();
                var runtimes = globalDocValidatorRuntimeTableManager.FindAll<GlobalDocValidatorRuntime>();
                var items = runtimes.GroupBy(r => r.PartitionKey);
                foreach (var group in items)
                {
                    var start = group.FirstOrDefault(r => r.RowKey == "START");
                    var end = group.FirstOrDefault(r => r.RowKey == "END");
                    if (start != null && end != null)
                    {
                        list.Add(new ExecutionTimeValidation
                        {
                            DocumentKey = end.PartitionKey,
                            ExecutionTime = end.Timestamp.Subtract(start.Timestamp).TotalSeconds,
                            Date = end.Timestamp
                        });
                    }
                }

                list = list.OrderByDescending(l => l.Date).ToList();
                var csv = StringUtil.ToCSV(list);
                var csvBytes = Encoding.ASCII.GetBytes(csv);

                var utcNow = DateTime.UtcNow;
                var path = $"execution time validations";
                var result = await fileManager.UploadAsync(blobContainer, $"{blobContainerFolder}/execution time validations/result.csv", csvBytes);
                if (!result)
                    throw new Exception("Error al almacenar archivo en el storage.");

                log.Info($"Archivo csv generado correctamente en {DateTime.UtcNow.Subtract(startDate).TotalMinutes} minutos.");
            }
            catch (Exception ex)
            {
                log.Error($"Error al exportar tiempo de ejecución de validaciónes. {ex.Message} - {ex.StackTrace}");
            }
        }

        public class ExecutionTimeValidation
        {
            public string DocumentKey { get; set; }
            public double? ExecutionTime { get; set; }
            public DateTimeOffset Date { get; set; }
        }
    }
}
