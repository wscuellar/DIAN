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
        private readonly IRadianContributorService _radianContributorService;

        public RadianContributorFileTypeController(IRadianContributorFileTypeService radianContributorFileTypeService, IRadianContributorService radianContributorService)
        {
            _radianContributorFileTypeService = radianContributorFileTypeService;
            _radianContributorService = radianContributorService;
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

        private RadianContributorFileTypeViewModel GenerateNewRadianContributorFileTypeViewModel()
        {
            var newModel = new RadianContributorFileTypeViewModel();
            newModel.RadianContributorTypes = new SelectList(_radianContributorFileTypeService.ContributorTypeList(), "Id", "Name");
            newModel.SelectedRadianContributorTypeId = _radianContributorFileTypeService.ContributorTypeList().First().Id.ToString();
            return newModel;
        }

        public ActionResult List()
        {
            var model = new RadianContributorFileTypeTableViewModel();
            var fileTypes = _radianContributorFileTypeService.FileTypeList();

            model.RadianContributorFileTypes = RadianContributorFileTypeToViewModel(fileTypes);
            model.RadianContributorTypes = new SelectList(_radianContributorFileTypeService.ContributorTypeList(), "Id", "Name");

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

            fileTypes = _radianContributorFileTypeService.Filter(model.Name, model.SelectedRadianContributorTypeId);

            model.RadianContributorFileTypes = RadianContributorFileTypeToViewModel(fileTypes);
            model.RadianContributorTypes = new SelectList(_radianContributorFileTypeService.ContributorTypeList(), "Id", "Name");
            model.SearchFinished = true;
            model.RadianContributorFileTypeViewModel = GenerateNewRadianContributorFileTypeViewModel();
            ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(RadianContributorFileTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var fileType = new RadianContributorFileType
                {
                    Mandatory = model.Mandatory,
                    Name = model.Name,
                    CreatedBy = User.Identity.Name,
                    Timestamp = DateTime.Now,
                    RadianContributorTypeId = int.Parse(model.SelectedRadianContributorTypeId),
                };

                _ = _radianContributorFileTypeService.Update(fileType);

                ViewBag.CurrentPage = Navigation.NavigationEnum.ContributorFileType;
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("List", model);
            }

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
            if (ModelState.IsValid)
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
                _ = _radianContributorFileTypeService.Update(fileType);
                ViewBag.CurrentPage = Navigation.NavigationEnum.RadianContributorFileType;
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("List", model);
            }
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
            var fileType = new RadianContributorFileType
            {
                Id = model.Id,
                Mandatory = model.Mandatory,
                Name = model.Name,
                CreatedBy = User.Identity.Name,
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