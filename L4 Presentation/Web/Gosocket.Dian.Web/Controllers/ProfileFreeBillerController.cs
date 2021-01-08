using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using Gosocket.Dian.Application.FreeBiller;
using Gosocket.Dian.Domain.Sql.FreeBiller;
using Gosocket.Dian.Web.Models.FreeBiller;

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
            this.GetMenuOption();
            model.MenuOptionsByProfile = this.staticMenuOptions;
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateProfile(ProfileFreeBillerModel model)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var item in allErrors)
                    ModelState.AddModelError("", item.ErrorMessage);

                return View(model);
            }
            
            Profile newProfile = profileService.CreateNewProfile(
                new Profile
                {
                    Name = model.Name,
                    IsEditable = true
                });

            List<string> verificationMenuIds = this.VerificationFatherIds(model.ValuesSelected);

            List<MenuOptionsByProfiles> menuOptions = this.GenerateMenuOptionsForInsert(newProfile.Id, verificationMenuIds);

            profileService.SaveOptionsMenuByProfile(menuOptions);

            return RedirectToAction("FreeBillerUser", "FreeBiller");
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

        private void GetMenuOption()
        {
            var options = profileService.GetMenuOptions();

            this.staticMenuOptions = this.staticMenuOptions ?? new List<MenuOptionsModel>();
            if (options != null)
            {
                foreach (var item in options)
                {
                    this.staticMenuOptions.Add(
                        new MenuOptionsModel
                        {
                            MenuId = item.Id,
                            Name = item.Name,
                            FatherId = item.ParentId,
                            Level = item.MenuLevel
                        });
                }
            }
        }

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