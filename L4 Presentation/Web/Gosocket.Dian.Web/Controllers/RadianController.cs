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
            List<Domain.RadianContributor> radianContributor =  radianContributorService.GetRadianContributor(1, 1, t => t.Contributor.Code == userCode);
            ViewBag.ContributorId = 1;//contributor.Id;
            ViewBag.WithSoft = true; //contributor != null && contributor.Softwares != null && contributor.Softwares.Count > 1;
            ViewBag.ExistInRadian = false; //radianContributor != null && radianContributor.Count > 0;
            return View();
        }

         
        
        
        public ActionResult EnablingInvoiceDirect(int contributorId)
        {
            string userCode = User.UserCode();
            List<Domain.RadianContributor>  radianContributor =  radianContributorService.GetRadianContributor(1, 1, t => t.Contributor.Code == userCode);
            if(!radianContributor.Any())
            {
                Domain.RadianContributor newRadianContributor = new Domain.RadianContributor()
                {
                    ContributorId = contributorId,
                    CreatedBy = User.UserName(),
                    RadianContributorTypeId = 1,
                    RadianOperationModeId = 1,
                    RadianState = "Registrado",
                    CreatedDate = System.DateTime.Now,
                    Update = System.DateTime.Now,
                };
                radianContributorService.AddOrUpdate(newRadianContributor);
                radianContributor = radianContributorService.GetRadianContributor(1, 1, t => t.Contributor.Code == userCode);
            }
            return View();
        }

        public ActionResult AdminRadianView()
        {
            var model = new AdminRadianViewModel();
            return View(model);
        }

    }
}