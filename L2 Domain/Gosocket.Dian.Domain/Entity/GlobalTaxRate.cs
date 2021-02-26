using Microsoft.WindowsAzure.Storage.Table;
namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalTaxRate : TableEntity
    {
        public GlobalTaxRate() { }

        public GlobalTaxRate(string pk, string rk) : base(pk, rk)
        {

        }

        public string Concepto { get; set; }
        public string TaxSchemeName { get; set; }
    }
}
