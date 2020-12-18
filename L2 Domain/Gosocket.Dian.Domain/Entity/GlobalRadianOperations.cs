using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalRadianOperations : TableEntity
    {
        public GlobalRadianOperations() { }

        public GlobalRadianOperations(string code, string softwareId) : base(code, softwareId)
        {
            PartitionKey = code; // track id zip
            RowKey = softwareId; // track id xml
        }

        public bool ElectronicInvoicer { get; set; }

        public bool TecnologicalSupplier { get; set; }

        public bool NegotiationSystem { get; set; }

        public bool Factor { get; set; }

        public bool IndirectElectronicInvoicer { get; set; }

        public int RadianContributorTypeId { get; set; }

        public int SoftwareType { get; set; }

        public string RadianState { get; set; }

        public bool Deleted { get; set; }


    }
}
