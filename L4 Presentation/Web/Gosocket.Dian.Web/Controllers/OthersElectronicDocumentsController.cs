using Gosocket.Dian.Application;
using Gosocket.Dian.Application.FreeBillerSoftwares;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Services.Utils.Helpers;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
        private static readonly TableManager tableManagerNumberRangeManager = new TableManager("GlobalNumberRange");
        private readonly IOthersElectronicDocumentsService _othersElectronicDocumentsService;
        private readonly IOthersDocsElecContributorService _othersDocsElecContributorService;
        private readonly IContributorService _contributorService;
        private readonly IElectronicDocumentService _electronicDocumentService;
        private readonly IOthersDocsElecSoftwareService _othersDocsElecSoftwareService;
        private readonly IContributorOperationsService _contributorOperationsService;
        private readonly ITestSetOthersDocumentsResultService _testSetOthersDocumentsResultService;
        private readonly IEquivalentElectronicDocumentRepository _equivalentElectronicDocumentRepository;
        private readonly TelemetryClient _telemetry;
        private readonly IGlobalOtherDocElecOperationService _globalOtherDocElecOperationService;

        public OthersElectronicDocumentsController(IOthersElectronicDocumentsService othersElectronicDocumentsService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IContributorService contributorService,
            IElectronicDocumentService electronicDocumentService,
            IOthersDocsElecSoftwareService othersDocsElecSoftwareService,
            IContributorOperationsService contributorOperationsService,
            ITestSetOthersDocumentsResultService testSetOthersDocumentsResultService,
            IEquivalentElectronicDocumentRepository equivalentElectronicDocumentRepository,
            TelemetryClient telemetry, 
            IGlobalOtherDocElecOperationService globalOtherDocElecOperationService)
        {
            _othersElectronicDocumentsService = othersElectronicDocumentsService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _contributorService = contributorService;
            _electronicDocumentService = electronicDocumentService;
            _othersDocsElecSoftwareService = othersDocsElecSoftwareService;
            _contributorOperationsService = contributorOperationsService;
            _equivalentElectronicDocumentRepository = equivalentElectronicDocumentRepository;
            _testSetOthersDocumentsResultService = testSetOthersDocumentsResultService;
            _telemetry = telemetry;
            _globalOtherDocElecOperationService = globalOtherDocElecOperationService;
        }

        /// <summary>
        /// Listado de los otros documentos que se encuentran en la BD de SQLServer ElectronicDocument
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.UserCode = User.UserCode();
            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;
            ViewBag.ContributorTypeIde = User.ContributorTypeId();

            if (ViewBag.ContributorTypeIde == (int)Domain.Common.ContributorType.BillerNoObliged)
            {
                ViewBag.ListElectronicDocuments = _electronicDocumentService.GetElectronicDocuments().Where(x => x.Id == 13)?.Select(t => new AutoListModel(t.Id.ToString(), t.Name.Replace("No OFE", ""))).ToList();
            }
            else
            {
                ViewBag.ListElectronicDocuments = _electronicDocumentService.GetElectronicDocuments().Where(x => x.Id == 1)?.Select(t => new AutoListModel(t.Id.ToString(), t.Name)).ToList();
            }

            ViewBag.ContributorId = User.ContributorId();
            ViewBag.ContributorOpMode = GetContributorOperation(ViewBag.ContributorId);
            return View();
        }

        [HttpGet]
        public ActionResult AddOrUpdate(ValidacionOtherDocsElecViewModel dataentity)
        {
            bool contributorIsOfe = User.ContributorTypeId() == (int)Domain.Common.ContributorType.Biller;
            bool electronicDocumentIsSupport = dataentity.ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument;
            bool electronicDocumentIsEquivalentDocuments = dataentity.ElectronicDocumentId == (int)ElectronicsDocuments.ElectronicEquivalent;

            OthersElectronicDocumentsViewModel model = new OthersElectronicDocumentsViewModel();

            List<ElectronicDocument> listED = _electronicDocumentService.GetElectronicDocuments();
            List<Domain.Sql.OtherDocElecOperationMode> listOM = _othersDocsElecContributorService.GetOperationModes();

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
            model.ContributorIsOfe = contributorIsOfe;

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

            var contributor = _contributorService.GetContributorById(User.ContributorId(), model.ContributorIdType);

            List<Domain.RadianOperationMode> operationModesList = new List<Domain.RadianOperationMode>();
            if (dataentity.ContributorIdType == Domain.Common.OtherDocElecContributorType.TechnologyProvider)
            {
                operationModesList.Add(new Domain.RadianOperationMode { Id = (int)Domain.Common.OtherDocElecOperationMode.OwnSoftware, Name = Domain.Common.OtherDocElecOperationMode.OwnSoftware.GetDescription() });
            }
            else
            {
                if (contributorIsOfe && electronicDocumentIsSupport)
                {
                    var tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
                    var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);

                    var operationModesDictionary = listOM.ToDictionary(t => t.OperationModeId);
                    var contributorOperationsModeInElectronicBiller = _contributorOperationsService
                        .GetContributorOperations(User.ContributorId()).Where(t => !t.Deleted).ToList();

                    foreach (var operationMode in contributorOperationsModeInElectronicBiller)
                    {
                        if (!operationModesDictionary.ContainsKey(operationMode.OperationModeId))
                        {
                            return Json(new ResponseMessage(
                                $@"El modo de operación {operationMode.OperationModeId} - {operationMode.OperationMode.Name} 
                                    NO se encuentra asociado para los otros documentos electrónicos.",
                                TextResources.alertType, 500), JsonRequestBehavior.AllowGet);
                        }

                        var testSetOfOPerationMode = GetTestSetResult(testSetResults, operationMode);
                        if (testSetOfOPerationMode.Status != (int)TestSetStatus.Accepted)
                        {
                            continue;
                        }

                        var otherDocOperationMode = operationModesDictionary[operationMode.OperationModeId];
                        operationModesList.Add(new Domain.RadianOperationMode(otherDocOperationMode.Id, otherDocOperationMode.Name));
                        model.OperationModesAsociatedInElectronicBiller.Add(new OperationModeElectronicBillerViewModel
                        {
                            OperationModeId = otherDocOperationMode.Id,
                            OperationModeName = otherDocOperationMode.Name,
                            Data = new OperationModeElectronicBillerDataViewModel
                            {
                                ProviderCompanyName = otherDocOperationMode.Id == (int)Domain.Common.OtherDocElecOperationMode.FreeBiller
                                    ? (ConfigurationManager.GetValue("DianBillerName") ?? "DIRECCIÓN DE IMPUESTO Y ADUANAS NACIONALES - DIAN")
                                    : contributor.Name,
                                SoftwareId = operationMode.Software.Id.ToString(),
                                SoftwareName = operationMode.Software.Name,
                                SoftwarePin = operationMode.Software.Pin
                            }
                        });
                    }
                }
                else
                {
                    operationModesList.Add(new Domain.RadianOperationMode { Id = (int)Domain.Common.OtherDocElecOperationMode.OwnSoftware, Name = Domain.Common.OtherDocElecOperationMode.OwnSoftware.GetDescription() });
                    operationModesList.Add(new Domain.RadianOperationMode { Id = (int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider, Name = Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider.GetDescription() });
                    if (!electronicDocumentIsEquivalentDocuments)
                    {
                        var OperationsModes = _othersDocsElecContributorService.GetOperationModes().Where(x => x.Id == 3).FirstOrDefault();
                        operationModesList.Add(new Domain.RadianOperationMode { Id = OperationsModes.Id, Name = OperationsModes.Name });
                    }
                }
            }

            operationModesList = operationModesList.OrderBy(t => t.Id).ToList();
            model.ContributorName = contributor?.Name;
            model.SoftwareId = Guid.NewGuid().ToString();
            model.SoftwareIdPr = model.SoftwareId;

            var technologicalProvidersList = GetTechnologicalProvidersList(model.ElectronicDocumentId, model.OperationModesAsociatedInElectronicBiller);
            ViewBag.ListTechnoProviders = new SelectList(technologicalProvidersList, "Id", "Name");

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


            operationModesList = operationModesList.GroupBy(t => t.Id)
                .Select(t => new Domain.RadianOperationMode()
                {
                    Id = t.Key,
                    Name = t.FirstOrDefault()?.Name ?? "",
                    RadiantContributors = t.FirstOrDefault()?.RadiantContributors
                }).ToList();

            ViewBag.OperationModes = new SelectList(operationModesList, "Id", "Name", operationModesList.FirstOrDefault()?.Id);
            ViewBag.IsSupportDocument = model.ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument;
            ViewBag.IsEquivalentDocument = model.ElectronicDocumentId == (int)ElectronicsDocuments.ElectronicEquivalent;
            ViewBag.IsElectronicPayroll = model.ElectronicDocumentId == (int)ElectronicsDocuments.ElectronicPayroll;
            ViewBag.IsElectronicPayrollNoOfe = model.ElectronicDocumentId == (int)ElectronicsDocuments.ElectronicPayrollNoOFE;
            model.FreeBillerSoftwareId = FreeBillerSoftwareService.Get(model.ElectronicDocumentId);

            return View(model);
        }

        private IEnumerable<ContributorViewModel> GetTechnologicalProvidersList(int electronicDocumentId, List<OperationModeElectronicBillerViewModel> operationModesAsociatedInElectronicBiller)
        {
            bool contributorIsOfe = User.ContributorTypeId() == (int)Domain.Common.ContributorType.Biller;
            bool electronicDocumentIsSupport = electronicDocumentId == (int)ElectronicsDocuments.SupportDocument;
            bool electronicDocumentIsEquivalent = electronicDocumentId == (int)ElectronicsDocuments.ElectronicEquivalent;
            bool electronicDocumentIsElectronicPayrollNoOFE = electronicDocumentId == (int)ElectronicsDocuments.ElectronicPayrollNoOFE;

            List<Contributor> providersList;
            var providersListDto = new List<ContributorViewModel>();

            if (electronicDocumentIsSupport || electronicDocumentIsEquivalent)
            {
                /*Filtrar los proveedores tecnologicos que fueron asociados y están habilitados 
                * en el modo de operación de facturación electrónica*/
                var softwaresIdAssociatedInElectronicBiller = operationModesAsociatedInElectronicBiller
                        .Where(t => t.OperationModeId == (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                        .Select(t => new Guid(t.Data.SoftwareId))
                        .ToList();

                /*proveedores que esten habilitados y que tengan software que esté en produccion y no esté eliminado*/
                providersList = _contributorService.GetContributorsByType((int)Domain.Common.ContributorType.Provider)
                    .Where(x => x.AcceptanceStatusId == (int)ContributorStatus.Enabled &&
                        x.Softwares.Any(t =>
                            t.AcceptanceStatusSoftwareId == (int)SoftwareStatus.Production && !t.Deleted &&
                            (!contributorIsOfe || softwaresIdAssociatedInElectronicBiller.Contains(t.Id))
                        )
                    ).ToList();

                providersListDto.AddRange(providersList.Select(c => new ContributorViewModel { Id = c.Id, Name = c.Name }).ToList());
            }
            else if (electronicDocumentIsElectronicPayrollNoOFE)
            {
                /*Deberá trae el listado de los proveedores tecnológicos autorizados por la DIAN y que
                ya tengan modos de operación nómina electrónica Habilitados como PT - Software Propio,
                ellos están ubicados por la sección de OFES de la plataforma*/
                providersList = _othersDocsElecContributorService
                    .GetTechnologicalProviders(
                        User.ContributorId(),
                        (int)ElectronicsDocuments.ElectronicPayroll,
                        (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider,
                        OtherDocElecState.Habilitado.GetDescription());

                providersListDto.AddRange(providersList.Select(c => new ContributorViewModel { Id = c.Id, Name = c.Name }).ToList());
            }
            else
            {
                providersList = _othersDocsElecContributorService
                    .GetTechnologicalProviders(
                        User.ContributorId(),
                        electronicDocumentId,
                        (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider,
                        OtherDocElecState.Habilitado.GetDescription());

                providersListDto.AddRange(providersList.Select(c => new ContributorViewModel { Id = c.Id, Name = c.Name }).ToList());
            }

            return providersListDto;
        }

        private GlobalTestSetResult GetTestSetResult(List<GlobalTestSetResult> testSetResults, ContributorOperations operation)
        {
            var userContributorTypeId = User.ContributorTypeId();
            var softwareId = GetSoftwareId(operation.SoftwareId);

            var testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{operation.ContributorTypeId}|{softwareId}" && t.OperationModeId == operation.OperationModeId);
            if (testSetResult != null) return testSetResult;

            if (testSetResult == null)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{userContributorTypeId}|{softwareId}" && t.OperationModeId == operation.OperationModeId);
            if (testSetResult == null)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)Domain.Common.ContributorType.Zero}|{operation.SoftwareId}" && t.OperationModeId == operation.OperationModeId);

            if (testSetResult == null && userContributorTypeId == (int)Domain.Common.ContributorType.Provider)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && int.Parse(t.ContributorTypeId) == (int)Domain.Common.ContributorType.Provider && t.SoftwareId == softwareId && t.OperationModeId == operation.OperationModeId);

            if (testSetResult == null && userContributorTypeId == (int)Domain.Common.ContributorType.Provider)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && int.Parse(t.ContributorTypeId) == (int)Domain.Common.ContributorType.Biller && t.SoftwareId == softwareId && t.OperationModeId == operation.OperationModeId);

            return testSetResult;
        }
        private string GetSoftwareId(Guid? softwareId)
        {
            return softwareId != null ? softwareId.ToString() : ConfigurationManager.GetValue("BillerSoftwareId");
        }




        [HttpPost]
        public async Task<ActionResult> AddOrUpdateContributor(OthersElectronicDocumentsViewModel model)
        {
            if (ConfigurationManager.GetValue("Environment") == "Prod")
            {
                return Json(new ResponseMessage(
                    TextResources.OperationNotAllowedInProduction,
                    TextResources.alertType, 500),
                    JsonRequestBehavior.AllowGet);
            }

            bool contributorIsOfe = User.ContributorTypeId() == (int)Domain.Common.ContributorType.Biller;
            bool electronicDocumentIsSupport = model.ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument;

            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;
            var tipo = model.OperationModeId;
            if (model.OperationModeId == 0)
            {
                model.OperationModeId = Int32.Parse(model.OperationModeSelectedId);
            }

            if (model.OperationModeSelectedId == "3")
            {
                var freeBillerSoftwarePin = ConfigurationManager.GetValue("BillerSoftwarePin") ?? "0000";
                Guid g = Guid.NewGuid();
                model.OperationModeId = (int)Domain.Common.OtherDocElecOperationMode.FreeBiller;
                model.SoftwareName = "Solución gratuita";
                model.SoftwareId = !string.IsNullOrWhiteSpace(model.SoftwareIdPr) ? model.SoftwareIdPr : g.ToString();
                model.PinSW = !string.IsNullOrWhiteSpace(model.PinSW) ? model.PinSW : freeBillerSoftwarePin;
            }

            if (model.SoftwareId == null)
            {
                Guid g = Guid.NewGuid();
                model.SoftwareId = !string.IsNullOrWhiteSpace(model.SoftwareIdPr) ? model.SoftwareIdPr : g.ToString();
            }

            GlobalTestSetOthersDocuments testSet = _othersDocsElecContributorService
                .GetTestResult(model.OperationModeId, model.ElectronicDocumentId);

            if (testSet == null)
            {
                return Json(new ResponseMessage(
                    TextResources.ModeElectroniDocWithoutTestSet,
                    TextResources.alertType, 500),
                    JsonRequestBehavior.AllowGet);
            }

            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var item in allErrors) ModelState.AddModelError("", item.ErrorMessage);
                return View("AddOrUpdate", new ValidacionOtherDocsElecViewModel { ContributorId = 1 });
            }

            int providerId = model.ProviderId;
            if (model.OperationModeId != 2)
            {
                providerId = User.ContributorId();
            }

            var now = DateTime.Now;
            string freeBillerSoftwareId = FreeBillerSoftwareService.Get(model.ElectronicDocumentId);

            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService
                .CreateContributorNew(
                    User.ContributorId(),
                    //model.OtherDocElecContributorId,
                    OtherDocElecState.Registrado,
                    model.ContributorIdType,
                    model.OperationModeId,
                    model.ElectronicDocumentId,
                    User.UserName());

            var ContributorId = otherDocElecContributor.Id.ToString();

            OtherDocElecSoftware software = new OtherDocElecSoftware()
            {
                Id = Guid.NewGuid(),
                Url = model.UrlEventReception,
                Name = model.SoftwareName,
                Pin = model.PinSW,
                ProviderId = providerId,
                CreatedBy = User.UserName(),
                Deleted = false,
                Status = true,
                OtherDocElecSoftwareStatusId = (contributorIsOfe && electronicDocumentIsSupport) ? (int)OtherDocElecSoftwaresStatus.Accepted : (int)OtherDocElecSoftwaresStatus.InProcess,
                SoftwareDate = now,
                Timestamp = now,
                Updated = now,
                SoftwareId = model.OperationModeSelectedId != "3" ? new Guid(model.SoftwareId) : new Guid(freeBillerSoftwareId),
                OtherDocElecContributorId = int.Parse(ContributorId)
            };

            OtherDocElecContributorOperations contributorOperation = new OtherDocElecContributorOperations()
            {
                OtherDocElecContributorId = int.Parse(ContributorId),
                OperationStatusId = (contributorIsOfe && electronicDocumentIsSupport) ? (int)OtherDocElecState.Habilitado : (int)OtherDocElecState.Test,
                Deleted = false,
                Timestamp = now,
                SoftwareType = model.OperationModeId,
                SoftwareId = software.Id
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
                return RedirectToAction("AddOrUpdate", new { ElectronicDocumentId = model.ElectronicDocumentId, OperationModeId = 0, ContributorIdType = model.ContributorIdType, ContributorId = model.OtherDocElecContributorId, Message = response.Message });
            }
            else
            {
                if (model.OperationModeSelectedId == "3")
                {
                    Session["ShowFree"] = "1";
                }

                if (electronicDocumentIsSupport)
                {
                    await RegisterNumberingRangeForSupportDocument(contributorOperation, software);
                }
                if (model.ElectronicDocumentIsEquivalent)
                {
                    await RegisterNumberingRangeForEquivalentDocumentPos(contributorOperation, software);
                }
            }

            _othersElectronicDocumentsService
                .ChangeParticipantStatus(
                    contributorId: otherDocElecContributor.Id,
                    newState: (contributorIsOfe && electronicDocumentIsSupport) ? OtherDocElecState.Habilitado.GetDescription() : OtherDocElecState.Test.GetDescription(),
                    ContributorTypeId: model.ContributorIdType,
                    actualState: OtherDocElecState.Registrado.GetDescription(),
                    description: string.Empty);

            if(contributorIsOfe && electronicDocumentIsSupport)
            {
                await SycnToProductionElectronicDocument(otherDocElecContributor, software);
            }

            if (electronicDocumentIsSupport || model.ElectronicDocumentIsEquivalent)
            {
                return RedirectToAction("AddOrUpdate", "OthersElectronicDocuments", new
                {
                    ElectronicDocumentId = model.ElectronicDocumentId,
                    OperationModeId = 0,
                    ContributorIdType = 1,
                    ContributorId = User.ContributorId()
                });
            }

            NotificationsController notification = new NotificationsController();
            int id = model.OperationModeSelectedId == "3" ? 1 : model.OperationModeSelectedId == "2" ? 3 : 2;
            notification.EventNotificationsAsyncOperationMode("04", User.UserCode(), id);

            return RedirectToAction("Index", "OthersElectronicDocAssociated", new { id = contributorOperation.Id });
        }

        private async Task RegisterNumberingRangeForSupportDocument(OtherDocElecContributorOperations contributorOperation, OtherDocElecSoftware software)
        {
            var taskRegisterInCosmos = Task.Run(async () =>
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
                    CreationDate = new DateTime(DateTime.Now.Year, 01, 01),
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
            });

            var taskRegisterInAzureTable = Task.Run(async () =>
            {
                var softwareProvider = _contributorService.Get(software.ProviderId);
                var globalNumerRange = new GlobalNumberRange(
                    User.ContributorCode(), $"SEDS|{((int)DocumentType.DocumentSupportInvoice).ToString().PadLeft(2,'0')}|18760000001")
                {
                    Serie = "SEDS",
                    ResolutionNumber = "18760000001",
                    AssociationDate = DateTime.Now,
                    ResolutionDate = new DateTime(DateTime.Now.Year, 01, 01),
                    ValidDateNumberFrom = int.Parse($"{new DateTime(DateTime.Now.Year, 01, 01):yyyyMMdd}"),
                    ValidDateNumberTo = int.Parse($"{new DateTime(DateTime.Now.Year, 12, 31):yyyyMMdd}"),
                    FromNumber = 984000000,
                    ToNumber = 985000000,
                    SoftwareId = software.SoftwareId.ToString(),
                    SoftwareName = software.Name,
                    SoftwareOwnerCode = softwareProvider.Code,
                    SoftwareOwnerName = softwareProvider.Name,
                    State = (int)NumberRangeState.Authorized,
                };
                await tableManagerNumberRangeManager.InsertOrUpdateAsync(globalNumerRange);
            });

            await taskRegisterInCosmos;
            await taskRegisterInAzureTable;
        }

        private async Task RegisterNumberingRangeForEquivalentDocumentPos(OtherDocElecContributorOperations contributorOperation, OtherDocElecSoftware software)
        {
            var taskRegisterInCosmos = Task.Run(async () =>
            {
                var accountId = await ApiHelpers.ExecuteRequestAsync<string>(ConfigurationManager.GetValue("AccountByNit"), new { Nit = User.ContributorCode() });
                var rangoDePrueba = new NumberingRange
                {
                    id = Guid.NewGuid(),
                    OtherDocElecContributorOperation = contributorOperation.Id,
                    Prefix = "EPOS",
                    ResolutionNumber = "18760000001",
                    NumberFrom = 1,
                    NumberTo = 1000,
                    CurrentNumber = 1,
                    CreationDate = new DateTime(DateTime.Now.Year, 01, 01),
                    ExpirationDate = new DateTime(2030, 01, 01),
                    IdDocumentTypePayroll = "20",
                    DocumentTypePayroll = "Documento Equivalente POS",
                    Current = "EPOS (1 - 1000)",
                    N102 = "EPOS (1 - 1000)",
                    N103 = "EPOS (1 - 1000)",
                    State = 3,
                    AccountId = Guid.Parse(accountId),
                    PartitionKey = accountId.ToString(),
                };
                var cosmosManager = new CosmosDbManagerNumberingRange();
                await cosmosManager.SaveNumberingRange(rangoDePrueba);
            });

            var taskRegisterInAzureTable = Task.Run(async () =>
            {
                var softwareProvider = _contributorService.Get(software.ProviderId);
                var globalNumerRange = new GlobalNumberRange(
                    User.ContributorCode(), $"EPOS|{((int)DocumentType.EquivalentDocumentPOS).ToString().PadLeft(2, '0')}|18760000001")
                {
                    Serie = "EPOS",
                    ResolutionNumber = "18760000001",
                    AssociationDate = DateTime.Now,
                    ResolutionDate = new DateTime(DateTime.Now.Year, 01, 01),
                    ValidDateNumberFrom = int.Parse($"{new DateTime(DateTime.Now.Year, 01, 01):yyyyMMdd}"),
                    ValidDateNumberTo = int.Parse($"{new DateTime(2030, 01, 01):yyyyMMdd}"),
                    FromNumber = 1,
                    ToNumber = 100,
                    SoftwareId = software.SoftwareId.ToString(),
                    SoftwareName = software.Name,
                    SoftwareOwnerCode = softwareProvider.Code,
                    SoftwareOwnerName = softwareProvider.Name,
                    State = (int)NumberRangeState.Authorized,
                };
                await tableManagerNumberRangeManager.InsertOrUpdateAsync(globalNumerRange);
            });

            await taskRegisterInCosmos;
            await taskRegisterInAzureTable;
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
            bool contributorIsOfe = User.ContributorTypeId() == (int)Domain.Common.ContributorType.Biller;
            bool electronicDocumentIsSupport = ValidacionOtherDocs.ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument;
            AuthToken auth = GetAuthData();
            var contributorLoggedByOfe = auth.LoginMenu == "OFE";

            if (ValidacionOtherDocs.Accion == "SeleccionElectronicDocument")
            {
                if (contributorLoggedByOfe && electronicDocumentIsSupport)
                {
                    /*Se debe consultar si el usuario ya tiene un proceso de habilitacion de facturación electronica habilitado*/
                    var contributor = _contributorService.Get(User.ContributorId());
                    var tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
                    var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);
                    var hasTestSetResultsAccepted = testSetResults.Any(t => !t.Deleted && t.Status == (int)TestSetStatus.Accepted);

                    if (!hasTestSetResultsAccepted && contributor.AcceptanceStatusId != (int)ContributorStatus.Enabled)
                    {
                        return Json(new ResponseMessage(TextResources.ElectronicBillerNoEnabled, TextResources.alertType), JsonRequestBehavior.AllowGet);
                    }
                }

                var ResponseMessageRedirectTo = new ResponseMessage("", TextResources.redirectType);
                ResponseMessageRedirectTo.RedirectTo = Url.Action("AddOrUpdate", "OthersElectronicDocuments",
                        new
                        {
                            ElectronicDocumentId = ValidacionOtherDocs.ElectronicDocumentId,
                            OperationModeId = 0,
                            ContributorIdType = 1,
                            ContributorId = User.ContributorId(),
                            message = ""
                        });

                var mode = _othersDocsElecContributorService.GetDocElecContributorsByContributorId(ValidacionOtherDocs.ContributorId)
                    .Where(x => x.ElectronicDocumentId == ValidacionOtherDocs.ElectronicDocumentId && x.OtherDocElecContributorTypeId == 1);// 1 es emisor

                if (!mode.Any())
                {
                    if (contributorLoggedByOfe && electronicDocumentIsSupport)
                    {
                        return Json(ResponseMessageRedirectTo, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new ResponseMessage(TextResources.OthersElectronicDocumentsSelect_Confirm.Replace("@docume", ValidacionOtherDocs.ComplementoTexto), TextResources.confirmType), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (electronicDocumentIsSupport)
                    {
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
                                               ElectronicDocumentId = ValidacionOtherDocs.ElectronicDocumentId,
                                               OperationModeId = (int)ValidacionOtherDocs.OperationModeId,
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
            bool electronicDocumentIsSupport = electronicDocumentId == (int)ElectronicsDocuments.SupportDocument;
            bool electronicDocumentIsElectronicPayrollNoOFE = electronicDocumentId == (int)ElectronicsDocuments.ElectronicPayrollNoOFE;
            List<SoftwareViewModel> softwareList;

            if (electronicDocumentIsSupport)
            {
                var provider = _contributorService.Get(id);
                softwareList = provider.SoftwaresInProduction().Select(s => new SoftwareViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();

                var tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
                // se seleccionan solo los softwares que tenga aceptado su set de pruebas
                var softwareIds = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(provider.Code)
                    .Where(t => !t.Deleted && t.Status == (int)TestSetStatus.Accepted).Select(t => t.SoftwareId);

                softwareList = softwareList.Where(s => softwareIds.Contains(s.Id.ToString())).ToList();
            }
            else if (electronicDocumentIsElectronicPayrollNoOFE)
            {
                softwareList = _othersDocsElecSoftwareService
                    .GetSoftwaresByProviderTechnologicalServices(
                        id,
                        (int)ElectronicsDocuments.ElectronicPayroll,
                        (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider,
                        OtherDocElecState.Habilitado.GetDescription()).Select(s => new SoftwareViewModel
                        {
                            Id = s.SoftwareId,
                            Name = s.Name
                        }).ToList();
            }
            else
            {
                softwareList = _othersDocsElecSoftwareService
                    .GetSoftwaresByProviderTechnologicalServices(id,
                        electronicDocumentId, (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider,
                        OtherDocElecState.Habilitado.GetDescription()).Select(s => new SoftwareViewModel
                        {
                            Id = s.SoftwareId,
                            Name = s.Name
                        }).ToList();
            }

            return Json(new { res = softwareList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetDataBySoftwareId(Guid SoftwareId)
        {
            var software = _othersDocsElecSoftwareService.GetBySoftwareIdV2(SoftwareId);
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
            return Json(new
            {
                response.Code,
                response.data,
                response.Message,
                response.MessageType,
                response.RedirectTo,
                ExistOperationModeAsociated = response.ExistOperationModeAsociated
            }, JsonRequestBehavior.AllowGet);
        }


        public string GetContributorOperation(int code)
        {

            try
            {
                string sqlQuery = "SELECT c.OperationModeId  FROM ContributorOperations C " +
                                      "WHERE C.Contributorid = " + code +
                                      " AND C.Deleted <> 1";

                SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["Dian"]);
                conn.Open();
                DataTable table = new DataTable();
                SqlCommand command = new SqlCommand(sqlQuery, conn);

                using (var da = new SqlDataAdapter(command))
                {
                    da.Fill(table);
                }
                conn.Close();

                var conteo = table.Rows.Count;

                return conteo.ToString();
                //return contributorType;

            }
            catch (Exception exc)
            {
                return null;
            }
        }

        [HttpGet]
        public JsonResult GetTestSetResultAcepted(int otherDocElecContributorOperationsId)
        {
            var response = new List<TestSetResultAceptedModel>();

            var otherDocElecContributorOperation = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationById(otherDocElecContributorOperationsId);

            var testSetResult = _testSetOthersDocumentsResultService.GetTestSetResultAcepted(
                User.ContributorCode(),
                otherDocElecContributorOperation.OtherDocElecContributor.ElectronicDocumentId,
                otherDocElecContributorOperation.OtherDocElecContributorId,
                otherDocElecContributorOperation.SoftwareId.ToString());

            var equivalentElectronicDocuments = _equivalentElectronicDocumentRepository.GetEquivalentElectronicDocuments().ToDictionary(t => t.Id);

            foreach (var test in testSetResult)
            {
                var nameEquivalentElectronicDocument = "";
                if (equivalentElectronicDocuments.ContainsKey(test.EquivalentElectronicDocumentId.Value))
                {
                    nameEquivalentElectronicDocument = equivalentElectronicDocuments[test.EquivalentElectronicDocumentId.Value].Name;
                }

                var testResponse = new TestSetResultAceptedModel()
                {
                    EquivalentElectronicDocumentId = test.EquivalentElectronicDocumentId.Value,
                    EquivalentElectronicDocumentName = nameEquivalentElectronicDocument,
                    EquivalentElectronicDocumentState = test.State == TestSetStatus.Accepted.GetDescription() ? "Habilitado" : test.State
                };

                response.Add(testResponse);
            }


            return Json(response, JsonRequestBehavior.AllowGet);
        }
    
        
        /*********************************/
        private async Task SycnToProductionElectronicDocument(OtherDocElecContributor otherDocContributor, OtherDocElecSoftware software)
        {
            var contributor = _contributorService.Get(otherDocContributor.ContributorId);
            var globalOperation = _globalOtherDocElecOperationService.GetOperation(contributor.Code, software.SoftwareId);
            var testSetResult = _testSetOthersDocumentsResultService.GetTestSetResult(contributor.Code, $"{globalOperation.OperationModeId}|{software.SoftwareId}");

            var request = new OtherDocumentActivationRequest();
            request.Code = contributor.Code;
            request.ContributorId = otherDocContributor.ContributorId;
            request.ContributorTypeId = int.Parse(testSetResult.ContributorTypeId);
            request.Pin = software.Pin;
            request.SoftwareId = software.SoftwareId.ToString();
            request.SoftwareName = software.Name;
            request.SoftwarePassword = software.SoftwarePassword;
            request.SoftwareType = globalOperation.OperationModeId.ToString();
            request.SoftwareUser = software.SoftwareUser;
            request.Url = software.Url;
            request.Enabled = true;
            request.TestSetId = testSetResult.Id;
            request.ContributorOpertaionModeId = globalOperation.OperationModeId;
            request.OtherDocElecContributorId = testSetResult.OtherDocElecContributorId;
            request.ElectronicDocumentId = testSetResult.ElectronicDocumentId;

            var function = ConfigurationManager.GetValue("SendToActivateOtherDocumentContributorUrl");
            var response = await ApiHelpers.ExecuteRequestAsync<GlobalContributorActivation>(function, request);

            if (!response.Success)
            {
                _telemetry.TrackTrace($"Fallo en la sincronización del Code {contributor.Code}:  Mensaje: {response.Message} ", SeverityLevel.Error);
            }
            else
            {
                _telemetry.TrackTrace($"Se sincronizó el Code {contributor.Code}. Mensaje: {response.Message}", SeverityLevel.Verbose);
            }
        }

        private AuthToken GetAuthData()
        {
            var dianAuthTableManager = new TableManager("AuthToken");
            var userIdentificationType = User.IdentificationTypeId();
            var userCode = User.UserCode();
            var partitionKey = $"{userIdentificationType}|{userCode}";
            var contributorCode = User.ContributorCode();
            var auth = dianAuthTableManager.Find<AuthToken>(partitionKey, contributorCode);
            return auth;
        }
    }
}