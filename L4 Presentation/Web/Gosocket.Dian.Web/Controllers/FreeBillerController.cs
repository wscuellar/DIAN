using Gosocket.Dian.Web.Models.FreeBiller;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class FreeBillerController : Controller
    {
        // GET: FreeBiller
        public ActionResult FreeBillerUser()
        {
            UserFiltersFreeBillerModel model = new UserFiltersFreeBillerModel();
            model.DocTypes = this.DataTiposDoc();
            model.Profiles = this.DataPerfiles();
            model.Users = this.Datausuarios(); //new List<UserFreeBillerModel>();
            return View(model);
        }

        [HttpPost]
        public ActionResult FreeBillerUser(UserFiltersFreeBillerModel model)
        {

            var algo = model;
            return RedirectToAction("FreeBillerUser");
        }




        public ActionResult EditFreeBillerUser(UserFreeBillerModel model)
        {
            return View();
        }

        public ActionResult CreateUser()
        {
            UserFreeBillerModel model = new UserFreeBillerModel();
            model.TypesDoc = this.DataTiposDoc();
            model.Profiles = this.DataPerfiles();
            
            return View(model);
        }



        private List<SelectListItem> DataPerfiles()
        {
            return new List<SelectListItem> {
                new SelectListItem{ Value="1", Text= "Administrador (TODOS)" },
                new SelectListItem{ Value="2", Text= "Contador" },
                new SelectListItem{ Value="3", Text= "Facturador" },
                new SelectListItem{ Value="4", Text= "Fiscal" }
            };
        }

        private List<SelectListItem> DataTiposDoc()
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

        private List<UserFreeBillerModel> Datausuarios()
        {
            return new List<UserFreeBillerModel>{
                new UserFreeBillerModel{
                    Id=1,
                    FullName= "Pepito perez",
                    DescriptionTypeDoc = "Cedula de ciudadanía",
                    DescriptionProfile = "Facturador",
                    NumberDoc = "1000223674",
                    //LastUpdate = DateTime.Now,
                    IsActive = true
                },
                new UserFreeBillerModel{
                    Id=2,
                    FullName= "lala lolo",
                    DescriptionTypeDoc = "Cedula de ciudadanía",
                    DescriptionProfile = "Fiscal",
                    NumberDoc = "45722258",
                    LastUpdate = DateTime.Now,
                    IsActive = false
                }
            };
        }
    }
}