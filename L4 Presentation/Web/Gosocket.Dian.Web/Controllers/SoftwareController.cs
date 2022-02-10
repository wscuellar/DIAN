using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class SoftwareController : Controller
    {
        ContributorService contributorService = new ContributorService();
        ContributorOperationsService operationService = new ContributorOperationsService();
        SoftwareService softwareService = new SoftwareService();
        private static readonly TableManager tableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");
        private static readonly TableManager tableManagerNumberRangeManager = new TableManager("GlobalNumberRange");
        private static readonly TableManager tableManagerGlobalTestSetResult = new TableManager("GlobalTestSetResult");

        public ActionResult AddNumberRange()
        {

            var contributorId = User.ContributorId();
            var contributor = contributorService.Get(contributorId);
            var operations = operationService.GetContributorOperations(contributorId).Where(o => !o.Deleted);

            var model = new SoftwareTableViewModel { ContributorCode = contributor.Code, ContributorName = contributor.BusinessName };

            model.Softwares = operations.Where(o => o.Software?.AcceptanceStatusSoftwareId == (int)SoftwareStatus.Production).Select(o => new SoftwareViewModel
            {
                Id = o.SoftwareId.Value,
                Name = $"{o.Provider?.BusinessName ?? contributor.BusinessName} - {o.Software?.Name}"
            }).ToList();

            var nowNumberDate = long.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
            var ranges = tableManagerNumberRangeManager.FindByPartition<GlobalNumberRange>(contributor.Code);
            model.Prefixes = ranges.Where(r => r.Active && string.IsNullOrEmpty(r.SoftwareId) && nowNumberDate >= r.ValidDateNumberFrom && r.ValidDateNumberTo >= nowNumberDate && r.RowKey.Contains('|')).Select(r => new NumberRangeViewModel
            {
                RowKey = r.RowKey,
                Serie = $"{r.Serie ?? "Sin prefijo"} - {r.ResolutionNumber} ({r.FromNumber} - {r.ToNumber})"
            }).ToList();

            //TableContinuationToken continuationToken = null;
            //continuationToken = (TableContinuationToken)Session["NextTableContinuationTokenAssociatedRanges_" + model.ContributorCode + "_" + model.Page];
            //if (model.Page == 0)
            //    continuationToken = null;

            //var result = tableManagerNumberRangeManager.GetRangeRows<GlobalNumberRange>(model.ContributorCode, model.Length, continuationToken);
            //Session["NextTableContinuationTokenAssociatedRanges_" + model.ContributorCode + "_" + (model.Page + 1)] = result.Item2;

            model.AssociatedRanges = ranges.Where(r => r.State == (long)NumberRangeState.Authorized && r.Active && !string.IsNullOrEmpty(r.SoftwareId)).Select(r => new NumberRangeViewModel
            {
                AssociationDate = r.AssociationDate.HasValue ? r.AssociationDate.Value.ToString("dd-MM-yyyy") : null,
                ExpirationDate = DateTime.ParseExact(r.ValidDateNumberTo.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy"),
                PartitionKey = r.PartitionKey,
                RowKey = r.RowKey,
                Serie = $"{r.Serie ?? "Sin prefijo"} - {r.ResolutionNumber} ({r.FromNumber} - {r.ToNumber})",
                SoftwareId = r.SoftwareId,
                SoftwareName = r.SoftwareName,
                SoftwareOwner = r.SoftwareOwnerName,
                ValidDateNumberTo = r.ValidDateNumberTo.ToString(),
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddNumberRange(SoftwareTableViewModel model)
        {
            var contributorId = User.ContributorId();
            var contributor = contributorService.Get(contributorId);
            var operations = operationService.GetContributorOperations(contributorId).Where(o => !o.Deleted);

            model.Softwares = operations.Where(o => o.Software?.AcceptanceStatusSoftwareId == (int)SoftwareStatus.Production).Select(o => new SoftwareViewModel
            {
                Id = o.SoftwareId.Value,
                Name = $"{o.Provider?.BusinessName ?? contributor.BusinessName} - {o.Software?.Name}"
            }).ToList();

            var nowNumberDate = long.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
            var ranges = tableManagerNumberRangeManager.FindByPartition<GlobalNumberRange>(contributor.Code);
            model.Prefixes = ranges.Where(r => r.Active && string.IsNullOrEmpty(r.SoftwareId) && nowNumberDate >= r.ValidDateNumberFrom && r.ValidDateNumberTo >= nowNumberDate).Select(r => new NumberRangeViewModel
            {
                RowKey = r.RowKey,
                Serie = r.Serie
            }).ToList();

            //TableContinuationToken continuationToken = null;
            //continuationToken = (TableContinuationToken)Session["NextTableContinuationTokenAssociatedRanges_" + model.ContributorCode + "_" + model.Page];
            //if (model.Page == 0)
            //    continuationToken = null;

            //var result = tableManagerNumberRangeManager.GetRangeRows<GlobalNumberRange>(model.ContributorCode, model.Length, continuationToken);
            //Session["NextTableContinuationTokenAssociatedRanges_" + model.ContributorCode + "_" + (model.Page + 1)] = result.Item2;

            model.AssociatedRanges = ranges.Where(r => r.State == (long)NumberRangeState.Authorized && r.Active && !string.IsNullOrEmpty(r.SoftwareId)).Select(r => new NumberRangeViewModel
            {
                AssociationDate = r.AssociationDate.HasValue ? r.AssociationDate.Value.ToString("dd-MM-yyyy") : null,
                PartitionKey = r.PartitionKey,
                RowKey = r.RowKey,
                Serie = r.Serie,
                SoftwareId = r.SoftwareId,
                SoftwareName = r.SoftwareName,
                SoftwareOwner = r.SoftwareOwnerName
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public JsonResult AddPrefix(string softwareId, string rangeRowKey)
        {
            var contributorCode = User.ContributorCode();
            var software = softwareService.Get(Guid.Parse(softwareId));
            var softwareOwner = contributorService.Get(software.ContributorId);
            var range = tableManagerNumberRangeManager.Find<GlobalNumberRange>(contributorCode, rangeRowKey);
            range.AssociationDate = DateTime.UtcNow;
            range.SoftwareId = softwareId;
            range.SoftwareName = software?.Name;
            range.SoftwareOwnerCode = softwareOwner.Code;
            range.SoftwareOwnerName = softwareOwner.BusinessName;
            tableManagerNumberRangeManager.InsertOrUpdate(range);

            if (tableManagerGlobalAuthorization.Find<GlobalAuthorization>(softwareOwner.Code, contributorCode) == null)
            {
                var auth = new GlobalAuthorization(softwareOwner.Code, contributorCode);
                tableManagerGlobalAuthorization.InsertOrUpdate(auth);
            }

            var json = Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
            return json;
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult List()
        {
            var contributors = contributorService.GetContributors((int)Domain.Common.ContributorType.Provider, (int)ContributorStatus.Enabled);

            List<ContributorViewModel> initialContributor = new List<ContributorViewModel>() { new ContributorViewModel { Id = -1, Name = "Seleccione..." } };
            initialContributor.AddRange(contributors.Select(p => new ContributorViewModel
            {
                Id = p.Id,
                Name = p.Name

            }).ToList());
            var model = new SoftwareTableViewModel
            {
                Contributors = initialContributor
            };
            int? contributorId = null;
            if (User.IsInAnyRole("Administrador", "Super"))
                contributorId = User.ContributorId();
            var softwares = softwareService.GetSoftwares(null, contributorId, model.Page, model.Length);
            model.Softwares = softwares.Where(s => !s.Deleted).Select(s => new SoftwareViewModel
            {
                Deleted = s.Deleted,
                Id = s.Id,
                Name = s.Name,
                Pin = s.Pin,
                Date = s.SoftwareDate,
                SoftwareUser = s.SoftwareUser,
                Timestamp = s.Timestamp,
                Url = s.Url,
                CreatedBy = s.CreatedBy,
                AcceptanceStatusSoftwareId = s.AcceptanceStatusSoftwareId,
                ContributorCode = s.Contributor.Code
            }).ToList();
            ViewBag.CurrentPage = Navigation.NavigationEnum.Software;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult List(SoftwareTableViewModel model)
        {
            var contributors = contributorService.GetContributors((int)Domain.Common.ContributorType.Provider, (int)ContributorStatus.Enabled);

            List<ContributorViewModel> initialContributor = new List<ContributorViewModel>() { new ContributorViewModel { Id = -1, Name = "Seleccione..." } };
            initialContributor.AddRange(contributors.Select(p => new ContributorViewModel
            {
                Id = p.Id,
                Name = p.Name

            }).ToList());

            model.Contributors = initialContributor;
            int? contributorId = null;
            if (!User.IsInAnyRole("Administrador", "Super"))
                contributorId = User.ContributorId();
            else
                contributorId = model.ContributorId == -1 ? null : model.ContributorId;
            var softwares = softwareService.GetSoftwares(model.Pin, string.IsNullOrEmpty(model.Pin) ? contributorId : null, model.Page, model.Length);
            model.Softwares = softwares.Where(s => !s.Deleted).Select(s => new SoftwareViewModel
            {
                Deleted = s.Deleted,
                Id = s.Id,
                Name = s.Name,
                Pin = s.Pin,
                Date = s.SoftwareDate,
                SoftwareUser = s.SoftwareUser,
                Timestamp = s.Timestamp,
                Url = s.Url,
                CreatedBy = s.CreatedBy,
                AcceptanceStatusSoftwareId = s.AcceptanceStatusSoftwareId,
                //StatusName = s.AcceptanceStatusSoftware.Name,
                ContributorCode = s.Contributor.Code
            }).ToList();
            ViewBag.CurrentPage = Navigation.NavigationEnum.Software;
            return View(model);
        }

        [HttpPost]
        public ActionResult AddFromWizard(ContributorViewModel model)
        {
            var software = new Software
            {
                Id = Guid.NewGuid(),
                Name = model.Software.Name,
                Url = model.Software.Url,
                Pin = Guid.NewGuid().ToString(),
                ContributorId = model.Id,
                SoftwareDate = DateTime.UtcNow,
                SoftwarePassword = model.Software.SoftwarePassword,
                SoftwareUser = model.Software.SoftwareUser,
                Deleted = false,
                Status = true,
                AcceptanceStatusSoftwareId = 1,
                Timestamp = DateTime.Now,
                Updated = DateTime.Now,
                CreatedBy = User.Identity.Name
            };
            softwareService.AddOrUpdate(software);

            ViewBag.CurrentPage = Navigation.NavigationEnum.Software;
            return RedirectToAction("Wizard", "Contributor");
        }

        public ActionResult View(Guid id)
        {
            Software software = softwareService.GetAndContributorAndAcceptanceStatus(id);
            var model = new SoftwareViewModel
            {
                Deleted = software.Deleted,
                Id = software.Id,
                Name = software.Name,
                Pin = software.Pin,
                SoftwarePassword = software.SoftwarePassword,
                Date = software.SoftwareDate,
                SoftwareUser = software.SoftwareUser,
                Timestamp = software.Timestamp,
                Url = software.Url,
                CreatedBy = software.CreatedBy,
                AcceptanceStatusSoftwareId = software.AcceptanceStatusSoftwareId,
                ContributorCode = software.Contributor.Code,
                StatusName = software.AcceptanceStatusSoftware.Name,
                Statuses = softwareService.GetAcceptanceStatuses().Select(x => new AcceptanceStatusSoftwareViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList()
            };

            ViewBag.CurrentPage = Navigation.NavigationEnum.Software;
            return View(model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Edit(Guid id)
        {
            Software software = softwareService.GetAndContributorAndAcceptanceStatus(id);
            var model = new SoftwareViewModel
            {
                Deleted = software.Deleted,
                Id = software.Id,
                Name = software.Name,
                Pin = software.Pin,
                SoftwarePassword = software.SoftwarePassword,
                Date = software.SoftwareDate,
                SoftwareUser = software.SoftwareUser,
                Timestamp = software.Timestamp,
                Url = software.Url,
                CreatedBy = software.CreatedBy,
                AcceptanceStatusSoftwareId = software.AcceptanceStatusSoftwareId,
                ContributorCode = software.Contributor.Code,
                StatusName = software.AcceptanceStatusSoftware.Name,
                Statuses = softwareService.GetAcceptanceStatuses().Select(x => new AcceptanceStatusSoftwareViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList()
            };

            ViewBag.CurrentPage = Navigation.NavigationEnum.Software;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Edit(SoftwareViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Contributor contributor = contributorService.GetByCode(model.ContributorCode);
            Software software = new Software
            {
                Deleted = model.Deleted,
                Id = model.Id,
                Name = model.Name,
                Pin = model.Pin,
                SoftwarePassword = model.SoftwarePassword,
                SoftwareDate = model.Date,
                SoftwareUser = model.SoftwareUser,
                Timestamp = model.Timestamp,
                Url = model.Url,
                CreatedBy = model.CreatedBy,
                AcceptanceStatusSoftwareId = model.AcceptanceStatusSoftwareId,
                ContributorId = contributor.Id,
            };

            softwareService.AddOrUpdate(software);

            ViewBag.CurrentPage = Navigation.NavigationEnum.Software;
            return RedirectToAction("View", model);
        }

        [HttpPost]
        public JsonResult RemoveRangeAssociation(string partitionKey, string rowKey)
        {
            var range = tableManagerNumberRangeManager.Find<GlobalNumberRange>(partitionKey, rowKey);
            range.AssociationDate = null;
            range.SoftwareId = null;
            range.SoftwareName = null;
            range.SoftwareOwnerCode = null;
            range.SoftwareOwnerName = null;
            tableManagerNumberRangeManager.InsertOrUpdate(range);
            var json = Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
            return json;
        }

        public JsonResult GetSoftwaresContributor(int contributorId)
        {
            var softwares = softwareService.GetSoftwares(contributorId).Select(x => new { x.Id, x.Name, x.Pin });
            var json = Json(new
            {
                success = softwares != null,
                softwares
            }, JsonRequestBehavior.AllowGet);

            return json;
        }
    }
}