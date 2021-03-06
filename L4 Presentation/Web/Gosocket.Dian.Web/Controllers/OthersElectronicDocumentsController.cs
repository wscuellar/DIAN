using Gosocket.Dian.Application;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
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
        private readonly IElectronicDocumentService _electronicDocumentService;
        private readonly IOthersDocsElecSoftwareService _othersDocsElecSoftwareService;

        public OthersElectronicDocumentsController(IOthersElectronicDocumentsService othersElectronicDocumentsService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IContributorService contributorService,
            IElectronicDocumentService electronicDocumentService,
            IOthersDocsElecSoftwareService othersDocsElecSoftwareService)
        {
            _othersElectronicDocumentsService = othersElectronicDocumentsService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _contributorService = contributorService;
            _electronicDocumentService = electronicDocumentService;
            _othersDocsElecSoftwareService = othersDocsElecSoftwareService;
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
            
            var opeMode = listOM.FirstOrDefault(o => o.Id == (int)dataentity.OperationModeId);
            if (opeMode != null) model.OperationMode = opeMode.Name;

            // ViewBag's
            ViewBag.Title = $"Asociar modo de operación {model.OperationMode}";
            ViewBag.ContributorName = dataentity.ContributorIdType.GetDescription();
            ViewBag.ElectronicDocumentName = _electronicDocumentService.GetNameById(dataentity.ElectronicDocumentId);
            ViewBag.ListSoftwares = new List<SoftwareViewModel>();

            // Validación Software en proceso...
            var softwareActive = false;
            //var softwareInProcess = _othersDocsElecContributorService.GetContributorSoftwareInProcess(User.ContributorId(), (int)OtherDocElecSoftwaresStatus.InProcess);
            var softwareInProcess = _othersDocsElecContributorService.GetContributorSoftwareInProcess(User.ContributorId(), (int)OtherDocElecState.Test);
            if (softwareInProcess != null)
            {
                if (softwareInProcess.OtherDocElecContributorTypeId == (int)dataentity.ContributorIdType 
                    && softwareInProcess.OtherDocElecOperationModeId == (int)dataentity.OperationModeId)
                {
                    softwareActive = true;
                }
                else
                {
                    var msg = $"No se puede {ViewBag.Title}, ya que tiene uno en estado: \"En Proceso\"";
                    return this.RedirectToAction("AddParticipants", new { electronicDocumentId = dataentity.ElectronicDocumentId, message = msg });
                }
            }

            ViewBag.softwareActive = softwareActive;
            
            // Model
            model.ElectronicDocumentId = dataentity.ElectronicDocumentId;
            model.OperationModeId = (int)dataentity.OperationModeId;
            model.ContributorIdType = (int)dataentity.ContributorIdType;
            model.OtherDocElecContributorId = (int)dataentity.ContributorId;
            model.UrlEventReception = ConfigurationManager.GetValue("WebServiceUrl");

            PagedResult<OtherDocsElectData> List = _othersDocsElecContributorService.List(User.ContributorId(), (int)dataentity.ContributorIdType, (int)dataentity.OperationModeId);

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
                StateSoftware = ((OtherDocElecState)int.Parse(t.StateSoftware)).GetDescription(),
                StateContributor = t.StateContributor,
                Url = t.Url,
                CreatedDate = t.CreatedDate
            }).ToList();

            List<Domain.RadianOperationMode> operationModesList = new List<Domain.RadianOperationMode>();
            if (model.OperationModeId == 1)
            {
                operationModesList.Add(new Domain.RadianOperationMode { Id = (int)Domain.Common.OtherDocElecOperationMode.OwnSoftware, Name = Domain.Common.OtherDocElecOperationMode.OwnSoftware.GetDescription() });
                // get contributor name...
                var contributor = this._contributorService.GetContributorById(User.ContributorId(), model.ContributorIdType);
                model.ContributorName = contributor?.Name;
                model.SoftwareId = Guid.NewGuid().ToString();
            }
            else
            {
                operationModesList.Add(new Domain.RadianOperationMode { Id = (int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider, Name = Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider.GetDescription() });
                var providersList = new List<ContributorViewModel>();
                var contributorsList = _othersDocsElecContributorService.GetTechnologicalProviders(User.ContributorId(), model.ElectronicDocumentId, (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider, OtherDocElecState.Habilitado.GetDescription());
                if (contributorsList != null)
                    providersList.AddRange(contributorsList.Select(c => new ContributorViewModel { Id = c.Id, Name = c.Name }).ToList());
                ViewBag.ListTechnoProviders = new SelectList(providersList, "Id", "Name");

                model.SoftwareName = " ";
                model.PinSW = " ";
            }

            ViewBag.OperationModes = new SelectList(operationModesList, "Id", "Name", operationModesList.FirstOrDefault().Id);

            return View(model);
        }

        [HttpPost]
        public ActionResult AddOrUpdateContributor(OthersElectronicDocumentsViewModel model)
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;
            
            GlobalTestSetOthersDocuments testSet = _othersDocsElecContributorService.GetTestResult((int)model.OperationModeId, model.ElectronicDocumentId);
            if (testSet == null)
                return Json(new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);
            
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var item in allErrors) ModelState.AddModelError("", item.ErrorMessage);
                return View("AddOrUpdate", new ValidacionOtherDocsElecViewModel { ContributorId = 1 });
            }

            int providerId = model.ProviderId;
            if(model.OperationModeId != 2) providerId = User.ContributorId();

            var IdS = Guid.NewGuid();
            OtherDocElecSoftware software = new OtherDocElecSoftware()
            {
                Id = IdS,
                Url = model.UrlEventReception,
                Name = model.SoftwareName,
                Pin = model.PinSW,
                ProviderId = providerId,
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
            // Validar si esta tabla serían los Logs...?
            OtherDocElecContributorOperations contributorOperation = new OtherDocElecContributorOperations()
            {
                OtherDocElecContributorId = model.OtherDocElecContributorId,
                OperationStatusId = (int)OtherDocElecState.Test,
                Deleted = false,
                Timestamp = DateTime.Now,
                SoftwareType = model.OperationModeId,
                SoftwareId = IdS
            };
            
            ResponseMessage response = _othersElectronicDocumentsService.AddOtherDocElecContributorOperation(contributorOperation, software, true, true);
            if (response.Code == 500) // error...
            {
                return this.RedirectToAction("AddParticipants", new { electronicDocumentId = model.ElectronicDocumentId, message = response.Message });
            }

            _othersElectronicDocumentsService.ChangeParticipantStatus(model.OtherDocElecContributorId, OtherDocElecState.Test.GetDescription(), model.ContributorIdType, OtherDocElecState.Registrado.GetDescription(), string.Empty);

            return RedirectToAction("Index", "OthersElectronicDocAssociated", new { id = model.OtherDocElecContributorId });
        }

        public ActionResult AddParticipants(int electronicDocumentId, string message)
        {
            ViewBag.Message = message;
            ViewBag.ContributorId = User.ContributorId();
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

            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(registrationData.ContributorId,
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
            if (ValidacionOtherDocs.Accion == "SeleccionElectronicDocument")
                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelect_Confirm.Replace("@docume", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);

            if (ValidacionOtherDocs.Accion == "SeleccionParticipante")
            {
                // El proveedor tecnólogico debe estar habilitado en el catalogo de validación previa...
                if (ValidacionOtherDocs.ContributorIdType == Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                {
                    Contributor contributor = _contributorService.Get(ValidacionOtherDocs.ContributorId);
                    if(contributor.ContributorTypeId != (int)Domain.Common.ContributorType.Provider || !contributor.Status)
                    {
                        return Json(new ResponseMessage(TextResources.TechnologProviderDisabled, TextResources.alertType), JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectParticipante_Confirm.Replace("@Participante", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);
            }

            if (ValidacionOtherDocs.Accion == "SeleccionOperationMode")
            {
                List<OtherDocElecContributor> Lista = _othersDocsElecContributorService.ValidateExistenciaContribuitor(ValidacionOtherDocs.ContributorId, (int)ValidacionOtherDocs.ContributorIdType, (int)ValidacionOtherDocs.OperationModeId, OtherDocElecState.Cancelado.GetDescription());
                //List<OtherDocElecContributor> Lista = _othersDocsElecContributorService.ValidateExistenciaContribuitor(ValidacionOtherDocs.ContributorId, (int)ValidacionOtherDocs.ContributorIdType, (int)ValidacionOtherDocs.OperationModeId, OtherDocElecState.Habilitado.GetDescription());
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
                                                            User.ContributorId(),
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
        public JsonResult GetSoftwaresByContributorId(int id, int electronicDocumentId)
        {
            var softwareList = _othersDocsElecSoftwareService.GetSoftwaresByProviderTechnologicalServices(id,
                electronicDocumentId, (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider, 
                OtherDocElecState.Habilitado.GetDescription()).Select(s => new SoftwareViewModel
                {
                    //Id = s.Id,
                    Id = s.SoftwareId,
                    Name = s.Name
                }).ToList();

            return Json(new { res = softwareList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetDataBySoftwareId(Guid SoftwareId)
        {
            var software = _othersDocsElecSoftwareService.GetBySoftwareId(SoftwareId);
            if (software != null)
            {
                return Json(new
                {
                    url = software.Url,
                    SoftwareType = 1,
                    SoftwarePIN = software.Pin
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        
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