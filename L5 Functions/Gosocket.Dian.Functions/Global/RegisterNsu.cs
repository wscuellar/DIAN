using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gosocket.Dian.Functions.Global
{
    public static class RegisterNsu
    {
        // must be moved to dian functions
        private static readonly TableManager nsuControlTableManager = new TableManager("DianNsuControl");
        private static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");

        //[FunctionName("RegisterNsu")]
        public static void Run([QueueTrigger("register-nsu-input", Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            var requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(myQueueItem);
            var trackId = requestData["trackId"].ToString();

            var globalDocValidatordocumentMeta = GetGlobalDocValidatorDocumentMeta(trackId);
            if (globalDocValidatordocumentMeta == null)
            {
                return;
            }

            // nsu for dian
            var dianNsu = new DianNsuControl($"DianNsu", "DianNsu");
            dianNsu = nsuControlTableManager.Find<DianNsuControl>(dianNsu.PartitionKey, dianNsu.RowKey);
            if (dianNsu == null)
            {
                dianNsu = new DianNsuControl($"DianNsu", "DianNsu")
                {
                    Last = 1,
                    Actual = 1,
                    SenderCode = globalDocValidatordocumentMeta.SenderCode,
                    ReceiverCode = globalDocValidatordocumentMeta.ReceiverCode,
                    DocumentKey = globalDocValidatordocumentMeta.DocumentKey
                };
                nsuControlTableManager.InsertOrUpdate(dianNsu);
                // set rowkey actual
                dianNsu.RowKey = dianNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(dianNsu);
                // set rowkey documet key
                dianNsu.RowKey = trackId;
                nsuControlTableManager.InsertOrUpdate(dianNsu);
            }
            else if (dianNsu.DocumentKey != globalDocValidatordocumentMeta.DocumentKey)
            {
                dianNsu.Last = dianNsu.Actual;
                dianNsu.Actual++;
                dianNsu.DocumentKey = globalDocValidatordocumentMeta.DocumentKey;
                dianNsu.SenderCode = globalDocValidatordocumentMeta.SenderCode;
                dianNsu.ReceiverCode = globalDocValidatordocumentMeta.ReceiverCode;
                nsuControlTableManager.InsertOrUpdate(dianNsu);
                dianNsu.RowKey = dianNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(dianNsu);
                // set rowkey actual
                dianNsu.RowKey = dianNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(dianNsu);
                // set rowkey documet key
                dianNsu.RowKey = trackId;
                nsuControlTableManager.InsertOrUpdate(dianNsu);
            }
            // nsu for dian

            // nsu for sender
            var senderNsu = new DianNsuControl($"Sender|{globalDocValidatordocumentMeta.SenderCode}", globalDocValidatordocumentMeta.SenderCode);
            senderNsu = nsuControlTableManager.Find<DianNsuControl>(senderNsu.PartitionKey, senderNsu.RowKey);
            if (senderNsu == null)
            {
                senderNsu = new DianNsuControl($"Sender|{globalDocValidatordocumentMeta.SenderCode}", globalDocValidatordocumentMeta.SenderCode)
                {
                    Last = 1,
                    Actual = 1,
                    SenderCode = globalDocValidatordocumentMeta.SenderCode,
                    ReceiverCode = globalDocValidatordocumentMeta.ReceiverCode,
                    DocumentKey = globalDocValidatordocumentMeta.DocumentKey,
                    DianNsu = dianNsu.Actual.ToString()
                };
                nsuControlTableManager.InsertOrUpdate(senderNsu);
                // set rowkey actual
                senderNsu.RowKey = senderNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(senderNsu);
                // set rowkey documet key
                senderNsu.RowKey = senderNsu.DocumentKey.ToString();
                nsuControlTableManager.InsertOrUpdate(senderNsu);
            }
            else if (senderNsu.DocumentKey != globalDocValidatordocumentMeta.DocumentKey)
            {
                senderNsu.Last = senderNsu.Actual;
                senderNsu.Actual++;
                senderNsu.DocumentKey = globalDocValidatordocumentMeta.DocumentKey;
                senderNsu.SenderCode = globalDocValidatordocumentMeta.SenderCode;
                senderNsu.ReceiverCode = globalDocValidatordocumentMeta.ReceiverCode;
                senderNsu.DianNsu = dianNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(senderNsu);
                // set rowkey actual
                senderNsu.RowKey = senderNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(senderNsu);
                // set rowkey documet key
                senderNsu.RowKey = senderNsu.DocumentKey.ToString();
                nsuControlTableManager.InsertOrUpdate(senderNsu);
            }
            // nsu for sender

            // nsu for receiver
            var receiverNsu = new DianNsuControl($"Receiver|{globalDocValidatordocumentMeta.ReceiverCode}", globalDocValidatordocumentMeta.ReceiverCode);
            receiverNsu = nsuControlTableManager.Find<DianNsuControl>(receiverNsu.PartitionKey, receiverNsu.RowKey);
            if (receiverNsu == null)
            {
                receiverNsu = new DianNsuControl($"Receiver|{globalDocValidatordocumentMeta.ReceiverCode}", globalDocValidatordocumentMeta.ReceiverCode)
                {
                    Last = 1,
                    Actual = 1,
                    SenderCode = globalDocValidatordocumentMeta.SenderCode,
                    ReceiverCode = globalDocValidatordocumentMeta.ReceiverCode,
                    DocumentKey = globalDocValidatordocumentMeta.DocumentKey,
                    DianNsu = dianNsu.Actual.ToString()
                };
                nsuControlTableManager.InsertOrUpdate(receiverNsu);
                // set rowkey actual
                receiverNsu.RowKey = receiverNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(receiverNsu);
                // set rowkey documet key
                receiverNsu.RowKey = receiverNsu.DocumentKey.ToString();
                nsuControlTableManager.InsertOrUpdate(receiverNsu);
            }
            else if (receiverNsu.DocumentKey != globalDocValidatordocumentMeta.DocumentKey)
            {
                receiverNsu.Last = receiverNsu.Actual;
                receiverNsu.Actual++;
                receiverNsu.DocumentKey = globalDocValidatordocumentMeta.DocumentKey;
                receiverNsu.SenderCode = globalDocValidatordocumentMeta.SenderCode;
                receiverNsu.ReceiverCode = globalDocValidatordocumentMeta.ReceiverCode;
                receiverNsu.DianNsu = dianNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(receiverNsu);
                // set rowkey actual
                receiverNsu.RowKey = receiverNsu.Actual.ToString();
                nsuControlTableManager.InsertOrUpdate(receiverNsu);
                // set rowkey documet key
                receiverNsu.RowKey = receiverNsu.DocumentKey.ToString();
                nsuControlTableManager.InsertOrUpdate(receiverNsu);
            }
            // nsu for receiver
        }

        public static GlobalDocValidatorDocumentMeta GetGlobalDocValidatorDocumentMeta(string trackId)
        {
            try
            {
                return documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }
    }
}
