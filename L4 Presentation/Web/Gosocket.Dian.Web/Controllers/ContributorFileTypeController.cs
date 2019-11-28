using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
    public class ContributorFileTypeController : Controller
    {
        ContributorFileTypeService contributorFileTypeService = new ContributorFileTypeService();
        ContributorService contributorService = new ContributorService();
        public ActionResult List(string type)
        {
            var model = new ContributorFileTypeTableViewModel();
            var fileTypes = contributorFileTypeService.GetContributorFileTypes(null, model.Page, model.Length);
            model.ContributorFileTypes = fileTypes.Select(ft => new ContributorFileTypeViewModel
            {
                Id = ft.Id,
                Name = ft.Name,
                Mandatory = ft.Mandatory,
                Timestamp = ft.Timestamp,
                Updated = ft.Updated
            }).ToList();

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return View(model);
        }

        [HttpPost]
        public ActionResult List(ContributorFileTypeTableViewModel model)
        {

            var fileTypes = new List<ContributorFileType>();
            fileTypes = contributorFileTypeService.GetContributorFileTypes(model.Name, model.Page, model.Length);


            model.ContributorFileTypes = fileTypes.Select(ft => new ContributorFileTypeViewModel
            {
                Id = ft.Id,
                Name = ft.Name,
                Mandatory = ft.Mandatory,
                Timestamp = ft.Timestamp,
                Updated = ft.Updated
            }).ToList();

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return View(model);
        }

        public ActionResult Add()
        {
            var model = new ContributorFileTypeViewModel();
            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(ContributorFileTypeViewModel model)
        {
            var fileType = new ContributorFileType
            {
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User.Identity.Name,
                Timestamp = DateTime.Now,
            };
            var contributorFileTypeId = contributorFileTypeService.AddOrUpdate(fileType);
            var providers = contributorService.GetContributorsByType((int)Domain.Common.ContributorType.Provider);
            providers.AddRange(contributorService.GetContributorsByType((int)Domain.Common.ContributorType.AuthorizedProvider));
            foreach (var item in providers)
            {
                contributorService.AddOrUpdateContributorFile(new ContributorFile
                {
                    Id = Guid.NewGuid(),
                    FileName = model.Name,
                    Status = 0,
                    Comments = "",
                    Deleted = false,
                    FileType = contributorFileTypeId,
                    ContributorId = item.Id,
                    CreatedBy = User.Identity.Name,
                    Timestamp = DateTime.Now,
                    Updated = DateTime.Now
                });
            }

            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return RedirectToAction(nameof(View), new { fileType.Id });
        }

        public ActionResult View(int id)
        {
            ContributorFileType fileType = contributorFileTypeService.Get(id);
            ContributorFileTypeViewModel contributorFileTypeViewModel = new ContributorFileTypeViewModel
            {
                Id = fileType.Id,
                Name = fileType.Name,
                Mandatory = fileType.Mandatory
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return View(contributorFileTypeViewModel);
        }

        public ActionResult Edit(int id)
        {
            ContributorFileType fileType = contributorFileTypeService.Get(id);
            ContributorFileTypeViewModel contributorFileTypeViewModel = new ContributorFileTypeViewModel
            {
                Id = fileType.Id,
                Name = fileType.Name,
                Mandatory = fileType.Mandatory
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return View(contributorFileTypeViewModel);
        }

        [HttpPost]
        public ActionResult Edit(ContributorFileTypeViewModel model)
        {
            var fileType = new ContributorFileType
            {
                Id = model.Id,
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User.Identity.Name,
                Updated = DateTime.Now,
            };
            var result = contributorFileTypeService.AddOrUpdate(fileType);
            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;

            return RedirectToAction(nameof(View), new { fileType.Id });
        }
    }
}