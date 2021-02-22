﻿using Gosocket.Dian.Application;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Gosocket.Dian.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gosocket.Dian.Web.Common;

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

        public OthersElectronicDocAssociatedController(IContributorService contributorService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IOthersElectronicDocumentsService othersElectronicDocumentsService,
            ITestSetOthersDocumentsResultService testSetOthersDocumentsResultService,
            IOthersDocsElecSoftwareService othersDocsElecSoftwareService)
        {
            _contributorService = contributorService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _othersElectronicDocumentsService = othersElectronicDocumentsService;
            _testSetOthersDocumentsResultService = testSetOthersDocumentsResultService;
            _othersDocsElecSoftwareService = othersDocsElecSoftwareService;
        }


        private OthersElectronicDocAssociatedViewModel DataAssociate(int Id)
        {
            List<UserViewModel> LegalRepresentativeList = new List<UserViewModel>();
            OtherDocsElectData entity = _othersDocsElecContributorService.GetCOntrinutorODE(Id);
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

            if (model.Step == 3 && model.ContributorTypeId == 2)
            {
                PagedResult<OtherDocElecCustomerList> customers = _othersElectronicDocumentsService.CustormerList(model.Id, string.Empty, OtherDocElecState.none, 1, 10);

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

            ViewBag.TestSetId = testSet.TestSetId;
            OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(Guid.Parse(model.SoftwareId));

            string key = model.OperationModeId.ToString() + "|" + model.SoftwareId.ToString();
            model.GTestSetOthersDocumentsResult = _testSetOthersDocumentsResultService.GetTestSetResult(model.Nit, key);

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
                OtherDocElecSoftwareStatusName = _othersDocsElecSoftwareService.GetSoftwareStatusName(software.OtherDocElecSoftwareStatusId),
                ProviderId = software.ProviderId,
                SoftwareId = software.SoftwareId,
            };
            
            model.EsElectronicDocNomina = model.ElectronicDocId == (int)Domain.Common.ElectronicsDocuments.ElectronicPayroll;
            model.TitleDoc1 = model.EsElectronicDocNomina ? "Nomina Electrónica" : model.ElectronicDoc;
            model.TitleDoc2 = model.EsElectronicDocNomina ? "Nomina electrónica de Ajuste" : "";

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

            string key = model.OperationModeId.ToString() +  "|" + model.SoftwareId.ToString();
            model.GTestSetOthersDocumentsResult = _testSetOthersDocumentsResultService.GetTestSetResult(model.Nit, key);

            model.EsElectronicDocNomina = model.ElectronicDocId == (int)Domain.Common.ElectronicsDocuments.ElectronicPayroll;
            model.TitleDoc1 = model.EsElectronicDocNomina ? "Nomina Electrónica" : model.ElectronicDoc;
            model.TitleDoc2 = model.EsElectronicDocNomina ? "Nomina electrónica de Ajuste" : "";

            GlobalTestSetOthersDocuments testSet = _othersDocsElecContributorService.GetTestResult((int)model.OperationModeId, model.ElectronicDocId);
            ViewBag.TestSetId = (testSet != null) ? testSet.TestSetId : string.Empty;

            var softwareStatusName = string.Empty;
            OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(Guid.Parse(model.SoftwareId));
            if(software != null) softwareStatusName = _othersDocsElecSoftwareService.GetSoftwareStatusName(software.OtherDocElecSoftwareStatusId);
            ViewBag.OtherDocElecSoftwareStatusName = softwareStatusName;

            return View(model);
        }

        public ActionResult CustomersList(int ContributorId, string code, OtherDocElecState State, int page, int pagesize)
        {
            PagedResult<OtherDocElecCustomerList> customers = _othersElectronicDocumentsService.CustormerList(ContributorId, code, State, page, pagesize);

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

            List<SelectListItem> OperationsModes = _othersDocsElecContributorService.GetOperationModes().Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            model.Id = entity.Id;
            model.ContributorId = entity.ContributorId;
            model.OperationMode = entity.OperationMode;
            model.OperationModeId = entity.OperationModeId;
            model.ElectronicDoc = entity.ElectronicDoc;
            model.ElectronicDocId = entity.ElectronicDocId;
            model.OperationModeList = OperationsModes;
            model.ContributorType = entity.ContributorType;
            model.ContributorTypeId = entity.ContributorTypeId;
            model.SoftwareUrl = ConfigurationManager.GetValue("WebServiceUrl");
            model.SoftwareId = Guid.NewGuid();
            model.SoftwareIdBase = entity.SoftwareIdBase;
            model.ProviderId = entity.ProviderId;
            model.Provider = _contributorService.GetContributorById(model.ProviderId, entity.ContributorTypeId).Name;

            PagedResult<OtherDocsElectData> List = _othersDocsElecContributorService.List(User.UserCode(), 0);
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

            OtherDocElecContributor otherDocElecContributor = _othersDocsElecContributorService.CreateContributor(User.UserCode(),
                                              OtherDocElecState.Registrado,
                                              model.ContributorTypeId,
                                              model.OperationModeId,
                                              model.ElectronicDocId,
                                              User.UserName());

            OtherDocElecSoftware software = new OtherDocElecSoftware()
            {
                Id = model.SoftwareId,
                Url = model.SoftwareUrl,
                Name = model.SoftwareName,
                Pin = model.SoftwarePin,
                ProviderId = model.ProviderId,
                CreatedBy = User.UserName(),
                Deleted = false,
                Status = true,
                OtherDocElecSoftwareStatusId = (int)OtherDocElecSoftwaresStatus.InProcess,
                SoftwareDate = DateTime.Now,
                Timestamp = DateTime.Now,
                Updated = DateTime.Now,
                SoftwareId = model.SoftwareIdBase,
                OtherDocElecContributorId = otherDocElecContributor.Id
            };
            OtherDocElecContributorOperations contributorOperation = new OtherDocElecContributorOperations()
            {
                OtherDocElecContributorId = otherDocElecContributor.Id,
                OperationStatusId = (int)OtherDocElecState.Test,
                Deleted = false,
                Timestamp = DateTime.Now,
                SoftwareType = model.OperationModeId,
                SoftwareId = model.SoftwareId
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
        public JsonResult DeleteOperationMode(string Id)
        {
            var result = _othersElectronicDocumentsService.OperationDelete(Convert.ToInt32(Id));
            return Json(new
            {
                message = result.Message,
                success = true,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}