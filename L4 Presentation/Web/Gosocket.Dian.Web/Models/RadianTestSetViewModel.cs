﻿using System;
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
        [Display(Name = "Documentos (Total)")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int TotalDocumentRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Documentos (Total)      ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int TotalDocumentAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Acuse de recibo de la FEV       ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ReceiptNoticeTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Acuse de recibo de la FEV  ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ReceiptNoticeTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Recibo del bien            ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ReceiptServiceTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Recibo del bien            ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ReceiptServiceTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Aceptación expresa         ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ExpressAcceptanceTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Aceptación expresa          ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ExpressAcceptanceTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Aceptación tácita ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int AutomaticAcceptanceTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Aceptación tácita ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int AutomaticAcceptanceTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Rechazo de la FEV ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int RejectInvoiceTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Rechazo de la FEV ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int RejectInvoiceTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Solicitud disponibilización ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ApplicationAvailableTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Solicitud disponibilización ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ApplicationAvailableTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Endoso en propiedad ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndorsementTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Endoso en propiedad ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndorsementTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Endoso en garantía          ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndorsementWarrantyTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Endoso en garantía         ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndorsementWarrantyTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Endoso en procuración         ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndorsementProcurationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Endoso en procuración        ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndorsementProcurationTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Anulación del endoso       ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndorsementCancellationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Anulación del endoso       ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndorsementCancellationTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Avales                      ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int GuaranteeTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Avales                      ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int GuaranteeTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Mandato         ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ElectronicMandateTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Mandato        ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ElectronicMandateTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Terminación del mandato         ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndMandateTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Terminación del mandato        ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndMandateTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Notificación de pago        ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int PaymentNotificationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Notificación pago           ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int PaymentNotificationTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Limitación de circulación   ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int CirculationLimitationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Limitación de circulación   ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int CirculationLimitationTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Terminación de la limitación de circulación     ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndCirculationLimitationTotalRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Terminación de la limitación de circulación     ")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int EndCirculationLimitationTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Informe para el pago")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ReportForPaymentTotalAcceptedRequired { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        [Display(Name = "Informe para el pago")]
        [Range(0, short.MaxValue, ErrorMessage = "El valor {0} debe ser max 32767.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "El valor {0} debe ser numérico")]
        public int ReportForPaymentTotalRequired { get; set; }

        

        public DateTime Date { get; set; }

        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Active { get; set; }

        public List<OperationModeViewModel> OperationModes
        {
            get; set;
        }
    }
}