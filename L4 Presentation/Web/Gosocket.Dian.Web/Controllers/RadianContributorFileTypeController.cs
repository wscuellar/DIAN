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
    public class RadianContributorFileTypeController : Controller
    {
        RadianContributorFileTypeService radianContributorFileTypeService = new RadianContributorFileTypeService();
        RadianContributorService radianContributorService = new RadianContributorService();
        public ActionResult List(string type)
        {
            var model = new RadianContributorFileTypeTableViewModel();
            var fileTypes = radianContributorFileTypeService.GetRadianContributorFileTypes(null, model.Page, model.Length);
            model.RadianContributorFileTypes = fileTypes.Select(ft => new RadianContributorFileTypeViewModel
            {
                Id = ft.Id,
                Name = ft.Name,
                Mandatory = ft.Mandatory,
                Timestamp = ft.Timestamp,
                Updated = ft.Updated
            }).ToList();

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(model);
        }

        [HttpPost]
        public ActionResult List(RadianContributorFileTypeTableViewModel model)
        {

            var fileTypes = new List<RadianContributorFileType>();
            fileTypes = radianContributorFileTypeService.GetRadianContributorFileTypes(model.Name, model.Page, model.Length);


            model.RadianContributorFileTypes = fileTypes.Select(ft => new RadianContributorFileTypeViewModel
            {
                Id = ft.Id,
                Name = ft.Name,
                Mandatory = ft.Mandatory,
                Timestamp = ft.Timestamp,
                Updated = ft.Updated
            }).ToList();

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(model);
        }

        public ActionResult Add()
        {
            var model = new RadianContributorFileTypeViewModel();
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(RadianContributorFileTypeViewModel model)
        {
            var fileType = new RadianContributorFileType
            {
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User.Identity.Name,
                Timestamp = DateTime.Now,
            };
            var radianContributorFileTypeId = radianContributorFileTypeService.AddOrUpdate(fileType);
            /*var providers = radianContributorService.GetContributorsByType((int)Domain.Common.ContributorType.Provider);
            providers.AddRange(radianContributorService.GetContributorsByType((int)Domain.Common.ContributorType.AuthorizedProvider));
            foreach (var item in providers)
            {
                radianContributorService.AddOrUpdateContributorFile(new RadianContributorFile
                {
                    Id = Guid.NewGuid(),
                    FileName = model.Name,
                    Status = 0,
                    Comments = "",
                    Deleted = false,
                    FileType = radianContributorFileTypeId,
                    ContributorId = item.Id,
                    CreatedBy = User.Identity.Name,
                    Timestamp = DateTime.Now,
                    Updated = DateTime.Now
                });
            }*/

            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return RedirectToAction(nameof(View), new { fileType.Id });
        }

        public ActionResult View(int id)
        {
            RadianContributorFileType fileType = radianContributorFileTypeService.Get(id);
            RadianContributorFileTypeViewModel radianContributorFileTypeViewModel = new RadianContributorFileTypeViewModel
            {
                Id = fileType.Id,
                Name = fileType.Name,
                Mandatory = fileType.Mandatory
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(radianContributorFileTypeViewModel);
        }

        public ActionResult Edit(int id)
        {
            RadianContributorFileType fileType = radianContributorFileTypeService.Get(id);
            RadianContributorFileTypeViewModel radianContributorFileTypeViewModel = new RadianContributorFileTypeViewModel
            {
                Id = fileType.Id,
                Name = fileType.Name,
                Mandatory = fileType.Mandatory
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(radianContributorFileTypeViewModel);
        }

        [HttpPost]
        public ActionResult Edit(RadianContributorFileTypeViewModel model)
        {
            var fileType = new RadianContributorFileType
            {
                Id = model.Id,
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User.Identity.Name,
                Updated = DateTime.Now,
            };
            var result = radianContributorFileTypeService.AddOrUpdate(fileType);
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;

            return RedirectToAction(nameof(View), new { fileType.Id });
        }
    }
}