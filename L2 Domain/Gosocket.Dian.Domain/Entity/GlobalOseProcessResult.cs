using Microsoft.WindowsAzure.Storage.Table;
using System;


namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalOseProcessResult : TableEntity
    {
        public GlobalOseProcessResult()
        {

        }

        public GlobalOseProcessResult(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey; // track id
            RowKey = rowKey; // track id
        }

        public string TrackId { get; set; }

        public string FileName { get; set; }

        public string SenderCode { get; set; }

        public DateTime EmisionDate { get; set; }

        public string ReceiverCode { get; set; }

        public int DocumentType { get; set; }

        public string SerieNumber { get; set; }

        public string TransactionId { get; set; }

        public DateTime ProcessDate { get; set; }

        public bool? XmlUploaded { get; set; }

        public bool? SuccessValidator { get; set; }

        public bool? SuccessOutput { get; set; }

        public int OutputRetry { get; set; }
    }
}
