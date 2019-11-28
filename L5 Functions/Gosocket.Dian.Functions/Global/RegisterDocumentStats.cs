using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Global
{
    public static class RegisterDocumentStats
    {
        private readonly static object lockUpdate = new object();
        private static readonly TableManager docValidatorStatsTableManager = new TableManager("GlobalDocValidatorStats");

        //[FunctionName("RegisterDocumentStats")]
        public static void Run([QueueTrigger("global-register-document-stats-input", Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
            var globalDataDocument = JsonConvert.DeserializeObject<GlobalDataDocument>(eventGridEvent.Data.ToString());

            try
            {
                lock (lockUpdate)
                {
                    RegisterGlobalStats(globalDataDocument);

                    RegisterDayStats(globalDataDocument);

                    RegisterMonthStats(globalDataDocument);

                    RegisterSenderStats(globalDataDocument);
                }
                

            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                throw;
            }
        }

        private static void RegisterGlobalStats(GlobalDataDocument globalDataDocument)
        {
            switch (globalDataDocument.ValidationResultInfo.Status)
            {
                case (int)DocumentStatus.Approved:
                    var globalAccepted = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>("STATS", "ACCEPTED");
                    if (globalAccepted != null)
                        globalAccepted.Count++;
                    else
                        globalAccepted = new GlobalDocValidatorStats("STATS", "ACCEPTED") { Count = 1 };
                    docValidatorStatsTableManager.InsertOrUpdate(globalAccepted);
                    break;
                case (int)DocumentStatus.Notification:
                    var globalRepair = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>("STATS", "NOTIFICATION");
                    if (globalRepair != null)
                        globalRepair.Count++;
                    else
                        globalRepair = new GlobalDocValidatorStats("STATS", "NOTIFICATION") { Count = 1 };
                    docValidatorStatsTableManager.InsertOrUpdate(globalRepair);
                    break;
                case (int)DocumentStatus.Rejected:
                    var globalRejected = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>("STATS", "REJECTED");
                    if (globalRejected != null)
                        globalRejected.Count++;
                    else
                        globalRejected = new GlobalDocValidatorStats("STATS", "REJECTED") { Count = 1 };
                    docValidatorStatsTableManager.InsertOrUpdate(globalRejected);
                    break;
                default:
                    break;
            }
        }

        private static void RegisterDayStats(GlobalDataDocument globalDataDocument)
        {
            var day = globalDataDocument.EmissionDate.Day.ToString().PadLeft(2, '0');
            var month = globalDataDocument.EmissionDate.Month.ToString().PadLeft(2, '0');
            var year = globalDataDocument.EmissionDate.Year;
            switch (globalDataDocument.ValidationResultInfo.Status)
            {
                case (int)DocumentStatus.Approved:
                    var globalAccepted = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"DAY|{day}|{month}|{year}|STATS", "ACCEPTED");
                    if (globalAccepted != null)
                    {
                        globalAccepted.Count++;
                    }
                    else
                    {
                        globalAccepted = new GlobalDocValidatorStats($"DAY|{day}|{month}|{year}|STATS", "ACCEPTED") { Count = 1, Day = day, Month = month, Year = year };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(globalAccepted);
                    break;
                case (int)DocumentStatus.Notification:
                    var globalNotification = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"DAY|{day}|{month}|{year}|STATS", "NOTIFICATION");
                    if (globalNotification != null)
                    {
                        globalNotification.Count++;
                    }
                    else
                    {
                        globalNotification = new GlobalDocValidatorStats($"DAY|{day}|{month}|{year}|STATS", "NOTIFICATION") { Count = 1, Day = day, Month = month, Year = year };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(globalNotification);
                    break;
                case (int)DocumentStatus.Rejected:
                    var globalRejected = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"DAY|{day}|{month}|{year}|STATS", "REJECTED");
                    if (globalRejected != null)
                    {
                        globalRejected.Count++;
                    }
                    else
                    {
                        globalRejected = new GlobalDocValidatorStats($"DAY|{day}|{month}|{year}|STATS", "REJECTED") { Count = 1, Day = day, Month = month, Year = year };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(globalRejected);
                    break;
                default:
                    break;
            }
        }

        private static void RegisterMonthStats(GlobalDataDocument globalDataDocument)
        {
            var day = globalDataDocument.EmissionDate.Day.ToString().PadLeft(2, '0');
            var month = globalDataDocument.EmissionDate.Month.ToString().PadLeft(2, '0');
            var year = globalDataDocument.EmissionDate.Year;
            switch (globalDataDocument.ValidationResultInfo.Status)
            {
                case (int)DocumentStatus.Approved:
                    var globalAccepted = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"MONTH|{month}|{year}|STATS", "ACCEPTED");
                    if (globalAccepted != null)
                    {
                        globalAccepted.Count++;
                    }
                    else
                    {
                        globalAccepted = new GlobalDocValidatorStats($"MONTH|{month}|{year}|STATS", "ACCEPTED") { Count = 1, Month = month, Year = year };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(globalAccepted);
                    break;
                case (int)DocumentStatus.Notification:
                    var globalNotification = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"MONTH|{month}|{year}|STATS", "NOTIFICATION");
                    if (globalNotification != null)
                    {
                        globalNotification.Count++;
                    }
                    else
                    {
                        globalNotification = new GlobalDocValidatorStats($"MONTH|{month}|{year}|STATS", "NOTIFICATION") { Count = 1, Month = month, Year = year };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(globalNotification);
                    break;
                case (int)DocumentStatus.Rejected:
                    var globalRejected = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"MONTH|{month}|{year}|STATS", "REJECTED");
                    if (globalRejected != null)
                    {
                        globalRejected.Count++;
                    }
                    else
                    {
                        globalRejected = new GlobalDocValidatorStats($"MONTH|{month}|{year}|STATS", "REJECTED") { Count = 1, Month = month, Year = year };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(globalRejected);
                    break;
                default:
                    break;
            }
        }

        private static async Task RegisterReceiverStats(GlobalDataDocument globalDataDocument)
        {
            switch (globalDataDocument.ValidationResultInfo.Status)
            {
                case (int)DocumentStatus.Approved:
                    var receiverAccpeted = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"Receiver|{globalDataDocument.ReceiverCode}", "ACCEPTED");
                    if (receiverAccpeted != null)
                    {
                        receiverAccpeted.Count++;
                    }
                    else
                    {
                        receiverAccpeted = new GlobalDocValidatorStats($"Receiver|{globalDataDocument.ReceiverCode}", "ACCEPTED") { Count = 1 };
                    }

                    await docValidatorStatsTableManager.InsertOrUpdateAsync(receiverAccpeted);
                    break;
                case (int)DocumentStatus.Notification:
                    var receiverRepair = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"Receiver|{globalDataDocument.ReceiverCode}", "NOTIFICATION");
                    if (receiverRepair != null)
                    {
                        receiverRepair.Count++;
                    }
                    else
                    {
                        receiverRepair = new GlobalDocValidatorStats($"Receiver|{globalDataDocument.ReceiverCode}", "NOTIFICATION") { Count = 1 };
                    }

                    await docValidatorStatsTableManager.InsertOrUpdateAsync(receiverRepair);
                    break;
                case (int)DocumentStatus.Rejected:
                    var receiverRejected = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"Receiver|{globalDataDocument.ReceiverCode}", "REJECTED");
                    if (receiverRejected != null)
                    {
                        receiverRejected.Count++;
                    }
                    else
                    {
                        receiverRejected = new GlobalDocValidatorStats($"Receiver|{globalDataDocument.ReceiverCode}", "REJECTED") { Count = 1 };
                    }

                    await docValidatorStatsTableManager.InsertOrUpdateAsync(receiverRejected);
                    break;
                default:
                    break;
            }
        }

        private static void RegisterSenderStats(GlobalDataDocument globalDataDocument)
        {
            switch (globalDataDocument.ValidationResultInfo.Status)
            {
                case (int)DocumentStatus.Approved:
                    var senderAccpeted = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"Sender|{globalDataDocument.SenderCode}", "ACCEPTED");
                    if (senderAccpeted != null)
                    {
                        senderAccpeted.Count++;
                    }
                    else
                    {
                        senderAccpeted = new GlobalDocValidatorStats($"Sender|{globalDataDocument.SenderCode}", "ACCEPTED") { Count = 1 };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(senderAccpeted);
                    break;
                case (int)DocumentStatus.Notification:
                    var senderRepair = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"Sender|{globalDataDocument.SenderCode}", "NOTIFICATION");
                    if (senderRepair != null)
                    {
                        senderRepair.Count++;
                    }
                    else
                    {
                        senderRepair = new GlobalDocValidatorStats($"Sender|{globalDataDocument.SenderCode}", "NOTIFICATION") { Count = 1 };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(senderRepair);
                    break;
                case (int)DocumentStatus.Rejected:
                    var senderRejected = docValidatorStatsTableManager.Find<GlobalDocValidatorStats>($"Sender|{globalDataDocument.SenderCode}", "REJECTED");
                    if (senderRejected != null)
                    {
                        senderRejected.Count++;
                    }
                    else
                    {
                        senderRejected = new GlobalDocValidatorStats($"Sender|{globalDataDocument.SenderCode}", "REJECTED") { Count = 1 };
                    }

                    docValidatorStatsTableManager.InsertOrUpdate(senderRejected);
                    break;
                default:
                    break;
            }
        }
    }
}
