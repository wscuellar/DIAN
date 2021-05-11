using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Services.Utils
{
    public class BatchInsertionUtils
    {
        private static Lazy<CloudTableClient> lazyClient = new Lazy<CloudTableClient>(InitializeBlobClient);
        public static CloudTableClient tableClient => lazyClient.Value;

        private static CloudTableClient InitializeBlobClient()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var tableClient = account.CreateCloudTableClient();
            return tableClient;
        }

        

        static readonly int maxBatch = 50;

        public static bool InsertOrUpdateValidations(List<GlobalBatchFileResult> items, string tableName)
        {
            bool result = true;
            var subLists = SplitIntoPartitionedSublists(items);

            Task.WaitAll(subLists.Select(list => Task.Factory.StartNew(() =>
            {
                try
                {
                    var tableBatchOperation = new TableBatchOperation();
                    var tableRef = GetTableRef(tableName);

                    foreach (var item in list)
                        tableBatchOperation.Add(TableOperation.InsertOrReplace(item));

                    tableRef.ExecuteBatch(tableBatchOperation);
                }

                catch (Exception e)
                {
                    result = false;
                }
            })).ToArray());

            return result;
        }

        private static IEnumerable<List<GlobalBatchFileResult>> SplitIntoPartitionedSublists(IEnumerable<GlobalBatchFileResult> items)
        {
            var itemsByPartion = new Dictionary<string, List<GlobalBatchFileResult>>();

            foreach (var item in items)
            {
                var partition = item.PartitionKey;
                if (!itemsByPartion.ContainsKey(partition))
                    itemsByPartion[partition] = new List<GlobalBatchFileResult>();

                item.PartitionKey = partition;
                item.ETag = "*";
                itemsByPartion[partition].Add(item);
            }

            //split into subsets
            var subLists = new List<List<GlobalBatchFileResult>>();

            foreach (var partition in itemsByPartion.Keys)
            {
                var partitionItems = itemsByPartion[partition];
                for (var i = 0; i < partitionItems.Count; i += maxBatch)
                    subLists.Add(partitionItems.Skip(i).Take(maxBatch).ToList());
            }
            return subLists;
        }

        private static CloudTable GetTableRef(string nameTable)
        {
            CloudTable tableRef = null;            
            tableRef = tableClient.GetTableReference(nameTable);
            return tableRef;
        }
    }
}
