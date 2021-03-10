using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalTestSetOthersDocuments : TableEntity
    {
        public GlobalTestSetOthersDocuments()
        {

        }

        public GlobalTestSetOthersDocuments(string pk, string rk) : base(pk, rk)
        {
            // PartitionKey: Id del otro documento electrónico
            // RowKey: modo de operación
            Date = DateTime.UtcNow;

        }

        public string TestSetId { get; set; }

        public int ElectronicDocumentId { get; set; }
        public string OperationModeId { get; set; }
        public string Description { get; set; }

        public int TotalDocumentRequired { get; set; }

        public int OthersDocumentsRequired { get; set; }

        /// <summary>
        /// Nomina electrónica de Ajuste
        /// </summary>
        public int ElectronicPayrollAjustmentRequired { get; set; }


        public int TotalDocumentAcceptedRequired { get; set; }

        public int OthersDocumentsAcceptedRequired { get; set; }

        /// <summary>
        /// Nomina electrónica de Ajuste Aceptada requerida
        /// </summary>
        public int ElectronicPayrollAjustmentAcceptedRequired { get; set; }

        public DateTime Date { get; set; }

        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }
        public int Status { get; set; }
        public bool Deleted { get; set; }
        public string Id { get; set; }
    }
}