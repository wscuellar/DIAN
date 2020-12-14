using Gosocket.Dian.Web.Models.FreeBiller;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class FreeBillerController : Controller
    {
        // GET: FreeBiller
        public ActionResult FreeBillerUser()
        {
            UserFreeBillerModel model = new UserFreeBillerModel();
            model.TiposDoc = this.DataTiposDoc();
            model.Perfiles = this.DataPerfiles();
            return View(model);
        }


            return View();
        }
        public ActionResult EditFreeBillerUser(UserFreeBillerModel model)
        {
            return View();
        }
        public ActionResult CreateUser()
        {
            UserFreeBillerModel model = new UserFreeBillerModel();
            model.TiposDoc = this.DataTiposDoc();
            model.Perfiles = this.DataPerfiles();

            return View(model);

        }



        private List<SelectListItem> DataTiposDoc()
        {
            return new List<SelectListItem> {
                new SelectListItem{ Value="1", Text= "Administrador (TODOS)" },
                new SelectListItem{ Value="2", Text= "Contador" },
                new SelectListItem{ Value="3", Text= "Facturador" },
                new SelectListItem{ Value="4", Text= "Fiscal" }
            };
        }

        private List<SelectListItem> DataPerfiles()
        {
            return new List<SelectListItem> {
                new SelectListItem{ Value="11", Text="Registro civil" },
                new SelectListItem{ Value="12", Text="Tarjeta de identidad" },
                new SelectListItem{ Value="13", Text="Cedula de ciudadanía" },
                new SelectListItem{ Value="21", Text="Tarjeta de extranjería" },
                new SelectListItem{ Value="22", Text="Cedula de extranjería " },
                new SelectListItem{ Value="31", Text="Nit" },
                new SelectListItem{ Value="41", Text="Pasaporte" },
                new SelectListItem{ Value="42", Text="Documento de identificación de extranjero" },
                new SelectListItem{ Value="47", Text="PEP" },
                new SelectListItem{ Value="50", Text="Nit de otro país " },
                new SelectListItem{ Value="91", Text="NIUP" }
            };
        }
    }
}