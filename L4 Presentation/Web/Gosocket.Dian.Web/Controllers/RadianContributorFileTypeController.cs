using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces;
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
        private readonly IRadianContributorFileTypeService _RadianContributorFileTypeService;
        private readonly IRadianContributorService _RadianContributorService;

        public RadianContributorFileTypeController(IRadianContributorFileTypeService radianContributorFileTypeService, IRadianContributorService radianContributorService)
        {
            _RadianContributorFileTypeService = radianContributorFileTypeService;
            _RadianContributorService = radianContributorService;
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
                RadianContributorType = ft.RadianContributorType
            }).ToList();
        }

        private RadianContributorFileTypeViewModel GenerateNewRadianContributorFileTypeViewModel()
        {
            var newModel = new RadianContributorFileTypeViewModel();
            newModel.RadianContributorTypes = new SelectList(_RadianContributorService.GetRadianContributorTypes(rct => rct.Id == rct.Id), "Id", "Name");
            newModel.SelectedRadianContributorTypeId = _RadianContributorService.GetRadianContributorTypes(rct => rct.Id == rct.Id).First().Id.ToString();
            return newModel;
        }

        public ActionResult List()
        {
            var model = new RadianContributorFileTypeTableViewModel();
            var fileTypes = _RadianContributorFileTypeService.GetRadianContributorFileTypes(model.Page, model.Length, (ft => ft.Id == ft.Id && ft.Deleted == false));
            model.RadianContributorFileTypes = RadianContributorFileTypeToViewModel(fileTypes);
            model.RadianContributorTypes = new SelectList(_RadianContributorService.GetRadianContributorTypes(rct => rct.Id == rct.Id), "Id", "Name");
            model.SearchFinished = true;
            model.RadianContributorFileTypeViewModel = GenerateNewRadianContributorFileTypeViewModel();
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(model);
        }

        [HttpPost]
        public ActionResult List(RadianContributorFileTypeTableViewModel model)
        {

            List<RadianContributorFileType> fileTypes;
            int selectedType = (model.SelectedRadianContributorTypeId == null) ? 0 : int.Parse(model.SelectedRadianContributorTypeId);
            fileTypes = _RadianContributorFileTypeService.GetRadianContributorFileTypes(model.Page, model.Length, ft => ft.Id == ft.Id && ((model.Name == null) || ft.Name.Contains(model.Name)) && ((model.SelectedRadianContributorTypeId == null) || ft.RadianContributorTypeId == selectedType) && !ft.Deleted);
            model.RadianContributorFileTypes = RadianContributorFileTypeToViewModel(fileTypes);
            model.RadianContributorTypes = new SelectList(_RadianContributorService.GetRadianContributorTypes(rct => rct.Id == rct.Id), "Id", "Name");
            model.SearchFinished = true;
            model.RadianContributorFileTypeViewModel = GenerateNewRadianContributorFileTypeViewModel();
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
                RadianContributorTypeId = int.Parse(model.SelectedRadianContributorTypeId),
            };
            _ = _RadianContributorFileTypeService.AddOrUpdate(fileType);
            ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
            return RedirectToAction("List");
        }

        public PartialViewResult GetEditRadianContributorFileTypePartialView(int id)
        {
            RadianContributorFileType fileType = _RadianContributorFileTypeService.Get(id);
            RadianContributorFileTypeViewModel radianContributorFileTypeViewModel = new RadianContributorFileTypeViewModel
            {
                Id = fileType.Id,
                Name = fileType.Name,
                Mandatory = fileType.Mandatory,
                RadianContributorType = fileType.RadianContributorType,
                SelectedRadianContributorTypeId = fileType.RadianContributorType.Id.ToString(),
                RadianContributorTypes = new SelectList(_RadianContributorService.GetRadianContributorTypes(rct => rct.Id == rct.Id), "Id", "Name"),
            };
            Response.Headers["InjectingPartialView"] = "true";
            return PartialView("~/Views/RadianContributorFileType/_Edit.cshtml", radianContributorFileTypeViewModel);
        }

        public ActionResult Edit(int id)
        {
            RadianContributorFileType fileType = _RadianContributorFileTypeService.Get(id);
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
                RadianContributorTypeId = int.Parse(model.SelectedRadianContributorTypeId),
                RadianContributorType = model.RadianContributorType
            };
            _ = _RadianContributorFileTypeService.AddOrUpdate(fileType);
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;

            return RedirectToAction("List");
        }

        public PartialViewResult GetDeleteRadianContributorFileTypePartialView(int id)
        {
            RadianContributorFileType fileType = _RadianContributorFileTypeService.Get(id);
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
            var fileType = new RadianContributorFileType
            {
                Id = model.Id,
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User.Identity.Name,
                Updated = DateTime.Now,
                Deleted = true,
            };
            if (_RadianContributorFileTypeService.IsAbleForDelete(fileType))
            {
                _ = _RadianContributorFileTypeService.Delete(fileType);
            }
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return RedirectToAction("List");
        }

    }
}