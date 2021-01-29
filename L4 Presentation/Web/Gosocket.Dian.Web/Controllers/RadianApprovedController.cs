using Gosocket.Dian.Application;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.RadianApproved;
using Gosocket.Dian.Web.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianApprovedController : Controller
    {
        private readonly IRadianContributorService _radianContributorService;
        private readonly IRadianTestSetService _radianTestSetService;
        private readonly IRadianApprovedService _radianAprovedService;
        private readonly IRadianTestSetResultService _radianTestSetResultService;
        private readonly IRadianTestSetAppliedService _radianTestSetAppliedService;
        private readonly UserService userService = new UserService();

        public RadianApprovedController(IRadianContributorService radianContributorService,
                                        IRadianTestSetService radianTestSetService,
                                        IRadianApprovedService radianAprovedService,
                                        IRadianTestSetResultService radianTestSetResultService,
                                        IRadianTestSetAppliedService radianTestSetAppliedService)
        {
            _radianContributorService = radianContributorService;
            _radianTestSetService = radianTestSetService;
            _radianAprovedService = radianAprovedService;
            _radianTestSetResultService = radianTestSetResultService;
            _radianTestSetAppliedService = radianTestSetAppliedService;
        }

        [HttpGet]
        public ActionResult Index(RegistrationDataViewModel registrationData)
        {
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(registrationData.ContributorId, (int)registrationData.RadianContributorType);
            List<RadianContributorFileType> listFileType = _radianAprovedService.ContributorFileTypeList((int)registrationData.RadianContributorType);
            if (radianAdmin.Contributor.RadianState == "Cancelado")
            {
                return RedirectToAction("Index", "Radian");
            }
            RadianApprovedViewModel model = new RadianApprovedViewModel()
            {
                Contributor = radianAdmin.Contributor,
                ContributorId = radianAdmin.Contributor.Id,
                Name = radianAdmin.Contributor.TradeName,
                Nit = radianAdmin.Contributor.Code,
                BusinessName = radianAdmin.Contributor.BusinessName,
                Email = radianAdmin.Contributor.Email,
                Files = radianAdmin.Files,
                FilesRequires = listFileType,
                Step = radianAdmin.Contributor.Step,
                RadianState = radianAdmin.Contributor.RadianState,
                RadianContributorTypeId = radianAdmin.Contributor.RadianContributorTypeId,
                LegalRepresentativeList = userService.GetUsers(radianAdmin.LegalRepresentativeIds).Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Code = u.Code,
                    Name = u.Name,
                    Email = u.Email
                }).ToList()
            };

            //aqui se adiciona los clientes asociados.
            PagedResult<RadianCustomerList> customers = _radianAprovedService.CustormerList(radianAdmin.Contributor.RadianContributorId, string.Empty, RadianState.none, 1, 10);
            model.Customers = customers.Results.Select(t => new RadianCustomerViewModel()
            {
                BussinessName = t.BussinessName,
                Nit = t.Nit,
                RadianState = t.RadianState,
                Page = t.Page,
                Lenght = t.Length
            }).ToList();
            model.CustomerTotalCount = customers.RowCount;

            var data = _radianAprovedService.FileHistoryFilter(radianAdmin.Contributor.RadianContributorId, string.Empty, string.Empty, string.Empty, 1, 10);
            FileHistoryListViewModel resultH = new FileHistoryListViewModel()
            {
                Page = 1,
                RowCount = data.RowCount,
                Customers = data.Results.Select(t => new FileHistoryItemViewModel()
                {
                    FileName = t.FileName,
                    Comments = t.Comments,
                    CreatedBy = t.CreatedBy,
                    Status = t.RadianContributorFileStatus?.Name,
                    Updated = t.Timestamp.ToString("yyyy-MM-dd HH:mm")
                }).ToList()
            };
            model.FileHistories = resultH;
            model.FileHistoriesRowCount = data.RowCount;

            if ((int)registrationData.RadianOperationMode == 2)
            {
                if (model.RadianState == "Habilitado")
                    return View(model);
                else
                {
                    Software software = _radianAprovedService.SoftwareByContributor(registrationData.ContributorId);
                    List<Domain.RadianOperationMode> operationModeList = _radianTestSetService.OperationModeList(registrationData.RadianOperationMode);
                    RadianContributorOperationWithSoftware radianContributorOperations = _radianAprovedService.ListRadianContributorOperations(radianAdmin.Contributor.RadianContributorId);
                    RadianApprovedOperationModeViewModel radianApprovedOperationModeViewModel = new RadianApprovedOperationModeViewModel()
                    {
                        Contributor = radianAdmin.Contributor,
                        OperationModeList = operationModeList,
                        OperationModes = new SelectList(operationModeList, "Id", "Name"),
                        RadianContributorOperations = radianContributorOperations,
                        SoftwareUrl = ConfigurationManager.GetValue("WebServiceUrl")
                    };
                    if(software != null)
                    {
                        radianApprovedOperationModeViewModel.Software = software;
                        radianApprovedOperationModeViewModel.CreatedBy = software.CreatedBy;
                        radianApprovedOperationModeViewModel.SoftwareId = software.Id;
                    }
                    return View("GetFactorOperationMode", radianApprovedOperationModeViewModel);
                }
            }
            else
                return View(model);
        }

        [HttpPost]
        public JsonResult Add(RegistrationDataViewModel registrationData)
        {
            RadianTestSet testSet = null;
            if (registrationData.RadianOperationMode == Domain.Common.RadianOperationMode.Direct)
            {
                testSet = _radianAprovedService.GetTestResult(((int)RadianOperationModeTestSet.OwnSoftware).ToString());
                if (testSet == null)
                    return Json(new ResponseMessage(TextResources.ModeWithoutTestSet, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);
            }

            RadianContributor radianContributor = _radianContributorService.CreateContributor(registrationData.ContributorId,
                                                RadianState.Registrado,
                                                registrationData.RadianContributorType,
                                                registrationData.RadianOperationMode,
                                                User.UserName());

            if (registrationData.RadianOperationMode == Domain.Common.RadianOperationMode.Indirect)
                return Json(new ResponseMessage(TextResources.SuccessSoftware, TextResources.alertType), JsonRequestBehavior.AllowGet);

            if (radianContributor.RadianSoftwares == null || radianContributor.RadianSoftwares.Count == 0)
                return Json(new ResponseMessage(TextResources.ParticipantWithoutSoftware, TextResources.alertType, 500), JsonRequestBehavior.AllowGet);

            RadianSoftware software = radianContributor.RadianSoftwares.FirstOrDefault();
            RadianContributorOperation radianContributorOperation = new RadianContributorOperation()
            {
                RadianContributorId = radianContributor.Id,
                SoftwareId = software.Id,
                OperationStatusId = (int)RadianState.Registrado,
                SoftwareType = (int)RadianOperationModeTestSet.OwnSoftware,
                Timestamp = DateTime.Now
            };
            ResponseMessage result = _radianAprovedService.AddRadianContributorOperation(radianContributorOperation, software, testSet, true, false);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UploadFiles()
        {
            string nit = Request.Form.Get("nit");
            string email = Request.Form.Get("email");
            string ContributorId = Request.Form.Get("contributorId");
            string RadianContributorType = Request.Form.Get("radianContributorType");
            string RadianOperationMode = Request.Form.Get("radianOperationMode");
            string filesNumber = Request.Form.Get("filesNumber");
            string step = Request.Form.Get("step");
            string radianState = Request.Form.Get("radianState");
            string radianContributorTypeiD = Request.Form.Get("radianContributorTypeiD");


            ParametersDataViewModel data = new ParametersDataViewModel()
            {
                ContributorId = ContributorId,
                RadianContributorType = RadianContributorType,
                RadianOperationMode = RadianOperationMode
            };

            int idRadianContributor = _radianAprovedService.RadianContributorId(Convert.ToInt32(ContributorId), Convert.ToInt32(radianContributorTypeiD), radianState);
            for (int i = 0; i < Request.Files.Count; i++)
            {
                RadianContributorFile radianContributorFile = new RadianContributorFile();
                RadianContributorFileHistory radianFileHistory = new RadianContributorFileHistory();
                string typeId = Request.Form.Get("TypeId_" + i);

                var file = Request.Files[i];
                radianContributorFile.FileName = file.FileName;
                radianContributorFile.Timestamp = DateTime.Now;
                radianContributorFile.Updated = DateTime.Now;
                radianContributorFile.CreatedBy = email;
                radianContributorFile.RadianContributorId = idRadianContributor;
                radianContributorFile.Deleted = false;
                radianContributorFile.FileType = Convert.ToInt32(typeId);
                radianContributorFile.Status = 1;
                radianContributorFile.Comments = "Comentario";

                ResponseMessage responseUpload = _radianAprovedService.UploadFile(file.InputStream, nit, radianContributorFile);

                if (responseUpload.Message != "")
                {
                    radianFileHistory.FileName = file.FileName;
                    radianFileHistory.Comments = "";
                    radianFileHistory.Timestamp = DateTime.Now;
                    radianFileHistory.CreatedBy = email;
                    radianFileHistory.Status = 1;
                    radianFileHistory.RadianContributorFileId = Guid.Parse(responseUpload.Message);
                    ResponseMessage responseUpdateFileHistory = _radianAprovedService.AddFileHistory(radianFileHistory);
                }
            }
            if (Convert.ToInt32(filesNumber) == Request.Files.Count && Convert.ToInt32(step) != 2)
            {
                int newStep = Convert.ToInt32(step) + 1;
                int contributorId = idRadianContributor;
                ResponseMessage responseUpdateStep = _radianAprovedService.UpdateRadianContributorStep(contributorId, newStep);
            }

            return Json(new
            {
                message = "Datos actualizados correctamente.",
                success = true,
                data
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetSetTestResult(RadianApprovedViewModel model)
        {
            const int softwareType = 1;
            string contributorId = Request.Params["ContributorId"];
            string radianContributorId = Request.Params["Contributor.RadianContributorId"];

            string sType = softwareType.ToString();
            RadianSoftware software = _radianAprovedService.GetSoftware(model.Contributor.RadianContributorId, softwareType);
            string key = softwareType.ToString() + "|" + software.Id.ToString();
            model.RadianTestSetResult = _radianTestSetResultService.GetTestSetResult(model.Nit, key);
            RadianTestSet testSet = _radianTestSetService.GetTestSet(sType, sType);
            model.RadianTestSetResult.OperationModeName = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.RadianOperationModeTestSet), sType)));
            model.RadianTestSetResult.StatusDescription = testSet.Description;
            model.Software = software;
            model.ContributorId = Int32.Parse(contributorId);
            model.Contributor.RadianContributorId = Int32.Parse(radianContributorId);
            return View(model);
        }

        public ActionResult GetSetTestResult(int RadianContributorId, string Nit, string ContributorId)
        {
            RadianApprovedViewModel model = new RadianApprovedViewModel();
            model.Contributor = new RedianContributorWithTypes();
            const int softwareType = 1;
            string sType = softwareType.ToString();
            RadianSoftware software = _radianAprovedService.GetSoftware(RadianContributorId, softwareType);
            string key = softwareType.ToString() + "|" + software.Id.ToString();
            model.RadianTestSetResult = _radianTestSetResultService.GetTestSetResult(Nit, key);
            RadianTestSet testSet = _radianTestSetService.GetTestSet(sType, sType);
            model.RadianTestSetResult.OperationModeName = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.RadianOperationModeTestSet), sType)));
            model.RadianTestSetResult.StatusDescription = testSet.Description;
            model.Software = software;
            model.ContributorId = Int32.Parse(ContributorId);
            model.Contributor.RadianContributorId = RadianContributorId;
            return View(model);
        }

        [HttpPost]
        public JsonResult RestartSetTestResult(RadianTestSetResult result, string softwareId)
        {
            RadianTestSetResult testSetResult = result;
            testSetResult.ApplicationAvailableAccepted = 0;
            testSetResult.ApplicationAvailableRejected = 0;
            testSetResult.AutomaticAcceptanceAccepted = 0;
            testSetResult.AutomaticAcceptanceRejected = 0;
            testSetResult.CirculationLimitationAccepted = 0;
            testSetResult.CirculationLimitationRejected = 0;
            testSetResult.ElectronicMandateAccepted = 0;
            testSetResult.ElectronicMandateRejected = 0;
            testSetResult.TotalElectronicMandateSent = 0;
            testSetResult.TotalEndCirculationLimitationSent = 0;
            testSetResult.TotalEndMandateSent = 0;
            testSetResult.TotalEndorsementCancellationSent = 0;
            testSetResult.TotalEndorsementGuaranteeSent = 0;
            testSetResult.TotalEndorsementProcurementSent = 0;
            testSetResult.TotalEndorsementPropertySent = 0;
            testSetResult.TotalExpressAcceptanceSent = 0;
            testSetResult.TotalGuaranteeSent = 0;
            testSetResult.TotalPaymentNotificationSent = 0;
            testSetResult.TotalReceiptNoticeSent = 0;
            testSetResult.TotalReceiptServiceSent = 0;
            testSetResult.TotalRejectInvoiceSent = 0;
            testSetResult.EndCirculationLimitationAccepted = 0;
            testSetResult.EndCirculationLimitationRejected = 0;
            testSetResult.EndMandateAccepted = 0;
            testSetResult.EndMandateRejected = 0;
            testSetResult.EndorsementCancellationAccepted = 0;
            testSetResult.EndorsementCancellationRejected = 0;
            testSetResult.EndorsementGuaranteeAccepted = 0;
            testSetResult.EndorsementGuaranteeRejected = 0;
            testSetResult.EndorsementProcurementAccepted = 0;
            testSetResult.EndorsementProcurementRejected = 0;
            testSetResult.EndorsementPropertyAccepted = 0;
            testSetResult.EndorsementPropertyRejected = 0;
            testSetResult.ExpressAcceptanceAccepted = 0;
            testSetResult.ExpressAcceptanceRejected = 0;
            testSetResult.GuaranteeAccepted = 0;
            testSetResult.GuaranteeRejected = 0;
            testSetResult.PaymentNotificationAccepted = 0;
            testSetResult.PaymentNotificationRejected = 0;
            testSetResult.ReceiptNoticeAccepted = 0;
            testSetResult.ReceiptNoticeRejected = 0;
            testSetResult.ReceiptServiceAccepted = 0;
            testSetResult.ReceiptServiceRejected = 0;
            testSetResult.RejectInvoiceAccepted = 0;
            testSetResult.RejectInvoiceRejected = 0;
            testSetResult.TotalApplicationAvailableSent = 0;
            testSetResult.TotalAutomaticAcceptanceSent = 0;
            testSetResult.TotalCirculationLimitationSent = 0;
            testSetResult.TotalDocumentAccepted = 0;
            testSetResult.TotalDocumentSent = 0;
            testSetResult.TotalDocumentsRejected = 0;
            testSetResult.ReportForPaymentTotalRequired = 0;
            testSetResult.ReportForPaymentRejected = 0;
            testSetResult.State = "En proceso";
            testSetResult.Status = 0;
            testSetResult.StatusDescription = "En proceso";
            testSetResult.SoftwareId = softwareId;
            testSetResult.SenderCode = result.PartitionKey;
            bool isUpdate = _radianTestSetAppliedService.InsertOrUpdateTestSet(testSetResult) &&  _radianTestSetAppliedService.ResetPreviousCounts(testSetResult.Id);

            ResponseMessage response = new ResponseMessage();
            response.Message = isUpdate ? "Contadores reiniciados correctamente" : "¡Error en la actualización!";
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        

        [HttpPost]
        public ActionResult GetFactorOperationMode(RadianApprovedViewModel radianApprovedViewModel)
        {
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(radianApprovedViewModel.ContributorId, radianApprovedViewModel.RadianContributorTypeId);
            Software software = _radianAprovedService.SoftwareByContributor(radianApprovedViewModel.ContributorId);
            List<Domain.RadianOperationMode> operationModeList = _radianTestSetService.OperationModeList((Domain.Common.RadianOperationMode)radianAdmin.Contributor.RadianOperationModeId);
            RadianContributorOperationWithSoftware radianContributorOperations = _radianAprovedService.ListRadianContributorOperations(radianAdmin.Contributor.RadianContributorId);
            RadianApprovedOperationModeViewModel radianApprovedOperationModeViewModel = new RadianApprovedOperationModeViewModel()
            {
                Contributor = radianAdmin.Contributor,
                Software = software,
                OperationModeList = operationModeList,
                RadianContributorOperations = radianContributorOperations,
                CreatedBy = software.CreatedBy,
                SoftwareId = software.Id,
                SoftwareUrl = ConfigurationManager.GetValue("WebServiceUrl"),
                OperationModes = new SelectList(operationModeList, "Id", "Name")
            };

            return View(radianApprovedOperationModeViewModel);
        }


        //public ActionResult GetFactorOperationModeReturn()
        //{
        //    RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(ContributorId, RadianContributorTypeId);
        //    Software software = _radianAprovedService.SoftwareByContributor(ContributorId);
        //    List<Domain.RadianOperationMode> operationModeList = _radianTestSetService.OperationModeList((Domain.Common.RadianOperationMode)radianAdmin.Contributor.RadianOperationModeId);
        //    RadianContributorOperationWithSoftware radianContributorOperations = _radianAprovedService.ListRadianContributorOperations(radianAdmin.Contributor.RadianContributorId);
        //    RadianApprovedOperationModeViewModel radianApprovedOperationModeViewModel = new RadianApprovedOperationModeViewModel()
        //    {
        //        Contributor = radianAdmin.Contributor,
        //        Software = software,
        //        OperationModeList = operationModeList,
        //        RadianContributorOperations = radianContributorOperations,
        //        CreatedBy = software.CreatedBy,
        //        SoftwareId = software.Id,
        //        SoftwareUrl = ConfigurationManager.GetValue("WebServiceUrl"),
        //        OperationModes = new SelectList(operationModeList, "Id", "Name")
        //    };

        //    return View("GetFactorOperationMode");
        //}

        [HttpPost]
        public JsonResult UpdateFactorOperationMode(SetOperationViewModel data)
        {
            RadianContributor participant = _radianAprovedService.GetRadianContributor(data.RadianContributorId);
            RadianContributorOperation contributorOperation = new RadianContributorOperation()
            {
                RadianContributorId = data.RadianContributorId,
                OperationStatusId = (int)RadianState.Test,
                Deleted = false,
                Timestamp = DateTime.Now,
                SoftwareType = data.SoftwareType,
                SoftwareId = data.SoftwareId != null ? new Guid(data.SoftwareId) : Guid.Empty,
            };
            RadianSoftware software = new RadianSoftware()
            {
                Url = data.Url,
                Name = data.SoftwareName,
                Pin = data.Pin,
                CreatedBy = User.UserName(),
                Deleted = false,
                Status = true,
                RadianSoftwareStatusId = (int)RadianSoftwareStatus.InProcess,
                SoftwareDate = System.DateTime.Now,
                Timestamp = System.DateTime.Now,
                Updated = System.DateTime.Now,
                ContributorId = participant.ContributorId
            };


            RadianTestSet testSet = _radianAprovedService.GetTestResult(data.SoftwareType.ToString());
            ResponseMessage response = _radianAprovedService.AddRadianContributorOperation(contributorOperation, software, testSet, !string.IsNullOrEmpty(data.SoftwareName), true);
            if (response.Code != 500)
            {
                
                if (participant.RadianState != RadianState.Habilitado.GetDescription())
                    _radianContributorService.ChangeParticipantStatus(participant.ContributorId, RadianState.Test.GetDescription(), participant.RadianContributorTypeId, participant.RadianState, string.Empty);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetTestDetails(RadianApprovedViewModel radianApprovedViewModel)
        {
            string contributorId = Request.Params["ContributorId"];
            string radianContributorId = Request.Params["Contributor.RadianContributorId"];
            radianApprovedViewModel.RadianTestSetResult = _radianAprovedService.RadianTestSetResultByNit(radianApprovedViewModel.Nit, radianApprovedViewModel.RadianTestSetResult.Id);
            radianApprovedViewModel.ContributorId = Int32.Parse(contributorId);
            radianApprovedViewModel.Contributor.RadianContributorId = Int32.Parse(radianContributorId);
            return View(radianApprovedViewModel);
        }

        public JsonResult RadianTestResultByNit(string nit, string idTestSet)
        {
            RadianTestSetResult testSetResult = _radianAprovedService.RadianTestSetResultByNit(nit, idTestSet);
            return Json(new { data = testSetResult }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteUser(int id, int radianContributorTypeId, string radianState, string description)
        {
            string state = RadianState.Cancelado.GetDescription();
            _radianContributorService.ChangeParticipantStatus(id, state, radianContributorTypeId, radianState, description);
            return Json(new
            {
                message = "Datos actualizados",
                success = true,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteOperationMode(string Id)
        {
            var result = _radianAprovedService.OperationDelete(Convert.ToInt32(Id));
            return Json(new
            {
                message = result.Message,
                success = true,
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ViewTestSet(int id, int radianTypeId, string softwareId, int softwareType)
        {
            RadianApprovedViewModel radianApprovedViewModel = new RadianApprovedViewModel();
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(id, radianTypeId);
            string key = softwareType.ToString() + "|" + softwareId;
            radianApprovedViewModel.RadianTestSetResult = _radianTestSetResultService.GetTestSetResult(radianAdmin.Contributor.Code, key);
            RadianTestSet testSet = _radianTestSetService.GetTestSet(softwareType.ToString(), softwareType.ToString());

            radianApprovedViewModel.RadianTestSetResult.OperationModeName = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.RadianOperationModeTestSet), softwareType.ToString())));
            radianApprovedViewModel.RadianTestSetResult.StatusDescription = testSet.Description;
            radianApprovedViewModel.Contributor = radianAdmin.Contributor;
            radianApprovedViewModel.ContributorId = radianAdmin.Contributor.Id;
            radianApprovedViewModel.Name = radianAdmin.Contributor.TradeName;
            radianApprovedViewModel.Nit = radianAdmin.Contributor.Code;
            radianApprovedViewModel.BusinessName = radianAdmin.Contributor.BusinessName;
            radianApprovedViewModel.Email = radianAdmin.Contributor.Email;
            radianApprovedViewModel.Files = radianAdmin.Files;
            radianApprovedViewModel.FileTypes = radianAdmin.FileTypes;
            radianApprovedViewModel.RadianState = radianAdmin.Contributor.RadianState;
            radianApprovedViewModel.RadianContributorTypeId = radianAdmin.Contributor.RadianContributorTypeId;
            radianApprovedViewModel.Software = _radianAprovedService.GetSoftware(new Guid(softwareId));

            return View("GetSetTestResult", radianApprovedViewModel);
        }

        public ActionResult AutoCompleteProvider(int contributorId, int contributorTypeId, RadianOperationModeTestSet softwareType, string term)
        {
            List<RadianContributor> softwares = _radianAprovedService.AutoCompleteProvider(contributorId, contributorTypeId, softwareType, term);
            List<AutoListModel> filteredItems = softwares.Select(t => new AutoListModel(t.Id.ToString(), t.Contributor.BusinessName)).ToList();
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SoftwareList(int radianContributorId)
        {
            List<RadianSoftware> softwares = _radianAprovedService.SoftwareList(radianContributorId, RadianSoftwareStatus.Accepted);
            List<AutoListModel> filteredItems = softwares.Select(t => new AutoListModel(t.Id.ToString(), t.Name)).ToList();
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomersList(int radianContributorId, string code, RadianState radianState, int page, int pagesize)
        {
            PagedResult<RadianCustomerList> customers = _radianAprovedService.CustormerList(radianContributorId, code, radianState, page, pagesize);

            List<RadianCustomerViewModel> customerModel = customers.Results.Select(t => new RadianCustomerViewModel()
            {
                BussinessName = t.BussinessName,
                Nit = t.Nit,
                RadianState = t.RadianState,
                Page = t.Page,
                Lenght = t.Length
            }).ToList();

            RadianApprovedViewModel model = new RadianApprovedViewModel()
            {
                CustomerTotalCount = customers.RowCount,
                Customers = customerModel
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        public ActionResult FileHistoyList(FileHistoryFilterViewModel filter)
        {
            PagedResult<RadianContributorFileHistory> data = _radianAprovedService.FileHistoryFilter(filter.RadianContributorId, filter.FileName, filter.Initial, filter.End, filter.Page, filter.PageSize);
            FileHistoryListViewModel result = new FileHistoryListViewModel()
            {
                Page = filter.Page,
                RowCount = data.RowCount,
                Customers = data.Results.OrderByDescending(t=> t.Timestamp).Select(t => new FileHistoryItemViewModel()
                {
                    FileName = t.FileName,
                    Comments = t.Comments,
                    CreatedBy = t.CreatedBy,
                    Status = t.RadianContributorFileStatus?.Name,
                    Updated = t.Timestamp.ToString("yyyy-MM-dd HH:mm")
                }).ToList()
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}