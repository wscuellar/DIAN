using Gosocket.Dian.Application;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Gosocket.Dian.Web.Filters;

namespace Gosocket.Dian.Web.Controllers
{
    [Authorization]
    public class DashboardController : Controller
    {
        [Authorization(Roles = "Administrador")]
        public ActionResult Admin()
        {
            return View();
        }

        [Authorization(Roles = "Proveedor")]
        public ActionResult Provider()
        {
            return View();
        }

        [Authorization(Roles = "Facturador")]
        public ActionResult Client()
        {
            return View();
        }
    }
}