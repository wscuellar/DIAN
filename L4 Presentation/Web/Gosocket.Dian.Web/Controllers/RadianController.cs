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



        // GET: Radian
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ElectronicInvoiceView()
        {
            string userCode = User.UserCode();
            Domain.Contributor contributor = ContributorService.GetByCode(userCode);
            List<Domain.RadianContributor> radianContributor =  radianContributorService.GetRadianContributor(t => t.Contributor.Code == userCode);
            ViewBag.ContributorId = contributor.Id;
            ViewBag.WithSoft = contributor != null && contributor.Softwares != null && contributor.Softwares.Count > 1;
            ViewBag.ExistInRadian = radianContributor != null && radianContributor.Count > 0 && radianContributor[0].RadianState !=  Domain.Common.EnumHelper.GetDescription( Domain.Common.RadianState.Cancel);
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