using Gosocket.Dian.Application;
using Gosocket.Dian.Web.Common;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianEnablingInvoiceIndirectController : Controller
    {
        // GET: RadianEnablingInvoiceIndirect

        RadianContributorService radianContributorService = new RadianContributorService();

        // GET: RadianEnablingInvoiceDirect
        public ActionResult Index(int contributorId)
        {
            List<Domain.RadianContributor> radianContributor = radianContributorService.List(t => t.ContributorId == contributorId && t.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.ElectronicInvoice);
            if (!radianContributor.Any())
            {
                Domain.RadianContributor newRadianContributor = new Domain.RadianContributor()
                {
                    ContributorId = contributorId,
                    CreatedBy = User.UserName(),
                    RadianContributorTypeId = (int)Domain.Common.RadianContributorType.ElectronicInvoice,
                    RadianOperationModeId = (int)Domain.Common.RadianOperationMode.Indirect,
                    RadianState = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Registrado),
                    CreatedDate = System.DateTime.Now,
                    Update = System.DateTime.Now,
                };
                int id = radianContributorService.AddOrUpdate(newRadianContributor);
                newRadianContributor.Id = id;
            }
            return View();
        }
    }
}