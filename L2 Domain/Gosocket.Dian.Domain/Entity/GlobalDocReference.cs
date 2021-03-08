using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocReference : TableEntity
    {
        public GlobalDocReference() { }

        public GlobalDocReference(string pk, string rk) : base(pk, rk)
        {
            // PartitionKey represent referenced document
            // RowKey dinamic: Note, Invoice, etc...
        }

        public string AccountCode { get; set; }
        public string DocumentKey { get; set; }
        public int DateNumber { get; set; }
        public string DocumentTypeName { get; set; }
    }
}
