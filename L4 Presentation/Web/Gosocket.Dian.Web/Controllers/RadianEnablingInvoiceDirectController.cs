using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Web.Common;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianEnablingInvoiceDirectController : Controller
    {

        RadianContributorService radianContributorService = new RadianContributorService();

        // GET: RadianEnablingInvoiceDirect
        public ActionResult Index(int contributorId)
        {
            List<Domain.RadianContributor> radianContributor = radianContributorService.GetRadianContributor(t => t.ContributorId == contributorId &&  t.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.ElectronicInvoice);
            if (!radianContributor.Any())
            {
                RadianContributor newRadianContributor = new Domain.RadianContributor()
                {
                    ContributorId = contributorId,
                    CreatedBy = User.UserName(),
                    RadianContributorTypeId = (int)Domain.Common.RadianContributorType.ElectronicInvoice,
                    RadianOperationModeId = (int)Domain.Common.RadianOperationMode.Direct,
                    RadianState = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Registered),
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