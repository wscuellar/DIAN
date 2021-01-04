using Gosocket.Dian.Application;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IContributorService _contributorService;

        public OthersElectronicDocumentsController(IOthersElectronicDocumentsService othersElectronicDocumentsService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IContributorService contributorService)
        {
            _othersElectronicDocumentsService = othersElectronicDocumentsService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _contributorService = contributorService;
        }

        /// <summary>
        /// Listado de los otros documentos que se encuentran en la BD de SQLServer ElectronicDocument
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.UserCode = User.UserCode();
            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;
            ViewBag.ListElectronicDocuments = new ElectronicDocumentService().GetElectronicDocuments().Select(t => new AutoListModel(t.Id.ToString(), t.Name)).ToList();
            ViewBag.ContributorId = User.ContributorId();

            return View();
        }

        [HttpGet]
        public ActionResult AddOrUpdate(ValidacionOtherDocsElecViewModel dataentity)
        {
            List<ElectronicDocument> listED = new ElectronicDocumentService().GetElectronicDocuments();
            List<Domain.Sql.OtherDocElecOperationMode> listOM = _othersDocsElecContributorService.GetOperationModes();
            OthersElectronicDocumentsViewModel model = new OthersElectronicDocumentsViewModel();
            List<ContributorViewModel> listContri = new List<ContributorViewModel>();// { new ContributorViewModel() { Id = 0, Name = "Seleccione..." } };

            ViewBag.ListSoftwares = new List<SoftwareViewModel>();// { new SoftwareViewModel() { Id = System.Guid.Empty, Name = "Seleccione..." } };

            var listCont = _contributorService.GetContributors((int)Domain.Common.ContributorType.Provider.GetHashCode(), ContributorStatus.Enabled.GetHashCode()).ToList();

            if (listCont != null)
                listContri.AddRange(listCont.Select(c => new ContributorViewModel { Id = c.Id, Name = c.Name }).ToList());

            ViewBag.softwareActive = _othersDocsElecContributorService.ValidateSoftwareActive(User.ContributorId(), (int)dataentity.ContributorIdType, (int)dataentity.OperationModeId, (int)OtherDocElecSoftwaresStatus.InProcess);
            ViewBag.ListTechnoProviders = listContri;

            var opeMode = listOM.FirstOrDefault(o => o.Id == (int)dataentity.OperationModeId);
            if (opeMode != null)
                model.OperationMode = opeMode.Name;

            model.ElectronicDocumentId = dataentity.ElectronicDocumentId;
            model.OperationModeId = (int)dataentity.OperationModeId;
            model.ContributorIdType = (int)dataentity.ContributorIdType;
            model.OtherDocElecContributorId = (int)dataentity.ContributorId;

            PagedResult<OtherDocsElectData> List = _othersDocsElecContributorService.List(User.UserCode(), (int)dataentity.OperationModeId);

            model.ListTable = List.Results.Select(t => new OtherDocsElectListViewModel()
            {
                Id = t.Id,
                ContributorId = t.ContributorId,
                OperationMode = t.OperationMode,
                ContributorType = t.ContributorType,
                ElectronicDoc = t.ElectronicDoc,
                Software = t.Software,
                SoftwareId = t.SoftwareId,
                PinSW = t.PinSW,
                StateSoftware = t.StateSoftware,
                StateContributor = t.StateContributor,
                Url = t.Url,
                CreatedDate = t.CreatedDate,
            }).ToList();


            ViewBag.Title = $"Asociar modo de operación {model.OperationMode}";

            if (model.OperationModeId == 2)
            {
                model.SoftwareName = " ";
                model.PinSW = " ";
            }
            return View(model);
        }


        [HttpPost]
        public ActionResult AddOrUpdateContributor(OthersElectronicDocumentsViewModel model)
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;

            GlobalTestSetOthersDocuments testSet = null;

            testSet = _othersDocsElecContributorService.GetTestResult((int)model.OperationModeId, model.ElectronicDocumentId);
            if (testSet == null)
                return Json(new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);

            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var item in allErrors) ModelState.AddModelError("", item.ErrorMessage);
                return View("AddOrUpdate", new ValidacionOtherDocsElecViewModel { ContributorId = 1 });
            }

            var IdS = new Guid();
            OtherDocElecSoftware software = new OtherDocElecSoftware()
            {
                Id = IdS,
                Url = model.UrlEventReception,
                Name = model.SoftwareName,
                Pin = model.PinSW,
                ProviderId = model.ProviderId,
                CreatedBy = User.UserName(),
                Deleted = false,
                Status = true,
                OtherDocElecSoftwareStatusId = (int)OtherDocElecSoftwaresStatus.InProcess,
                SoftwareDate = DateTime.Now,
                Timestamp = DateTime.Now,
                Updated = DateTime.Now,
                SoftwareId = new Guid(model.SoftwareId),
                OtherDocElecContributorId = model.OtherDocElecContributorId
            };
            OtherDocElecContributorOperations contributorOperation = new OtherDocElecContributorOperations()
            {
                OtherDocElecContributorId = model.OtherDocElecContributorId,
                OperationStatusId = (int)OtherDocElecState.Test,
                Deleted = false,
                Timestamp = DateTime.Now,
                SoftwareType = model.SoftwareType,
                SoftwareId = IdS
            };

            ResponseMessage response = _othersElectronicDocumentsService.AddOtherDocElecContributorOperation(contributorOperation, software, true, true);
            if (response.Code != 500)
            {
                _othersElectronicDocumentsService.ChangeParticipantStatus(model.OtherDocElecContributorId, OtherDocElecState.Registrado.GetDescription(), model.ContributorIdType, OtherDocElecState.Registrado.GetDescription(), string.Empty);
            }

            return RedirectToAction("Index", "OthersElectronicDocAssociated", new { id = model.OtherDocElecContributorId });
        }


        public ActionResult AddParticipants(int electronicDocumentId)
        {
            ViewBag.UserCode = User.UserCode();
            ViewBag.electronicDocumentId = electronicDocumentId;

            IEnumerable<SelectListItem> OperationsModes = _othersDocsElecContributorService.GetOperationModes().Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            ViewBag.ListOperationMode = OperationsModes;

            return View();
        }


        [HttpPost]
        public JsonResult Add(ValidacionOtherDocsElecViewModel registrationData)
        {
            GlobalTestSetOthersDocuments testSet = null;

            testSet = _othersDocsElecContributorService.GetTestResult((int)registrationData.OperationModeId, registrationData.ElectronicDocumentId);
            if (testSet == null)
                return Json(new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);

            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(registrationData.UserCode.ToString(),
                                                OtherDocElecState.Registrado,
                                                (int)registrationData.ContributorIdType,
                                                (int)registrationData.OperationModeId,
                                                registrationData.ElectronicDocumentId,
                                                User.UserName());

            ResponseMessage result = new ResponseMessage(TextResources.OtherSuccessSoftware, TextResources.alertType);
            result.data = otherDocElecContributor.Id.ToString();
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Validation(ValidacionOtherDocsElecViewModel ValidacionOtherDocs)
        {
            Contributor contributor = _contributorService.GetByCode(ValidacionOtherDocs.UserCode.ToString());
            if (contributor == null || contributor.AcceptanceStatusId != 4)
                return Json(new ResponseMessage(TextResources.NonExistentParticipant, TextResources.alertType), JsonRequestBehavior.AllowGet);


            if (ValidacionOtherDocs.Accion == "SeleccionElectronicDocument")
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelect_Confirm.Replace("@docume", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);

            if (ValidacionOtherDocs.Accion == "SeleccionParticipante")
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectParticipante_Confirm.Replace("@Participante", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);

            if (ValidacionOtherDocs.Accion == "SeleccionOperationMode")
            {
                List<OtherDocElecContributor> Lista = _othersDocsElecContributorService.ValidateExistenciaContribuitor(contributor.Id, (int)ValidacionOtherDocs.OperationModeId, OtherDocElecState.Cancelado.GetDescription());
                if (Lista.Any())
                {
                    string ContributorId = null;
                    var ResponseMessageRedirectTo = new ResponseMessage("", TextResources.redirectType);
                    if (!Lista.Where(x => x.ElectronicDocumentId == ValidacionOtherDocs.ElectronicDocumentId).Any())
                    {
                        GlobalTestSetOthersDocuments testSet = null;

                        testSet = _othersDocsElecContributorService.GetTestResult((int)ValidacionOtherDocs.OperationModeId, ValidacionOtherDocs.ElectronicDocumentId);
                        if (testSet == null)
                            return Json(new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);


                        OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(
                                                            ValidacionOtherDocs.UserCode.ToString(),
                                                            OtherDocElecState.Registrado,
                                                            (int)ValidacionOtherDocs.ContributorIdType,
                                                            (int)ValidacionOtherDocs.OperationModeId,
                                                            ValidacionOtherDocs.ElectronicDocumentId,
                                                            User.UserName());

                        ContributorId = otherDocElecContributor.Id.ToString();
                    }
                    else
                    {
                        ContributorId = Lista.Where(x => x.ElectronicDocumentId == ValidacionOtherDocs.ElectronicDocumentId).FirstOrDefault().Id.ToString();
                    }


                    ResponseMessageRedirectTo.RedirectTo = Url.Action("AddOrUpdate", "OthersElectronicDocuments",
                                            new
                                            {
                                                ElectronicDocumentId = ValidacionOtherDocs.ElectronicDocumentId,
                                                OperationModeId = (int)ValidacionOtherDocs.OperationModeId,
                                                ContributorIdType = (int)ValidacionOtherDocs.ContributorIdType,
                                                ContributorId
                                            });
                    return Json(ResponseMessageRedirectTo, JsonRequestBehavior.AllowGet);

                }
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectOperationMode_Confirm.Replace("@Participante", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);
            }

            if (ValidacionOtherDocs.Accion == "CancelRegister")
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectOperationMode_Confirm.Replace("@Participante", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);


            return Json(new ResponseMessage(TextResources.FailedValidation, TextResources.alertType), JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult GetSoftwaresByContributorId(int id)
        {
            List<SoftwareViewModel> softwareList = new List<SoftwareViewModel>();
            var softs = new SoftwareService().GetSoftwaresByContributorAndState(id, true);

            if (softs != null)
            {
                softwareList = softs.Select(s => new SoftwareViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();
            }

            return Json(new { res = softwareList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetDataBySoftwareId(Guid SoftwareId)
        {
            var soft = new SoftwareService().Get(SoftwareId);

            if (soft != null)
            {
                return Json(new
                {
                    url = soft.Url,
                    SoftwareType = 1
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                url = string.Empty,
                SoftwareType = 1
            }, JsonRequestBehavior.AllowGet);
        }
    }
}