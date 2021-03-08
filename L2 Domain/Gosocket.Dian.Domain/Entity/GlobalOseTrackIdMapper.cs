using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalOseTrackIdMapper : TableEntity
    {
        public GlobalOseTrackIdMapper() { }

        public GlobalOseTrackIdMapper(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey; // file name
            RowKey = rowKey; // track id
        }
    }
}
