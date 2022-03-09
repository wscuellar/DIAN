using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

using System.Web.Mvc;
using Gosocket.Dian.Application;
using Gosocket.Dian.Application.FreeBiller;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql.FreeBiller;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models.FreeBiller;
using Newtonsoft.Json;

namespace Gosocket.Dian.Web.Controllers
{
    public class ProfileFreeBillerController : Controller
    {

        /// <summary>
        /// Servicio para obtener los perfiles de Facturador gratuito.
        /// Tabla: ProfilesFreeBiller.
        /// </summary>
        private readonly IProfileService _profileService;
        private ContributorService contributorService = new ContributorService();

        public ProfileFreeBillerController(IProfileService profileService) {
            _profileService = profileService;
        }

        private List<MenuOptionsModel> staticMenuOptions { get; set; }

        // GET: ProfileFreeBiller
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult CreateProfile()
        {
                ProfileFreeBillerModel model = new ProfileFreeBillerModel();
                model.MenuOptionsByProfile = _profileService.GetOptionsByProfile(0);
                string output = JsonConvert.SerializeObject(model.MenuOptionsByProfile);
                ViewBag.ContributorId = User.ContributorId();
                var electronicDocument = contributorService.GetOtherDocElecContributorPermisos(User.ContributorId());
                var contributorOp = contributorService.GetContributorOperations(User.ContributorId());
                ViewBag.OperationMode = contributorOp.OperationModeId;
                ViewBag.configurationManager = ConfigurationManager.GetValue("Environment");
                foreach (var elemento in electronicDocument)
                    if (elemento.OtherDocElecOperationModeId == 3 && elemento.Step == 3)
                    {
                        ViewBag.DocElectronicDocument = elemento.ElectronicDocumentId;
                    }

                return View(model);
        }

        [HttpPost]
        public JsonResult CreateProfile(ProfileFreeBillerModel model)
        {
            //Valida si el modelo trae errores
            StringBuilder errors = new StringBuilder();
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var item in allErrors)
                    errors.AppendLine(item.ErrorMessage);
                return Json(new ResponseMessage(errors.ToString(), TextResources.alertType, (int)HttpStatusCode.BadRequest), JsonRequestBehavior.AllowGet);
            }

            Profile newProfile = _profileService.CreateNewProfile(
                new Profile
                {
                    Name = model.Name,
                    IsEditable = true
                });

            List<string> verificationMenuIds = this.VerificationFatherIds(model.ValuesSelected);
            List<MenuOptionsByProfiles> menuOptions = this.GenerateMenuOptionsForInsert(newProfile.Id, verificationMenuIds);
            bool changes = _profileService.SaveOptionsMenuByProfile(menuOptions);
            ResponseMessage response = new ResponseMessage();
            if (changes)
            {
                response.Message = "El perfil fue creado exitosamente";
                response.MessageType = "confirmation";
                response.Code = 200;
            }
            else
            {
                response.Message = "El perfil no fue creado!";
                response.MessageType = "alert";
                response.Code = 200;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private List<MenuOptionsByProfiles> GenerateMenuOptionsForInsert(int id, List<string> verificationMenuIds)
        {
            List<MenuOptionsByProfiles> menuOptions = new List<MenuOptionsByProfiles>();

            foreach (string menuOption in verificationMenuIds)
            {
                menuOptions.Add(
                    new MenuOptionsByProfiles
                    {
                        ProfileId = id,
                        MenuOptionId = Convert.ToInt32(menuOption)
                    });
            }

            return menuOptions;
        }

        //private void GetMenuOption()
        //{
        //    var options = profileService.GetMenuOptions();

        //    this.staticMenuOptions = this.staticMenuOptions ?? new List<MenuOptionsModel>();
        //    if (options != null)
        //    {
        //        foreach (var item in options)
        //        {
        //            this.staticMenuOptions.Add(
        //                new MenuOptionsModel
        //                {
        //                    MenuId = item.Id,
        //                    Name = item.Name,
        //                    FatherId = item.ParentId,
        //                    Level = item.MenuLevel
        //                });
        //        }
        //    }
        //}

        private List<string> VerificationFatherIds(string[] valuesSelected)
        {
            List<string> local = new List<string>();

            foreach (string item in valuesSelected)
            {
                string[] allFatherIds = item.Split(',');

                foreach (string innerItem in allFatherIds)
                {
                    if (!string.IsNullOrEmpty(innerItem))
                    {
                        if (!local.Any(l => l == innerItem))
                        {
                            local.Add(innerItem);
                        }
                    }
                }
            }

            return local;
        }

    }
}