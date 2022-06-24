using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocPayrollRegister : TableEntity
    {
        public GlobalDocPayrollRegister() { }

        public GlobalDocPayrollRegister(string pk, string rk) : base(pk, rk)
        {
            PartitionKey = pk;
            RowKey = rk;
        }

        public DateTime? FechaPagoFin { get; set; }
        public DateTime? FechaPagoInicio { get; set; }
        public string NumeroDocumento { get; set; }
        public string CUNE { get; set; }
        public string TipoXML { get; set; }
    }
}
