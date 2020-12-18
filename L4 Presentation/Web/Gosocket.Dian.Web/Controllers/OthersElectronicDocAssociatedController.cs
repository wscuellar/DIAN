using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
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

        private readonly IContributorService _contributorService;
        private readonly IOthersDocsElecContributorService _othersDocsElecContributorService;

        public OthersElectronicDocAssociatedController(IContributorService contributorService,
            IOthersDocsElecContributorService othersDocsElecContributorService)
        {
            _contributorService = contributorService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id de la tabla OtherDocElecContributor</param>
        /// <param name="electronicDocumentId"></param>
        /// <param name="operationModeId"></param>
        /// <param name="ContributorIdType"></param>
        /// <returns></returns>
        public ActionResult Index(int id=0, int electronicDocumentId = 0, int operationModeId = 0, int ContributorIdType = 0)
        {
            ViewBag.OtherDocElecContributorId = id;
            ViewBag.Participant = "Emisor";

            var electronicDoc = new ElectronicDocumentService().GetElectronicDocuments()
                .Where(d => d.Id == electronicDocumentId).FirstOrDefault();/*.Select(e => new ElectronicDocumentViewModel
                {
                    Id = e.Id,
                    Name = e.Name
                });*/

            if (electronicDoc != null)
            {
                ViewBag.ElectronicDocumentId = electronicDoc.Id;
                ViewBag.ElectronicDocumentName = electronicDoc.Name;
            }

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


        //[HttpPost]
        //public ActionResult CancelRegister(int ContributorId, int ContributorTypeId, string State, string description)
        //{
        //    ResponseMessage response = new ResponseMessage();

        //    return Json(response, JsonRequestBehavior.AllowGet);
        //}

        /// <summary>
        /// Cancelar una asociación de la tabla OtherDocElecContributor, OtherDocElecContributorOperations y OtherDocElecSoftware
        /// </summary>
        /// <param name="id">Id de la tabla OtherDocElecContributor</param>
        /// <param name="desciption">Descripción de por que se cancela</param>
        /// <returns><see cref="ResponseMessage"/></returns>
        [HttpPost]
        public JsonResult CancelRegister(int id, string description)
        {
            ResponseMessage response = _othersDocsElecContributorService.CancelRegister(id, description);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

    }
}