using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianTestSetController : Controller
    {

        private readonly IRadianTestSetService _radianTestSetService;
        private readonly IRadianApprovedService _radianAprovedService;

        public RadianTestSetController(IRadianTestSetService radianTestSetService, IRadianApprovedService radianAprovedService)
        {
            _radianTestSetService = radianTestSetService;
            _radianAprovedService = radianAprovedService;
        }

        // GET: RadianSetTest
        public ActionResult Index()
        {
            RadianTestSetTableViewModel model = new RadianTestSetTableViewModel
            {
                RadianTestSets = _radianTestSetService.GetAllTestSet().Select(x => new RadianTestSetViewModel
                {
                    OperationModeName = _radianTestSetService.GetOperationMode(int.Parse(x.PartitionKey))?.Name,
                    Active = x.Active,
                    CreatedBy = x.CreatedBy,
                    Date = x.Date,
                    Description = x.Description,
                    TotalDocumentRequired = x.TotalDocumentRequired,
                    TotalDocumentAcceptedRequired = x.TotalDocumentAcceptedRequired,
                    ReceiptNoticeTotalRequired = x.ReceiptNoticeTotalRequired,
                    ReceiptServiceTotalRequired = x.ReceiptServiceTotalRequired,
                    ExpressAcceptanceTotalRequired = x.ExpressAcceptanceTotalRequired,
                    AutomaticAcceptanceTotalRequired = x.AutomaticAcceptanceTotalRequired,
                    RejectInvoiceTotalRequired = x.RejectInvoiceTotalRequired,
                    ApplicationAvailableTotalRequired = x.ApplicationAvailableTotalRequired,
                    // Endosos
                    EndorsementTotalRequired = x.EndorsementPropertyTotalRequired,
                    EndorsementCancellationTotalRequired = x.EndorsementCancellationTotalRequired,
                    EndorsementGuaranteeTotalAcceptedRequired = x.EndorsementGuaranteeTotalAcceptedRequired,
                    EndorsementGuaranteeTotalRequired = x.EndorsementGuaranteeTotalRequired,
                    EndorsementProcurementTotalAcceptedRequired = x.EndorsementProcurementTotalAcceptedRequired,
                    EndorsementProcurementTotalRequired = x.EndorsementProcurementTotalRequired,
                    GuaranteeTotalRequired = x.GuaranteeTotalRequired,
                    ElectronicMandateTotalRequired = x.ElectronicMandateTotalRequired,
                    EndMandateTotalRequired = x.EndMandateTotalRequired,
                    PaymentNotificationTotalRequired = x.PaymentNotificationTotalRequired,
                    CirculationLimitationTotalRequired = x.CirculationLimitationTotalRequired,
                    EndCirculationLimitationTotalRequired = x.EndCirculationLimitationTotalRequired,
                    TransferEconomicRightsTotalRequired = x.TransferEconomicRightsTotalRequired,
                    TransferEconomicRightsTotalAcceptedRequired = x.TransferEconomicRightsTotalAcceptedRequired,
                    NotificationDebtorOfTransferEconomicRightsTotalRequired = x.NotificationDebtorOfTransferEconomicRightsTotalRequired,
                    NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired = x.NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired,
                    PaymentOfTransferEconomicRightsTotalRequired = x.PaymentOfTransferEconomicRightsTotalRequired,
                    PaymentOfTransferEconomicRightsTotalAcceptedRequired = x.PaymentOfTransferEconomicRightsTotalAcceptedRequired,
                    EndorsementWithEffectOrdinaryAssignmentTotalRequired = x.EndorsementWithEffectOrdinaryAssignmentTotalRequired,
                    EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired = x.EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired,
                    ObjectionTotalRequired = x.ObjectionTotalRequired,
                    ObjectionTotalAcceptedRequired = x.ObjectionTotalAcceptedRequired,
                    TestSetId = x.TestSetId.ToString(),
                    UpdateBy = x.UpdateBy,
                    OperationModeId = int.Parse(x.PartitionKey)
                }).ToList()
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianSetPruebas;
            return View(model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Add()
        {
            List<OperationModeViewModel> list;
            list = LoadSoftwareOperationMode();

            RadianTestSetViewModel model = new RadianTestSetViewModel
            {
                OperationModes = list,
                TotalDocumentRequired = 0,
                TotalDocumentAcceptedRequired = 0,
                ReceiptNoticeTotalRequired = 0,
                ReceiptServiceTotalRequired = 0,
                ExpressAcceptanceTotalRequired = 0,
                AutomaticAcceptanceTotalRequired = 0,
                RejectInvoiceTotalRequired = 0,
                ApplicationAvailableTotalRequired = 0,
                EndorsementTotalRequired = 0,
                EndorsementCancellationTotalRequired = 0,
                GuaranteeTotalRequired = 0,
                ElectronicMandateTotalRequired = 0,
                EndMandateTotalRequired = 0,
                PaymentNotificationTotalRequired = 0,
                CirculationLimitationTotalRequired = 0,
                EndCirculationLimitationTotalRequired = 0,
                ReportForPaymentTotalRequired = 0,
                ReportForPaymentTotalAcceptedRequired = 0,
                TransferEconomicRightsTotalRequired = 0,
                TransferEconomicRightsTotalAcceptedRequired = 0,
                NotificationDebtorOfTransferEconomicRightsTotalRequired = 0,
                NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired = 0,
                PaymentOfTransferEconomicRightsTotalRequired = 0,
                PaymentOfTransferEconomicRightsTotalAcceptedRequired = 0,
                EndorsementWithEffectOrdinaryAssignmentTotalRequired = 0,
                EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired = 0,
                ObjectionTotalRequired = 0,
                ObjectionTotalAcceptedRequired = 0
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianSetPruebas;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Add(RadianTestSetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Add", model);
            }
            if (!model.TestSetReplace)
            {
                RadianTestSet testSetExists = _radianTestSetService.GetTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString());
                if (testSetExists != null)
                {
                    ViewBag.ErrorExistsTestSet = true;
                    model.OperationModes = LoadSoftwareOperationMode();
                    return View("Add", model);
                }
            }
            bool result = _radianTestSetService.InsertTestSet(new RadianTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString())
            {
                TestSetId = Guid.NewGuid().ToString(),
                Active = true,
                CreatedBy = User.Identity.Name,
                Description = model.Description,
                TotalDocumentRequired = model.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = model.TotalDocumentAcceptedRequired,
                ReceiptNoticeTotalRequired = model.ReceiptNoticeTotalRequired,
                ReceiptNoticeTotalAcceptedRequired = model.ReceiptNoticeTotalAcceptedRequired,
                ReceiptServiceTotalRequired = model.ReceiptServiceTotalRequired,
                ReceiptServiceTotalAcceptedRequired = model.ReceiptServiceTotalAcceptedRequired,
                ExpressAcceptanceTotalRequired = model.ExpressAcceptanceTotalRequired,
                ExpressAcceptanceTotalAcceptedRequired = model.ExpressAcceptanceTotalAcceptedRequired,
                AutomaticAcceptanceTotalRequired = model.AutomaticAcceptanceTotalRequired,
                AutomaticAcceptanceTotalAcceptedRequired = model.AutomaticAcceptanceTotalAcceptedRequired,
                RejectInvoiceTotalRequired = model.RejectInvoiceTotalRequired,
                RejectInvoiceTotalAcceptedRequired = model.RejectInvoiceTotalAcceptedRequired,
                ApplicationAvailableTotalRequired = model.ApplicationAvailableTotalRequired,
                ApplicationAvailableTotalAcceptedRequired = model.ApplicationAvailableTotalAcceptedRequired,
                // Endosos
                EndorsementPropertyTotalRequired = model.EndorsementTotalRequired,
                EndorsementPropertyTotalAcceptedRequired = model.EndorsementTotalAcceptedRequired,
                EndorsementProcurementTotalRequired = model.EndorsementProcurementTotalRequired,
                EndorsementProcurementTotalAcceptedRequired = model.EndorsementProcurementTotalAcceptedRequired,
                EndorsementGuaranteeTotalRequired = model.EndorsementGuaranteeTotalRequired,
                EndorsementGuaranteeTotalAcceptedRequired = model.EndorsementGuaranteeTotalAcceptedRequired,
                EndorsementCancellationTotalRequired = model.EndorsementCancellationTotalRequired,
                EndorsementCancellationTotalAcceptedRequired = model.EndorsementCancellationTotalAcceptedRequired,
                GuaranteeTotalRequired = model.GuaranteeTotalRequired,
                GuaranteeTotalAcceptedRequired = model.GuaranteeTotalAcceptedRequired,
                ElectronicMandateTotalRequired = model.ElectronicMandateTotalRequired,
                ElectronicMandateTotalAcceptedRequired = model.ElectronicMandateTotalAcceptedRequired,
                EndMandateTotalRequired = model.EndMandateTotalRequired,
                EndMandateTotalAcceptedRequired = model.EndMandateTotalAcceptedRequired,
                PaymentNotificationTotalRequired = model.PaymentNotificationTotalRequired,
                PaymentNotificationTotalAcceptedRequired = model.PaymentNotificationTotalAcceptedRequired,
                CirculationLimitationTotalRequired = model.CirculationLimitationTotalRequired,
                CirculationLimitationTotalAcceptedRequired = model.CirculationLimitationTotalAcceptedRequired,
                EndCirculationLimitationTotalAcceptedRequired = model.EndCirculationLimitationTotalAcceptedRequired,
                EndCirculationLimitationTotalRequired = model.EndCirculationLimitationTotalRequired,
                ReportForPaymentTotalRequired = model.ReportForPaymentTotalRequired,
                ReportForPaymentTotalAcceptedRequired = model.ReportForPaymentTotalAcceptedRequired,
                TransferEconomicRightsTotalRequired = model.TransferEconomicRightsTotalRequired,
                TransferEconomicRightsTotalAcceptedRequired = model.TransferEconomicRightsTotalAcceptedRequired,
                NotificationDebtorOfTransferEconomicRightsTotalRequired = model.NotificationDebtorOfTransferEconomicRightsTotalRequired,
                NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired = model.NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired,
                PaymentOfTransferEconomicRightsTotalRequired = model.PaymentOfTransferEconomicRightsTotalRequired,
                PaymentOfTransferEconomicRightsTotalAcceptedRequired = model.PaymentOfTransferEconomicRightsTotalAcceptedRequired,
                EndorsementWithEffectOrdinaryAssignmentTotalRequired = model.EndorsementWithEffectOrdinaryAssignmentTotalRequired,
                EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired = model.EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired,
                ObjectionTotalRequired = model.ObjectionTotalRequired,
                ObjectionTotalAcceptedRequired = model.ObjectionTotalAcceptedRequired
            }
            );
            if (result)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Ocurrio un problema creando el Set de Pruebas de Radian";
            return View("Add", model);
        }

        private List<OperationModeViewModel> LoadSoftwareOperationMode()
        {
            List<Domain.RadianOperationMode> list = _radianTestSetService.OperationModeList(Domain.Common.RadianOperationMode.None);
            List<OperationModeViewModel> OperationModes = list.Select(t => new OperationModeViewModel() { Id = t.Id, Name = t.Name }).ToList();
            return OperationModes;
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Edit(int operationModeId)
        {
            RadianTestSet testSet = _radianTestSetService.GetTestSet(operationModeId.ToString(), operationModeId.ToString());
            if (testSet == null)
                return RedirectToAction(nameof(Index));

            RadianTestSetViewModel model = new RadianTestSetViewModel
            {
                TotalDocumentRequired = testSet.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = testSet.TotalDocumentAcceptedRequired,
                Description = testSet.Description,
                ReceiptNoticeTotalRequired = testSet.ReceiptNoticeTotalRequired,
                ReceiptNoticeTotalAcceptedRequired = testSet.ReceiptNoticeTotalAcceptedRequired,
                ReceiptServiceTotalRequired = testSet.ReceiptServiceTotalRequired,
                ReceiptServiceTotalAcceptedRequired = testSet.ReceiptServiceTotalAcceptedRequired,
                ExpressAcceptanceTotalRequired = testSet.ExpressAcceptanceTotalRequired,
                ExpressAcceptanceTotalAcceptedRequired = testSet.ExpressAcceptanceTotalAcceptedRequired,
                AutomaticAcceptanceTotalRequired = testSet.AutomaticAcceptanceTotalRequired,
                AutomaticAcceptanceTotalAcceptedRequired = testSet.AutomaticAcceptanceTotalAcceptedRequired,
                RejectInvoiceTotalRequired = testSet.RejectInvoiceTotalRequired,
                RejectInvoiceTotalAcceptedRequired = testSet.RejectInvoiceTotalAcceptedRequired,
                ApplicationAvailableTotalRequired = testSet.ApplicationAvailableTotalRequired,
                ApplicationAvailableTotalAcceptedRequired = testSet.ApplicationAvailableTotalAcceptedRequired,
                // Endosos
                EndorsementTotalRequired = testSet.EndorsementPropertyTotalRequired,
                EndorsementTotalAcceptedRequired = testSet.EndorsementPropertyTotalAcceptedRequired,
                EndorsementGuaranteeTotalRequired = testSet.EndorsementGuaranteeTotalRequired,
                EndorsementGuaranteeTotalAcceptedRequired = testSet.EndorsementGuaranteeTotalAcceptedRequired,
                EndorsementProcurementTotalRequired = testSet.EndorsementProcurementTotalRequired,
                EndorsementProcurementTotalAcceptedRequired = testSet.EndorsementProcurementTotalAcceptedRequired,
                EndorsementCancellationTotalRequired = testSet.EndorsementCancellationTotalRequired,
                EndorsementCancellationTotalAcceptedRequired = testSet.EndorsementCancellationTotalAcceptedRequired,
                GuaranteeTotalRequired = testSet.GuaranteeTotalRequired,
                GuaranteeTotalAcceptedRequired = testSet.GuaranteeTotalAcceptedRequired,
                ElectronicMandateTotalRequired = testSet.ElectronicMandateTotalRequired,
                ElectronicMandateTotalAcceptedRequired = testSet.ElectronicMandateTotalAcceptedRequired,
                EndMandateTotalRequired = testSet.EndMandateTotalRequired,
                EndMandateTotalAcceptedRequired = testSet.EndMandateTotalAcceptedRequired,
                PaymentNotificationTotalRequired = testSet.PaymentNotificationTotalRequired,
                PaymentNotificationTotalAcceptedRequired = testSet.PaymentNotificationTotalAcceptedRequired,
                CirculationLimitationTotalRequired = testSet.CirculationLimitationTotalRequired,
                CirculationLimitationTotalAcceptedRequired = testSet.CirculationLimitationTotalAcceptedRequired,
                EndCirculationLimitationTotalAcceptedRequired = testSet.EndCirculationLimitationTotalAcceptedRequired,
                EndCirculationLimitationTotalRequired = testSet.EndCirculationLimitationTotalRequired,
                ReportForPaymentTotalRequired = testSet.ReportForPaymentTotalRequired,
                ReportForPaymentTotalAcceptedRequired = testSet.ReportForPaymentTotalAcceptedRequired,
                TransferEconomicRightsTotalRequired = testSet.TransferEconomicRightsTotalRequired,
                TransferEconomicRightsTotalAcceptedRequired = testSet.TransferEconomicRightsTotalAcceptedRequired,
                NotificationDebtorOfTransferEconomicRightsTotalRequired = testSet.NotificationDebtorOfTransferEconomicRightsTotalRequired,
                NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired = testSet.NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired,
                PaymentOfTransferEconomicRightsTotalRequired = testSet.PaymentOfTransferEconomicRightsTotalRequired,
                PaymentOfTransferEconomicRightsTotalAcceptedRequired = testSet.PaymentOfTransferEconomicRightsTotalAcceptedRequired,
                EndorsementWithEffectOrdinaryAssignmentTotalRequired = testSet.EndorsementWithEffectOrdinaryAssignmentTotalRequired,
                EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired = testSet.EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired,
                ObjectionTotalRequired = testSet.ObjectionTotalRequired,
                ObjectionTotalAcceptedRequired = testSet.ObjectionTotalAcceptedRequired,
                TestSetId = testSet.TestSetId.ToString(),
                OperationModeId = int.Parse(testSet.PartitionKey)
            };
            model.OperationModes = LoadSoftwareOperationMode();

            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianSetPruebas;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Edit(RadianTestSetViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Edit", model);

            bool result = _radianTestSetService.InsertTestSet(new RadianTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString())
            {
                TestSetId = Guid.Parse(model.TestSetId).ToString(),
                Active = true,
                CreatedBy = User.Identity.Name,
                Description = model.Description,
                TotalDocumentRequired = model.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = model.TotalDocumentAcceptedRequired,
                ReceiptNoticeTotalRequired = model.ReceiptNoticeTotalRequired,
                ReceiptNoticeTotalAcceptedRequired = model.ReceiptNoticeTotalAcceptedRequired,
                ReceiptServiceTotalRequired = model.ReceiptServiceTotalRequired,
                ReceiptServiceTotalAcceptedRequired = model.ReceiptServiceTotalAcceptedRequired,
                ExpressAcceptanceTotalRequired = model.ExpressAcceptanceTotalRequired,
                ExpressAcceptanceTotalAcceptedRequired = model.ExpressAcceptanceTotalAcceptedRequired,
                AutomaticAcceptanceTotalRequired = model.AutomaticAcceptanceTotalRequired,
                AutomaticAcceptanceTotalAcceptedRequired = model.AutomaticAcceptanceTotalAcceptedRequired,
                RejectInvoiceTotalRequired = model.RejectInvoiceTotalRequired,
                RejectInvoiceTotalAcceptedRequired = model.RejectInvoiceTotalAcceptedRequired,
                ApplicationAvailableTotalRequired = model.ApplicationAvailableTotalRequired,
                ApplicationAvailableTotalAcceptedRequired = model.ApplicationAvailableTotalAcceptedRequired,
                // Endosos
                EndorsementPropertyTotalRequired = model.EndorsementTotalRequired,
                EndorsementPropertyTotalAcceptedRequired = model.EndorsementTotalAcceptedRequired,
                EndorsementProcurementTotalRequired = model.EndorsementProcurementTotalRequired,
                EndorsementProcurementTotalAcceptedRequired = model.EndorsementProcurementTotalAcceptedRequired,
                EndorsementGuaranteeTotalRequired = model.EndorsementGuaranteeTotalRequired,
                EndorsementGuaranteeTotalAcceptedRequired = model.EndorsementGuaranteeTotalAcceptedRequired,
                EndorsementCancellationTotalRequired = model.EndorsementCancellationTotalRequired,
                EndorsementCancellationTotalAcceptedRequired = model.EndorsementCancellationTotalAcceptedRequired,
                GuaranteeTotalRequired = model.GuaranteeTotalRequired,
                GuaranteeTotalAcceptedRequired = model.GuaranteeTotalAcceptedRequired,
                ElectronicMandateTotalRequired = model.ElectronicMandateTotalRequired,
                ElectronicMandateTotalAcceptedRequired = model.ElectronicMandateTotalAcceptedRequired,
                EndMandateTotalRequired = model.EndMandateTotalRequired,
                EndMandateTotalAcceptedRequired = model.EndMandateTotalAcceptedRequired,
                PaymentNotificationTotalRequired = model.PaymentNotificationTotalRequired,
                PaymentNotificationTotalAcceptedRequired = model.PaymentNotificationTotalAcceptedRequired,
                CirculationLimitationTotalRequired = model.CirculationLimitationTotalRequired,
                CirculationLimitationTotalAcceptedRequired = model.CirculationLimitationTotalAcceptedRequired,
                EndCirculationLimitationTotalAcceptedRequired = model.EndCirculationLimitationTotalAcceptedRequired,
                EndCirculationLimitationTotalRequired = model.EndCirculationLimitationTotalRequired,
                ReportForPaymentTotalRequired = model.ReportForPaymentTotalRequired,
                ReportForPaymentTotalAcceptedRequired = model.ReportForPaymentTotalAcceptedRequired,
                TransferEconomicRightsTotalRequired = model.TransferEconomicRightsTotalRequired,
                TransferEconomicRightsTotalAcceptedRequired = model.TransferEconomicRightsTotalAcceptedRequired,
                NotificationDebtorOfTransferEconomicRightsTotalRequired = model.NotificationDebtorOfTransferEconomicRightsTotalRequired,
                NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired = model.NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired,
                PaymentOfTransferEconomicRightsTotalRequired = model.PaymentOfTransferEconomicRightsTotalRequired,
                PaymentOfTransferEconomicRightsTotalAcceptedRequired = model.PaymentOfTransferEconomicRightsTotalAcceptedRequired,
                EndorsementWithEffectOrdinaryAssignmentTotalRequired = model.EndorsementWithEffectOrdinaryAssignmentTotalRequired,
                EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired = model.EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired,
                ObjectionTotalRequired = model.ObjectionTotalRequired,
                ObjectionTotalAcceptedRequired = model.ObjectionTotalAcceptedRequired,
                UpdateBy = User.Identity.Name,
                Date = DateTime.UtcNow
            });

            if (result)
                return RedirectToAction(nameof(Index));

            ViewBag.ErrorMessage = "Ocurrio un problema editando el Set de Pruebas de Radian";
            return View("Edit", model);
        }

        [HttpPost]
        public JsonResult GetTestSetSummary(string softwareType)
        {

            RadianOperationModeTestSet softwareTypeEnum = Domain.Common.EnumHelper.GetValueFromDescription<RadianOperationModeTestSet>(softwareType);
            string key = ((int)softwareTypeEnum).ToString();
            RadianTestSet testSet = _radianAprovedService.GetTestSet(key);
            List<EventCountersViewModel> events = new List<EventCountersViewModel>();
            if (testSet != null)
            {
                // Acuse de recibo
                events.Add(new EventCountersViewModel() { EventName = EventStatus.Received.GetDescription(), Counter1 = testSet.ReceiptNoticeTotalAcceptedRequired, Counter2 = testSet.ReceiptNoticeTotalRequired, Counter3 = 0 });

                //Recibo del bien
                events.Add(new EventCountersViewModel() { EventName = EventStatus.Receipt.GetDescription(), Counter1 = testSet.ReceiptServiceTotalAcceptedRequired, Counter2 = testSet.ReceiptServiceTotalRequired, Counter3 = 0 });

                // Aceptación expresa
                events.Add(new EventCountersViewModel() { EventName = EventStatus.Accepted.GetDescription(), Counter1 = testSet.ExpressAcceptanceTotalAcceptedRequired, Counter2 = testSet.ExpressAcceptanceTotalRequired, Counter3 = 0 });

                //Manifestación de aceptación
                events.Add(new EventCountersViewModel() { EventName = EventStatus.AceptacionTacita.GetDescription(), Counter1 = testSet.AutomaticAcceptanceTotalAcceptedRequired, Counter2 = testSet.AutomaticAcceptanceTotalRequired, Counter3 = 0 });

                //Rechazo factura electrónica
                events.Add(new EventCountersViewModel() { EventName = EventStatus.Rejected.GetDescription(), Counter1 = testSet.RejectInvoiceTotalAcceptedRequired, Counter2 = testSet.RejectInvoiceTotalRequired, Counter3 = 0 });

                // Solicitud disponibilización
                events.Add(new EventCountersViewModel() { EventName = EventStatus.SolicitudDisponibilizacion.GetDescription(), Counter1 = testSet.ApplicationAvailableTotalAcceptedRequired, Counter2 = testSet.ApplicationAvailableTotalRequired, Counter3 = 0 });

                // Endoso en Propiedad
                events.Add(new EventCountersViewModel() { EventName = EventStatus.EndosoPropiedad.GetDescription(), Counter1 = testSet.EndorsementPropertyTotalAcceptedRequired, Counter2 = testSet.EndorsementPropertyTotalRequired, Counter3 = 0 });

                // Endoso en Procuracion
                events.Add(new EventCountersViewModel() { EventName = EventStatus.EndosoProcuracion.GetDescription(), Counter1 = testSet.EndorsementProcurementTotalAcceptedRequired, Counter2 = testSet.EndorsementProcurementTotalRequired, Counter3 = 0 });

                // Endoso en Garantia
                events.Add(new EventCountersViewModel() { EventName = EventStatus.EndosoGarantia.GetDescription(), Counter1 = testSet.EndorsementGuaranteeTotalAcceptedRequired, Counter2 = testSet.EndorsementGuaranteeTotalRequired, Counter3 = 0 });

                // Cancelación de endoso
                events.Add(new EventCountersViewModel() { EventName = EventStatus.InvoiceOfferedForNegotiation.GetDescription(), Counter1 = testSet.EndorsementCancellationTotalAcceptedRequired, Counter2 = testSet.EndorsementCancellationTotalRequired, Counter3 = 0 });

                // Avales
                events.Add(new EventCountersViewModel() { EventName = EventStatus.Avales.GetDescription(), Counter1 = testSet.GuaranteeTotalAcceptedRequired, Counter2 = testSet.GuaranteeTotalRequired, Counter3 = 0 });

                // Mandato electrónico
                events.Add(new EventCountersViewModel() { EventName = EventStatus.Mandato.GetDescription(), Counter1 = testSet.ElectronicMandateTotalAcceptedRequired, Counter2 = testSet.ElectronicMandateTotalRequired, Counter3 = 0 });

                // Terminación mandato
                events.Add(new EventCountersViewModel() { EventName = EventStatus.TerminacionMandato.GetDescription(), Counter1 = testSet.EndMandateTotalAcceptedRequired, Counter2 = testSet.EndMandateTotalRequired, Counter3 = 0 });

                // Notificación de pago
                events.Add(new EventCountersViewModel() { EventName = EventStatus.NotificacionPagoTotalParcial.GetDescription(), Counter1 = testSet.PaymentNotificationTotalAcceptedRequired, Counter2 = testSet.PaymentNotificationTotalRequired, Counter3 = 0 });

                // Limitación de circulación
                events.Add(new EventCountersViewModel() { EventName = EventStatus.NegotiatedInvoice.GetDescription(), Counter1 = testSet.CirculationLimitationTotalAcceptedRequired, Counter2 = testSet.CirculationLimitationTotalRequired, Counter3 = 0 });

                // Terminación limitación 
                events.Add(new EventCountersViewModel() { EventName = EventStatus.AnulacionLimitacionCirculacion.GetDescription(), Counter1 = testSet.EndCirculationLimitationTotalAcceptedRequired, Counter2 = testSet.EndCirculationLimitationTotalRequired, Counter3 = 0 });

                // Informe para el pago
                events.Add(new EventCountersViewModel() { EventName = EventStatus.ValInfoPago.GetDescription(), Counter1 = testSet.ReportForPaymentTotalAcceptedRequired, Counter2 = testSet.ReportForPaymentTotalRequired, Counter3 = 0 });

                // Endoso con efectos de cesión ordinaria
                events.Add(new EventCountersViewModel() { EventName = EventStatus.EndorsementWithEffectOrdinaryAssignment.GetDescription(), Counter1 = testSet.EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired, Counter2 = testSet.EndorsementWithEffectOrdinaryAssignmentTotalRequired, Counter3 = 0 });

                // Protesto
                events.Add(new EventCountersViewModel() { EventName = EventStatus.Objection.GetDescription(), Counter1 = testSet.ObjectionTotalAcceptedRequired, Counter2 = testSet.ObjectionTotalRequired, Counter3 = 0 });

                // Transferencia de los derechos económicos
                events.Add(new EventCountersViewModel() { EventName = EventStatus.TransferEconomicRights.GetDescription(), Counter1 = testSet.TransferEconomicRightsTotalAcceptedRequired, Counter2 = testSet.TransferEconomicRightsTotalRequired, Counter3 = 0 });

                // Notificación al deudor sobre la transferencia de los derechos económicos
                events.Add(new EventCountersViewModel() { EventName = EventStatus.NotificationDebtorOfTransferEconomicRights.GetDescription(), Counter1 = testSet.NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired, Counter2 = testSet.NotificationDebtorOfTransferEconomicRightsTotalRequired, Counter3 = 0 });

                // Pago de la transferencia de los derechos económicos
                events.Add(new EventCountersViewModel() { EventName = EventStatus.PaymentOfTransferEconomicRights.GetDescription(), Counter1 = testSet.PaymentOfTransferEconomicRightsTotalAcceptedRequired, Counter2 = testSet.PaymentOfTransferEconomicRightsTotalRequired, Counter3 = 0 });

            }
            return Json(events, JsonRequestBehavior.AllowGet);


        }


    }
}