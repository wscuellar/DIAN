using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocValidatorDocument : TableEntity
    {
        public GlobalDocValidatorDocument() { }

        public GlobalDocValidatorDocument(string pk, string rk) : base(pk, rk)
        {

        }

        public string DocumentTypeId { get; set; }
        public string GlobalDocumentId { get; set; }
        public string DocumentKey { get; set; }
        public string EmissionDateNumber { get; set; }
        public long ValidationStatus { get; set; }
        public string ValidationStatusName { get; set; }
    }
}
