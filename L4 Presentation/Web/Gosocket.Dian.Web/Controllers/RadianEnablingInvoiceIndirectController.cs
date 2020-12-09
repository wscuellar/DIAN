using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianEnablingInvoiceIndirectController : Controller
    {
        private readonly IRadianContributorService _radianContributorService;

        public RadianEnablingInvoiceIndirectController(IRadianContributorService radianContributorService)
        {
            _radianContributorService = radianContributorService;
        }

        // GET: RadianEnablingInvoiceDirect
        public ActionResult Index(int contributorId)
        {
            _radianContributorService.CreateContributor(contributorId,
                                                        RadianState.Registrado,
                                                        Domain.Common.RadianContributorType.ElectronicInvoice,
                                                        Domain.Common.RadianOperationMode.Indirect,
                                                        User.UserName());

            return View();
        }
    }
}