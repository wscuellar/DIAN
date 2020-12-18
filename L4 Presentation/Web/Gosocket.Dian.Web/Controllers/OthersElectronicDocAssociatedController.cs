using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    /// <summary>
    /// Controlador Otros Documentos en estado asociado. HU DIAN-HU-070_3_ODC_HabilitacionParticipanteOD
    /// </summary>
    [Authorize]
    public class OthersElectronicDocAssociatedController : Controller
    {
        private UserService userService = new UserService();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public readonly IContributorService _contributorService;

        public OthersElectronicDocAssociatedController(IContributorService contributorService)
        {
            _contributorService = contributorService;
        }

        public ActionResult Index(int electronicDocumentId = 0, int operationModeId = 0, int ContributorIdType = 0)
        {
            var contributor = _contributorService.GetContributorByUserId(User.Identity.GetUserId(), ContributorIdType);

            if (contributor == null)
            {
                ModelState.AddModelError("", "No existe contribuyente!");
                return View();
            }

            OthersElectronicDocAssociatedViewModel model = new OthersElectronicDocAssociatedViewModel()
            {
                ContributorId = contributor.Id,
                Name = contributor.Name,
                Nit = contributor.Code,
                BusinessName = contributor.BusinessName,
                Email = contributor.Email,
                Step = 1,
                State = "Dev",
            };

            return View(model);
        }


        [HttpPost]
        public ActionResult CancelRegister(int ContributorId, int ContributorTypeId, string State, string description)
        {
            ResponseMessage response = new ResponseMessage();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

    }
}