using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class RadianTestSetResult : TableEntity
    {
        public RadianTestSetResult() { }

        public RadianTestSetResult(string pk, string rk) : base(pk, rk)
        {
            // PartitionKey represent nit contributor
            // RowKey represent contributor type id and software id
        }

        public int ContributorId { get; set; }
        public string SenderCode { get; set; }
        public string SoftwareId { get; set; }
        public string ContributorTypeId { get; set; }
        public int OperationModeId { get; set; }
        public string OperationModeName { get; set; }
        public int? ProviderId { get; set; }
        public string TestSetReference { get; set; }

        public int TotalDocumentRequired { get; set; }
        public int TotalDocumentAcceptedRequired { get; set; }
        public int TotalDocumentSent { get; set; }
        public int TotalDocumentAccepted { get; set; }
        public int TotalDocumentsRejected { get; set; }

        public int InvoicesTotalRequired { get; set; }
        public int TotalInvoicesAcceptedRequired { get; set; }
        public int InvoicesTotalSent { get; set; }
        public int TotalInvoicesAccepted { get; set; }
        public int TotalInvoicesRejected { get; set; }

        public int TotalCreditNotesRequired { get; set; }
        public int TotalCreditNotesAcceptedRequired { get; set; }
        public int TotalCreditNotesSent { get; set; }
        public int TotalCreditNotesAccepted { get; set; }
        public int TotalCreditNotesRejected { get; set; }

        public int TotalDebitNotesRequired { get; set; }
        public int TotalDebitNotesAcceptedRequired { get; set; }
        public int TotalDebitNotesSent { get; set; }
        public int TotalDebitNotesAccepted { get; set; }
        public int TotalDebitNotesRejected { get; set; }

        public int ReceiptNoticeTotalRequired { get; set; }
        public int ReceiptNoticeTotalAcceptedRequired { get; set; }
        public int TotalReceiptNoticeSent { get; set; }
        public int ReceiptNoticeAccepted { get; set; }
        public int ReceiptNoticeRejected { get; set; }

        public int ReceiptServiceTotalRequired { get; set; }
        public int ReceiptServiceTotalAcceptedRequired { get; set; }
        public int TotalReceiptServiceSent { get; set; }
        public int ReceiptServiceAccepted { get; set; }
        public int ReceiptServiceRejected { get; set; }

        public int ExpressAcceptanceTotalRequired { get; set; }
        public int ExpressAcceptanceTotalAcceptedRequired { get; set; }
        public int TotalExpressAcceptanceSent { get; set; }
        public int ExpressAcceptanceAccepted { get; set; }
        public int ExpressAcceptanceRejected { get; set; }

        public int AutomaticAcceptanceTotalRequired { get; set; }
        public int AutomaticAcceptanceTotalAcceptedRequired { get; set; }
        public int TotalAutomaticAcceptanceSent { get; set; }
        public int AutomaticAcceptanceAccepted { get; set; }
        public int AutomaticAcceptanceRejected { get; set; }

        public int RejectInvoiceTotalRequired { get; set; }
        public int RejectInvoiceTotalAcceptedRequired { get; set; }
        public int TotalRejectInvoiceSent { get; set; }
        public int RejectInvoiceAccepted { get; set; }
        public int RejectInvoiceRejected { get; set; }

        public int ApplicationAvailableTotalRequired { get; set; }
        public int ApplicationAvailableTotalAcceptedRequired { get; set; }
        public int TotalApplicationAvailableSent { get; set; }
        public int ApplicationAvailableAccepted { get; set; }
        public int ApplicationAvailableRejected { get; set; }

        public int EndorsementTotalRequired { get; set; }
        public int EndorsementTotalAcceptedRequired { get; set; }
        public int TotalEndorsementSent { get; set; }
        public int EndorsementAccepted { get; set; }
        public int EndorsementRejected { get; set; }

        public int EndorsementCancellationTotalRequired { get; set; }
        public int EndorsementCancellationTotalAcceptedRequired { get; set; }
        public int TotalEndorsementCancellationSent { get; set; }
        public int EndorsementCancellationAccepted { get; set; }
        public int EndorsementCancellationRejected { get; set; }

        public int GuaranteeTotalRequired { get; set; }
        public int GuaranteeTotalAcceptedRequired { get; set; }
        public int TotalGuaranteeSent { get; set; }
        public int GuaranteeAccepted { get; set; }
        public int GuaranteeRejected { get; set; }

        public int ElectronicMandateTotalRequired { get; set; }
        public int ElectronicMandateTotalAcceptedRequired { get; set; }
        public int TotalElectronicMandateSent { get; set; }
        public int ElectronicMandateAccepted { get; set; }
        public int ElectronicMandateRejected { get; set; }

        public int EndMandateTotalRequired { get; set; }
        public int EndMandateTotalAcceptedRequired { get; set; }
        public int TotalEndMandateSent { get; set; }
        public int EndMandateAccepted { get; set; }
        public int EndMandateRejected { get; set; }

        public int PaymentNotificationTotalRequired { get; set; }
        public int PaymentNotificationTotalAcceptedRequired { get; set; }
        public int TotalPaymentNotificationSent { get; set; }
        public int PaymentNotificationAccepted { get; set; }
        public int PaymentNotificationRejected { get; set; }

        public int CirculationLimitationTotalRequired { get; set; }
        public int CirculationLimitationTotalAcceptedRequired { get; set; }
        public int TotalCirculationLimitationSent { get; set; }
        public int CirculationLimitationAccepted { get; set; }
        public int CirculationLimitationRejected { get; set; }

        public int EndCirculationLimitationTotalRequired { get; set; }
        public int EndCirculationLimitationTotalAcceptedRequired { get; set; }
        public int TotalEndCirculationLimitationSent { get; set; }
        public int EndCirculationLimitationAccepted { get; set; }
        public int EndCirculationLimitationRejected { get; set; }


        public string StatusDescription { get; set; }
        public int Status { get; set; }
        public bool Deleted { get; set; }
        public string Id { get; set; }
    }
}
