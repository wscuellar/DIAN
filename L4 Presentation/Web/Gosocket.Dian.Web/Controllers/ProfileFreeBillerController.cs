using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using Gosocket.Dian.Application.FreeBiller;
using Gosocket.Dian.Web.Models.FreeBiller;

namespace Gosocket.Dian.Web.Controllers
{
    public class ProfileFreeBillerController : Controller
    {


        private const int LevelOne = 1;

        private const int LevelTwo = 2;
        
        private const int LevelThree = 3;



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

    }
}