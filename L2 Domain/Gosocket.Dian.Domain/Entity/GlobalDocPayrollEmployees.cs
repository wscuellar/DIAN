using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocPayrollEmployees: TableEntity
    {
        public GlobalDocPayrollEmployees() { }

        public GlobalDocPayrollEmployees(string pk, string rk) : base(pk, rk)
        {
            PartitionKey = pk;
            RowKey = rk;
        }

        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string PrimerApellido { get; set; }
        public string PrimerNombre { get; set; }
        public string NitEmpresa { get; set; }
    }
}
