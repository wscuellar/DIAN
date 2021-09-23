using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Gosocket.Dian.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Services.Utils.Helpers;
using Gosocket.Dian.Domain;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;

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
        private readonly IOthersDocsElecSoftwareService _othersDocsElecSoftwareService;
        private readonly ITestSetOthersDocumentsResultService _testSetOthersDocumentsResultService;
        private readonly IGlobalOtherDocElecOperationService _globalOtherDocElecOperationService;
        private readonly TelemetryClient telemetry;

        public OthersElectronicDocAssociatedController(IContributorService contributorService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IOthersElectronicDocumentsService othersElectronicDocumentsService,
            ITestSetOthersDocumentsResultService testSetOthersDocumentsResultService,
            IOthersDocsElecSoftwareService othersDocsElecSoftwareService,
            IGlobalOtherDocElecOperationService globalOtherDocElecOperationService,
            TelemetryClient telemetry
            )
        {
            _contributorService = contributorService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _othersElectronicDocumentsService = othersElectronicDocumentsService;
            _testSetOthersDocumentsResultService = testSetOthersDocumentsResultService;
            _othersDocsElecSoftwareService = othersDocsElecSoftwareService;
            _globalOtherDocElecOperationService = globalOtherDocElecOperationService;
            this.telemetry = telemetry;
        }

        private OthersElectronicDocAssociatedViewModel DataAssociate(int Id)
        {
            var operation = this._othersElectronicDocumentsService.GetOtherDocElecContributorOperationById(Id);

            List<UserViewModel> LegalRepresentativeList = new List<UserViewModel>();
            OtherDocsElectData entity = _othersDocsElecContributorService.GetCOntrinutorODE(operation.OtherDocElecContributorId);
            if (entity == null)
            {
                return new OthersElectronicDocAssociatedViewModel()
                {
                    Id = -1,
                };
            }
            var contributor = _contributorService.GetContributorById(entity.ContributorId, entity.ContributorTypeId);

            if (contributor == null)
            {
                return new OthersElectronicDocAssociatedViewModel()
                {
                    Id = -2,
                };
            }

            if (entity.Step == 3)
            {
                LegalRepresentativeList = userService.GetUsers(entity.LegalRepresentativeIds).Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Code = u.Code,
                    Name = u.Name,
                    Email = u.Email
                }).ToList();
            }

            return new OthersElectronicDocAssociatedViewModel()
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
                ContributorType = entity.ContributorType,
                ContributorTypeId = entity.ContributorTypeId,
                SoftwareId = entity.SoftwareId,
                SoftwareIdBase = entity.SoftwareIdBase,
                ProviderId = entity.ProviderId,
                LegalRepresentativeList = LegalRepresentativeList
            };

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
            
            OthersElectronicDocAssociatedViewModel model = DataAssociate(Id);

            if (model.Id == -1)
                return RedirectToAction("Index", "OthersElectronicDocuments");

            ViewBag.ValidateRequest = true;

            if (model.Id == -2)
            {
                ViewBag.ValidateRequest = false;
                ModelState.AddModelError("", "No existe contribuyente!");
                return View(new OthersElectronicDocAssociatedViewModel());
            }

            if (model.Step == 3 && model.ContributorTypeId == (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider)
            {
                PagedResult<OtherDocElecCustomerList> customers = _othersElectronicDocumentsService.CustormerList(User.ContributorId(), string.Empty, OtherDocElecState.none, 1, 10);

                model.Customers = customers.Results.Select(t => new OtherDocElecCustomerListViewModel()
                {
                    BussinessName = t.BussinessName,
                    Nit = t.Nit,
                    State = t.State,
                    Page = t.Page,
                    Lenght = t.Length
                }).ToList();
                model.CustomerTotalCount = customers.RowCount;
            }
            else
            {
                model.Customers = new List<OtherDocElecCustomerListViewModel>();
                model.CustomerTotalCount = 0;
            }

            ViewBag.Id = Id;

            return View(model);
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
            var result = DeleteOperationInStorageTable(id);
            if (result != null) return result;

            // SQL
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

        public ActionResult GetSetTestResult(int Id)
        {
            OthersElectronicDocAssociatedViewModel model = DataAssociate(Id);

            if (model.Id == -1)
                return RedirectToAction("Index", "OthersElectronicDocuments");

            ViewBag.ValidateRequest = true;

            if (model.Id == -2)
            {
                ViewBag.ValidateRequest = false;
                ModelState.AddModelError("", "No existe contribuyente!");
                return View(new OthersElectronicDocAssociatedViewModel());
            }

            GlobalTestSetOthersDocuments testSet = null;

            testSet = _othersDocsElecContributorService.GetTestResult((int)model.OperationModeId, model.ElectronicDocId);
            if (testSet == null)
                return Json(new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);

            var operation = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationById(Id);

            //ViewBag.TestSetId = testSet.TestSetId;
            //OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(Guid.Parse(model.SoftwareId));
            OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(operation.SoftwareId);

            string key = model.OperationModeId.ToString() + "|" + software.SoftwareId.ToString();
            model.GTestSetOthersDocumentsResult = _testSetOthersDocumentsResultService.GetTestSetResult(model.Nit, key);

            ViewBag.TestSetId = model.GTestSetOthersDocumentsResult.Id;

            model.GTestSetOthersDocumentsResult.OperationModeName = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.OtherDocElecOperationMode), model.OperationModeId.ToString())));
            model.GTestSetOthersDocumentsResult.StatusDescription = testSet.Description;
            model.Software = new OtherDocElecSoftwareViewModel()
            {
                Id = software.Id,
                Name = software.Name,
                Pin = software.Pin,
                Url = software.Url,
                Status = software.Status,
                OtherDocElecSoftwareStatusId = software.OtherDocElecSoftwareStatusId,
                //OtherDocElecSoftwareStatusName = _othersDocsElecSoftwareService.GetSoftwareStatusName(software.OtherDocElecSoftwareStatusId),
                OtherDocElecSoftwareStatusName = model.GTestSetOthersDocumentsResult.State,
                ProviderId = software.ProviderId,
                SoftwareId = software.SoftwareId,
            };

            model.EsElectronicDocNomina = model.ElectronicDocId == (int)Domain.Common.ElectronicsDocuments.ElectronicPayroll;
            model.TitleDoc1 = model.EsElectronicDocNomina ? "Nomina Electrónica" : model.ElectronicDoc;
            model.TitleDoc2 = model.EsElectronicDocNomina ? "Nomina electrónica de Ajuste" : "";

            ViewBag.Id = Id;

            return View(model);
        }

        [HttpPost]
        public ActionResult SetTestDetails(int Id)
        {
            OthersElectronicDocAssociatedViewModel model = DataAssociate(Id);

            if (model.Id == -1)
                return RedirectToAction("Index", "OthersElectronicDocuments");

            if (model.Id == -2)
            {
                ViewBag.ValidateRequest = false;
                ModelState.AddModelError("", "No existe contribuyente!");
                return View(new OthersElectronicDocAssociatedViewModel());
            }

            var operation = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationById(Id);

            //OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(Guid.Parse(model.SoftwareId));
            OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(operation.SoftwareId);

            string key = model.OperationModeId.ToString() + "|" + software.SoftwareId.ToString();
            model.GTestSetOthersDocumentsResult = _testSetOthersDocumentsResultService.GetTestSetResult(model.Nit, key);

            model.EsElectronicDocNomina = model.ElectronicDocId == (int)Domain.Common.ElectronicsDocuments.ElectronicPayroll;
            model.TitleDoc1 = model.EsElectronicDocNomina ? "Nomina Electrónica" : model.ElectronicDoc;
            model.TitleDoc2 = model.EsElectronicDocNomina ? "Nomina electrónica de Ajuste" : "";

            GlobalTestSetOthersDocuments testSet = _othersDocsElecContributorService.GetTestResult((int)model.OperationModeId, model.ElectronicDocId);
            //ViewBag.TestSetId = (testSet != null) ? testSet.TestSetId : string.Empty;
            ViewBag.TestSetId = model.GTestSetOthersDocumentsResult.Id;

            //var softwareStatusName = string.Empty;

            //if(software != null) softwareStatusName = _othersDocsElecSoftwareService.GetSoftwareStatusName(software.OtherDocElecSoftwareStatusId);
            ViewBag.OtherDocElecSoftwareStatusName = model.GTestSetOthersDocumentsResult.State;

            ViewBag.Id = Id;

            return View(model);
        }

        public ActionResult CustomersList(int ContributorId, string code, OtherDocElecState State, int page, int pagesize)
        {
            PagedResult<OtherDocElecCustomerList> customers = _othersElectronicDocumentsService.CustormerList(User.ContributorId(), code, State, page, pagesize);

            List<OtherDocElecCustomerListViewModel> customerModel = customers.Results.Select(t => new OtherDocElecCustomerListViewModel()
            {
                BussinessName = t.BussinessName,
                Nit = t.Nit,
                State = t.State,
                Page = t.Page,
                Lenght = t.Length
            }).ToList();

            OthersElectronicDocAssociatedViewModel model = new OthersElectronicDocAssociatedViewModel()
            {
                CustomerTotalCount = customers.RowCount,
                Customers = customerModel
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetupOperationMode(int Id)
        {
            OthersElectronicDocAssociatedViewModel entity = DataAssociate(Id);

            if (entity.Id == -1)
                return RedirectToAction("Index", "OthersElectronicDocuments");

            if (entity.Id == -2)
            {
                ViewBag.ValidateRequest = false;
                ModelState.AddModelError("", "No existe contribuyente!");
                return View(new OthersElectronicDocAssociatedViewModel());
            }

            OtherDocElecSetupOperationModeViewModel model = new OtherDocElecSetupOperationModeViewModel();
            var operationModesList = new List<SelectListItem>();

            if (entity.ContributorTypeId == (int)Domain.Common.OtherDocElecContributorType.Transmitter) // emisor
            {
                operationModesList.Add(new SelectListItem { Value = ((int)Domain.Common.OtherDocElecOperationMode.OwnSoftware).ToString(), Text = Domain.Common.OtherDocElecOperationMode.OwnSoftware.GetDescription() });
                operationModesList.Add(new SelectListItem { Value = ((int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider).ToString(), Text = Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider.GetDescription() });
            }
            else
            {
                operationModesList.Add(new SelectListItem { Value = ((int)Domain.Common.OtherDocElecOperationMode.OwnSoftware).ToString(), Text = Domain.Common.OtherDocElecOperationMode.OwnSoftware.GetDescription() });
            }

            var providersList = new List<ContributorViewModel>();
            var contributorsList = _othersDocsElecContributorService.GetTechnologicalProviders(User.ContributorId(), entity.ElectronicDocId, (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider, OtherDocElecState.Habilitado.GetDescription());
            if (contributorsList != null)
                providersList.AddRange(contributorsList.Select(c => new ContributorViewModel { Id = c.Id, Name = c.Name }).ToList());

            ViewBag.ListTechnoProviders = new SelectList(providersList, "Id", "Name");
            ViewBag.ListSoftwares = new List<SoftwareViewModel>();

            model.Id = entity.Id;
            model.ContributorId = entity.ContributorId;
            model.OperationMode = entity.OperationMode;
            model.OperationModeId = entity.OperationModeId;
            model.ElectronicDoc = entity.ElectronicDoc;
            model.ElectronicDocId = entity.ElectronicDocId;
            model.OperationModeList = operationModesList;
            model.ContributorType = entity.ContributorType;
            model.ContributorTypeId = entity.ContributorTypeId;
            model.SoftwareUrl = ConfigurationManager.GetValue("WebServiceUrl");
            model.SoftwareId = Guid.NewGuid();
            model.SoftwareIdBase = entity.SoftwareIdBase;
            model.ProviderId = entity.ProviderId;
            model.Provider = _contributorService.GetContributorById(model.ProviderId, entity.ContributorTypeId).Name;

            PagedResult<OtherDocsElectData> List = _othersDocsElecContributorService.List2(User.ContributorId());

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

            ViewBag.Id = Id;

            return View(model);
        }

        [HttpPost]
        public JsonResult SetupOperationModePost(OtherDocElecSetupOperationModeViewModel model)
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;
            
            GlobalTestSetOthersDocuments testSet = null;

            testSet = _othersDocsElecContributorService.GetTestResult((int)model.OperationModeId, model.ElectronicDocId);
            if (testSet == null)
                return Json(new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);

            if (_othersDocsElecContributorService.ValidateSoftwareActive(User.ContributorId(), (int)model.ContributorTypeId, (int)model.OperationModeId, (int)OtherDocElecSoftwaresStatus.InProcess))
                return Json(new ResponseMessage(TextResources.OperationFailOtherInProcess, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);

            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(User.ContributorId(),
                                              OtherDocElecState.Registrado,
                                              model.ContributorTypeId,
                                              model.OperationModeId,
                                              model.ElectronicDocId,
                                              User.UserName());

            int providerId = model.ProviderId;
            if (model.OperationModeId != 2) providerId = User.ContributorId();

            var IdS = Guid.NewGuid();
            var now = DateTime.Now;
            OtherDocElecSoftware software = new OtherDocElecSoftware()
            {
                Id = IdS,
                Url = model.SoftwareUrl,
                Name = model.SoftwareName,
                Pin = model.SoftwarePin,
                ProviderId = providerId,
                CreatedBy = User.UserName(),
                Deleted = false,
                Status = true,
                OtherDocElecSoftwareStatusId = (int)OtherDocElecSoftwaresStatus.InProcess,
                SoftwareDate = now,
                Timestamp = now,
                Updated = now,
                SoftwareId = model.SoftwareId,
                OtherDocElecContributorId = otherDocElecContributor.Id
            };

            OtherDocElecContributorOperations contributorOperation = new OtherDocElecContributorOperations()
            {
                OtherDocElecContributorId = otherDocElecContributor.Id,
                OperationStatusId = (int)OtherDocElecState.Test,
                Deleted = false,
                Timestamp = now,
                SoftwareType = model.OperationModeId,
                SoftwareId = IdS
            };

            ResponseMessage response = _othersElectronicDocumentsService.AddOtherDocElecContributorOperation(contributorOperation, software, true, true);
            if (response.Code != 500)
            {
                _othersElectronicDocumentsService.ChangeParticipantStatus(otherDocElecContributor.Id, OtherDocElecState.Test.GetDescription(), model.ContributorTypeId, OtherDocElecState.Registrado.GetDescription(), string.Empty);
            }

            //if (response.Code != 500)
            //{
            //    RadianContributor participant = _radianAprovedService.GetRadianContributor(data.RadianContributorId);
            //    if (participant.RadianState != RadianState.Habilitado.GetDescription())
            //        _radianContributorService.ChangeParticipantStatus(participant.ContributorId, RadianState.Test.GetDescription(), participant.RadianContributorTypeId, participant.RadianState, string.Empty);
            //}

            response.Message = TextResources.OtherDocEleSuccesModeOperation;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult DeleteOperationMode(int Id)
        {
            var result = DeleteOperationInStorageTable(Id);
            if (result != null) return result;

            var _result = _othersElectronicDocumentsService.OperationDelete(Id);
            return Json(new
            {
                code = _result.Code,
                message = _result.Message,
                success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        private JsonResult DeleteOperationInStorageTable(int id)
        {
            var operation = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationById(id);
            if (operation != null && operation.OperationStatusId == (int)OtherDocElecState.Habilitado)
            {
                return Json(new
                {
                    code = 500,
                    message = $"Modo de operación se encuentra en estado '{ OtherDocElecState.Habilitado.GetDescription() }', no se permite eliminar.",
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }

            OthersElectronicDocAssociatedViewModel model = DataAssociate(id);
            OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(operation.SoftwareId);

            // AZURE
            string key = model.OperationModeId.ToString() + "|" + software.SoftwareId.ToString();
            var globalResult = _testSetOthersDocumentsResultService.GetTestSetResult(model.Nit, key);
            if (globalResult != null)
            {
                globalResult.Deleted = true;
                _testSetOthersDocumentsResultService.InsertTestSetResult(globalResult);
            }
            //
            var globalOperation = _globalOtherDocElecOperationService.GetOperation(model.Nit, software.SoftwareId);
            if (globalOperation != null)
            {
                globalOperation.Deleted = true;
                _globalOtherDocElecOperationService.Update(globalOperation);
            }

            return null;
        }

        [HttpPost]
        public JsonResult RestartSetTestResult(GlobalTestSetOthersDocumentsResult model, Guid docElecSoftwareId)
        {
            model.State = TestSetStatus.InProcess.GetDescription();
            model.Status = (int)TestSetStatus.InProcess;
            model.StatusDescription = TestSetStatus.InProcess.GetDescription();

            // Totales Generales
            model.TotalDocumentSent = 0;
            model.TotalDocumentAccepted = 0;
            model.TotalDocumentsRejected = 0;
            // EndTotales Generales

            // OthersDocuments
            model.TotalOthersDocumentsSent = 0;
            model.OthersDocumentsAccepted = 0;
            model.OthersDocumentsRejected = 0;
            //End OthersDocuments

            //ElectronicPayrollAjustment
            model.TotalElectronicPayrollAjustmentSent = 0;
            model.ElectronicPayrollAjustmentAccepted = 0;
            model.ElectronicPayrollAjustmentRejected = 0;
            //EndElectronicPayrollAjustment

            bool isUpdate = _testSetOthersDocumentsResultService.InsertTestSetResult(model);
            if(isUpdate)
            {
                // OtherDocElecContributor
                //var operationModeId = int.Parse(model.RowKey.Split("|".ToCharArray())[0]);
                //this._othersDocsElecContributorService.CreateContributor(int.Parse(model.PartitionKey),
                //    OtherDocElecState.Test,
                //    int.Parse(model.ContributorTypeId),
                //    operationModeId, 
                //    model.ElectronicDocumentId,
                //    User.UserName());

                // OtherDocElecSoftware
                //var software = _othersDocsElecSoftwareService.Get(docElecSoftwareId);
                //software.OtherDocElecSoftwareStatusId = (int)OtherDocElecSoftwaresStatus.InProcess;
                //_othersDocsElecSoftwareService.CreateSoftware(software);

                // OtherDocElecContributorOperations
                var softwareOperation = _othersElectronicDocumentsService.GetOtherDocElecContributorOperationBySoftwareId(docElecSoftwareId);
                softwareOperation.OperationStatusId = (int)OtherDocElecState.Test;
                _othersElectronicDocumentsService.UpdateOtherDocElecContributorOperation(softwareOperation);
            }

            ResponseMessage response = new ResponseMessage();
            response.Message = isUpdate ? "Contadores reiniciados correctamente" : "¡Error en la actualización!";
            return Json(response, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SyncToProduction(int code, int contributorId, string softwareId, string softwareIdBase)
        {
            try
            {
                //TODO afinar filtro
                OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(Guid.Parse(softwareId));

                if (software == null)
                {
                    telemetry.TrackTrace($"Fallo en la sincronización del Code {code}:  Mensaje: No se encontró el softwareid {softwareId} ", SeverityLevel.Warning);
                    return Json(new
                    {
                        success = false,
                        message = $"No se pudo localizar el Software con el id {softwareId}"
                    }, JsonRequestBehavior.AllowGet);
                }

                var globalRadianOperations = _globalOtherDocElecOperationService.GetOperation(code.ToString(), Guid.Parse(softwareIdBase));

                if (globalRadianOperations == null)
                {
                    telemetry.TrackTrace($"Fallo en la sincronización del Code {code}:  Mensaje: No se encontró operación. code: {code} - softwareid {software.SoftwareId} ", SeverityLevel.Warning);
                    return Json(new
                    {
                        success = false,
                        message = $"No se encontró operación para el softwareid {software.SoftwareId}"
                    }, JsonRequestBehavior.AllowGet);
                }


                var pk = code.ToString();
                var rk = globalRadianOperations.OperationModeId + "|" + softwareIdBase;
                GlobalTestSetOthersDocumentsResult testSetResult = _testSetOthersDocumentsResultService.GetTestSetResult(pk, rk);

                if (testSetResult == null)
                {
                    telemetry.TrackTrace($"Fallo en la sincronización del Code {code}:  Mensaje: No se encontró el testSetResult pk {pk} - rk {rk} ", SeverityLevel.Warning);
                    return Json(new
                    {
                        success = false,
                        message = $"No se encontró el testSetResult pk {pk} - rk {rk} "
                    }, JsonRequestBehavior.AllowGet);
                }




                var data = new OtherDocumentActivationRequest();
                data.Code = code.ToString();
                data.ContributorId = contributorId;
                data.ContributorTypeId = int.Parse(testSetResult.ContributorTypeId);
                data.Pin = software.Pin;
                data.SoftwareId = softwareIdBase;
                data.SoftwareName = software.Name;
                data.SoftwarePassword = software.SoftwarePassword;
                data.SoftwareType = globalRadianOperations.OperationModeId.ToString();
                data.SoftwareUser = software.SoftwareUser;                
                data.Url = software.Url;                                
                data.Enabled = true;


                var function = ConfigurationManager.GetValue("SendToActivateOtherDocumentContributorUrl");
                var response = ApiHelpers.ExecuteRequest<GlobalContributorActivation>(function, data);

                if (!response.Success) {
                    telemetry.TrackTrace($"Fallo en la sincronización del Code {code}:  Mensaje: {response.Message} ", SeverityLevel.Error);
                    return Json(new
                    {
                        success = false,
                        message = response.Message
                    }, JsonRequestBehavior.AllowGet);
                }
                telemetry.TrackTrace($"Se sincronizó el Code {code}. Mensaje: {response.Message}", SeverityLevel.Verbose);
                return Json(new
                {
                    success = true,
                    message = response.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

    
    }
    class OtherDocumentActivationRequest
    {

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "contributorId")]
        public int ContributorId { get; set; }

        [JsonProperty(PropertyName = "contributorTypeId")]
        public int ContributorTypeId { get; set; }

        [JsonProperty(PropertyName = "softwareId")]
        public string SoftwareId { get; set; }

        [JsonProperty(PropertyName = "softwareType")]
        public string SoftwareType { get; set; }

        [JsonProperty(PropertyName = "softwareUser")]
        public string SoftwareUser { get; set; }

        [JsonProperty(PropertyName = "softwarePassword")]
        public string SoftwarePassword { get; set; }

        [JsonProperty(PropertyName = "pin")]
        public string Pin { get; set; }

        [JsonProperty(PropertyName = "softwareName")]
        public string SoftwareName { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "testSetId")]
        public string TestSetId { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

    }
}