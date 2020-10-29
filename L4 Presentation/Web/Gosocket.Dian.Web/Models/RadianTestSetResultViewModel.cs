using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{
    public class RadianTestSetResultViewModel
    {
        public RadianTestSetResultViewModel()
        {
            Page = 0;
            Length = 10;
            RadianTestSets = new List<RadianTestSetTrackingViewModel>();
        }

        public int Page { get; set; }
        public int Length { get; set; }

        public int ContributorId { get; set; }
        public string ContributorCode { get; set; }
        public int OperationModeId { get; set; }
        public string OperationModeName { get; set; }
        public string SoftwareId { get; set; }

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

        public int Status { get; set; }
        public string StatusDescription { get; set; }
        public bool Deleted { get; set; }
        public string Id { get; set; }

        public List<RadianTestSetTrackingViewModel> RadianTestSets { get; set; }
    }
}