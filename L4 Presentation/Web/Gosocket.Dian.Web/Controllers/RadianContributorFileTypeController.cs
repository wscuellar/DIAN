using Gosocket.Dian.Domain;
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
    [IPFilter]
    [Authorization]
    [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
    public class RadianContributorFileTypeController : Controller
    {
        private readonly IRadianContributorFileTypeService _radianContributorFileTypeService;

        public RadianContributorFileTypeController(IRadianContributorFileTypeService radianContributorFileTypeService)
        {
            _radianContributorFileTypeService = radianContributorFileTypeService;
        }


        private List<RadianContributorFileTypeViewModel> RadianContributorFileTypeToViewModel(List<RadianContributorFileType> fileTypes)
        {
            return fileTypes.Select(ft => new RadianContributorFileTypeViewModel
            {
                Id = ft.Id,
                Name = ft.Name,
                Mandatory = ft.Mandatory,
                Timestamp = ft.Timestamp,
                Updated = ft.Updated,
                RadianContributorType = ft.RadianContributorType,
                HideDelete = ft.HideDelete

            }).ToList();
        }

        private RadianContributorFileTypeViewModel GenerateNewRadianContributorFileTypeViewModel(List<RadianContributorType> lst)
        {
            RadianContributorFileTypeViewModel newModel = new RadianContributorFileTypeViewModel();
            newModel.RadianContributorTypes = new SelectList(lst, "Id", "Name");
            newModel.SelectedRadianContributorTypeId = lst.First().Id.ToString();
            return newModel;
        }

        public ActionResult List()
        {
            RadianContributorFileTypeTableViewModel model = new RadianContributorFileTypeTableViewModel();
            List<RadianContributorFileType> fileTypes = _radianContributorFileTypeService.FileTypeList();
            List<RadianContributorType> fileTypeList = _radianContributorFileTypeService.ContributorTypeList();

            model.RadianContributorFileTypes = RadianContributorFileTypeToViewModel(fileTypes);
            model.RadianContributorTypes = new SelectList(fileTypeList, "Id", "Name");
            model.SearchFinished = true;
            model.RadianContributorFileTypeViewModel = GenerateNewRadianContributorFileTypeViewModel(fileTypeList);

            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(model);
        }

        [HttpPost]
        public ActionResult List(RadianContributorFileTypeTableViewModel model)
        {
            List<RadianContributorFileType> fileTypesByContributorType = _radianContributorFileTypeService.Filter(model.Name, model.SelectedRadianContributorTypeId);
            List<RadianContributorType> fileTypeList = _radianContributorFileTypeService.ContributorTypeList();

            model.RadianContributorFileTypes = RadianContributorFileTypeToViewModel(fileTypesByContributorType);
            model.RadianContributorTypes = new SelectList(fileTypeList, "Id", "Name");
            model.SearchFinished = true;
            model.RadianContributorFileTypeViewModel = GenerateNewRadianContributorFileTypeViewModel(fileTypeList);

            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(RadianContributorFileTypeViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("List", model);

            RadianContributorFileType fileType = new RadianContributorFileType
            {
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User != null ? User.Identity.Name : "",
                Timestamp = DateTime.Now,
                RadianContributorTypeId = int.Parse(model.SelectedRadianContributorTypeId),
            };
            _ = _radianContributorFileTypeService.Update(fileType);

            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return RedirectToAction("List");
        }

        public PartialViewResult GetEditRadianContributorFileTypePartialView(int id)
        {
            RadianContributorFileType fileType = _radianContributorFileTypeService.Get(id);
            RadianContributorFileTypeViewModel radianContributorFileTypeViewModel = new RadianContributorFileTypeViewModel
            {
                Id = fileType.Id,
                Name = fileType.Name,
                Mandatory = fileType.Mandatory,
                RadianContributorType = fileType.RadianContributorType,
                SelectedRadianContributorTypeId = fileType.RadianContributorType.Id.ToString(),
                RadianContributorTypes = new SelectList(_radianContributorFileTypeService.ContributorTypeList(), "Id", "Name"),
            };
            Response.Headers["InjectingPartialView"] = "true";
            return PartialView("~/Views/RadianContributorFileType/_Edit.cshtml", radianContributorFileTypeViewModel);
        }

        public ActionResult Edit(int id)
        {
            RadianContributorFileType fileType = _radianContributorFileTypeService.Get(id);
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
            if (!ModelState.IsValid)
                return RedirectToAction("List", model);

            RadianContributorFileType fileType = new RadianContributorFileType
            {
                Id = model.Id,
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User != null ? User.Identity.Name : "",
                Updated = DateTime.Now,
                RadianContributorTypeId = int.Parse(model.SelectedRadianContributorTypeId),
                RadianContributorType = model.RadianContributorType
            };
            _ = _radianContributorFileTypeService.Update(fileType);

            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return RedirectToAction("List");
        }

        public PartialViewResult GetDeleteRadianContributorFileTypePartialView(int id)
        {
            RadianContributorFileType fileType = _radianContributorFileTypeService.Get(id);
            RadianContributorFileTypeViewModel radianContributorFileTypeViewModel = new RadianContributorFileTypeViewModel
            {
                Id = fileType.Id,
                Name = fileType.Name,
                Mandatory = fileType.Mandatory,
                RadianContributorType = fileType.RadianContributorType,
                SelectedRadianContributorTypeId = fileType.RadianContributorType.Id.ToString()
            };
            Response.Headers["InjectingPartialView"] = "true";
            return PartialView("~/Views/RadianContributorFileType/_Delete.cshtml", radianContributorFileTypeViewModel);
        }

        [HttpPost]
        public ActionResult Delete(RadianContributorFileTypeViewModel model)
        {
            RadianContributorFileType fileType = new RadianContributorFileType
            {
                Id = model.Id,
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User != null ? User.Identity.Name : "",
                Updated = DateTime.Now,
                Deleted = true,
            };
            if (_radianContributorFileTypeService.IsAbleForDelete(fileType))
            {
                _ = _radianContributorFileTypeService.Delete(fileType);
            }
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return RedirectToAction("List");
        }

    }
}