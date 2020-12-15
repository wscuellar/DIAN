using Gosocket.Dian.Application;
using Gosocket.Dian.DataContext.Repositories;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Activation
{
    public static class UpdateTestSetResult
    {
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly ContributorOperationsService contributorOperationService = new ContributorOperationsService();
        private static readonly SoftwareService softwareService = new SoftwareService();
        private static readonly IRadianSoftwareRepository _radianSoftwareRepository = new RadianSoftwareRepository();
        private static readonly IRadianCallSoftwareService radianSoftwareService = new RadianCallSoftwareService(_radianSoftwareRepository);
        private static readonly TableManager globalTestSetTableManager = new TableManager("GlobalTestSet");
        private static readonly TableManager globalTestSetResultTableManager = new TableManager("GlobalTestSetResult");
        private static readonly TableManager radianTestSetResultTableManager = new TableManager("RadianTestSetResult");
        private static readonly TableManager globalTestSetTrackingTableManager = new TableManager("GlobalTestSetTracking");
        private static readonly TableManager contributorTableManager = new TableManager("GlobalContributor");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        private static readonly GlobalRadianOperationService globalRadianOperationService = new GlobalRadianOperationService();
        private static readonly TableManager radianTestSetTableManager = new TableManager("RadianTestSet");
        private static readonly TableManager globalRadianOperations = new TableManager("GlobalRadianOperations");
        private static readonly TableManager TableManagerGlobalLogger = new TableManager("GlobalLogger");
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");


        // Set queue name 
        private const string queueName = "global-test-set-tracking-input%Slot%";

        [FunctionName("UpdateTestSetResult")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")] string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            try
            {
                var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(myQueueItem);
                var globalTestSetTracking = JsonConvert.DeserializeObject<GlobalTestSetTracking>(eventGridEvent.Data.ToString());

                await globalTestSetTrackingTableManager.InsertOrUpdateAsync(globalTestSetTracking);

                var allGlobalTestSetTracking = globalTestSetTrackingTableManager.FindByPartition<GlobalTestSetTracking>(globalTestSetTracking.TestSetId);
                var operation = globalRadianOperations.Find<GlobalRadianOperations>(globalTestSetTracking.SenderCode, globalTestSetTracking.SoftwareId);
                var radianTestSet = radianTestSetTableManager.FindByPartition<RadianTestSet>(operation.SoftwareType.ToString());


                //Valida RADIAN
                if (radianTestSet != null)
                {
                    // Roberto Alvarado 20202/11/25
                    // Proceso de RADIAN TestSetResults

                    var result = new List<XmlParamsResponseTrackId>();
                   
                    // traigo los datos de RadianTestSetResult
                    var radianTestSetResults = radianTestSetResultTableManager.FindByPartition<RadianTestSetResult>(globalTestSetTracking.SenderCode);

                    // Valido que este en Process el registro de Set de pruebas
                    var radianTesSetResult = radianTestSetResults.SingleOrDefault(t => !t.Deleted &&
                                                                                t.Id == globalTestSetTracking.TestSetId &&
                                                                                t.Status == (int)TestSetStatus.InProcess);

                    // Ubico con el servicio si RadianOperation esta activo y no continua el proceso.
                    bool isActive = globalRadianOperationService.IsActive(globalTestSetTracking.SenderCode, new Guid(globalTestSetTracking.SoftwareId));
                    if (isActive)
                        return;


                    // string key = operation.SoftwareType.ToString() + "|" + globalTestSetTracking.SoftwareId;

                    // Busco todos los Set de Pruebas por el NIT del Contributor
                    var listradianTestSetResult = radianTestSetResultTableManager.FindByPartition<RadianTestSetResult>(globalTestSetTracking.SenderCode);

                    // busco el registro del set de pruebas a actualizar
                    var radianTestSetResult = listradianTestSetResult.Where(r => r.Id == globalTestSetTracking.TestSetId);
                    // Si no lo encuentro, no continuo
                    if (radianTestSetResult == null)
                        return;


                    foreach (var item in allGlobalTestSetTracking)
                    {
                        //Consigue inbformacion del CUDE
                        GlobalDocValidatorDocumentMeta validatorDocumentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(item.TrackId, item.TrackId);

                        item.DocumentTypeId = validatorDocumentMeta.EventCode;
                    }

                    // Esto ya no es neceasario 20202-12-14 Roberto Alvarado
                    // Le asigno el Id 
                    // radianTesSetResult.Id = globalTestSetTracking.TestSetId;

                    radianTesSetResult.TotalDocumentSent = allGlobalTestSetTracking.Count;
                    radianTesSetResult.TotalDocumentAccepted = allGlobalTestSetTracking.Count(a => a.IsValid);
                    radianTesSetResult.TotalDocumentsRejected = allGlobalTestSetTracking.Count(a => !a.IsValid);

                    // Acuse de Recibo
                    string tipo = EventStatus.Receipt.ToString();
                    radianTesSetResult.TotalReceiptNoticeSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.ReceiptNoticeAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.ReceiptNoticeRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);
                  
                    // Recibo del Bien
                    tipo = EventStatus.Received.ToString();
                    radianTesSetResult.TotalReceiptServiceSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.ReceiptServiceAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.ReceiptServiceRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);

                    //  Aceptación expresa
                    tipo = EventStatus.Accepted.ToString();
                    radianTesSetResult.TotalExpressAcceptanceSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.ExpressAcceptanceAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.ExpressAcceptanceRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);


                    // Manifestación de aceptación
                    tipo = EventStatus.AceptacionTacita.ToString();
                    radianTesSetResult.TotalAutomaticAcceptanceSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.AutomaticAcceptanceAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.AutomaticAcceptanceRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);



                    // Rechazo factura electrónica
                    tipo = EventStatus.Rejected.ToString();
                    radianTesSetResult.TotalRejectInvoiceSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.RejectInvoiceAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.RejectInvoiceRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);


                    // Solicitud disponibilización
                    tipo = EventStatus.SolicitudDisponibilizacion.ToString();
                    radianTesSetResult.TotalApplicationAvailableSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.ApplicationAvailableAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.ApplicationAvailableRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);


                    // Endoso de propiedad 
                    tipo = EventStatus.EndosoPropiedad.ToString();
                    radianTesSetResult.TotalEndorsementPropertySent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.EndorsementPropertyAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.EndorsementPropertyRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);

                    // Endoso de Garantia 
                    tipo = EventStatus.EndosoGarantia.ToString();
                    radianTesSetResult.TotalEndorsementGuaranteeSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.EndorsementGuaranteeAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.EndorsementGuaranteeRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);

                    // Endoso de Procuracion 
                    tipo = EventStatus.EndosoProcuracion.ToString();
                    radianTesSetResult.TotalEndorsementProcurementSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.EndorsementProcurementAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.EndorsementProcurementRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);

                    // Cancelación de endoso 
                    tipo = EventStatus.InvoiceOfferedForNegotiation.ToString();
                    radianTesSetResult.TotalEndorsementCancellationSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.EndorsementCancellationAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.EndorsementCancellationRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);


                    // Avales
                    tipo = EventStatus.Avales.ToString();
                    radianTesSetResult.TotalGuaranteeSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.GuaranteeAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.GuaranteeRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);


                    // Mandato electrónico
                    tipo = EventStatus.Mandato.ToString();
                    radianTesSetResult.TotalElectronicMandateSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.ElectronicMandateAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.ElectronicMandateRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);


                    // Terminación mandato
                    tipo = EventStatus.TerminacionMandato.ToString();
                    radianTesSetResult.TotalEndMandateSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.EndMandateAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.EndMandateRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);

                    // Notificación de pago
                    tipo = EventStatus.NotificacionPagoTotalParcial.ToString();
                    radianTesSetResult.TotalPaymentNotificationSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.PaymentNotificationAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.PaymentNotificationRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);


                    // Limitación de circulación     
                    tipo = EventStatus.NegotiatedInvoice.ToString();
                    radianTesSetResult.TotalCirculationLimitationSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.CirculationLimitationAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.CirculationLimitationRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);

                    // Terminación limitación  
                    tipo = EventStatus.AnulacionLimitacionCirculacion.ToString();
                    radianTesSetResult.TotalEndCirculationLimitationSent = allGlobalTestSetTracking.Count(a => a.DocumentTypeId == tipo);
                    radianTesSetResult.EndCirculationLimitationAccepted = allGlobalTestSetTracking.Count(a => a.IsValid && a.DocumentTypeId == tipo);
                    radianTesSetResult.EndCirculationLimitationRejected = allGlobalTestSetTracking.Count(a => !a.IsValid && a.DocumentTypeId == tipo);

                    // Definimos la Aceptacion y cambio de estado
                    if (radianTesSetResult.TotalDocumentAccepted >= radianTesSetResult.TotalDocumentAcceptedRequired
                            && radianTesSetResult.ReceiptNoticeTotalRequired >= radianTesSetResult.ReceiptNoticeTotalAcceptedRequired
                            && radianTesSetResult.ReceiptServiceTotalRequired >= radianTesSetResult.ReceiptServiceTotalAcceptedRequired
                            && radianTesSetResult.ExpressAcceptanceTotalRequired >= radianTesSetResult.ExpressAcceptanceTotalAcceptedRequired
                            && radianTesSetResult.AutomaticAcceptanceTotalRequired >= radianTesSetResult.AutomaticAcceptanceTotalAcceptedRequired
                            && radianTesSetResult.RejectInvoiceTotalRequired >= radianTesSetResult.RejectInvoiceTotalAcceptedRequired
                            && radianTesSetResult.ApplicationAvailableTotalRequired >= radianTesSetResult.ApplicationAvailableTotalAcceptedRequired
                            && radianTesSetResult.EndorsementPropertyTotalRequired >= radianTesSetResult.EndorsementPropertyTotalAcceptedRequired
                            && radianTesSetResult.EndorsementProcurementTotalRequired >= radianTesSetResult.EndorsementProcurementTotalAcceptedRequired
                            && radianTesSetResult.EndorsementGuaranteeTotalRequired >= radianTesSetResult.EndorsementGuaranteeTotalAcceptedRequired
                            && radianTesSetResult.EndorsementCancellationTotalRequired >= radianTesSetResult.EndorsementCancellationTotalAcceptedRequired
                            && radianTesSetResult.GuaranteeTotalRequired >= radianTesSetResult.GuaranteeTotalAcceptedRequired
                            && radianTesSetResult.ElectronicMandateTotalRequired >= radianTesSetResult.ElectronicMandateTotalAcceptedRequired
                            && radianTesSetResult.EndMandateTotalRequired >= radianTesSetResult.EndMandateTotalAcceptedRequired
                            && radianTesSetResult.PaymentNotificationTotalRequired >= radianTesSetResult.PaymentNotificationTotalAcceptedRequired
                            && radianTesSetResult.CirculationLimitationTotalRequired >= radianTesSetResult.CirculationLimitationTotalAcceptedRequired
                            && radianTesSetResult.EndCirculationLimitationTotalRequired >= radianTesSetResult.EndCirculationLimitationTotalAcceptedRequired
                            && radianTesSetResult.Status == (int)TestSetStatus.InProcess)
                        radianTesSetResult.Status = (int)TestSetStatus.Accepted;

                    if (radianTesSetResult.TotalDocumentsRejected > (radianTesSetResult.TotalDocumentRequired - radianTesSetResult.TotalDocumentAcceptedRequired)
                            && radianTesSetResult.Status == (int)TestSetStatus.InProcess)
                        radianTesSetResult.Status = (int)TestSetStatus.Rejected;

                    // Escribo el registro de RadianTestResult
                    await globalTestSetResultTableManager.InsertOrUpdateAsync(radianTesSetResult);

                    // Si es aceptado el set de pruebas se activa el contributor en el ambiente de habilitacion
                    if (radianTesSetResult.Status == (int)TestSetStatus.Accepted)
                    {       

                        // Send to activate contributor in production
                        if (ConfigurationManager.GetValue("Environment") == "Hab")
                        {
                            
                            try
                            {
                                #region Proceso Radian

                                Contributor contributor = contributorService.GetByCode(globalTestSetTracking.SenderCode);
                                GlobalRadianOperations isPartipantActive = globalRadianOperationService.EnableParticipantRadian(globalTestSetTracking.SenderCode, globalTestSetTracking.SoftwareId);
                                if (!string.IsNullOrEmpty(isPartipantActive.RadianStatus))
                                {
                                    contributorService.SetToEnabledRadian(contributor.Id, isPartipantActive.RadianContributorTypeId, isPartipantActive.RowKey);
                                }
                                #endregion

                                #region Pendiente migracion SQL

                                #endregion

                            }
                            catch (Exception ex)
                            {
                                log.Error($"Error al enviar a activar RADIAN contribuyente con id {globalTestSetTracking.SenderCode} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                                throw;
                            }
                        }
                    }
                }
                else // Factura Electronica
                {

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
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                throw;
            }
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
        }

    }
}
