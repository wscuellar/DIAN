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

        // Acuse de recibo
        public int ReceiptNoticeTotalRequired { get; set; }
        public int ReceiptNoticeTotalAcceptedRequired { get; set; }
        public int TotalReceiptNoticeSent { get; set; }
        public int ReceiptNoticeAccepted { get; set; }
        public int ReceiptNoticeRejected { get; set; }

        //Recibo del bien
        public int ReceiptServiceTotalRequired { get; set; }
        public int ReceiptServiceTotalAcceptedRequired { get; set; }
        public int TotalReceiptServiceSent { get; set; }
        public int ReceiptServiceAccepted { get; set; }
        public int ReceiptServiceRejected { get; set; }

        // Aceptación expresa
        public int ExpressAcceptanceTotalRequired { get; set; }
        public int ExpressAcceptanceTotalAcceptedRequired { get; set; }
        public int TotalExpressAcceptanceSent { get; set; }
        public int ExpressAcceptanceAccepted { get; set; }
        public int ExpressAcceptanceRejected { get; set; }

        //Manifestación de aceptación
        public int AutomaticAcceptanceTotalRequired { get; set; }
        public int AutomaticAcceptanceTotalAcceptedRequired { get; set; }
        public int TotalAutomaticAcceptanceSent { get; set; }
        public int AutomaticAcceptanceAccepted { get; set; }
        public int AutomaticAcceptanceRejected { get; set; }

        //Rechazo factura electrónica
        public int RejectInvoiceTotalRequired { get; set; }
        public int RejectInvoiceTotalAcceptedRequired { get; set; }
        public int TotalRejectInvoiceSent { get; set; }
        public int RejectInvoiceAccepted { get; set; }
        public int RejectInvoiceRejected { get; set; }

        // Solicitud disponibilización
        public int ApplicationAvailableTotalRequired { get; set; }
        public int ApplicationAvailableTotalAcceptedRequired { get; set; }
        public int TotalApplicationAvailableSent { get; set; }
        public int ApplicationAvailableAccepted { get; set; }
        public int ApplicationAvailableRejected { get; set; }

        // Endoso electrónico
        public int EndorsementTotalRequired { get; set; }
        public int EndorsementTotalAcceptedRequired { get; set; }
        public int TotalEndorsementSent { get; set; }
        public int EndorsementAccepted { get; set; }
        public int EndorsementRejected { get; set; }

        // Cancelación de endoso
        public int EndorsementCancellationTotalRequired { get; set; }
        public int EndorsementCancellationTotalAcceptedRequired { get; set; }
        public int TotalEndorsementCancellationSent { get; set; }
        public int EndorsementCancellationAccepted { get; set; }
        public int EndorsementCancellationRejected { get; set; }

        // Avales
        public int GuaranteeTotalRequired { get; set; }
        public int GuaranteeTotalAcceptedRequired { get; set; }
        public int TotalGuaranteeSent { get; set; }
        public int GuaranteeAccepted { get; set; }
        public int GuaranteeRejected { get; set; }

        // Mandato electrónico
        public int ElectronicMandateTotalRequired { get; set; }
        public int ElectronicMandateTotalAcceptedRequired { get; set; }
        public int TotalElectronicMandateSent { get; set; }
        public int ElectronicMandateAccepted { get; set; }
        public int ElectronicMandateRejected { get; set; }

        // Terminación mandato
        public int EndMandateTotalRequired { get; set; }
        public int EndMandateTotalAcceptedRequired { get; set; }
        public int TotalEndMandateSent { get; set; }
        public int EndMandateAccepted { get; set; }
        public int EndMandateRejected { get; set; }

        // Notificación de pago
        public int PaymentNotificationTotalRequired { get; set; }
        public int PaymentNotificationTotalAcceptedRequired { get; set; }
        public int TotalPaymentNotificationSent { get; set; }
        public int PaymentNotificationAccepted { get; set; }
        public int PaymentNotificationRejected { get; set; }

        // Limitación de circulación
        public int CirculationLimitationTotalRequired { get; set; }
        public int CirculationLimitationTotalAcceptedRequired { get; set; }
        public int TotalCirculationLimitationSent { get; set; }
        public int CirculationLimitationAccepted { get; set; }
        public int CirculationLimitationRejected { get; set; }

        // Terminación limitación  
        public int EndCirculationLimitationTotalRequired { get; set; }
        public int EndCirculationLimitationTotalAcceptedRequired { get; set; }
        public int TotalEndCirculationLimitationSent { get; set; }
        public int EndCirculationLimitationAccepted { get; set; }
        public int EndCirculationLimitationRejected { get; set; }


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