using Gosocket.Dian.Application;
using Gosocket.Dian.Web.Common;
using System.Collections.Generic;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {


        ContributorService ContributorService = new ContributorService();
        RadianContributorService radianContributorService = new RadianContributorService();



        // GET: Radian
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ElectronicInvoiceView()
        {
            string userCode = User.UserCode();
            Domain.Contributor contributor = ContributorService.GetByCode(userCode);
            List<Domain.RadianContributor> radianContributor =  radianContributorService.GetRadianContributor(1, 1, t => t.Contributor.Code == userCode);
            ViewBag.WithSoft =contributor != null && contributor.Softwares != null && contributor.Softwares.Count > 1;
            ViewBag.ExistInRadian = radianContributor != null && radianContributor.Count > 0;
            return View();
        }

        public ActionResult AdminRadianView()
        {
            var model = new AdminRadianViewModel();
            return View(model);
        }

    }
}