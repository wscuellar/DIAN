using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocModelFolio : TableEntity
    {
        public GlobalDocModelFolio()
        {

        }

        public GlobalDocModelFolio(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey; // AccountCode
            RowKey = rowKey; // TipoDoc_Serie_
        }

        public GlobalDocModelFolio(string partitionKey, string rowKey, long initialRange, long finalRange, long currentRange)
        {
            PartitionKey = partitionKey; // AccountCode
            RowKey = rowKey; // TipoDoc_Serie_

            FromNumber = initialRange;
            ToNumber = finalRange;
        }
        
        public Int64 FromNumber { get; set; }
        public Int64 ToNumber { get; set; }
        public string ResolutionNumber { get; set; }
        public string TechnicalKey { get; set; }
        public DateTime ValidDateTimeFrom { get; set; }
        public DateTime ValidDateTimeTo { get; set; }
        public DateTime ResolutionDateTime { get; set; }
    }
}
