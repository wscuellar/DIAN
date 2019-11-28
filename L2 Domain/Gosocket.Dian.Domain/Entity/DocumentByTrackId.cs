using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class DocumentByTrackId : TableEntity
    {
        public DocumentByTrackId() { }

        public DocumentByTrackId(string partitionKey, string rowKey, string trackId, string cdrContainer, string cdrFileName)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            TrackId = trackId;
            CdrContainer = cdrContainer;
            CdrFileName = cdrFileName;
        }

        public string TrackId { get; set; }
        public string CdrFileName { get; set; }
        public string CdrContainer { get; set; }
    }
}
