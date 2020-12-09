using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class RadianTestSet : TableEntity
    {
        public RadianTestSet() { }

        public RadianTestSet(string pk, string rk) : base(pk, rk)
        {
            Date = DateTime.UtcNow;
        }

        public string TestSetId { get; set; }

        public string Description { get; set; }

        public int TotalDocumentRequired { get; set; }
        public int TotalDocumentAcceptedRequired { get; set; }

        public int ReceiptNoticeTotalRequired { get; set; }
        public int ReceiptNoticeTotalAcceptedRequired { get; set; }

        public int ReceiptServiceTotalRequired { get; set; }
        public int ReceiptServiceTotalAcceptedRequired { get; set; }

        public int ExpressAcceptanceTotalRequired { get; set; }
        public int ExpressAcceptanceTotalAcceptedRequired { get; set; }

        public int AutomaticAcceptanceTotalRequired { get; set; }
        public int AutomaticAcceptanceTotalAcceptedRequired { get; set; }

        public int RejectInvoiceTotalRequired { get; set; }
        public int RejectInvoiceTotalAcceptedRequired { get; set; }

        public int ApplicationAvailableTotalRequired { get; set; }
        public int ApplicationAvailableTotalAcceptedRequired { get; set; }

        public int EndorsementTotalRequired { get; set; }
        public int EndorsementTotalAcceptedRequired { get; set; }

        public int EndorsementCancellationTotalRequired { get; set; }
        public int EndorsementCancellationTotalAcceptedRequired { get; set; }

        public int GuaranteeTotalRequired { get; set; }
        public int GuaranteeTotalAcceptedRequired { get; set; }

        public int ElectronicMandateTotalRequired { get; set; }
        public int ElectronicMandateTotalAcceptedRequired { get; set; }

        public int EndMandateTotalRequired { get; set; }
        public int EndMandateTotalAcceptedRequired { get; set; }

        public int PaymentNotificationTotalRequired { get; set; }
        public int PaymentNotificationTotalAcceptedRequired { get; set; }

        public int CirculationLimitationTotalRequired { get; set; }
        public int CirculationLimitationTotalAcceptedRequired { get; set; }

        public int EndCirculationLimitationTotalRequired { get; set; }
        public int EndCirculationLimitationTotalAcceptedRequired { get; set; }

        public DateTime Date { get; set; }

        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }
    }
}
