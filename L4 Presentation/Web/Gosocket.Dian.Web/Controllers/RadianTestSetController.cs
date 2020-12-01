using Gosocket.Dian.Domain;
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

        public RadianTestSetController(IRadianTestSetService radianTestSetService)
        {
            _radianTestSetService = radianTestSetService;
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
                    EndorsementTotalRequired = x.EndorsementTotalRequired,
                    EndorsementCancellationTotalRequired = x.EndorsementCancellationTotalRequired,
                    GuaranteeTotalRequired = x.GuaranteeTotalRequired,
                    ElectronicMandateTotalRequired = x.ElectronicMandateTotalRequired,
                    EndMandateTotalRequired = x.EndMandateTotalRequired,
                    PaymentNotificationTotalRequired = x.PaymentNotificationTotalRequired,
                    CirculationLimitationTotalRequired = x.CirculationLimitationTotalRequired,
                    EndCirculationLimitationTotalRequired = x.EndCirculationLimitationTotalRequired,
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
                EndorsementTotalRequired =0,
                EndorsementCancellationTotalRequired = 0,
                GuaranteeTotalRequired =0,
                ElectronicMandateTotalRequired = 0,
                EndMandateTotalRequired = 0,
                PaymentNotificationTotalRequired = 0,
                CirculationLimitationTotalRequired = 0,
                EndCirculationLimitationTotalRequired = 0
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
                EndorsementTotalRequired = model.EndorsementTotalRequired,
                EndorsementTotalAcceptedRequired = model.EndorsementTotalAcceptedRequired,
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
                EndCirculationLimitationTotalRequired = model.EndCirculationLimitationTotalRequired
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
            List<RadianOperationMode> list = _radianTestSetService.OperationModeList( Domain.Common.RadianOperationMode.None);
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
                EndorsementTotalRequired = testSet.EndorsementTotalRequired,
                EndorsementTotalAcceptedRequired = testSet.EndorsementTotalAcceptedRequired,
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
                EndorsementTotalRequired = model.EndorsementTotalRequired,
                EndorsementTotalAcceptedRequired = model.EndorsementTotalAcceptedRequired,
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
                UpdateBy = User.Identity.Name,
                Date = DateTime.UtcNow
            });

            if (result)
                return RedirectToAction(nameof(Index));

            ViewBag.ErrorMessage = "Ocurrio un problema editando el Set de Pruebas de Radian";
            return View("Edit", model);
        }

    }
}