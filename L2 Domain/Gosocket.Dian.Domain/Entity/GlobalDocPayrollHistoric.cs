using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocPayrollHistoric : TableEntity
    {
        public GlobalDocPayrollHistoric() { }

        public GlobalDocPayrollHistoric(string pk, string rk) : base(pk, rk)
        { }

        public string DocumentTypeId { get; set; }
        public bool Deleted { get; set; }
    }
}
