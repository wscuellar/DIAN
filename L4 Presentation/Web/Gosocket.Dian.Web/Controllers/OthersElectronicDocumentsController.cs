using Gosocket.Dian.Application;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Services.Utils.Helpers;
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
            ViewBag.ListElectronicDocuments = _electronicDocumentService.GetElectronicDocuments().Where(x => x.Id == 1)?.Select(t => new AutoListModel(t.Id.ToString(), t.Name)).ToList();
            ViewBag.ContributorId = User.ContributorId();

            return View();
        }

        [HttpGet]
        public ActionResult AddOrUpdate(ValidacionOtherDocsElecViewModel dataentity)
        {
            List<ElectronicDocument> listED = _electronicDocumentService.GetElectronicDocuments();
            List<Domain.Sql.OtherDocElecOperationMode> listOM = _othersDocsElecContributorService.GetOperationModes();
            OthersElectronicDocumentsViewModel model = new OthersElectronicDocumentsViewModel();
            if (dataentity.Message != null)
            {

                ViewBag.Message = dataentity.Message;
            }

            var opeMode = listOM.FirstOrDefault(o => o.Id == (int)dataentity.OperationModeId);
            //if((int)dataentity.OperationModeId)==0)
            if (opeMode != null) model.OperationMode = opeMode.Name;

            // ViewBag's

            ViewBag.Title = $"Asociar modo de operación";

            if (dataentity.ContributorIdType == Domain.Common.OtherDocElecContributorType.TechnologyProvider)
            {
                ViewBag.Title = $"Asociar modo de operación - Proveedor de soluciones Tecnológicas";
            }
            ViewBag.ContributorName = dataentity.ContributorIdType.GetDescription();
            ViewBag.ElectronicDocumentName = _electronicDocumentService.GetNameById(dataentity.ElectronicDocumentId);
            ViewBag.ListSoftwares = new List<SoftwareViewModel>();

            // Validación Software en proceso...
            var softwareActive = false;
            //var softwareInProcess = _othersDocsElecContributorService.GetContributorSoftwareInProcess(User.ContributorId(), (int)OtherDocElecSoftwaresStatus.InProcess);
            var docElecContributorsList = _othersDocsElecContributorService.GetDocElecContributorsByContributorId(User.ContributorId());
            if (docElecContributorsList != null && docElecContributorsList.Count > 0)
            {
                var stateName = OtherDocElecState.Habilitado.GetDescription();
                var contributorsEnabled = docElecContributorsList.Where(x => x.State == stateName).ToList();
                if (contributorsEnabled != null && contributorsEnabled.Count > 0)
                {
                    var contributorEnabled = contributorsEnabled.FirstOrDefault(x => x.OtherDocElecContributorTypeId == (int)dataentity.ContributorIdType
                        && x.OtherDocElecOperationModeId == (int)dataentity.OperationModeId);

                    if (contributorEnabled != null)
                    {
                        //var operations = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationsListByDocElecContributorId(contributorEnabled.Id);
                        //if(operations != null && operations.Any(x => x.Deleted == false))
                        //{
                        //    var operation = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationByDocEleContributorId(contributorEnabled.Id);
                        //    return this.RedirectToAction("Index", "OthersElectronicDocAssociated", new { Id = operation.Id });
                        //}

                        var operation = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationByDocEleContributorId(contributorEnabled.Id);
                        return this.RedirectToAction("Index", "OthersElectronicDocAssociated", new { Id = operation.Id });
                    }
                }

                stateName = OtherDocElecState.Test.GetDescription();
                var contributorsInTestSameOperation = docElecContributorsList.Where(x => x.State == stateName).ToList();
                if (contributorsInTestSameOperation != null && contributorsInTestSameOperation.Count > 0)
                {
                    var contributorInTestSameOperation = contributorsInTestSameOperation.FirstOrDefault(x => x.OtherDocElecContributorTypeId == (int)dataentity.ContributorIdType
                        && x.OtherDocElecOperationModeId == (int)dataentity.OperationModeId);

                    //cambiar 
                    if (contributorInTestSameOperation != null)
                    {
                        //var operations = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationsListByDocElecContributorId(contributorInTestSameOperation.Id);
                        //if (operations != null && operations.Any(x => x.Deleted == false))
                        //{
                        //    softwareActive = true;
                        //}

                        softwareActive = false;
                    }
                    //else
                    //{
                    //	var msg = $"No se puede {ViewBag.Title}, ya que tiene uno en estado: \"En Proceso\"";
                    //	return this.RedirectToAction("AddParticipants", new { electronicDocumentId = dataentity.ElectronicDocumentId, message = msg });
                    //}
                }
            }

            ViewBag.softwareActive = softwareActive;

            // Model
            model.ElectronicDocumentId = dataentity.ElectronicDocumentId;
            model.OperationModeId = (int)dataentity.OperationModeId;
            model.ContributorIdType = (int)dataentity.ContributorIdType;
            model.OtherDocElecContributorId = (int)dataentity.ContributorId;
            model.UrlEventReception = ConfigurationManager.GetValue("WebServiceUrlEvent");

            PagedResult<OtherDocsElectData> List = _othersDocsElecContributorService.List(User.ContributorId(), (int)dataentity.ContributorIdType, (int)dataentity.OperationModeId, model.ElectronicDocumentId);
            if (model.OperationModeId == 0)
            {
                List = _othersDocsElecContributorService.List3(User.ContributorId(), (int)dataentity.ContributorIdType, model.ElectronicDocumentId);
            }
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
            if (dataentity.ContributorIdType == Domain.Common.OtherDocElecContributorType.TechnologyProvider)
            {
                operationModesList.Add(new Domain.RadianOperationMode { Id = (int)Domain.Common.OtherDocElecOperationMode.OwnSoftware, Name = Domain.Common.OtherDocElecOperationMode.OwnSoftware.GetDescription() });
            }
            else
            {
                operationModesList.Add(new Domain.RadianOperationMode { Id = (int)Domain.Common.OtherDocElecOperationMode.OwnSoftware, Name = Domain.Common.OtherDocElecOperationMode.OwnSoftware.GetDescription() });
                operationModesList.Add(new Domain.RadianOperationMode { Id = (int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider, Name = Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider.GetDescription() });
                var OperationsModes = _othersDocsElecContributorService.GetOperationModes().Where(x => x.Id == 3).FirstOrDefault();
                operationModesList.Add(new Domain.RadianOperationMode { Id = OperationsModes.Id, Name = OperationsModes.Name });
            }

            var contributor = _contributorService.GetContributorById(User.ContributorId(), model.ContributorIdType);
            model.ContributorName = contributor?.Name;
            model.SoftwareId = Guid.NewGuid().ToString();
            model.SoftwareIdPr = model.SoftwareId;
            var providersList = new List<ContributorViewModel>();
            var contributorsList = _othersDocsElecContributorService.GetTechnologicalProviders(User.ContributorId(), model.ElectronicDocumentId, (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider, OtherDocElecState.Habilitado.GetDescription());
            if (contributorsList != null)
                providersList.AddRange(contributorsList.Select(c => new ContributorViewModel { Id = c.Id, Name = c.Name }).ToList());
            ViewBag.ListTechnoProviders = new SelectList(providersList, "Id", "Name");


            if (model.OperationModeId == 1)
            {
                model.ContributorName = contributor?.Name;
                model.SoftwareId = Guid.NewGuid().ToString();
                model.SoftwareIdPr = model.SoftwareId;
            }
            else
            {
                model.SoftwareName = " ";
                model.PinSW = " ";
            }

            ViewBag.OperationModes = new SelectList(operationModesList, "Id", "Name", operationModesList.FirstOrDefault().Id);
            ViewBag.IsElectronicPayroll = model.ElectronicDocumentId == 1;
            return View(model);
        }






        [HttpPost]
        public async Task<ActionResult> AddOrUpdateContributor(OthersElectronicDocumentsViewModel model)
        {
            if (model.OperationModeSelectedId == "3")
            {
                model.SoftwareName = "Solución gratuita";

            }
            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;
            var tipo = model.OperationModeId;
            if (model.OperationModeId == 0)
            {
                model.OperationModeId = Int32.Parse(model.OperationModeSelectedId);


            }
            if (model.OperationModeSelectedId == "3")
            {
                model.PinSW = "0000";
                Guid g = Guid.NewGuid();
                model.SoftwareId = g.ToString();

                model.OperationModeId = (int)Domain.Common.OtherDocElecOperationMode.FreeBiller;
            }
            if (model.SoftwareId == null)
            {
                Guid g = Guid.NewGuid();
                model.SoftwareId = g.ToString();
            }

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
            if (model.OperationModeId != 2) providerId = User.ContributorId();

            var IdS = Guid.NewGuid();
            var now = DateTime.Now;



            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributorNew(
                                                            User.ContributorId(),
                                                            OtherDocElecState.Registrado,
                                                            model.ContributorIdType,
                                                            (int)model.OperationModeId,
                                                            model.ElectronicDocumentId,
                                                            User.UserName());

            var ContributorId = otherDocElecContributor.Id.ToString();



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
                SoftwareDate = now,
                Timestamp = now,
                Updated = now,
                SoftwareId = model.OperationModeSelectedId != "3" ? new Guid(model.SoftwareId) : new Guid("FA326CA7-C1F8-40D3-A6FC-24D7C1040607"),
                OtherDocElecContributorId = Int32.Parse(ContributorId)
            };

            OtherDocElecContributorOperations contributorOperation = new OtherDocElecContributorOperations()
            {
                OtherDocElecContributorId = Int32.Parse(ContributorId),
                OperationStatusId = (int)OtherDocElecState.Test,
                Deleted = false,
                Timestamp = now,
                SoftwareType = model.OperationModeId,
                SoftwareId = IdS
            };
            ResponseMessage response = new ResponseMessage();
            if (tipo != 0)
            {
                response = _othersElectronicDocumentsService.AddOtherDocElecContributorOperation(contributorOperation, software, true, true);
            }
            else
            {
                response = _othersElectronicDocumentsService.AddOtherDocElecContributorOperationNew(contributorOperation, software, true, true, model.OtherDocElecContributorId, model.ContributorIdType, model.OtherDocElecContributorId);
            }
            if (response.Code == 500)
            {
                return this.RedirectToAction("AddOrUpdate", new { ElectronicDocumentId = model.ElectronicDocumentId, OperationModeId = 0, ContributorIdType = model.ContributorIdType, ContributorId = model.OtherDocElecContributorId, Message = response.Message });
            }
            else
            {
                if (model.OperationModeSelectedId == "3")
                {
                    Session["ShowFree"] = "1";

                }

                if (model.ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument)
                {
                    var accountId = await ApiHelpers.ExecuteRequestAsync<string>(ConfigurationManager.GetValue("AccountByNit"), new { Nit = User.ContributorCode() });
                    var rangoDePrueba = new NumberingRange
                    {
                        id = Guid.NewGuid(),
                        OtherDocElecContributorOperation = contributorOperation.Id,
                        Prefix = "SEDS",
                        ResolutionNumber = "18760000001",
                        NumberFrom = 984000000,
                        NumberTo = 985000000,
                        CurrentNumber = 984000000,
                        CreationDate = DateTime.Now,
                        ExpirationDate = new DateTime(DateTime.Now.Year, 12, 31),
                        IdDocumentTypePayroll = "104",
                        DocumentTypePayroll = "Documento Soporte",
                        Current = "SEDS (984000000 - 985000000)",
                        N102 = "SEDS (984000000 - 985000000)",
                        N103 = "SEDS (984000000 - 985000000)",
                        State = 3,
                        AccountId = Guid.Parse(accountId),
                        PartitionKey = accountId.ToString(),
                    };
                    var cosmosManager = new CosmosDbManagerNumberingRange();
                    await cosmosManager.SaveNumberingRange(rangoDePrueba);
                }
            }
            _othersElectronicDocumentsService.ChangeParticipantStatus(otherDocElecContributor.Id, OtherDocElecState.Test.GetDescription(), model.ContributorIdType, OtherDocElecState.Registrado.GetDescription(), string.Empty);

            if (model.ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument)
            {
                return RedirectToAction("AddOrUpdate", "OthersElectronicDocuments", new
                {
                    ElectronicDocumentId = model.ElectronicDocumentId,
                    OperationModeId = 0,
                    ContributorIdType = 1,
                    ContributorId = User.ContributorId()
                });
            }

            return RedirectToAction("Index", "OthersElectronicDocAssociated", new { id = contributorOperation.Id });
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

            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(User.ContributorId(),
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
            {
                var ResponseMessageRedirectTo = new ResponseMessage("", TextResources.redirectType);

                var mode = _othersDocsElecContributorService.GetDocElecContributorsByContributorId(ValidacionOtherDocs.ContributorId)
                    .Where(x => x.ElectronicDocumentId == ValidacionOtherDocs.ElectronicDocumentId && x.OtherDocElecContributorTypeId == 1);// 1 es emisor

                if (mode.Count() == 0)
                {
                    return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelect_Confirm.Replace("@docume", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (ValidacionOtherDocs.ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument)
                    {
                        ResponseMessageRedirectTo.RedirectTo = Url.Action("AddOrUpdate", "OthersElectronicDocuments",
                        new
                        {
                            ElectronicDocumentId = ValidacionOtherDocs.ElectronicDocumentId,
                            OperationModeId = 0,
                            ContributorIdType = 1,
                            ContributorId = User.ContributorId(),
                            message = ""
                        });
                        return Json(ResponseMessageRedirectTo, JsonRequestBehavior.AllowGet);
                    }

                    ResponseMessageRedirectTo.RedirectTo = Url.Action("AddParticipants", "OthersElectronicDocuments", new { electronicDocumentId = ValidacionOtherDocs.ElectronicDocumentId, message = "" });
                    return Json(ResponseMessageRedirectTo, JsonRequestBehavior.AllowGet);
                }
            }
            if (ValidacionOtherDocs.Accion == "SeleccionParticipante")
            {
                // El proveedor tecnólogico debe estar habilitado en el catalogo de validación previa...
                if (ValidacionOtherDocs.ContributorIdType == Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                {
                    Contributor contributor = _contributorService.Get(ValidacionOtherDocs.ContributorId);
                    if (contributor.ContributorTypeId != (int)Domain.Common.ContributorType.Provider || !contributor.Status)
                    {
                        return Json(new ResponseMessage(TextResources.TechnologProviderDisabled, TextResources.alertType), JsonRequestBehavior.AllowGet);
                    }
                }
                var mode = _othersDocsElecContributorService.GetDocElecContributorsByContributorId(ValidacionOtherDocs.ContributorId);
                if (mode.Where(x => x.OtherDocElecContributorTypeId == 1).Count() == 0 && ValidacionOtherDocs.ContributorIdType == Domain.Common.OtherDocElecContributorType.Transmitter)
                {
                    return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectParticipante_Confirm.Replace("@Participante", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);
                }
                else if (mode.Where(x => x.OtherDocElecContributorTypeId == 2).Count() == 0 && ValidacionOtherDocs.ContributorIdType == Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                {
                    return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectParticipante_Confirm.Replace("@Participante", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);

                }
                else
                {
                    string ContributorId = null;
                    var ResponseMessageRedirectTo = new ResponseMessage("", TextResources.redirectType);

                    ContributorId = mode.FirstOrDefault().ContributorId.ToString();


                    ResponseMessageRedirectTo.RedirectTo = Url.Action("AddOrUpdate", "OthersElectronicDocuments",
                                           new
                                           {
                                               ElectronicDocumentId = 1, //ValidacionOtherDocs.ElectronicDocumentId,
                                               OperationModeId = 0, //(int)ValidacionOtherDocs.OperationModeId,
                                               ContributorIdType = (int)ValidacionOtherDocs.ContributorIdType,
                                               ContributorId
                                           });

                    return Json(ResponseMessageRedirectTo, JsonRequestBehavior.AllowGet);


                }
            }

            if (ValidacionOtherDocs.Accion == "SeleccionParticipanteEmisor")
            {
                // El proveedor tecnólogico debe estar habilitado en el catalogo de validación previa...
                if (ValidacionOtherDocs.ContributorIdType == Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                {
                    Contributor contributor = _contributorService.Get(ValidacionOtherDocs.ContributorId);
                    if (contributor.ContributorTypeId != (int)Domain.Common.ContributorType.Provider || !contributor.Status)
                    {
                        return Json(new ResponseMessage(TextResources.TechnologProviderDisabled, TextResources.alertType), JsonRequestBehavior.AllowGet);
                    }
                }
                var mode = _othersDocsElecContributorService.GetDocElecContributorsByContributorId(ValidacionOtherDocs.ContributorId);
                if (mode.Where(x => x.OtherDocElecContributorTypeId == 1).Count() == 0)
                {
                    return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelectOperationMode_Confirm.Replace("@Participante", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string ContributorId = null;
                    var ResponseMessageRedirectTo = new ResponseMessage("", TextResources.redirectType);

                    ContributorId = mode.FirstOrDefault().ContributorId.ToString();

                    if (ValidacionOtherDocs.ContributorIdType == Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                    {
                        ResponseMessageRedirectTo.RedirectTo = Url.Action("AddOrUpdate", "OthersElectronicDocuments",
                                               new
                                               {
                                                   ElectronicDocumentId = 1, //ValidacionOtherDocs.ElectronicDocumentId,
                                                   OperationModeId = 1, //(int)ValidacionOtherDocs.OperationModeId,
                                                   ContributorIdType = (int)ValidacionOtherDocs.ContributorIdType,
                                                   ContributorId
                                               });
                    }
                    else
                    {
                        ResponseMessageRedirectTo.RedirectTo = Url.Action("AddOrUpdate", "OthersElectronicDocuments",
                                               new
                                               {
                                                   ElectronicDocumentId = 1, //ValidacionOtherDocs.ElectronicDocumentId,
                                                   OperationModeId = mode.FirstOrDefault().OtherDocElecOperationModeId, //(int)ValidacionOtherDocs.OperationModeId,
                                                   ContributorIdType = (int)ValidacionOtherDocs.ContributorIdType,
                                                   ContributorId
                                               });

                    }
                    return Json(ResponseMessageRedirectTo, JsonRequestBehavior.AllowGet);


                }
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