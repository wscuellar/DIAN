using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalBatchFileResult : TableEntity
    {
        public GlobalBatchFileResult() { }

        public GlobalBatchFileResult(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey; // track id zip
            RowKey = rowKey; // track id xml
        }

        public string FileName { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string TrackId { get; set; }
    }
}
