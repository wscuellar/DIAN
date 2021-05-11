using Gosocket.Dian.Infrastructure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Common
{
    public class AzureTableManager
    {
        private static readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");

        private static Lazy<CloudTableClient> lazyClient = new Lazy<CloudTableClient>(InitializeTableClient);
        public static CloudTableClient tableClient => lazyClient.Value;

        private static CloudTableClient InitializeTableClient()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var tableClient = account.CreateCloudTableClient();
            return tableClient;
        }

        public static GlobalDocValidatorDocumentMeta GetGlobalDocValidatorDocumentMeta(string trackId)
        {
            return documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
        }

        static readonly int maxBatch = 100;

        public static CloudTable GetTableRef(string nameTable)
        {
            CloudTable tableRef = null;            
            tableRef = tableClient.GetTableReference(nameTable);
            return tableRef;
        }

        public static async Task InsertOrUpdateBatchAsync<T>(IEnumerable<T> items, CloudTable table) where T : ITableEntity, new()
        {
            var offset = 0;
            while (offset < items.Count())
            {
                var batch = new TableBatchOperation();
                var rows = items.Skip(offset).Take(100);
                foreach (var row in rows)
                    batch.Add(TableOperation.InsertOrReplace(row));

                var result = await table.ExecuteBatchAsync(batch);
                offset += result.Count;
            }
        }

    }
}
