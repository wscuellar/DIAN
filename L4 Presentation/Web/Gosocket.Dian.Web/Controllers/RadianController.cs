using Gosocket.Dian.Web.Common;
using System.Collections.Generic;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;
using System.Linq;
using Gosocket.Dian.Interfaces;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {

        private readonly IContributorService _ContributorService;
        private readonly IRadianContributorService _RadianContributorService;
                
        public RadianController(IContributorService contributorService, IRadianContributorService radianContributorService)
        {

            _ContributorService = contributorService;
            _RadianContributorService = radianContributorService;
        }


        private void SetContributorInfo()
        {
            string userCode = User.UserCode();
            Domain.Contributor contributor = _ContributorService.GetByCode(userCode);
            if (contributor != null)
            {
                ViewBag.ContributorId = contributor.Id;
                ViewBag.ContributorTypeId = contributor.ContributorTypeId;
                ViewBag.Active = contributor.Status;
                ViewBag.WithSoft = contributor.Softwares?.Count > 0;
                
                List<Domain.RadianContributor> radianContributor = _RadianContributorService.Get(t => t.ContributorId == contributor.Id && t.RadianState != "Cancelado");
                string rcontributorTypes =  radianContributor?.Aggregate("", (current, next) => current + ", " + next.RadianContributorTypeId.ToString());               
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
            var radianContributors = _RadianContributorService.Get(t => true );
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