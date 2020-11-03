using Gosocket.Dian.Application;
using Gosocket.Dian.Web.Common;
using System.Collections.Generic;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;
using System.Linq;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {


        ContributorService ContributorService = new ContributorService();
        RadianContributorService radianContributorService = new RadianContributorService();


        private void SetContributorInfo()
        {
            string userCode = User.UserCode();
            Domain.Contributor contributor = ContributorService.GetByCode(userCode);
            if (contributor != null)
            {

                List<Domain.RadianContributor> radianContributor = radianContributorService.GetRadianContributor(t => t.ContributorId == contributor.Id && t.RadianState != "Cancelado");
                string rcontributorTypes = radianContributor.Aggregate("", (current, next) => current + ", " + next.RadianContributorTypeId.ToString());
                ViewBag.ContributorId = contributor.Id;
                ViewBag.ContributorTypeId = contributor.ContributorTypeId;
                ViewBag.Active = contributor != null && contributor.Status;
                ViewBag.WithSoft = contributor != null && contributor.Softwares != null && contributor.Softwares.Count > 0;
                ViewBag.ExistInRadian = rcontributorTypes;

            }
        }


        // GET: Radian
        public ActionResult Index()
        {
            SetContributorInfo();
            return View();
        }

        public ActionResult ElectronicInvoiceView()
        {
            SetContributorInfo();
            return View();
        }

        public ActionResult AdminRadianView()
        {
            var model = new AdminRadianViewModel();
            return View(model);
        }

    }
}