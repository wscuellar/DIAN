using Gosocket.Dian.Application;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    /// <summary>
    /// Controlador utilizado para la opción de menu, Otros Documentos. HU DIAN-HU-070_3_ODC_HabilitacionParticipanteOD
    /// </summary>
    [Authorize]
    public class OthersElectronicDocumentsController : Controller
    {
        private readonly IOthersElectronicDocumentsService _othersElectronicDocumentsService;
        private readonly IOthersDocsElecContributorService _othersDocsElecContributorService;

        public OthersElectronicDocumentsController(IOthersElectronicDocumentsService othersElectronicDocumentsService,
            IOthersDocsElecContributorService othersDocsElecContributorService)
        {
            _othersElectronicDocumentsService = othersElectronicDocumentsService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
        }

        /// <summary>
        /// Listado de los otros documentos que se encuentran en la BD de SQLServer ElectronicDocument
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            NameValueCollection result = _othersDocsElecContributorService.Summary(User.UserCode());
            ViewBag.ContributorId = result["ContributorId"];
            ViewBag.Transmitter_OtherDocElecContributorTypeId = result["Transmitter_OtherDocElecContributorTypeId"];
            ViewBag.Transmitter_OtherDocElecOperationModeId = result["Transmitter_OtherDocElecOperationModeId"];
            ViewBag.TechnologyProvider_OtherDocElecContributorTypeId = result["TechnologyProvider_OtherDocElecContributorTypeId"];
            ViewBag.TechnologyProvider_OtherDocElecOperationModeId = result["TechnologyProvider_OtherDocElecOperationModeId"];

            ViewBag.UserCode = User.UserCode();
            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;
            ViewBag.ListElectronicDocuments = new ElectronicDocumentService().GetElectronicDocuments().Select(t => new AutoListModel(t.Id.ToString(), t.Name)).ToList();
            ViewBag.ContributorId = User.ContributorId();

            return View();
        }

        public ActionResult AddOrUpdate(int electronicDocumentId = 0, int operationModeId = 0, int ContributorIdType = 0)
        {
            List<ElectronicDocument> listED = new ElectronicDocumentService().GetElectronicDocuments();
            List<OperationModeViewModel> listOM = new TestSetViewModel().GetOperationModes();
            OthersElectronicDocumentsViewModel model = new OthersElectronicDocumentsViewModel();

            var opeMode = listOM.FirstOrDefault(o => o.Id == operationModeId);
            if (opeMode != null)
                model.OperationMode = opeMode.Name;

            ViewBag.Title = $"Asociar modo de operación {model.OperationMode}";


            return View(model);
        }

        public ActionResult AddParticipants(int electronicDocumentId)
        {
            ViewBag.UserCode = User.UserCode();
            ViewBag.electronicDocumentId = electronicDocumentId;

            IEnumerable<SelectListItem> OperationsModes = _othersDocsElecContributorService.GetOperationModes()
             .Select(c => new SelectListItem
             {
                 Value = c.Id.ToString(),
                 Text = c.Name
             });
            ViewBag.ListOperationMode = OperationsModes;

            return View();
        }


        [HttpPost]
        public JsonResult Add(ValidacionOtherElectronicDocumentsViewModel registrationData)
        {


            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(registrationData.ContributorId,
                                                Domain.Common.OtherDocElecState.Registrado,
                                                registrationData.ContributorIdType,
                                                registrationData.OperationModeId,
                                                User.UserName());


            //if (otherDocElecContributor.RadianSoftwares == null || radianContributor.RadianSoftwares.Count == 0)
            //   return Json(new ResponseMessage(TextResources.ParticipantWithoutSoftware, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);

            OtherDocElecSoftware software = otherDocElecContributor.OtherDocElecSoftwares.FirstOrDefault();
            OtherDocElecContributorOperations ContributorOperation = new OtherDocElecContributorOperations()
            {
                OtherDocElecContributorId = otherDocElecContributor.Id,
                SoftwareId = software.Id,
                OperationStatusId = (int)OtherDocElecState.Registrado,
                SoftwareType = (int)RadianOperationModeTestSet.OwnSoftware,
                Timestamp = DateTime.Now
            };
            ResponseMessage result = _radianAprovedService.AddRadianContributorOperation(radianContributorOperation, software, testSet, true, false);
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Validation(ValidacionOtherElectronicDocumentsViewModel ValidacionOtherElectronicDocuments)
        {
            int contributorId = User.ContributorId();
            ResponseMessage validation =
                _othersElectronicDocumentsService.Validation(ValidacionOtherElectronicDocuments.UserCode.ToString(),
                ValidacionOtherElectronicDocuments.Accion,
                ValidacionOtherElectronicDocuments.ElectronicDocument,
                   ValidacionOtherElectronicDocuments.ComplementoTexto,
                   ValidacionOtherElectronicDocuments.ContributorIdType
                );

            return Json(validation, JsonRequestBehavior.AllowGet);
        }

    }
}