using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class RadianTestSetViewModel
    {

        public string TestSetId { get; set; }
        public int Status { get; set; }
        public bool TestSetReplace { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        public int OperationModeId { get; set; }
        public string OperationModeName { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documentos")]
        public int TotalDocumentRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documentos aceptados")]
        public int TotalDocumentAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Acuse de recibo")]
        public int ReceiptNoticeTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Acuse de recibo aceptados")]
        public int ReceiptNoticeTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Recibo del bien ")]
        public int ReceiptServiceTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Recibo del bien ")]
        public int ReceiptServiceTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Aceptación expresa")]
        public int ExpressAcceptanceTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Aceptación expresa")]
        public int ExpressAcceptanceTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Manifestación de aceptación")]
        public int AutomaticAcceptanceTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Manifestación de aceptación")]
        public int AutomaticAcceptanceTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Rechazo factura electrónica")]
        public int RejectInvoiceTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Rechazo factura electrónica")]
        public int RejectInvoiceTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Solicitud de disponibilización")]
        public int ApplicationAvailableTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Solicitud de disponibilización")]
        public int ApplicationAvailableTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Endoso electrónico")]
        public int EndorsementTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Endoso electrónico")]
        public int EndorsementTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Cancelación de endoso")]
        public int EndorsementCancellationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Cancelación de endoso")]
        public int EndorsementCancellationTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Avales")]
        public int GuaranteeTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Avales")]
        public int GuaranteeTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Mandato electrónico")]
        public int ElectronicMandateTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Manadato electrónico")]
        public int ElectronicMandateTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Terminación mandato")]
        public int EndMandateTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Terminación manadato")]
        public int EndMandateTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Notificación de pago")]
        public int PaymentNotificationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Notificación pago")]
        public int PaymentNotificationTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Limitación de circulación")]
        public int CirculationLimitationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Limitación de circulación")]
        public int CirculationLimitationTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Terminación limitación")]
        public int EndCirculationLimitationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Terminación limitación")]
        public int EndCirculationLimitationTotalAcceptedRequired { get; set; }

        public DateTime Date { get; set; }

        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }

        public List<OperationModeViewModel> GetOperationModes()
        {
            return new List<OperationModeViewModel>
            {
                new OperationModeViewModel{ Id = 1, Name = "Software Propio" },
                new OperationModeViewModel{ Id = 2, Name = "Software de un Proveedor Tecnológico" },
                new OperationModeViewModel{ Id = 3, Name = "Software de un Sistema de Negociación" },
                new OperationModeViewModel{ Id = 4, Name = "Software de un Factor" }
            };
        }
    }
}