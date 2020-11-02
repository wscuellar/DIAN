using Gosocket.Dian.Application;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;
using System.Linq;


namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {

        RadianContributorService radianContributorService = new RadianContributorService();

        // GET: Radian
        public ActionResult Index()
        {
                //radianContributorService.AddOrUpdate(new Domain.RadianContributor() { ContributorId = 4486972, RadianState="Camilo", RadianContributorTypeId = 1, RadianOperationModeId = 1, Update = System.DateTime.Now, CreatedDate = System.DateTime.Now, CreatedBy = "Camilo" });
            //var res = radianContributorService.GetRadianContributor(1, 3, t => t.RadianState == "Activo" ||  t.RadianState == "Camilo" );
            ////VALUES(1, 4486972, 1, 1, 'ACTIVO', GETDATE(), GETDATE(), 'TEST')

            //radianContributorService.RemoveRadianContributor(new Domain.RadianContributor() { Id = 2 });
                return View();
        }

        public ActionResult ElectronicInvoiceView()
        {
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