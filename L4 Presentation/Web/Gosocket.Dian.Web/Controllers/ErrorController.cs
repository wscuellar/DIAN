using Gosocket.Dian.Web.Filters;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [ActionName("400")]
        public ActionResult Error400()
        {
            return View();
        }

        [ActionName("Unauthorized")]
        public ActionResult Error401()
        {
            return View();
        }

        [ActionName("404")]
        public ActionResult Error404()
        {
            return View();
        }
    }
}