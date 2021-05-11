using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Cosmos
{
    public static class RegisterDocumentStatsJob
    {
        //private static readonly TableManager docValidatorNewStatsTableManager = new TableManager("GlobalDocValidatorNewStats");
        private static readonly TableManager docValidatorNewStatsTableManager = new TableManager("GlobalDocValidatorStats");
        private static readonly TableManager rejectedDocumentTableManager = new TableManager("GlobalRejectedDocument");

        private static Lazy<CloudTableClient> lazyClient = new Lazy<CloudTableClient>(InitializeTableClient);
        public static CloudTableClient tableClient => lazyClient.Value;

        private static CloudTableClient InitializeTableClient()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var tableClient = account.CreateCloudTableClient();
            return tableClient;
        }

        [FunctionName("RegisterDocumentStatsJob")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var totalAccepted = 0;
            var totalNotification = 0;
            var totalRejected = 0L;
            try
            {
                var lastUpdate = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>("LASTUPDATE", "LASTUPDATE");
                if (lastUpdate == null)
                {
                    lastUpdate = new GlobalDocValidatorStats("LASTUPDATE", "LASTUPDATE") { Day = "20", Month = "02", Year = 2020, LastDateTimeUpdate = DateTime.Parse("2020-02-20"), Count = 0 };
                    await docValidatorNewStatsTableManager.InsertOrUpdateAsync(lastUpdate);
                }

                var utcNow = DateTime.UtcNow.AddSeconds(1);

                var model = new GlobalDataDocumentModel { EmissionDate = lastUpdate.LastDateTimeUpdate, LastDateTimeUpdate = lastUpdate.LastDateTimeUpdate, UtcNow = utcNow };
                var documents = await CosmosDBService.Instance(lastUpdate.LastDateTimeUpdate).ReadDocumentsByLastDateTimeUpdateAsync(model);

                
                // Global stats
                if (documents.Count() > 0)
                {
                    totalAccepted = documents.Count(d => d.ValidationResultInfo.Status == (int)DocumentStatus.Approved);
                    totalNotification = documents.Count(d => d.ValidationResultInfo.Status == (int)DocumentStatus.Notification);

                    var globalAccepted = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>("STATS", "ACCEPTED");
                    if (globalAccepted == null)
                        globalAccepted = new GlobalDocValidatorStats("STATS", "ACCEPTED") { LastDateTimeUpdate = utcNow, Count = totalAccepted };
                    else
                    {
                        globalAccepted.Count = globalAccepted.Count + totalAccepted;
                        globalAccepted.LastDateTimeUpdate = utcNow;
                    }
                    await docValidatorNewStatsTableManager.InsertOrUpdateAsync(globalAccepted);


                    var globalNotification = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>("STATS", "NOTIFICATION");
                    if (globalNotification == null)
                        globalNotification = new GlobalDocValidatorStats("STATS", "NOTIFICATION") { LastDateTimeUpdate = utcNow, Count = totalNotification };
                    else
                    {
                        globalNotification.Count = globalNotification.Count + totalNotification;
                        globalNotification.LastDateTimeUpdate = utcNow;
                    }
                    await docValidatorNewStatsTableManager.InsertOrUpdateAsync(globalNotification);
                    // Global stats

                    // Month stats
                    var month = string.Empty;
                    var year = string.Empty;
                    var documentsByMonth = documents.GroupBy(d => d.GenerationTimeStamp.Month);
                    foreach (var item in documentsByMonth)
                    {
                        //if (string.IsNullOrEmpty(month)) month = item.First().GenerationTimeStamp.Month.ToString().PadLeft(2, '0');
                        //if (string.IsNullOrEmpty(year)) year = item.First().GenerationTimeStamp.Year.ToString();

                        month = item.First().GenerationTimeStamp.Month.ToString().PadLeft(2, '0');
                        year = item.First().GenerationTimeStamp.Year.ToString();

                        var monthAccepted = item.Count(d => d.ValidationResultInfo.Status == (int)DocumentStatus.Approved);
                        var monthNotification = item.Count(d => d.ValidationResultInfo.Status == (int)DocumentStatus.Notification);

                        var totalMonthAccepted = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>($"MONTH|{month}|{year}|STATS", "ACCEPTED");
                        if (totalMonthAccepted == null)
                            totalMonthAccepted = new GlobalDocValidatorStats($"MONTH|{month}|{year}|STATS", "ACCEPTED") { Month = month, Year = int.Parse(year), LastDateTimeUpdate = utcNow, Count = monthAccepted };
                        else
                        {
                            totalMonthAccepted.Count = totalMonthAccepted.Count + monthAccepted;
                            totalMonthAccepted.LastDateTimeUpdate = utcNow;
                        }
                        await docValidatorNewStatsTableManager.InsertOrUpdateAsync(totalMonthAccepted);

                        var totalMonthNotification = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>($"MONTH|{month}|{year}|STATS", "NOTIFICATION");
                        if (totalMonthNotification == null)
                            totalMonthNotification = new GlobalDocValidatorStats($"MONTH|{month}|{year}|STATS", "NOTIFICATION") { Month = month, Year = int.Parse(year), LastDateTimeUpdate = utcNow, Count = monthNotification };
                        else
                        {
                            totalMonthNotification.Count = totalMonthNotification.Count + monthNotification;
                            totalMonthNotification.LastDateTimeUpdate = utcNow;
                        }
                        await docValidatorNewStatsTableManager.InsertOrUpdateAsync(totalMonthNotification);

                        // Day stats
                        var documentsByDay = item.GroupBy(d => d.GenerationTimeStamp.Day);
                        var day = string.Empty;
                        foreach (var item2 in documentsByDay)
                        {
                            //if (string.IsNullOrEmpty(day)) day = item2.First().GenerationTimeStamp.Day.ToString().PadLeft(2, '0');
                            day = item2.First().GenerationTimeStamp.Day.ToString().PadLeft(2, '0');

                            var dayAccepted = item2.Count(d => d.ValidationResultInfo.Status == (int)DocumentStatus.Approved);
                            var dayNotification = item2.Count(d => d.ValidationResultInfo.Status == (int)DocumentStatus.Notification);

                            var totalDayAccepted = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>($"DAY|{day}|{month}|{year}|STATS", "ACCEPTED");
                            if (totalDayAccepted == null)
                                totalDayAccepted = new GlobalDocValidatorStats($"DAY|{day}|{month}|{year}|STATS", "ACCEPTED") { Day = day, Month = month, Year = int.Parse(year), LastDateTimeUpdate = utcNow, Count = dayAccepted };
                            else
                            {
                                totalDayAccepted.Count = totalDayAccepted.Count + dayAccepted;
                                totalDayAccepted.LastDateTimeUpdate = utcNow;
                            }
                            await docValidatorNewStatsTableManager.InsertOrUpdateAsync(totalDayAccepted);

                            var totalDayNotification = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>($"DAY|{day}|{month}|{year}|STATS", "NOTIFICATION");
                            if (totalDayNotification == null)
                                totalDayNotification = new GlobalDocValidatorStats($"DAY|{day}|{month}|{year}|STATS", "NOTIFICATION") { Day = day, Month = month, Year = int.Parse(year), LastDateTimeUpdate = utcNow, Count = dayNotification };
                            else
                            {
                                totalDayNotification.Count = totalDayNotification.Count + dayNotification;
                                totalDayNotification.LastDateTimeUpdate = utcNow;
                            }
                            await docValidatorNewStatsTableManager.InsertOrUpdateAsync(totalDayNotification);

                            day = string.Empty;
                        }
                        // Day stats

                        month = string.Empty;
                        year = string.Empty;
                    }
                    // Month stats 
                }

                // Rejected stats
                var rejectdDocuments = rejectedDocumentTableManager.FindAll<GlobalRejectedDocument>("REJECTED");
                totalRejected = rejectdDocuments.Count();
                if (totalRejected > 0)
                {
                    var globalRejected = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>("STATS", "REJECTED");
                    if (globalRejected == null)
                        globalRejected = new GlobalDocValidatorStats("STATS", "REJECTED") { LastDateTimeUpdate = utcNow, Count = rejectdDocuments.Count() };
                    else
                    {
                        globalRejected.Count = globalRejected.Count + totalRejected;
                        globalRejected.LastDateTimeUpdate = utcNow;
                    }
                    await docValidatorNewStatsTableManager.InsertOrUpdateAsync(globalRejected);

                    // Month stats
                    var month = string.Empty;
                    var year = string.Empty;
                    var rejectedsByMonth = rejectdDocuments.GroupBy(d => d.GenerationTimeStamp.Month);
                    foreach (var item in rejectedsByMonth)
                    {
                        if (string.IsNullOrEmpty(month)) month = item.First().GenerationTimeStamp.Month.ToString().PadLeft(2, '0');
                        if (string.IsNullOrEmpty(year)) year = item.First().GenerationTimeStamp.Year.ToString();

                        var monthRejected = item.Count();

                        var totalMonthRejected = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>($"MONTH|{month}|{year}|STATS", "REJECTED");
                        if (totalMonthRejected == null)
                            totalMonthRejected = new GlobalDocValidatorStats($"MONTH|{month}|{year}|STATS", "REJECTED") { Month = month, Year = int.Parse(year), LastDateTimeUpdate = utcNow, Count = monthRejected };
                        else
                        {
                            totalMonthRejected.Count = totalMonthRejected.Count + monthRejected;
                            totalMonthRejected.LastDateTimeUpdate = utcNow;
                        }
                        await docValidatorNewStatsTableManager.InsertOrUpdateAsync(totalMonthRejected);

                        // Day stats
                        var documentsByDay = item.GroupBy(d => d.GenerationTimeStamp.Day);
                        var day = string.Empty;
                        foreach (var item2 in documentsByDay)
                        {
                            if (string.IsNullOrEmpty(day)) day = item2.First().GenerationTimeStamp.Day.ToString().PadLeft(2, '0');

                            var dayRejected = item2.Count();

                            var totalDayRejected = docValidatorNewStatsTableManager.Find<GlobalDocValidatorStats>($"DAY|{day}|{month}|{year}|STATS", "REJECTED");
                            if (totalDayRejected == null)
                                totalDayRejected = new GlobalDocValidatorStats($"DAY|{day}|{month}|{year}|STATS", "REJECTED") { Day = day, Month = month, Year = int.Parse(year), LastDateTimeUpdate = utcNow, Count = dayRejected };
                            else
                            {
                                totalDayRejected.Count = totalDayRejected.Count + dayRejected;
                                totalDayRejected.LastDateTimeUpdate = utcNow;
                            }
                            await docValidatorNewStatsTableManager.InsertOrUpdateAsync(totalDayRejected);

                            day = string.Empty;
                        }
                    }

                    await DeleteLastRejectedDocumentsAsync("REJECTED", GetTableRef("GlobalRejectedDocument"));
                }

                lastUpdate.Count = lastUpdate.Count + totalAccepted + totalNotification + totalRejected;
                lastUpdate.LastDateTimeUpdate = utcNow;
                await docValidatorNewStatsTableManager.InsertOrUpdateAsync(lastUpdate);

                log.Info($"Total aceptados: {totalAccepted}");
                log.Info($"Total notificación: {totalNotification}");
                log.Info($"Total rechazados: {totalRejected}");
            }
            catch (Exception ex)
            {
                log.Error($"Error al actualizar estadisticas. {ex.Message} - {ex.StackTrace}");
            }
        }

        private static async Task DeleteLastRejectedDocumentsAsync(string partitionKey, CloudTable table)
        {
            try
            {
                var projectionQuery = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)).Select(new[] { "RowKey" });

                var entities = table.ExecuteQuery(projectionQuery).ToArray();
                var offset = 0;
                while (offset < entities.Length)
                {
                    var batch = new TableBatchOperation();
                    var rows = entities.Skip(offset).Take(100);
                    foreach (var row in rows)
                        batch.Delete(row);

                    var result = await table.ExecuteBatchAsync(batch);
                    offset += result.Count;
                }
            }
            catch { }
        }

        private static CloudTable GetTableRef(string nameTable)
        {
            CloudTable tableRef = null;            
            tableRef = tableClient.GetTableReference(nameTable);
            return tableRef;
        }
    }
}
