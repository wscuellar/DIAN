using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
