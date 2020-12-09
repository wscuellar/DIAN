using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianTechnologyProviderController : Controller
    {

        private readonly IRadianContributorService _radianContributorService;

        public RadianTechnologyProviderController(IRadianContributorService radianContributorService)
        {
            _radianContributorService = radianContributorService;
        }

        // GET: RadianTechnologyProvider
        public ActionResult Index(int contributorId)
        {
            _radianContributorService.CreateContributor(contributorId,
                                                        RadianState.Registrado,
                                                        Domain.Common.RadianContributorType.TechnologyProvider,
                                                        Domain.Common.RadianOperationMode.Direct,
                                                        User.UserName());

            return View();
        }
    }
}