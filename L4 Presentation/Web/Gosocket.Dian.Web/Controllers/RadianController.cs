using Gosocket.Dian.Application;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {

        RadianContributorService radianContributorService = new RadianContributorService();

        // GET: Radian
        public ActionResult Index()
        {
            //radianContributorService.AddOrUpdate(new Domain.RadianContributor() { ContributorId = 4486972, RadianState="Camilo", ContributorTypeId = 1, OperationModeId = 1, Update = System.DateTime.Now, CreatedDate = System.DateTime.Now, CreatedBy = "Camilo" });
            //var res = radianContributorService.GetRadianContributor(1, 3, t => t.RadianState == "Activo" ||  t.RadianState == "Camilo" );
            ////VALUES(1, 4486972, 1, 1, 'ACTIVO', GETDATE(), GETDATE(), 'TEST')

            //radianContributorService.RemoveRadianContributor(new Domain.RadianContributor() { Id = 2 });
            return View();
        }

        
        public ActionResult ElectronicInvoiceView()
        {
            return View();
            //return PartialView("ElectronicInvoiceView");
        }

        public ActionResult AdminRadianView()
        {
            var model = new AdminRadianViewModel();
            return View(model);
        }

    }
}