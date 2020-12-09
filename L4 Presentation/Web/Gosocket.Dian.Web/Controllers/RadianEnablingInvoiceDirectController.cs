using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianEnablingInvoiceDirectController : Controller
    {
        private readonly IRadianContributorService _radianContributorService;

        public RadianEnablingInvoiceDirectController(IRadianContributorService radianContributorService)
        {
            _radianContributorService = radianContributorService;
        }


        // GET: RadianEnablingInvoiceDirect
        public ActionResult Index(int contributorId)
        {
            _radianContributorService.CreateContributor(contributorId,
                                                        RadianState.Registrado,
                                                        Domain.Common.RadianContributorType.ElectronicInvoice,
                                                        Domain.Common.RadianOperationMode.Direct,
                                                        User.UserName());

            return View();
        }
    }
}