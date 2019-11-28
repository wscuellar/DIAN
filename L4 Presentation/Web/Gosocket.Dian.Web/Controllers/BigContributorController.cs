using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class BigContributorController : Controller
    {
        private static readonly TableManager tableManagerGlobalBigContributorRequestAuthorization = new TableManager("GlobalBigContributorRequestAuthorization");
        private static readonly TableManager tableManagerGlobalBigContributorRequestAuthorizationTracking = new TableManager("GlobalBigContributorRequestAuthorizationTracking");


        public ActionResult Index()
        {
            if (User.IsInAnyRole("Administrador", "Super"))
                return RedirectToAction(nameof(ReviewRequests));

            return RedirectToAction(nameof(RequestAuthorization));
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult ProcessAuthorizationRequest(string contributorCode)
        {
            var model = new RequestAuthorizationBigContributorViewModel { ContributorCode = contributorCode };

            GetAuthorizationRequest(ref model);

            ViewBag.CurrentPage = Navigation.NavigationEnum.BigContributors;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult ProcessAuthorizationRequest(RequestAuthorizationBigContributorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (model.StatusCode == (int)Domain.Common.BigContributorAuthorizationStatus.Rejected)
                {
                    var req = tableManagerGlobalBigContributorRequestAuthorization.Find<GlobalBigContributorRequestAuthorization>(model.ContributorCode, model.ContributorCode);
                    model.StatusCode = req.StatusCode;
                    model.Observations = "";
                }

                GetAuthorizationRequestTrackings(ref model);
                return View(model);
            }

            var authorizationRequest = AddOrUpdateAuthorizationRequest(model);
            if (authorizationRequest != null)
                AddOrUpdateAuthorizationRequestTrackings(model);

            return RedirectToAction(nameof(ProcessAuthorizationRequest), new { contributorCode = model.ContributorCode });
        }

        [CustomRoleAuthorization(CustomRoles = "Facturador, Proveedor")]
        public ActionResult RequestAuthorization()
        {
            var contributorCode = User.ContributorCode();
            var contributorName = User.ContributorName();
            var requestAutorization = tableManagerGlobalBigContributorRequestAuthorization.Find<GlobalBigContributorRequestAuthorization>(contributorCode, contributorCode);
            var model = new RequestAuthorizationBigContributorViewModel
            {
                ContributorCode = contributorCode,
                ContributorName = contributorName
            };

            GetAuthorizationRequest(ref model);

            ViewBag.CurrentPage = Navigation.NavigationEnum.BigContributors;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomRoleAuthorization(CustomRoles = "Facturador, Proveedor")]
        public ActionResult RequestAuthorization(RequestAuthorizationBigContributorViewModel model)
        {
            if (model.AuthorizationRequestAlreadyExist)
            {
                // TODO:
                // Check if justification is changed when the request need updated
                var req = tableManagerGlobalBigContributorRequestAuthorization.Find<GlobalBigContributorRequestAuthorization>(model.ContributorCode, model.ContributorCode);
                if (req?.Justification?.Trim()?.ToLower() == model.Justification?.Trim()?.ToLower())
                {
                    model.StatusCode = req.StatusCode;
                    GetAuthorizationRequestTrackings(ref model);
                    ModelState.AddModelError("Justification", "Por favor actualice su justificación.");
                    return View(model);
                }

                if (model.MaximumInvoices < model.MinimumInvoices)
                {
                    model.StatusCode = req.StatusCode;
                    GetAuthorizationRequestTrackings(ref model);
                    ModelState.AddModelError("MinimumInvoices", "La cantidad mínima no puede ser superior a la cantidad máxima por lotes .");
                    return View(model);
                }
            }

            if (model.MaximumInvoices < model.MinimumInvoices)
            {
                GetAuthorizationRequestTrackings(ref model);
                ModelState.AddModelError("MinimumInvoices", "La cantidad mínima no puede ser superior a la cantidad máxima por lotes .");
                return View(model);
            }

            var authorizationRequest = AddOrUpdateAuthorizationRequest(model);
            if (authorizationRequest != null)
            {
                AddOrUpdateAuthorizationRequestTrackings(model);

                model.AuthorizationRequestAlreadyExist = true;
                model.StatusCode = authorizationRequest.StatusCode;
                model.StatusDescription = authorizationRequest.StatusDescription;
                GetAuthorizationRequestTrackings(ref model);
            }

            return RedirectToAction(nameof(RequestAuthorization));
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult ReviewRequests()
        {
            var model = new RequestAuthorizationBigContributorTableViewModel { };

            GetAuthorizationRequests(ref model);

            ViewBag.CurrentPage = Navigation.NavigationEnum.BigContributors;
            return View(model);
        }

        [HttpPost]
        public ActionResult ReviewRequests(RequestAuthorizationBigContributorTableViewModel model)
        {

            GetAuthorizationRequests(ref model);

            ViewBag.CurrentPage = Navigation.NavigationEnum.BigContributors;
            return View(model);
        }

        private GlobalBigContributorRequestAuthorization AddOrUpdateAuthorizationRequest(RequestAuthorizationBigContributorViewModel model)
        {
            var requestAuthorization = new GlobalBigContributorRequestAuthorization(model.ContributorCode)
            {
                RequestDate = model.AuthorizationRequestAlreadyExist ? model.RequestDate : DateTime.UtcNow,
                StatusCode = model.StatusCode,
                ContributorName = model.ContributorName,
                StatusDescription = EnumHelper.GetEnumDescription((Domain.Common.BigContributorAuthorizationStatus)model.StatusCode),
                Justification = model.Justification,
                MinimumInvoices = model.MinimumInvoices,
                MaximumInvoices = model.MaximumInvoices,
                Observations = model.StatusCode == (int)Domain.Common.BigContributorAuthorizationStatus.Authorized ? EnumHelper.GetEnumDescription((Domain.Common.BigContributorAuthorizationStatus)model.StatusCode) : model.Observations,
            };
            if (!tableManagerGlobalBigContributorRequestAuthorization.InsertOrUpdate(requestAuthorization)) return null;
            return requestAuthorization;
        }

        private void AddOrUpdateAuthorizationRequestTrackings(RequestAuthorizationBigContributorViewModel model)
        {
            var user = User.UserName();
            var utcNow = DateTime.UtcNow;
            var action = !model.AuthorizationRequestAlreadyExist ? "Creación" : "Modifica";
            var description = "Se solicita autorización para facturación por lote.";
            var observations = model.Observations;

            if (model.StatusCode == (int)Domain.Common.BigContributorAuthorizationStatus.Authorized)
            {
                action = "Autoriza";
                description = "Se autoriza solicitud para facturación por lote";
                observations = EnumHelper.GetEnumDescription((Domain.Common.BigContributorAuthorizationStatus)model.StatusCode);
            }

            if (model.StatusCode == (int)Domain.Common.BigContributorAuthorizationStatus.Rejected)
            {
                action = "Rechaza";
                description = "Se Rechaza solicitud para facturación por lote";
            }

            var tracking = new GlobalBigContributorRequestAuthorizationTracking(model.ContributorCode)
            {
                User = user,
                Action = action,
                Description = description,
                Observations = observations,
                Date = utcNow,
                RequestUrl = Request.Url.AbsoluteUri
            };
            tableManagerGlobalBigContributorRequestAuthorizationTracking.InsertOrUpdate(tracking);
        }

        private void GetAuthorizationRequest(ref RequestAuthorizationBigContributorViewModel model)
        {
            var requestAutorization = tableManagerGlobalBigContributorRequestAuthorization.Find<GlobalBigContributorRequestAuthorization>(model.ContributorCode, model.ContributorCode);

            if (requestAutorization != null)
            {
                model.ContributorName = !string.IsNullOrEmpty(model.ContributorName) ? model.ContributorName : requestAutorization.ContributorName;
                model.AuthorizationRequestAlreadyExist = true;
                model.RequestDate = requestAutorization.RequestDate;
                model.StatusCode = requestAutorization.StatusCode;
                model.StatusDescription = EnumHelper.GetEnumDescription((Domain.Common.BigContributorAuthorizationStatus)model.StatusCode);
                model.Justification = requestAutorization.Justification;
                model.MinimumInvoices = requestAutorization.MinimumInvoices;
                model.MaximumInvoices = requestAutorization.MaximumInvoices;
                model.Observations = requestAutorization.Observations ?? "";
                GetAuthorizationRequestTrackings(ref model);
            }
        }
        private void GetAuthorizationRequests(ref RequestAuthorizationBigContributorTableViewModel model)
        {
            TableContinuationToken continuationToken = null;
            continuationToken = (TableContinuationToken)Session[$"NextTableContinuationTokenAuthorizationRequests_{model.Page}"];
            if (model.Page == 0)
                continuationToken = null;

            IEnumerable<GlobalBigContributorRequestAuthorization> requests = new List<GlobalBigContributorRequestAuthorization>();

            if (string.IsNullOrEmpty(model.ContributorCode?.Trim()))
            {
                var result = tableManagerGlobalBigContributorRequestAuthorization.GetRangeRows<GlobalBigContributorRequestAuthorization>(model.Length, continuationToken);
                requests = result.Item1;
                Session[$"NextTableContinuationTokenAuthorizationRequests_{model.Page + 1}"] = result.Item2;
            }
            else
            {
                var result = tableManagerGlobalBigContributorRequestAuthorization.GetRangeRows<GlobalBigContributorRequestAuthorization>(model.ContributorCode, model.Length, continuationToken);
                requests = result.Item1;
                Session[$"NextTableContinuationTokenAuthorizationRequests_{model.Page + 1}"] = result.Item2;
            }

            model.Requests = requests.Select(r => new RequestAuthorizationBigContributorViewModel
            {
                ContributorCode = r.ContributorCode,
                ContributorName = r.ContributorName,
                RequestDate = r.RequestDate,
                StatusCode = r.StatusCode,
                StatusDescription = EnumHelper.GetEnumDescription((Domain.Common.BigContributorAuthorizationStatus)r.StatusCode),
                Observations = r.Observations ?? ""
            }).OrderBy(r => r.StatusCode).ToList();
        }

        private void GetAuthorizationRequestTrackings(ref RequestAuthorizationBigContributorViewModel model)
        {
            var trackings = tableManagerGlobalBigContributorRequestAuthorizationTracking.FindByPartition<GlobalBigContributorRequestAuthorizationTracking>(model.ContributorCode);
            model.Trackings = trackings.Select(t => new RequestAuthorizationBigContributorTrackingViewModel
            {
                ContributorCode = t.ContributorCode,
                Date = t.Date,
                User = t.User,
                Action = t.Action,
                Description = t.Description,
                Observations = t.Observations,
            }).OrderBy(t => t.Date).ToList();
        }
    }
}