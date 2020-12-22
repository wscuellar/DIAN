using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
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
        private readonly IOthersElectronicDocumentsService _othersElectronicDocumentsService;

        public OthersElectronicDocAssociatedController(IContributorService contributorService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IOthersElectronicDocumentsService othersElectronicDocumentsService)
        {
            _contributorService = contributorService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _othersElectronicDocumentsService = othersElectronicDocumentsService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id de la tabla OtherDocElecContributor</param>
        /// <param name="electronicDocumentId"></param>
        /// <param name="operationModeId"></param>
        /// <param name="ContributorIdType"></param>
        /// <returns></returns>
        public ActionResult Index(int Id = 0)//TODO:
        {
            ViewBag.ValidateRequest = true;
            OtherDocsElectData entity = _othersDocsElecContributorService.GetCOntrinutorODE(Id);

            if (entity == null)
            {
                ViewBag.ValidateRequest = false;
                return View(new OthersElectronicDocAssociatedViewModel());
            }
            var contributor = _contributorService.GetContributorById(entity.ContributorId, entity.ContibutorTypeId);
            ViewBag.ValidateRequest = true;

            if (contributor == null)
            {
                ViewBag.ValidateRequest = false;
                ModelState.AddModelError("", "No existe contribuyente!");
                return View(new OthersElectronicDocAssociatedViewModel());
            }

            OthersElectronicDocAssociatedViewModel model = new OthersElectronicDocAssociatedViewModel()
            {
                Id = entity.Id,
                ContributorId = contributor.Id,
                Name = contributor.Name,
                Nit = contributor.Code,
                BusinessName = contributor.BusinessName,
                Email = contributor.Email,
                Step = entity.Step,
                State = entity.State,//TODO:
                OperationMode = entity.OperationMode,
                OperationModeId = entity.OperationModeId,
                ElectronicDoc = entity.ElectronicDoc,
                ElectronicDocId = entity.ElectronicDocId,
                ContibutorType = entity.ContibutorType,
                ContibutorTypeId = entity.ContibutorTypeId,
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

        [HttpPost]
        public JsonResult EnviarContributor(OthersElectronicDocAssociatedViewModel entity)
        {
            bool updated = _othersElectronicDocumentsService.ChangeContributorStep(entity.Id, entity.Step + 1);

            if (updated)
            {
                return Json(new
                {
                    message = "Datos enviados correctamente.",
                    success = true,
                    data = new { Id = entity.Id }
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new ResponseMessage($"El registro no pudo ser actualizado", "Nulo"), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetSetTestResult(int Id)
        {
            OtherDocsElectData entity = _othersDocsElecContributorService.GetCOntrinutorODE(Id);

            /*GlobalTestSetOthersDocuments testSet = null;

            testSet = _othersDocsElecContributorService.GetTestResult((int)registrationData.OperationModeId, registrationData.ElectronicDocumentId);
            if (testSet == null)
                return Json(new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);

            model.RadianTestSetResult = _radianTestSetResultService.GetTestSetResult(model.Nit, key);
            RadianTestSet testSet = _radianTestSetService.GetTestSet(sType, sType);
            model.RadianTestSetResult.OperationModeName = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.RadianOperationModeTestSet), sType)));
            model.RadianTestSetResult.StatusDescription = testSet.Description;
            model.Software = software;*/
            return View();
        }
    }
}