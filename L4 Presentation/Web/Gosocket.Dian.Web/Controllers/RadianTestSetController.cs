﻿using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Managers;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianTestSetController : Controller
    {
        readonly ContributorService contributorService = new ContributorService();
        private RadianTestSetManager testSetManager = new RadianTestSetManager();

        // GET: RadianSetTest
        public ActionResult Index()
        {
            RadianTestSetTableViewModel model = new RadianTestSetTableViewModel
            {
                RadianTestSets = testSetManager.GetAllTestSet().Select(x => new RadianTestSetViewModel
                {
                    OperationModeName = contributorService.GetOperationMode(int.Parse(x.PartitionKey)).Name,
                    Active = x.Active,
                    CreatedBy = x.CreatedBy,
                    Date = x.Date,
                    Description = x.Description,
                    TotalDocumentRequired = x.TotalDocumentRequired,
                    TotalDocumentAcceptedRequired = x.TotalDocumentAcceptedRequired,
                    ReceiptNoticeTotalRequired              = x.ReceiptNoticeTotalRequired,
                    ReceiptServiceTotalRequired             = x.ReceiptServiceTotalRequired,
                    ExpressAcceptanceTotalRequired          = x.ExpressAcceptanceTotalRequired,
                    AutomaticAcceptanceTotalRequired        = x.AutomaticAcceptanceTotalRequired,
                    RejectInvoiceTotalRequired              = x.RejectInvoiceTotalRequired,
                    ApplicationAvailableTotalRequired       = x.ApplicationAvailableTotalRequired,
                    EndorsementTotalRequired                = x.EndorsementTotalRequired,
                    EndorsementCancellationTotalRequired    = x.EndorsementCancellationTotalRequired,
                    GuaranteeTotalRequired                  = x.GuaranteeTotalRequired,
                    ElectronicMandateTotalRequired          = x.ElectronicMandateTotalRequired,
                    EndMandateTotalRequired                 = x.EndMandateTotalRequired,
                    PaymentNotificationTotalRequired        = x.PaymentNotificationTotalRequired,
                    CirculationLimitationTotalRequired      = x.CirculationLimitationTotalRequired,
                    EndCirculationLimitationTotalRequired   = x.EndCirculationLimitationTotalRequired,
                    TestSetId = x.TestSetId.ToString(),
                    UpdateBy = x.UpdateBy,
                    OperationModeId = int.Parse(x.PartitionKey)
                }).ToList()
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSet;
            return View(model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Add()
        {
            var model = new RadianTestSetViewModel
            {
                //StartDate = DateTime.UtcNow,
                //EndDate = DateTime.UtcNow.AddMonths(3),
                TotalDocumentRequired = 14,
                TotalDocumentAcceptedRequired = 0,
                ReceiptNoticeTotalRequired = 1,
                ReceiptServiceTotalRequired = 1,
                ExpressAcceptanceTotalRequired = 1,
                AutomaticAcceptanceTotalRequired = 1,
                RejectInvoiceTotalRequired = 1,
                ApplicationAvailableTotalRequired = 1,
                EndorsementTotalRequired = 1,
                EndorsementCancellationTotalRequired = 1,
                GuaranteeTotalRequired = 1,
                ElectronicMandateTotalRequired = 1,
                EndMandateTotalRequired = 1,
                PaymentNotificationTotalRequired = 1,
                CirculationLimitationTotalRequired = 1,
                EndCirculationLimitationTotalRequired = 1
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
                var testSetExists = testSetManager.GetTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString());
                if (testSetExists != null)
                {
                    ViewBag.ErrorExistsTestSet = true;
                    return View("Add", model);
                }
            }
            var result = testSetManager.InsertTestSet(new RadianTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString())
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
                //EndDate = model.EndDate,
                //StartDate = model.StartDate,
            }
            );
            if (result)
            {
                return RedirectToAction("Index"/*, new { contributorId = model.ContributorId }*/);
            }

            ViewBag.ErrorMessage = "Ocurrio un problema creando el Set de Pruebas de Radian";
            return View("Add", model);
        }


        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Edit(int operationModeId)
        {
            var testSet = testSetManager.GetTestSet(operationModeId.ToString(), operationModeId.ToString());
            if (testSet == null)
                return RedirectToAction(nameof(Index));

            var model = new RadianTestSetViewModel
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
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianSetPruebas;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Edit(RadianTestSetViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Edit", model);

            var result = testSetManager.InsertTestSet(new RadianTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString())
            {
                TestSetId = Guid.Parse(model.TestSetId).ToString(),//Guid.NewGuid(),
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