using Gosocket.Dian.Application;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {

        RadianContributorService radianContributorService = new RadianContributorService();

        // GET: Radian
        public ActionResult Index()
        {
            var res = radianContributorService.GetRadianContributor(1, 3, t => true);
            return View();
        }
    }
}