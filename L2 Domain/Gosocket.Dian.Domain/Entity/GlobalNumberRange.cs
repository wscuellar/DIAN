using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalNumberRange : TableEntity
    {
        public GlobalNumberRange() { }
        public GlobalNumberRange(string partitionKey, string rowKey) :base(partitionKey, rowKey)
        {
            Active = true;
        }

        public bool Active { get; set; }
        public string Serie { get; set; }
        public string ResolutionNumber { get; set; }
        public DateTime? AssociationDate { get; set; }
        public DateTime ResolutionDate { get; set; }
        public int ValidDateNumberFrom { get; set; }
        public int ValidDateNumberTo { get; set; }
        public string TechnicalKey { get; set; }
        public long FromNumber { get; set; }
        public long ToNumber { get; set; }
        public string SoftwareId { get; set; }
        public string SoftwareName { get; set; }
        public string SoftwareOwnerCode { get; set; }
        public string SoftwareOwnerName { get; set; }
        public long State { get; set; }
    }
}
