using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class TestSetTableViewModel
    {
        public TestSetTableViewModel()
        {
            TestSets = new List<TestSetViewModel>();
        }
        public List<TestSetViewModel> TestSets { get; set; }
    }

    public class TestSetViewModel
    {

        public string TestSetId { get; set; }
        public int Status { get; set; }
        public bool TestSetReplace { get; set; }

        [Required(ErrorMessage = "La descripción requerida")]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        public int OperationModeId { get; set; }
        public string OperationModeName { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documentos")]
        public int TotalDocumentRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documentos")]
        public int TotalDocumentAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Facturas electrónicas")]
        public int InvoicesTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Facturas electrónicas")]
        public int TotalInvoicesAcceptedRequired { get; set; }


        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Notas de crédito")]
        public int TotalCreditNotesRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Notas de crédito")]
        public int TotalCreditNotesAcceptedRequired { get; set; }


        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Notas de débito")]
        public int TotalDebitNotesRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Notas de débito")]
        public int TotalDebitNotesAcceptedRequired { get; set; }

        [Display(Name = "Prefijo")]
        public string RangePrefix { get; set; }
        [Display(Name = "Identificación")]
        public string SoftwareId { get; set; }
        [Display(Name = "Nombre")]
        public string SoftwareName { get; set; }
        [Display(Name = "Pin")]
        public string SoftwarePin { get; set; }
        [Display(Name = "Número Resolución")]
        public string RangeResolutionNumber { get; set; }
        [Display(Name = "Rango desde")]
        public long RangeFromNumber { get; set; }
        [Display(Name = "Rango hasta")]
        public long RangeToNumber { get; set; }
        [Display(Name = "Fecha desde")]
        public string RangeFromDate { get; set; }
        [Display(Name = "Fecha hasta")]
        public string RangeToDate { get; set; }
        [Display(Name = "Clave técnica")]
        public string RangeTechnicalKey { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [Display(Name = "Fecha de inicio")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "La fecha de término es requerida")]
        [Display(Name = "Fecha de término")]
        public DateTime EndDate { get; set; }

        public string StartDateString { get; set; }
        public string EndDateString { get; set; }

        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }

        public List<OperationModeViewModel> GetOperationModes()
        {
            return new List<OperationModeViewModel>
            {
                new OperationModeViewModel{ Id = 1, Name = "Software gratuito" },
                new OperationModeViewModel{ Id = 2, Name = "Software propio" },
                new OperationModeViewModel{ Id = 3, Name = "Software de un proveedor tecnológico" }
            };
        }

    }
    public class TestSetResultViewModel
    {
        public TestSetResultViewModel()
        {
            Page = 0;
            Length = 10;
            TestSets = new List<TestSetTrackingViewModel>();
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

        public List<TestSetTrackingViewModel> TestSets { get; set; }
    }
    public class TestSetTrackingViewModel
    {
        public Guid TestSetId { get; set; }
        public Guid TrackId { get; set; }
        public string DocumentNumber { get; set; }
        public string ReceiverCode { get; set; }
        public int TotalRules { get; set; }
        public int TotalRulesSuccessfully { get; set; }
        public int TotalRulesUnsuccessfully { get; set; }
        public int TotalMandatoryRulesUnsuccessfully { get; set; }
    }
}