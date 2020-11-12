using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianApprovedController : Controller
    {

        private readonly IRadianContributorService _radianContributorService;

        public RadianApprovedController(IRadianContributorService radianContributorService)
        {
            _radianContributorService = radianContributorService;
        }


        public ActionResult Index()
        {
            return View();
        }

        // GET: RadianFactor
        [HttpPost]
        public ActionResult Index(RegistrationDataViewModel registrationData)
        {
            _radianContributorService.CreateContributor(registrationData.ContributorId,
                                                        RadianState.Registrado,
                                                        registrationData.RadianContributorType,
                                                        registrationData.RadianOperationMode,
                                                        User.UserName());

            return View();
        }

    }
}