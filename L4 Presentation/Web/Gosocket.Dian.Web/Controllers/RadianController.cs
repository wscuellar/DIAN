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
            var radianContributors = radianContributorService.GetRadianContributor(1, 3, t => true );
            var model = new AdminRadianViewModel();
            model.RadianContributors = radianContributors.Select(c => new RadianContributorsViewModel
            {
                Id = c.Contributor.Id,
                Code = c.Contributor.Code,
                TradeName = c.Contributor.Name,
                BusinessName = c.Contributor.BusinessName,
                AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name

            }).ToList();

            model.RadianType = radianContributors.Select(c => new SelectListItem
            {
                Value = c.Contributor.Code,
                Text = c.Contributor.Name

            }).ToList();

            model.SearchFinished = true;
            return View(model);
        }

    }
}