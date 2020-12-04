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

        public string SoftwareType { get; set; }

        public string RadianStatus { get; set; }
        
    }
}
