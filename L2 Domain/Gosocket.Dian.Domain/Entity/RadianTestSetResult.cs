﻿using Microsoft.WindowsAzure.Storage.Table;

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

        // Acuse de recibo
        /// <summary>
        /// Acuse de recibo - Total requerido
        /// </summary>
        public int ReceiptNoticeTotalRequired { get; set; }
        /// <summary>
        /// Acuse de recibo - Total requerido aceptado
        /// </summary>
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
        public int ReceiptServiceTotalRequired { get; set; }
        /// <summary>
        /// Recibo del bien - Total aceptado requerido
        /// </summary>
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
        public int ExpressAcceptanceTotalRequired { get; set; }
        /// <summary>
        /// Aceptación expresa - Total aceptado
        /// </summary>
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
        public int AutomaticAcceptanceTotalRequired { get; set; }
        /// <summary>
        /// Manifestación de aceptación - Total aceptado
        /// </summary>
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
        public int RejectInvoiceTotalRequired { get; set; }
        /// <summary>
        /// Rechazo factura electrónica - Total Aceptado
        /// </summary>
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
        public int ApplicationAvailableTotalRequired { get; set; }
        /// <summary>
        /// Solicitud disponibilización - Total Aceptados
        /// </summary>
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
        public int EndorsementPropertyTotalRequired { get; set; }
        /// <summary>
        ///  Endoso en Propiedad - Total Aceptado
        /// </summary>
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
        public int EndorsementProcurementTotalRequired { get; set; }

        public int EndorsementTotalRequired { get; set; }
        /// <summary>
        /// Endoso en Procuracion - Total Aceptado
        /// </summary>
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
        public int EndorsementGuaranteeTotalRequired { get; set; }
        /// <summary>
        /// Endoso en Garantia - Total Aceptados
        /// </summary>
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
        public int EndorsementCancellationTotalRequired { get; set; }
        /// <summary>
        /// Cancelación de endoso - Total Aceptado
        /// </summary>
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
        public int GuaranteeTotalRequired { get; set; }
        /// <summary>
        /// Avales - Total Aceptados
        /// </summary>
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
        public int ElectronicMandateTotalRequired { get; set; }
        /// <summary>
        /// Mandato electrónico - Total Aceptada requerida
        /// </summary>
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


        public string StatusDescription { get; set; }
        public int Status { get; set; }
        public bool Deleted { get; set; }
        public string Id { get; set; }
        // Estado: En Proceso, etc.
        public string State { get; set; }
    }
}
