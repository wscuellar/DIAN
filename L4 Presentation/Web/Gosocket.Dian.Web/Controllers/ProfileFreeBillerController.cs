using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using Gosocket.Dian.Application.FreeBiller;

namespace Gosocket.Dian.Web.Controllers
{
    public class ProfileFreeBillerController : Controller
    {

        /// <summary>
        /// Servicio para obtener los perfiles de Facturador gratuito.
        /// Tabla: ProfilesFreeBiller.
        /// </summary>
        private readonly ProfileService profileService = new ProfileService();

        private string[,] staticMenuOptions { get; set; }

        // GET: ProfileFreeBiller
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult CreateProfile() 
        {

            return View();
        }


        private void GetMenuOption() {

            var options = profileService.GetMenuOptions();
            this.staticMenuOptions = new string[,] { };
        }

    }
}