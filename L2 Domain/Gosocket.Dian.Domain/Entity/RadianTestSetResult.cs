using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Documentos (Total)")]
        public int TotalDocumentRequired { get; set; }
        
        [Display(Name = "Documentos (Total)")]
        public int TotalDocumentAcceptedRequired { get; set; }
        
        public int TotalDocumentSent { get; set; }
        public int TotalDocumentAccepted { get; set; }
        public int TotalDocumentsRejected { get; set; }

        // Acuse de recibo
        /// <summary>
        /// Acuse de recibo - Total requerido
        /// </summary>
        [Display(Name = "Acuse de recibo de Factura Electrónica de Venta")]
        public int ReceiptNoticeTotalRequired { get; set; }
        /// <summary>
        /// Acuse de recibo - Total requerido aceptado
        /// </summary>
        [Display(Name = "Acuse de recibo de Factura Electrónica de Venta")]
        public int ReceiptNoticeTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Acuse de recibo - Total documentos enviados
        /// </summary>
        public int TotalReceiptNoticeSent { get; set; }
        /// <summary>
        /// Acuse de recibo - Aceptados
        /// </summary>
        public int ReceiptNoticeAccepted { get; set; }
        /// <summary>
        /// Acuse de recibo - Rechazados
        /// </summary>
        public int ReceiptNoticeRejected { get; set; }

        /// <summary>
        /// Recibo del bien - Total requerido
        /// </summary>
        [Display(Name = "Recibo del bien y/o prestación del servicio")]
        public int ReceiptServiceTotalRequired { get; set; }
        /// <summary>
        /// Recibo del bien - Total aceptado requerido
        /// </summary>
        [Display(Name = "Recibo del bien y/o prestación del servicio")]
        public int ReceiptServiceTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Recibo del bien - Total enviado
        /// </summary>
        public int TotalReceiptServiceSent { get; set; }
        /// <summary>
        /// Recibo del bien - Aceptados
        /// </summary>
        public int ReceiptServiceAccepted { get; set; }
        /// <summary>
        /// Recibo del bien - Rechazados
        /// </summary>
        public int ReceiptServiceRejected { get; set; }

        // 
        /// <summary>
        /// Aceptación expresa - Total requerido
        /// </summary>
        [Display(Name = "Aceptación expresa")]
        public int ExpressAcceptanceTotalRequired { get; set; }
        /// <summary>
        /// Aceptación expresa - Total aceptado
        /// </summary>
        [Display(Name = "Aceptación expresa")]
        public int ExpressAcceptanceTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Aceptación expresa - Total enviados
        /// </summary>
        public int TotalExpressAcceptanceSent { get; set; }
        /// <summary>
        /// Aceptación expresa - Aceptados
        /// </summary>
        public int ExpressAcceptanceAccepted { get; set; }
        /// <summary>
        /// Aceptación expresa - Rechazados
        /// </summary>
        public int ExpressAcceptanceRejected { get; set; }

        //
        /// <summary>
        /// Manifestación de aceptación - Total requerido
        /// </summary>
        [Display(Name = "Aceptación tácita ")]
        public int AutomaticAcceptanceTotalRequired { get; set; }
        /// <summary>
        /// Manifestación de aceptación - Total aceptado
        /// </summary>
        [Display(Name = "Aceptación tácita ")]
        public int AutomaticAcceptanceTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Manifestación de aceptación - Total enviado
        /// </summary>
        public int TotalAutomaticAcceptanceSent { get; set; }
        /// <summary>
        /// Manifestación de aceptación - Aceptados
        /// </summary>
        public int AutomaticAcceptanceAccepted { get; set; }
        /// <summary>
        /// Manifestación de aceptación - Rechazados
        /// </summary>
        public int AutomaticAcceptanceRejected { get; set; }

        //
        /// <summary>
        /// Rechazo factura electrónica - Total Requerido
        /// </summary>
        [Display(Name = "Rechazo de la factura electrónica")]
        public int RejectInvoiceTotalRequired { get; set; }
        /// <summary>
        /// Rechazo factura electrónica - Total Aceptado
        /// </summary>
        [Display(Name = "Rechazo de la factura electrónica")]
        public int RejectInvoiceTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Rechazo factura electrónica - Total Enviiados
        /// </summary>
        public int TotalRejectInvoiceSent { get; set; }
        /// <summary>
        /// Rechazo factura electrónica - Aceptados
        /// </summary>
        public int RejectInvoiceAccepted { get; set; }
        /// <summary>
        /// Rechazo factura electrónica - Rechazados
        /// </summary>
        public int RejectInvoiceRejected { get; set; }

        // 
        /// <summary>
        /// Solicitud disponibilización - Total requerido
        /// </summary>
        [Display(Name = "Inscripción de la factura electrónica de venta como título valor - RADIAN")]
        public int ApplicationAvailableTotalRequired { get; set; }
        /// <summary>
        /// Solicitud disponibilización - Total Aceptados
        /// </summary>
        [Display(Name = "Inscripción de la factura electrónica de venta como título valor - RADIAN")]
        public int ApplicationAvailableTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Solicitud disponibilización - Total Enviados
        /// </summary>
        public int TotalApplicationAvailableSent { get; set; }
        /// <summary>
        /// Solicitud disponibilización - Aceptados
        /// </summary>
        public int ApplicationAvailableAccepted { get; set; }
        /// <summary>
        /// Solicitud disponibilización - Rechazados
        /// </summary>
        public int ApplicationAvailableRejected { get; set; }


        //
        /// <summary>
        ///  Endoso en Propiedad - Total Requerido
        /// </summary>
        [Display(Name = "Endoso en propiedad ")]
        public int EndorsementPropertyTotalRequired { get; set; }
        /// <summary>
        ///  Endoso en Propiedad - Total Aceptado
        /// </summary>
        [Display(Name = "Endoso en propiedad ")]
        public int EndorsementPropertyTotalAcceptedRequired { get; set; }
        /// <summary>
        ///  Endoso en Propiedad - Total Enviados
        /// </summary>
        public int TotalEndorsementPropertySent { get; set; }
        /// <summary>
        ///  Endoso en Propiedad - Aceptados
        /// </summary>
        public int EndorsementPropertyAccepted { get; set; }
        /// <summary>
        ///  Endoso en Propiedad - rechazados
        /// </summary>
        public int EndorsementPropertyRejected { get; set; }

        // 
        /// <summary>
        /// Endoso en Procuracion - Total requerido
        /// </summary>
        [Display(Name = "Endoso en procuración")]
        public int EndorsementProcurementTotalRequired { get; set; }

        public int EndorsementTotalRequired { get; set; }
        /// <summary>
        /// Endoso en Procuracion - Total Aceptado
        /// </summary>
        [Display(Name = "Endoso en procuración")]
        public int EndorsementProcurementTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Endoso en Procuracion - Total Enviados
        /// </summary>
        public int TotalEndorsementProcurementSent { get; set; }
        /// <summary>
        /// Endoso en Procuracion - Aceptados
        /// </summary>
        public int EndorsementProcurementAccepted { get; set; }
        /// <summary>
        /// Endoso en Procuracion - Rechazados
        /// </summary>
        public int EndorsementProcurementRejected { get; set; }

        // 
        /// <summary>
        /// Endoso en Garantia - Total Requerido
        /// </summary>
        [Display(Name = "Endoso en garantía          ")]
        public int EndorsementGuaranteeTotalRequired { get; set; }
        /// <summary>
        /// Endoso en Garantia - Total Aceptados
        /// </summary>
        [Display(Name = "Endoso en garantía          ")]
        public int EndorsementGuaranteeTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Endoso en Garantia - Total Enviados
        /// </summary>
        public int TotalEndorsementGuaranteeSent { get; set; }
        /// <summary>
        /// Endoso en Garantia - Aceptados
        /// </summary>
        public int EndorsementGuaranteeAccepted { get; set; }
        /// <summary>
        /// Endoso en Garantia - Rechazados
        /// </summary>
        public int EndorsementGuaranteeRejected { get; set; }

        // 
        /// <summary>
        /// Cancelación de endoso - Total requerido
        /// </summary>
        [Display(Name = "Cancelación de endoso")]
        public int EndorsementCancellationTotalRequired { get; set; }
        /// <summary>
        /// Cancelación de endoso - Total Aceptado
        /// </summary>
        [Display(Name = "Cancelación de endoso")]
        public int EndorsementCancellationTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Cancelación de endoso - Total Enviados
        /// </summary>
        public int TotalEndorsementCancellationSent { get; set; }
        /// <summary>
        /// Cancelación de endoso - Aceptados
        /// </summary>
        public int EndorsementCancellationAccepted { get; set; }
        /// <summary>
        /// Cancelación de endoso - Rechazados
        /// </summary>
        public int EndorsementCancellationRejected { get; set; }

        // 
        /// <summary>
        /// Avales - Total Requerido
        /// </summary>
        [Display(Name = "Aval")]
        public int GuaranteeTotalRequired { get; set; }
        /// <summary>
        /// Avales - Total Aceptados
        /// </summary>
        [Display(Name = "Aval")]
        public int GuaranteeTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Avales - Total Enviados
        /// </summary>
        public int TotalGuaranteeSent { get; set; }
        /// <summary>
        /// Avales - Total Aceptados
        /// </summary>
        public int GuaranteeAccepted { get; set; }
        /// <summary>
        /// Avales - Total Rechazados
        /// </summary>
        public int GuaranteeRejected { get; set; }

        // 
        /// <summary>
        /// Mandato electrónico - Total requerido
        /// </summary>
        [Display(Name = "Mandato")]
        public int ElectronicMandateTotalRequired { get; set; }
        /// <summary>
        /// Mandato electrónico - Total Aceptada requerida
        /// </summary>
        [Display(Name = "Mandato")]
        public int ElectronicMandateTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Mandato electrónico - Total Enviado
        /// </summary>
        public int TotalElectronicMandateSent { get; set; }
        /// <summary>
        /// Mandato electrónico - Total Aceptados
        /// </summary>
        public int ElectronicMandateAccepted { get; set; }
        /// <summary>
        /// Mandato electrónico - Total Rechazados
        /// </summary>
        public int ElectronicMandateRejected { get; set; }

        // 
        /// <summary>
        /// Terminación mandato - Total requerido
        /// </summary>
        [Display(Name = "Terminación del mandato")]
        public int EndMandateTotalRequired { get; set; }
        /// <summary>
        /// Terminación mandato - Total aceptado requerido
        /// </summary>
        [Display(Name = "Terminación del mandato")]
        public int EndMandateTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Terminación mandato - Total enviados 
        /// </summary>
        public int TotalEndMandateSent { get; set; }
        /// <summary>
        /// Terminación mandato - Total Aceptados
        /// </summary>
        public int EndMandateAccepted { get; set; }
        /// <summary>
        /// Terminación mandato - Total Rechazados
        /// </summary>
        public int EndMandateRejected { get; set; }

        // 
        /// <summary>
        /// Notificación de pago - Total requerido
        /// </summary>
        [Display(Name = "Pago de la factura electrónica de venta como título valor")]
        public int PaymentNotificationTotalRequired { get; set; }
        /// <summary>
        /// Notificación de pago - Total aceptado requerido
        /// </summary>
        [Display(Name = "Pago de la factura electrónica de venta como título valor")]
        public int PaymentNotificationTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Notificación de pago - Total enviados
        /// </summary>
        public int TotalPaymentNotificationSent { get; set; }
        /// <summary>
        /// Notificación de pago - Total Aceptados
        /// </summary>
        public int PaymentNotificationAccepted { get; set; }
        /// <summary>
        /// Notificación de pago - Total rechazados
        /// </summary>
        public int PaymentNotificationRejected { get; set; }

        // 
        /// <summary>
        /// Limitación de circulación - Total Requerido
        /// </summary>
        [Display(Name = "Limitaciones a la circulación de la factura electrónica de venta como título")]
        public int CirculationLimitationTotalRequired { get; set; }
        /// <summary>
        /// Limitación de circulación - Total Aceptados Requeridos
        /// </summary>
        [Display(Name = "Limitaciones a la circulación de la factura electrónica de venta como título")]
        public int CirculationLimitationTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Limitación de circulación - Total Enviados
        /// </summary>
        public int TotalCirculationLimitationSent { get; set; }
        /// <summary>
        /// Limitación de circulación - Total Aceptados
        /// </summary>
        public int CirculationLimitationAccepted { get; set; }
        /// <summary>
        /// Limitación de circulación - Total rechazados
        /// </summary>
        public int CirculationLimitationRejected { get; set; }

        //  
        /// <summary>
        /// Terminación limitación - Total Requerido
        /// </summary>
        [Display(Name = "Terminación de las limitaciones a la circulación de la factura electrónica de venta como título")]
        public int EndCirculationLimitationTotalRequired { get; set; }
        /// <summary>
        /// Terminación limitación - Total Aceptados Requeridos
        /// </summary>
        [Display(Name = "Terminación de las limitaciones a la circulación de la factura electrónica de venta como título")]
        public int EndCirculationLimitationTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Terminación limitación - Total Enviados
        /// </summary>
        public int TotalEndCirculationLimitationSent { get; set; }
        /// <summary>
        /// Terminación limitación - Total Aceptados
        /// </summary>
        public int EndCirculationLimitationAccepted { get; set; }
        /// <summary>
        /// Terminación limitación - Total Rechazados
        /// </summary>
        public int EndCirculationLimitationRejected { get; set; }

        /// <summary>
        /// Endoso - Total aceptados requeridos
        /// </summary>
        [Display(Name = "Endoso en propiedad ")]
        public int EndorsementTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Endoso en garantía - Total requeridos
        /// </summary>
        [Display(Name = "Endoso en garantía          ")]
        public int EndorsementWarrantyTotalRequired { get; set; }
        /// <summary>
        /// Endoso en garantía - Total aceptados requeridos
        /// </summary>
        [Display(Name = "Endoso en garantía          ")]
        public int EndorsementWarrantyTotalAcceptedRequired { get; set; }
        /// <summary>
        /// Endoso en procuración - Total aceptados requeridos
        /// </summary>
        [Display(Name = "Endoso en procuración         ")]
        public int EndorsementProcurationTotalRequired { get; set; }


        /// <summary>
        /// Reporte por pago - Total aceptados requeridos (3 dias)
        /// </summary>
        [Display(Name = "Informe para el pago")]
        public int ReportForPaymentTotalAcceptedRequired { get; set; }

        /// <summary>
        /// Reporte por pago - Total requeridos (3 dias)
        /// </summary>
        [Display(Name = "Informe para el pago")]
        public int ReportForPaymentTotalRequired { get; set; }

        /// <summary>
        /// Reporte por pago - Aceptado (3 dias)
        /// </summary>
        public int ReportForPaymentAccepted { get; set; }

        /// <summary>
        /// Reporte por pago - Rechazados (3 dias)
        /// </summary>
        public int ReportForPaymentRejected { get; set; }

        /// <summary>
        /// Reporte por pago  - Total Enviados (3 dias)
        /// </summary>
        public int TotalReportForPaymentSent { get; set; }

        //Transferencia de los derechos económicos 
        [Display(Name = "Transferencia de los derechos económicos")]
        public int TransferEconomicRightsTotalRequired { get; set; }
        [Display(Name = "Transferencia de los derechos económicos")]
        public int TransferEconomicRightsTotalAcceptedRequired { get; set; }
        public int TransferEconomicRightsAccepted { get; set; }
        public int TransferEconomicRightsRejected { get; set; }
        public int TotalTransferEconomicRightsSent { get; set; }

        //Notificación al deudor sobre la transferencia de los derechos económicos 
        [Display(Name = "Notificación al deudor sobre la transferencia de los derechos económicos")]
        public int NotificationDebtorOfTransferEconomicRightsTotalRequired { get; set; }
        [Display(Name = "Notificación al deudor sobre la transferencia de los derechos económicos")]
        public int NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired { get; set; }
        public int NotificationDebtorOfTransferEconomicRightsAccepted { get; set; }
        public int NotificationDebtorOfTransferEconomicRightsRejected { get; set; }
        public int TotalNotificationDebtorOfTransferEconomicRightsSent { get; set; }

        //Pago de la transferencia de los derechos económicos 
        [Display(Name = "Pago de la transferencia de los derechos económicos")]
        public int PaymentOfTransferEconomicRightsTotalRequired { get; set; }
        [Display(Name = "Pago de la transferencia de los derechos económicos")]
        public int PaymentOfTransferEconomicRightsTotalAcceptedRequired { get; set; }
        public int PaymentOfTransferEconomicRightsAccepted { get; set; }
        public int PaymentOfTransferEconomicRightsRejected { get; set; }
        public int TotalPaymentOfTransferEconomicRightsSent { get; set; }

        //Endoso con efectos de cesión ordinaria
        [Display(Name = "Endoso con efectos de cesión ordinaria")]
        public int EndorsementWithEffectOrdinaryAssignmentTotalRequired { get; set; }
        [Display(Name = "Endoso con efectos de cesión ordinaria")]
        public int EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired { get; set; }
        public int EndorsementWithEffectOrdinaryAssignmentAccepted { get; set; }
        public int EndorsementWithEffectOrdinaryAssignmentRejected { get; set; }
        public int TotalEndorsementWithEffectOrdinaryAssignmentSent { get; set; }

        //Protesto
        [Display(Name = "Protesto")]
        public int ObjectionTotalRequired { get; set; }
        [Display(Name = "Protesto")]
        public int ObjectionTotalAcceptedRequired { get; set; }
        public int ObjectionAccepted { get; set; }
        public int ObjectionRejected { get; set; }
        public int TotalObjectionSent { get; set; }

        public string StatusDescription { get; set; }
        public int Status { get; set; }
        public bool Deleted { get; set; }
        public string Id { get; set; }
        // Estado: En Proceso, etc.
        public string State { get; set; }
    }
}
