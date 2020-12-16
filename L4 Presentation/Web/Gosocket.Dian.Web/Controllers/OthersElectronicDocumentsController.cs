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

        public ActionResult AddOrUpdate(int electronicDocumentId = 0, int operationModeId = 0, int ContributorIdType = 0)
        {
 
            List<ElectronicDocument> listED = new ElectronicDocumentService().GetElectronicDocuments();
            List<OperationModeViewModel> listOM = new TestSetViewModel().GetOperationModes();
            OthersElectronicDocumentsViewModel model = new OthersElectronicDocumentsViewModel();
            List<ContributorViewModel> listContri = new List<ContributorViewModel>()
            {
                new ContributorViewModel() { Id=0, Name= "Seleccione..."}
            };

            ViewBag.ListSoftwares = new List<SoftwareViewModel>()
            {
                new SoftwareViewModel() { Id = System.Guid.Empty, Name = "Seleccione..."}
            };

            var listCont = _contributorService.GetContributors(ContributorType.Provider.GetHashCode(), 
                ContributorStatus.Enabled.GetHashCode()).ToList();

            if(listCont != null)
            {
                listContri.AddRange(listCont.Select(c => new ContributorViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList());
            }


            ViewBag.softwareActive = _othersDocsElecContributorService.ValidateSoftwareActive(User.ContributorId(), ContributorIdType, operationModeId,(int)OtherDocElecSoftwaresStatus.InProcess);
            ViewBag.ListTechnoProviders = listContri;

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

            IEnumerable<SelectListItem> OperationsModes = _othersDocsElecContributorService.GetOperationModes().Select(c => new SelectListItem   {  Value = c.Id.ToString(),  Text = c.Name  });
            ViewBag.ListOperationMode = OperationsModes;

            return View();
        }


        [HttpPost]
        public JsonResult Add(ValidacionOtherElectronicDocumentsViewModel registrationData)
        {

            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(registrationData.UserCode.ToString(),
                                                OtherDocElecState.Registrado,
                                                registrationData.ContributorIdType,
                                                registrationData.OperationModeId,
                                                registrationData.ElectronicDocument,
                                                User.UserName());

            ResponseMessage result = new ResponseMessage(TextResources.OtherSuccessSoftware, TextResources.alertType); ;
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Validation(ValidacionOtherElectronicDocumentsViewModel ValidacionOtherElectronicDocuments)
        {
            Contributor contributor = _contributorService.GetByCode(ValidacionOtherElectronicDocuments.UserCode.ToString());
            if (contributor == null || contributor.AcceptanceStatusId != 4)
                return Json(new ResponseMessage(TextResources.NonExistentParticipant, TextResources.alertType), JsonRequestBehavior.AllowGet);


            if (ValidacionOtherElectronicDocuments.Accion == "SeleccionElectronicDocument")
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelect_Confirm.Replace("@docume", ValidacionOtherElectronicDocuments.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);

            if (ValidacionOtherElectronicDocuments.Accion == "SeleccionParticipante")
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectParticipante_Confirm.Replace("@Participante", ValidacionOtherElectronicDocuments.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);

            if (ValidacionOtherElectronicDocuments.Accion == "SeleccionOperationMode")
            {
                List<OtherDocElecContributor> Lista = _othersDocsElecContributorService.ValidateExistenciaContribuitor(contributor.Id, (int)ValidacionOtherElectronicDocuments.ContributorIdType, OtherDocElecState.Cancelado.GetDescription());
                if (Lista.Any())
                {
                    if (!Lista.Where(x => x.ElectronicDocumentId == ValidacionOtherElectronicDocuments.ElectronicDocument).Any())
                    {
                        OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(
                                                            ValidacionOtherElectronicDocuments.UserCode.ToString(),
                                                            OtherDocElecState.Registrado,
                                                            ValidacionOtherElectronicDocuments.ContributorIdType,
                                                            ValidacionOtherElectronicDocuments.OperationModeId,
                                                            ValidacionOtherElectronicDocuments.ElectronicDocument,
                                                            User.UserName());
                    }
                    var ResponseMessageRedirectTo = new ResponseMessage("", TextResources.redirectType)
                    {
                        RedirectTo = Url.Action("AddOrUpdate", "OthersElectronicDocuments",
                                                new
                                                {
                                                    electronicDocumentId = ValidacionOtherElectronicDocuments.ElectronicDocument,
                                                    operationModeId = ValidacionOtherElectronicDocuments.OperationModeId,
                                                    ContributorIdType = ValidacionOtherElectronicDocuments.ContributorIdType
                                                })
                    };
                    return Json(ResponseMessageRedirectTo, JsonRequestBehavior.AllowGet);

                }
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectOperationMode_Confirm.Replace("@Participante", ValidacionOtherElectronicDocuments.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);
            }

            if (ValidacionOtherElectronicDocuments.Accion == "CancelRegister")
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectOperationMode_Confirm.Replace("@Participante", ValidacionOtherElectronicDocuments.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);


            return Json(new ResponseMessage(TextResources.FailedValidation, TextResources.alertType), JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult GetSoftwaresByContributorId(int id)
        {
            List<SoftwareViewModel> softwareList=new List<SoftwareViewModel>();
            var softs = new SoftwareService().GetSoftwaresByContributorAndState(id, true);

            if(softs != null)
            {
                softwareList = softs.Select(s => new SoftwareViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();
            }

            return Json(new { res = softwareList }, JsonRequestBehavior.AllowGet);
        }

    }
}