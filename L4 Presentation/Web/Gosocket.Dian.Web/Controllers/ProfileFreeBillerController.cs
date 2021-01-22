using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

using System.Web.Mvc;
using Gosocket.Dian.Application.FreeBiller;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql.FreeBiller;
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
        private readonly ProfileService profileService = new ProfileService();

        private List<MenuOptionsModel> staticMenuOptions { get; set; }

        // GET: ProfileFreeBiller
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult CreateProfile()
        {
            ProfileFreeBillerModel model = new ProfileFreeBillerModel();
            model.MenuOptionsByProfile = profileService.GetOptionsByProfile(0);
            string output = JsonConvert.SerializeObject(model.MenuOptionsByProfile);
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

            Profile newProfile = profileService.CreateNewProfile(
                new Profile
                {
                    Name = model.Name,
                    IsEditable = true
                });

            List<string> verificationMenuIds = this.VerificationFatherIds(model.ValuesSelected);
            List<MenuOptionsByProfiles> menuOptions = this.GenerateMenuOptionsForInsert(newProfile.Id, verificationMenuIds);
            bool changes = profileService.SaveOptionsMenuByProfile(menuOptions);
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