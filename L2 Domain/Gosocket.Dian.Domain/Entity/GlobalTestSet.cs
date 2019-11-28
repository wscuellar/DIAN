using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalTestSet : TableEntity
    {
        public GlobalTestSet() { }

        public GlobalTestSet(string pk, string rk) : base(pk, rk)
        {
            // PartitionKey represent nit legal representative
            // RowKey represent nit client
            Date = DateTime.UtcNow;

        }
        

        public Guid TestSetId { get; set; }
        public string Description { get; set; }
        public int TotalDocumentRequired { get; set; }
        public int TotalDocumentAcceptedRequired { get; set; }

        public int InvoicesTotalRequired { get; set; }
        public int TotalInvoicesAcceptedRequired { get; set; }

        public int TotalCreditNotesRequired { get; set; }
        public int TotalCreditNotesAcceptedRequired { get; set; }

        public int TotalDebitNotesRequired { get; set; }
        public int TotalDebitNotesAcceptedRequired { get; set; }

        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }

    }
}
