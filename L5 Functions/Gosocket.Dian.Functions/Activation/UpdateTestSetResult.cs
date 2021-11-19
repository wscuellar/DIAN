using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Activation
{
    public static class UpdateTestSetResult
    {
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly SoftwareService softwareService = new SoftwareService();
        private static readonly GlobalOtherDocElecOperationService globalOtherDocElecOperation = new GlobalOtherDocElecOperationService();
        private static readonly TableManager globalTestSetTableManager = new TableManager("GlobalTestSet");
        private static readonly TableManager globalTestSetResultTableManager = new TableManager("GlobalTestSetResult");
        private static readonly TableManager radianTestSetResultTableManager = new TableManager("RadianTestSetResult");
        private static readonly TableManager globalTestSetTrackingTableManager = new TableManager("GlobalTestSetTracking");
        private static readonly TableManager contributorTableManager = new TableManager("GlobalContributor");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        private static readonly GlobalRadianOperationService globalRadianOperationService = new GlobalRadianOperationService();
        //private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly TableLoggerManager TableLoggerManagerFACELogger = new TableLoggerManager("FACELogger");        
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager tableManagerGlobalTestSetOthersDocumentsResult = new TableManager("GlobalTestSetOthersDocumentsResult");


        // Set queue name 
        private const string queueName = "global-test-set-tracking-input%Slot%";

        [FunctionName("UpdateTestSetResult")]        
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")] string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            string uniqueKey = Guid.NewGuid().ToString();
            var testSetId = string.Empty;
            try
            {               
                //Obtengo informacion de la cola e insertamos el registro del tracking de envios
                EventGridEvent eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                GlobalTestSetTracking globalTestSetTracking = JsonConvert.DeserializeObject<GlobalTestSetTracking>(eventGridEvent.Data.ToString());
                await globalTestSetTrackingTableManager.InsertOrUpdateAsync(globalTestSetTracking);

                var start = DateTime.UtcNow;
                var startUpdateTest = new GlobalLogger(globalTestSetTracking.TestSetId, "1 Start startUpdateTest")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Start startUpdateTest"
                };
                await TableLoggerManagerFACELogger.InsertOrUpdateAsync(startUpdateTest);


                testSetId = globalTestSetTracking.TestSetId;
                //Listamos los tracking de los envios realizados para el set de pruebas en proceso
                List<GlobalTestSetTracking> allGlobalTestSetTracking = globalTestSetTrackingTableManager.FindByPartition<GlobalTestSetTracking>(globalTestSetTracking.TestSetId);
                GlobalTestSetOthersDocumentsResult setResultOther = tableManagerGlobalTestSetOthersDocumentsResult.FindByGlobalOtherDocumentTestId<GlobalTestSetOthersDocumentsResult>(globalTestSetTracking.TestSetId);

                //Se busca el set de pruebas procesado para el testsetid en curso
                RadianTestSetResult radianTesSetResult = radianTestSetResultTableManager.FindByTestSetId<RadianTestSetResult>(globalTestSetTracking.TestSetId);
                SetLogger(radianTesSetResult, "Step 0", globalTestSetTracking.TestSetId, uniqueKey);
                SetLogger(setResultOther, "Step 0", "Paso setResultOther" + setResultOther + "****" + globalTestSetTracking.TestSetId, uniqueKey);

                start = DateTime.UtcNow;
                var validateTestSet = new GlobalLogger(globalTestSetTracking.TestSetId, "2 validateTestSet")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "validateTestSet"
                };
                await TableLoggerManagerFACELogger.InsertOrUpdateAsync(validateTestSet);


                //Si existe el set de pruebas se Valida RADIAN
                if (radianTesSetResult != null && setResultOther == null)
                {

                    start = DateTime.UtcNow;
                    var validateRadian = new GlobalLogger(globalTestSetTracking.TestSetId, "3 radianTesSetResult")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "validateRadian"
                    };
                    await TableLoggerManagerFACELogger.InsertOrUpdateAsync(validateRadian);

                    // traigo los datos de RadianTestSetResult
                    SetLogger(radianTesSetResult, "Step 2", "Ingreso a proceso RADIAN", uniqueKey);

                    // Ubico con el servicio si RadianOperation esta activo y no continua el proceso.
                    string code = radianTesSetResult.PartitionKey;
                    SetLogger(radianTesSetResult, "Step 2", "Ingreso a proceso RADIAN", uniqueKey);
                    SetLogger(null, "Step 2.1", code, uniqueKey);
                    SetLogger(null, "Step 2.2", globalTestSetTracking.SoftwareId, uniqueKey);

                    // Se verifica si la operacion para el cliente y el software que usa esta habilitada en RADIAn
                    bool isActive = globalRadianOperationService.IsActive(code, new Guid(globalTestSetTracking.SoftwareId));
                    SetLogger(null, "Step 2.3", isActive.ToString(), uniqueKey);
                    if (isActive)
                        return;
                    SetLogger(null, "Step 3", "No esta Activo El RadianContributor", uniqueKey);

                    //Ajustamos los documentType para sean los eventos de la factura
                    SetLogger(null, "Step 3.1", allGlobalTestSetTracking.Count.ToString(), uniqueKey);
                    foreach (GlobalTestSetTracking item in allGlobalTestSetTracking)
                    {
                        //Consigue informacion del CUDE
                        GlobalDocValidatorDocumentMeta validatorDocumentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(item.TrackId, item.TrackId);
                        item.DocumentTypeId = validatorDocumentMeta.EventCode;
                        SetLogger(null, "Step 3.2", item.DocumentTypeId, uniqueKey);
                        SetLogger(null, "Step 3.3", item.IsValid.ToString(), uniqueKey);
                    }
                    SetLogger(null, "Step 3.4", allGlobalTestSetTracking[0].DocumentTypeId, uniqueKey);

                    //Total de los documentos
                    radianTesSetResult.TotalDocumentSent = allGlobalTestSetTracking.Count;
                    radianTesSetResult.TotalDocumentAccepted = allGlobalTestSetTracking.Count(a => a.IsValid);
                    radianTesSetResult.TotalDocumentsRejected = allGlobalTestSetTracking.Count(a => !a.IsValid);

                    // Acuse de Recibo
                    int tipo = (int)EventStatus.Received;
                    radianTesSetResult.TotalReceiptNoticeSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ReceiptNoticeAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ReceiptNoticeRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 3", "Acuse de recibo", uniqueKey);
                    SetLogger(null, "Step 3.0", tipo.ToString(), uniqueKey);
                    SetLogger(null, "Step 3.1", radianTesSetResult.TotalReceiptNoticeSent.ToString(), uniqueKey);
                    SetLogger(null, "Step 3.2", radianTesSetResult.ReceiptNoticeAccepted.ToString(), uniqueKey);
                    SetLogger(null, "Step 3.6", radianTesSetResult.ReceiptNoticeRejected.ToString(), uniqueKey);

                    // Recibo del Bien
                    tipo = (int)EventStatus.Receipt;
                    radianTesSetResult.TotalReceiptServiceSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ReceiptServiceAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ReceiptServiceRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 4", "Recibo del bien", uniqueKey);
                    SetLogger(null, "Step 4.0", tipo.ToString(), uniqueKey);
                    SetLogger(null, "Step 4.1", radianTesSetResult.TotalReceiptServiceSent.ToString(), uniqueKey);
                    SetLogger(null, "Step 4.2", radianTesSetResult.ReceiptServiceAccepted.ToString(), uniqueKey);
                    SetLogger(null, "Step 4.3", radianTesSetResult.ReceiptServiceRejected.ToString(), uniqueKey);

                    ////  Aceptación expresa
                    tipo = (int)EventStatus.Accepted;
                    radianTesSetResult.TotalExpressAcceptanceSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ExpressAcceptanceAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ExpressAcceptanceRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 5", "Aceptacion Expresa", uniqueKey);
                    SetLogger(null, "Step 5.0", tipo.ToString(), uniqueKey);
                    SetLogger(null, "Step 5.1", radianTesSetResult.TotalExpressAcceptanceSent.ToString(), uniqueKey);
                    SetLogger(null, "Step 5.2", radianTesSetResult.ExpressAcceptanceAccepted.ToString(), uniqueKey);
                    SetLogger(null, "Step 5.3", radianTesSetResult.ExpressAcceptanceRejected.ToString(), uniqueKey);


                    //// Manifestación de aceptación
                    tipo = (int)EventStatus.AceptacionTacita;
                    radianTesSetResult.TotalAutomaticAcceptanceSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.AutomaticAcceptanceAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.AutomaticAcceptanceRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 6", "Manifectacion de aceptacion", uniqueKey);
                    SetLogger(null, "Step 6.0", tipo.ToString(), uniqueKey);
                    SetLogger(null, "Step 6.1", radianTesSetResult.TotalAutomaticAcceptanceSent.ToString(), uniqueKey);
                    SetLogger(null, "Step 6.2", radianTesSetResult.AutomaticAcceptanceAccepted.ToString(), uniqueKey);
                    SetLogger(null, "Step 6.3", radianTesSetResult.AutomaticAcceptanceRejected.ToString(), uniqueKey);

                    //// Rechazo factura electrónica
                    tipo = (int)EventStatus.Rejected;
                    radianTesSetResult.TotalRejectInvoiceSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.RejectInvoiceAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.RejectInvoiceRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 7", "Rechazo factura electrónica", uniqueKey);

                    //// Solicitud disponibilización
                    tipo = (int)EventStatus.SolicitudDisponibilizacion;
                    radianTesSetResult.TotalApplicationAvailableSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ApplicationAvailableAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ApplicationAvailableRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 8", "Solicitud disponibilización", uniqueKey);

                    //// Endoso de propiedad 
                    tipo = (int)EventStatus.EndosoPropiedad;
                    radianTesSetResult.TotalEndorsementPropertySent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndorsementPropertyAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndorsementPropertyRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 9", "Endoso de propiedad", uniqueKey);

                    //// Endoso de Garantia 
                    tipo = (int)EventStatus.EndosoGarantia;
                    radianTesSetResult.TotalEndorsementGuaranteeSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndorsementGuaranteeAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndorsementGuaranteeRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 10", "Endoso de Garantia", uniqueKey);

                    //// Endoso de Procuracion 
                    tipo = (int)EventStatus.EndosoProcuracion;
                    radianTesSetResult.TotalEndorsementProcurementSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndorsementProcurementAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndorsementProcurementRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 11", "Endoso de Procuracion", uniqueKey);

                    //// Cancelación de endoso 
                    tipo = (int)EventStatus.InvoiceOfferedForNegotiation;
                    radianTesSetResult.TotalEndorsementCancellationSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndorsementCancellationAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndorsementCancellationRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 12", "Cancelación de endoso", uniqueKey);

                    //// Avales
                    tipo = (int)EventStatus.Avales;
                    radianTesSetResult.TotalGuaranteeSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.GuaranteeAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.GuaranteeRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 13", "Avales", uniqueKey);


                    //// Mandato electrónico
                    tipo = (int)EventStatus.Mandato;
                    radianTesSetResult.TotalElectronicMandateSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ElectronicMandateAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ElectronicMandateRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 14", "Mandato electrónico", uniqueKey);


                    //// Terminación mandato
                    tipo = (int)EventStatus.TerminacionMandato;
                    radianTesSetResult.TotalEndMandateSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndMandateAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndMandateRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 15", "Terminación mandato", uniqueKey);

                    //// Notificación de pago
                    tipo = (int)EventStatus.NotificacionPagoTotalParcial;
                    radianTesSetResult.TotalPaymentNotificationSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.PaymentNotificationAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.PaymentNotificationRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 16", "Notificación de pago", uniqueKey);


                    //// Limitación de circulación     
                    tipo = (int)EventStatus.NegotiatedInvoice;
                    radianTesSetResult.TotalCirculationLimitationSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.CirculationLimitationAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.CirculationLimitationRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 17", "Limitación de circulación", uniqueKey);

                    //// Terminación limitación  
                    tipo = (int)EventStatus.AnulacionLimitacionCirculacion;
                    radianTesSetResult.TotalEndCirculationLimitationSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndCirculationLimitationAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.EndCirculationLimitationRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 18", "Terminación limitación", uniqueKey);

                    //// Informe para el pago  
                    tipo = (int)EventStatus.ValInfoPago;
                    radianTesSetResult.TotalReportForPaymentSent = allGlobalTestSetTracking.Count(a => Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ReportForPaymentAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);
                    radianTesSetResult.ReportForPaymentRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && Convert.ToInt32(a.DocumentTypeId) == tipo);

                    SetLogger(null, "Step 19", "Informe para el pago", uniqueKey);

                    SetLogger(null, "Step 19.a", radianTesSetResult.ReceiptNoticeAccepted.ToString(), uniqueKey);
                    SetLogger(null, "Step 19.b", radianTesSetResult.ReceiptNoticeTotalAcceptedRequired.ToString(), uniqueKey);

                    Contributor contributor = contributorService.GetByCode(radianTesSetResult.PartitionKey);
                    GlobalRadianOperations isPartipantActive = globalRadianOperationService.GetOperation(radianTesSetResult.PartitionKey, new Guid(globalTestSetTracking.SoftwareId));


                    // Determinamos si se puede ya dar por aceptado al set de pruebas del cliente
                    if (radianTesSetResult.ReceiptNoticeAccepted >= radianTesSetResult.ReceiptNoticeTotalAcceptedRequired
                            && radianTesSetResult.ReceiptServiceAccepted >= radianTesSetResult.ReceiptServiceTotalAcceptedRequired
                            && radianTesSetResult.ExpressAcceptanceAccepted >= radianTesSetResult.ExpressAcceptanceTotalAcceptedRequired
                            && radianTesSetResult.AutomaticAcceptanceAccepted >= radianTesSetResult.AutomaticAcceptanceTotalAcceptedRequired
                            && radianTesSetResult.RejectInvoiceAccepted >= radianTesSetResult.RejectInvoiceTotalAcceptedRequired
                            && radianTesSetResult.ApplicationAvailableAccepted >= radianTesSetResult.ApplicationAvailableTotalAcceptedRequired
                            && radianTesSetResult.EndorsementPropertyAccepted >= radianTesSetResult.EndorsementPropertyTotalAcceptedRequired
                            && radianTesSetResult.EndorsementProcurementAccepted >= radianTesSetResult.EndorsementProcurementTotalAcceptedRequired
                            && radianTesSetResult.EndorsementGuaranteeAccepted >= radianTesSetResult.EndorsementGuaranteeTotalAcceptedRequired
                            && radianTesSetResult.EndorsementCancellationAccepted >= radianTesSetResult.EndorsementCancellationTotalAcceptedRequired
                            && radianTesSetResult.GuaranteeAccepted >= radianTesSetResult.GuaranteeTotalAcceptedRequired
                            && radianTesSetResult.ElectronicMandateAccepted >= radianTesSetResult.ElectronicMandateTotalAcceptedRequired
                            && radianTesSetResult.EndMandateAccepted >= radianTesSetResult.EndMandateTotalAcceptedRequired
                            && radianTesSetResult.PaymentNotificationAccepted >= radianTesSetResult.PaymentNotificationTotalAcceptedRequired
                            && radianTesSetResult.CirculationLimitationAccepted >= radianTesSetResult.CirculationLimitationTotalAcceptedRequired
                            && radianTesSetResult.EndCirculationLimitationAccepted >= radianTesSetResult.EndCirculationLimitationTotalAcceptedRequired
                            && radianTesSetResult.ReportForPaymentAccepted >= radianTesSetResult.ReportForPaymentTotalAcceptedRequired
                            && radianTesSetResult.Status == (int)TestSetStatus.InProcess)
                    {
                        radianTesSetResult.Status = (int)TestSetStatus.Accepted;
                        radianTesSetResult.State = TestSetStatus.Accepted.GetDescription();
                        contributorService.OperationUpdate(contributor.Id, isPartipantActive.RadianContributorTypeId, isPartipantActive.RowKey, isPartipantActive.SoftwareType, RadianState.Habilitado);
                    }


                    SetLogger(null, "Step 19.c", radianTesSetResult.PaymentNotificationRejected.ToString(), uniqueKey);
                    SetLogger(null, "Step 19.d", radianTesSetResult.PaymentNotificationTotalRequired.ToString(), uniqueKey);
                    SetLogger(null, "Step 19.e", radianTesSetResult.PaymentNotificationTotalAcceptedRequired.ToString(), uniqueKey);
                    // Determinamos si rechazamos el set de pruebas del cliente
                    if (radianTesSetResult.ReceiptNoticeRejected > (radianTesSetResult.ReceiptNoticeTotalRequired - radianTesSetResult.ReceiptNoticeTotalAcceptedRequired)||
                        radianTesSetResult.ReceiptServiceRejected > (radianTesSetResult.ReceiptServiceTotalRequired - radianTesSetResult.ReceiptServiceTotalAcceptedRequired) ||
                        radianTesSetResult.ExpressAcceptanceRejected > (radianTesSetResult.ExpressAcceptanceTotalRequired - radianTesSetResult.ExpressAcceptanceTotalAcceptedRequired)||
                        radianTesSetResult.AutomaticAcceptanceRejected > (radianTesSetResult.AutomaticAcceptanceTotalRequired - radianTesSetResult.AutomaticAcceptanceTotalAcceptedRequired) ||
                        radianTesSetResult.ApplicationAvailableRejected > (radianTesSetResult.ApplicationAvailableTotalRequired - radianTesSetResult.ApplicationAvailableTotalAcceptedRequired) ||
                        radianTesSetResult.EndorsementPropertyRejected > (radianTesSetResult.EndorsementPropertyTotalRequired - radianTesSetResult.EndorsementPropertyTotalAcceptedRequired)||
                        radianTesSetResult.EndorsementProcurementRejected > (radianTesSetResult.EndorsementProcurementTotalRequired - radianTesSetResult.EndorsementProcurementTotalAcceptedRequired)||
                        radianTesSetResult.EndorsementGuaranteeRejected > (radianTesSetResult.EndorsementGuaranteeTotalRequired - radianTesSetResult.EndorsementGuaranteeTotalAcceptedRequired) ||
                        radianTesSetResult.EndorsementCancellationRejected > (radianTesSetResult.EndorsementCancellationTotalRequired - radianTesSetResult.EndorsementCancellationTotalAcceptedRequired) ||
                        radianTesSetResult.GuaranteeRejected > (radianTesSetResult.GuaranteeTotalRequired - radianTesSetResult.GuaranteeTotalAcceptedRequired) ||
                        radianTesSetResult.ElectronicMandateRejected > (radianTesSetResult.ElectronicMandateTotalRequired - radianTesSetResult.ElectronicMandateTotalAcceptedRequired) ||
                        radianTesSetResult.EndMandateRejected > (radianTesSetResult.EndMandateTotalRequired - radianTesSetResult.EndMandateTotalAcceptedRequired) ||
                        radianTesSetResult.PaymentNotificationRejected > (radianTesSetResult.PaymentNotificationTotalRequired - radianTesSetResult.PaymentNotificationTotalAcceptedRequired) ||
                        radianTesSetResult.CirculationLimitationRejected > (radianTesSetResult.CirculationLimitationTotalRequired - radianTesSetResult.CirculationLimitationTotalAcceptedRequired) ||
                        radianTesSetResult.EndCirculationLimitationRejected > (radianTesSetResult.EndCirculationLimitationTotalRequired - radianTesSetResult.EndCirculationLimitationTotalAcceptedRequired) ||
                        radianTesSetResult.ReportForPaymentRejected > (radianTesSetResult.ReportForPaymentTotalRequired - radianTesSetResult.ReportForPaymentTotalAcceptedRequired))
                    {
                        SetLogger(null, "Step 19.e", radianTesSetResult.ReceiptNoticeTotalAcceptedRequired.ToString(), uniqueKey);
                        radianTesSetResult.Status = (int)TestSetStatus.Rejected;
                        radianTesSetResult.State = TestSetStatus.Rejected.GetDescription();
                        contributorService.OperationUpdate(contributor.Id, isPartipantActive.RadianContributorTypeId, isPartipantActive.RowKey, isPartipantActive.SoftwareType, RadianState.Cancelado);

                    }
                    SetLogger(null, "Step 19 New", " radianTesSetResult.Status " + radianTesSetResult.Status, uniqueKey);

                    // Actualizo el registro del set de pruebas del cliente
                    await radianTestSetResultTableManager.InsertOrUpdateAsync(radianTesSetResult);

                    // Si es aceptado el set de pruebas se activa el contributor en el ambiente de habilitacion
                    if (radianTesSetResult.Status == (int)TestSetStatus.Accepted)
                    {
                        SetLogger(null, "Step 19.1", "Fui aceptado", uniqueKey);

                        // Send to activate contributor in production
                        if (ConfigurationManager.GetValue("Environment") == "Hab")
                        {

                            try
                            {
                                SetLogger(null, "Step 19.2", "Estoy en habilitacion", uniqueKey);

                                #region Proceso Radian Habilitacion

                                //Traemos el contribuyente
                               

                                //Consultamos al participante en GlobalRadianOperations
                                

                                //--Traemos la informacion del software
                                string softwareId = globalTestSetTracking.SoftwareId;
                                RadianSoftware software = softwareService.GetByRadian(Guid.Parse(softwareId));

                                #endregion

                                #region Pendiente migracion SQL

                                //Organizamos el objeto con la informacion para la habilitacion del participante en la function.
                                var requestObject = new
                                {
                                    code = isPartipantActive.PartitionKey,
                                    contributorId = contributor.Id,
                                    contributorTypeId = isPartipantActive.RadianContributorTypeId,
                                    softwareId = isPartipantActive.RowKey,
                                    softwareType = isPartipantActive.SoftwareType,
                                    softwareUser = software.SoftwareUser,
                                    softwarePassword = software.SoftwarePassword,
                                    pin = software.Pin,
                                    url = software.Url,
                                    softwareName = software.Name,
                                    testSetId = globalTestSetTracking.TestSetId
                                };

                                //Enviamos la habilitacion para el usuario
                                string functionPath = ConfigurationManager.GetValue("SendToActivateRadianOperationUrl");
                                SetLogger(null, "Funciton Path", functionPath, uniqueKey);
                                SetLogger(requestObject, "Funciton Path", functionPath, uniqueKey);
                                var activation = await ApiHelpers.ExecuteRequestAsync<SendToActivateContributorResponse>(functionPath, requestObject);
                                SetLogger(activation, "Step 21", activation == null ? "Estoy vacio" : " functionPath " + functionPath, uniqueKey);

                                //Dejamos un registro en la globalradiancontributoractivation.
                                string guid = Guid.NewGuid().ToString();
                                GlobalContributorActivation contributorActivation = new GlobalContributorActivation(contributor.Code, guid)
                                {
                                    Success = true,
                                    ContributorCode = isPartipantActive.PartitionKey,
                                    ContributorTypeId = isPartipantActive.RadianContributorTypeId,
                                    OperationModeId = isPartipantActive.SoftwareType,
                                    OperationModeName = "RADIAN",
                                    SentToActivateBy = "Function",
                                    SoftwareId = isPartipantActive.RowKey,
                                    SendDate = DateTime.UtcNow,
                                    TestSetId = radianTesSetResult.Id,
                                    Request = JsonConvert.SerializeObject(requestObject)
                                };
                                await contributorActivationTableManager.InsertOrUpdateAsync(contributorActivation);

                                SetLogger(contributorActivation, "Step 22", " contributorActivationTableManager.InsertOrUpdateAsync ", uniqueKey);

                                #endregion
                            }
                            catch (Exception ex)
                            {
                                SetLogger(null, "Error", ex.Message, uniqueKey);
                                log.Error($"Error al enviar a activar RADIAN contribuyente con id {globalTestSetTracking.SenderCode} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                                throw;
                            }
                        }
                    }
                }
                else if (setResultOther != null) //Other Document
                {
                    start = DateTime.UtcNow;
                    var validateOtherDoc = new GlobalLogger(globalTestSetTracking.TestSetId, "4 setResultOther")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "setResultOther"
                    };
                    await TableLoggerManagerFACELogger.InsertOrUpdateAsync(validateOtherDoc);

                    string[] tempSoftwareID = setResultOther.RowKey.Split('|');

                    SetLogger(null, "Step 2 - Nomina", "setResultOther dieferente de NULL Software ID: " + tempSoftwareID[1], uniqueKey);
                    // traigo los datos de RadianTestSetResult

                    // Se verifica si la operacion para el cliente y el software que usa esta habilitada para otros documentos
                    bool isActive = globalOtherDocElecOperation.IsActive(setResultOther.PartitionKey, new Guid(tempSoftwareID[1]));
                    SetLogger(null, "Step 2.3", isActive.ToString(), uniqueKey);
                    if (isActive)
                        return;               

                    SetLogger(null, "Step 2 - Nomina", "estado en prueba ", uniqueKey);

                    string[] nomina = { "103" };
                    string[] otherDocument = { "102" };

                    //Totales docuemntos
                    setResultOther.TotalDocumentSent = allGlobalTestSetTracking.Count();
                    setResultOther.TotalDocumentAccepted = allGlobalTestSetTracking.Count(a => a.IsValid);
                    setResultOther.TotalDocumentsRejected = allGlobalTestSetTracking.Count(a => !a.IsValid);

                    //Nomina Individual de Ajuste
                    setResultOther.TotalElectronicPayrollAjustmentSent = allGlobalTestSetTracking.Count(a => nomina.Contains(a.DocumentTypeId));
                    setResultOther.ElectronicPayrollAjustmentAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && nomina.Contains(a.DocumentTypeId));
                    setResultOther.ElectronicPayrollAjustmentRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && nomina.Contains(a.DocumentTypeId));

                    //OtherDocument Nomina Individual
                    setResultOther.TotalOthersDocumentsSent = allGlobalTestSetTracking.Count(a => otherDocument.Contains(a.DocumentTypeId));
                    setResultOther.OthersDocumentsAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && otherDocument.Contains(a.DocumentTypeId));
                    setResultOther.OthersDocumentsRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && otherDocument.Contains(a.DocumentTypeId));

                    //Validacion Total Nomina aceptados
                    if (setResultOther.TotalDocumentAccepted >= setResultOther.TotalDocumentAcceptedRequired
                        && setResultOther.ElectronicPayrollAjustmentAccepted >= setResultOther.ElectronicPayrollAjustmentAcceptedRequired
                        && setResultOther.OthersDocumentsAccepted >= setResultOther.OthersDocumentsAcceptedRequired
                        && setResultOther.Status == (int)TestSetStatus.InProcess)
                    {
                        SetLogger(null, "Step 3 - Validacion Nomina", "Paso Validacion de nomina", uniqueKey);
                        setResultOther.Status = (int)TestSetStatus.Accepted;
                        setResultOther.StatusDescription = TestSetStatus.Accepted.GetDescription();
                        setResultOther.State = TestSetStatus.Accepted.GetDescription();
                    }

                    Contributor contributor = contributorService.GetByCode(setResultOther.PartitionKey);
                    GlobalOtherDocElecOperation isPartipantActiveOtherDoc = globalOtherDocElecOperation.GetOperation(setResultOther.PartitionKey, new Guid(tempSoftwareID[1]));
                    if (isPartipantActiveOtherDoc == null)
                        return;

                    SetLogger(isPartipantActiveOtherDoc, "Step 7", " isPartipantActive.GlobalOtherDocElecOperation ", uniqueKey);


                    //Validacion Total Nomina rechazados 
                    if (setResultOther.TotalDocumentsRejected > (setResultOther.TotalDocumentRequired - setResultOther.TotalDocumentAcceptedRequired)                        
                        && setResultOther.Status == (int)TestSetStatus.InProcess)
                    {
                        SetLogger(isPartipantActiveOtherDoc, "Step 5 - Validacion Nomina Reject", "Paso Validacion de Nomina Reject", uniqueKey);
                        setResultOther.Status = (int)TestSetStatus.Rejected;
                        setResultOther.StatusDescription = TestSetStatus.Rejected.GetDescription();
                        setResultOther.State = TestSetStatus.Rejected.GetDescription();
                        contributorService.OperationRejectOtherDoc(contributor.Id, isPartipantActiveOtherDoc.ContributorTypeId, isPartipantActiveOtherDoc.SoftwareId, isPartipantActiveOtherDoc.OperationModeId);
                    }
                    
                    //Registro en la table Azure
                    await tableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(setResultOther);

                    // Si es aceptado el set de pruebas se activa el contributor en el ambiente de habilitacion
                    if (setResultOther.Status == (int)TestSetStatus.Accepted)
                    {
                        SetLogger(null, "Step 6.1", "Aceptado", uniqueKey);                      

                        // Send to activate contributor in production
                        if (ConfigurationManager.GetValue("Environment") == "Hab")
                        {
                            try
                            {
                                SetLogger(null, "Step 6.2", "Estoy en habilitacion", uniqueKey);

                                //--Traemos la informacion del software
                                //string softwareId = globalTestSetTracking.SoftwareId;
                                string softwareId = tempSoftwareID[1];
                                OtherDocElecSoftware software = softwareService.GetByOtherDoc(Guid.Parse(softwareId));

                                SetLogger(isPartipantActiveOtherDoc, "Step 7.1", " softwareId.GlobalOtherDocElecOperation " + softwareId, uniqueKey);
                            
                                #region migracion SQL

                                var requestObject = new
                                {
                                    code = isPartipantActiveOtherDoc.PartitionKey,
                                    contributorId = contributor.Id,
                                    contributorTypeId = isPartipantActiveOtherDoc.ContributorTypeId,
                                    softwareId = isPartipantActiveOtherDoc.RowKey,
                                    softwareType = isPartipantActiveOtherDoc.OperationModeId,
                                    softwareUser = software.SoftwareUser,
                                    softwarePassword = software.SoftwarePassword,
                                    pin = software.Pin,
                                    url = software.Url,
                                    softwareName = software.Name,
                                    enabled = false
                                };

                                string functionPath = ConfigurationManager.GetValue("SendToActivateOtherDocumentContributorUrl");
                                SetLogger(null, "Funciton Path", functionPath, uniqueKey);
                                SetLogger(requestObject, "Funciton Path", functionPath, uniqueKey);

                                var activation = await ApiHelpers.ExecuteRequestAsync<SendToActivateContributorResponse>(functionPath, requestObject);

                                SetLogger(activation, "Step 8", activation == null ? "Estoy vacio" : " functionPath " + functionPath, uniqueKey);
                                //SetLogger(activation, "Step 21", " functionPath " + functionPath, "21212121");

                                var guid = Guid.NewGuid().ToString();
                                var contributorActivation = new GlobalContributorActivation(contributor.Code, guid)
                                {
                                    Success = true,
                                    ContributorCode = Convert.ToString(isPartipantActiveOtherDoc.OtherDocElecContributorId),
                                    ContributorTypeId = Convert.ToInt32(isPartipantActiveOtherDoc.ContributorTypeId),
                                    OperationModeId = Convert.ToInt32(isPartipantActiveOtherDoc.OperationModeId),
                                    OperationModeName = "OTHERDOCUMENTS",
                                    SentToActivateBy = "Function",
                                    SoftwareId = isPartipantActiveOtherDoc.RowKey,
                                    SendDate = DateTime.UtcNow,
                                    TestSetId = setResultOther.Id,
                                    Request = JsonConvert.SerializeObject(requestObject)
                                };
                                await contributorActivationTableManager.InsertOrUpdateAsync(contributorActivation);

                                SetLogger(contributorActivation, "Step 9", " contributorActivationTableManager.InsertOrUpdateAsync ", uniqueKey);

                                #endregion
                            }
                            catch (Exception ex)
                            {
                                SetLogger(null, "Error", ex.Message, uniqueKey);
                                log.Error($"Error al enviar a activar Nomina/OtherDocument contribuyente con id {globalTestSetTracking.SenderCode} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                                throw;
                            }
                        }
                    }

                }
                else // Factura Electronica
                {
                    start = DateTime.UtcNow;
                    var validateFE = new GlobalLogger(globalTestSetTracking.TestSetId, "5 FacturaElectronica")
                    {
                        Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                        Action = "Factura Electronica"
                    };
                    await TableLoggerManagerFACELogger.InsertOrUpdateAsync(validateFE);

                    var testSetResults = globalTestSetResultTableManager.FindByPartition<GlobalTestSetResult>(globalTestSetTracking.SenderCode);

                    if (testSetResults != null)  // Roberto Alvarado --> Esto es para mantener lo de Factura Electronica tal cual esta actualmente 2020/11/25
                    {

                        var globalTesSetResult = testSetResults.SingleOrDefault(t => !t.Deleted && t.Id == globalTestSetTracking.TestSetId && t.Status == (int)TestSetStatus.InProcess);

                        if (globalTesSetResult == null)
                            return;

                        var globalTestSet = globalTestSetTableManager.Find<GlobalTestSet>(globalTesSetResult.TestSetReference, globalTesSetResult.TestSetReference);

                        string[] invoiceCodes = { "1", "01", "02", "03" };
                        string[] creditNoteCodes = { "7", "91" };
                        string[] debitNoteCodes = { "8", "92" };

                        globalTesSetResult.Id = globalTestSetTracking.TestSetId;

                        globalTesSetResult.TotalDocumentSent = allGlobalTestSetTracking.Count;
                        globalTesSetResult.TotalDocumentAccepted = allGlobalTestSetTracking.Count(a => a.IsValid);
                        globalTesSetResult.TotalDocumentsRejected = allGlobalTestSetTracking.Count(a => !a.IsValid);


                        globalTesSetResult.InvoicesTotalSent = allGlobalTestSetTracking.Count(a => invoiceCodes.Contains(a.DocumentTypeId));
                        globalTesSetResult.TotalInvoicesAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && invoiceCodes.Contains(a.DocumentTypeId));
                        globalTesSetResult.TotalInvoicesRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && invoiceCodes.Contains(a.DocumentTypeId));

                        globalTesSetResult.TotalCreditNotesSent = allGlobalTestSetTracking.Count(a => creditNoteCodes.Contains(a.DocumentTypeId));
                        globalTesSetResult.TotalCreditNotesAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && creditNoteCodes.Contains(a.DocumentTypeId));
                        globalTesSetResult.TotalCreditNotesRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && creditNoteCodes.Contains(a.DocumentTypeId));

                        globalTesSetResult.TotalDebitNotesSent = allGlobalTestSetTracking.Count(a => debitNoteCodes.Contains(a.DocumentTypeId));
                        globalTesSetResult.TotalDebitNotesAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && debitNoteCodes.Contains(a.DocumentTypeId));
                        globalTesSetResult.TotalDebitNotesRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && debitNoteCodes.Contains(a.DocumentTypeId));

                        if (globalTesSetResult.TotalInvoicesAccepted >= globalTesSetResult.TotalInvoicesAcceptedRequired && globalTesSetResult.TotalCreditNotesAccepted >= globalTesSetResult.TotalCreditNotesAcceptedRequired && globalTesSetResult.TotalDebitNotesAccepted >= globalTesSetResult.TotalDebitNotesAcceptedRequired && globalTesSetResult.Status == (int)TestSetStatus.InProcess)
                            globalTesSetResult.Status = (int)TestSetStatus.Accepted;

                        if (globalTesSetResult.TotalDocumentsRejected > (globalTesSetResult.TotalDocumentRequired - globalTesSetResult.TotalDocumentAcceptedRequired) && globalTesSetResult.Status == (int)TestSetStatus.InProcess)
                            globalTesSetResult.Status = (int)TestSetStatus.Rejected;

                        await globalTestSetResultTableManager.InsertOrUpdateAsync(globalTesSetResult);

                        if (globalTesSetResult.Status == (int)TestSetStatus.Accepted)
                        {
                            // partition key are sender code.
                            var contributor = contributorService.GetByCode(globalTesSetResult.PartitionKey);
                            if (contributor.AcceptanceStatusId == (int)ContributorStatus.Registered)
                            {
                                contributorService.SetToEnabled(contributor);
                                var globalContributor = new GlobalContributor(contributor.Code, contributor.Code) { Code = contributor.Code, StatusId = contributor.AcceptanceStatusId, TypeId = contributor.ContributorTypeId };
                                await contributorTableManager.InsertOrUpdateAsync(globalContributor);
                            }

                            var software = softwareService.Get(Guid.Parse(globalTesSetResult.SoftwareId));
                            if (software.AcceptanceStatusSoftwareId == (int)Domain.Common.OperationMode.Own
                                && globalTesSetResult.OperationModeId != (int)Domain.Common.OperationMode.Free)
                            {
                                softwareService.SetToProduction(software);
                                var softwareId = software.Id.ToString();
                                var globalSoftware = new GlobalSoftware(softwareId, softwareId) { Id = software.Id, Deleted = software.Deleted, Pin = software.Pin, StatusId = software.AcceptanceStatusSoftwareId };
                                await softwareTableManager.InsertOrUpdateAsync(globalSoftware);
                            }


                            // Send to activate contributor in production
                            if (ConfigurationManager.GetValue("Environment") == "Hab")
                            {
                                try
                                {
                                    var requestObject = new { contributorId = contributor.Id };
                                    var activation = await ApiHelpers.ExecuteRequestAsync<SendToActivateContributorResponse>(ConfigurationManager.GetValue("SendToActivateContributorUrl"), requestObject);

                                    var guid = Guid.NewGuid().ToString();
                                    var contributorActivation = new GlobalContributorActivation(contributor.Code, guid)
                                    {
                                        Success = activation.Success,
                                        ContributorCode = contributor.Code,
                                        ContributorTypeId = contributor.ContributorTypeId,
                                        OperationModeId = globalTesSetResult.OperationModeId,
                                        OperationModeName = globalTesSetResult.OperationModeName,
                                        SentToActivateBy = "Function",
                                        SoftwareId = globalTesSetResult.SoftwareId,
                                        SendDate = DateTime.UtcNow,
                                        TestSetId = globalTesSetResult.Id,
                                        Trace = activation.Trace,
                                        Message = activation.Message,
                                        Detail = activation.Detail,
                                        Request = JsonConvert.SerializeObject(requestObject)
                                    };
                                    await contributorActivationTableManager.InsertOrUpdateAsync(contributorActivation);

                                    if (globalTesSetResult.OperationModeId == (int)SoftwareStatus.Production)
                                        await MigrateCertificate(contributor.Code);

                                }
                                catch (Exception ex)
                                {
                                    log.Error($"Error al enviar a activar contribuyente con id {contributor.Id} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                                    throw;
                                }
                            }
                        }
                    }
                }              
            }
            catch (Exception ex)
            {
                var start = DateTime.UtcNow;
                var endUpdateTest = new GlobalLogger(testSetId, "7 End endUpdateTest")
                {
                    Message = DateTime.UtcNow.Subtract(start).TotalSeconds.ToString(CultureInfo.InvariantCulture),
                    Action = "Step endUpdateTest Exception =>  " + ex.Message,
                    StackTrace = ex.StackTrace
                };
                await TableLoggerManagerFACELogger.InsertOrUpdateAsync(endUpdateTest);
                
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                throw;
            }
        }

        /// <summary>
        /// Metodo que permite registrar en el Log cualquier mensaje o evento que deeemos
        /// </summary>
        /// <param name="objData">Un Objeto que se serializara en Json a String y se mostrara en el Logger</param>
        /// <param name="Step">El paso del Log o de los mensajes</param>
        /// <param name="msg">Un mensaje adicional si no hay objdata, por ejemplo</param>
        private static void SetLogger(object objData, string Step, string msg, string keyUnique = "")
        {
            object resultJson;

            if (objData != null)
                resultJson = JsonConvert.SerializeObject(objData);
            else
                resultJson = String.Empty;

            GlobalLogger lastZone;
            if (string.IsNullOrEmpty(keyUnique))
                lastZone = new GlobalLogger("202012", "202012") { Message = Step + " --> " + resultJson + " -- Msg --" + msg };
            else
                lastZone = new GlobalLogger(keyUnique, keyUnique) { Message = Step + " --> " + resultJson + " -- Msg --" + msg };

            TableLoggerManagerFACELogger.InsertOrUpdate(lastZone);
        }

        private static async Task MigrateCertificate(string contributorCode)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>
            {
                new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "Migrate.Certificate.Event",
                    Data = JsonConvert.SerializeObject(contributorCode),
                    EventTime = DateTime.UtcNow,
                    Subject = $"|MigrateCertificate|",
                    DataVersion = "2.0"
                }
            };
            await EventGridManager.Instance("EventGridKeyProd", "EventGridTopicEndpointProd").SendMessagesToEventGridAsync(eventsList);
        }

        class SendToActivateContributorResponse
        {
            [JsonProperty(PropertyName = "success")]
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }

            [JsonProperty(PropertyName = "detail")]
            public string Detail { get; set; }

            [JsonProperty(PropertyName = "trace")]
            public string Trace { get; set; }

            [JsonProperty(PropertyName = "testSetId")]
            public string TestSetId { get; set; }            
        }

    }
}
