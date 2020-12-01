using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.RadianApproved;
using Gosocket.Dian.Web.Utils;
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
        private readonly UserService userService = new UserService();

        public RadianApprovedController(IRadianContributorService radianContributorService,
                                        IRadianTestSetService radianTestSetService,
                                        IRadianApprovedService radianAprovedService,
                                        IRadianTestSetResultService radianTestSetResultService)
        {
            _radianContributorService = radianContributorService;
            _radianTestSetService = radianTestSetService;
            _radianAprovedService = radianAprovedService;
            _radianTestSetResultService = radianTestSetResultService;
        }

        [HttpGet]
        public ActionResult Index(RegistrationDataViewModel registrationData)
        {
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(registrationData.ContributorId, (int)registrationData.RadianContributorType);
            List<RadianContributorFileType> listFileType = _radianAprovedService.ContributorFileTypeList((int)registrationData.RadianContributorType);

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
            PagedResult<RadianContributor> customers = _radianAprovedService.CustormerList(radianAdmin.Contributor.RadianContributorId, string.Empty, RadianState.none, 1, 10);
            model.Customers = customers.Results.Select(t => new RadianCustomerViewModel()
            {
                BussinessName = t.Contributor.BusinessName,
                Nit = t.Contributor.Code,
                RadianState = t.RadianState,
                Page = 1,
                Lenght = 10
            }).ToList();
            model.CustomerTotalCount = customers.RowCount;

            if ((int)registrationData.RadianOperationMode == 2)
            {
                if (model.RadianState == "Habilitado")
                {

                    return View(model);
                }
                else
                {
                    Software software = _radianAprovedService.SoftwareByContributor(registrationData.ContributorId);
                    List<Domain.RadianOperationMode> operationModeList = _radianTestSetService.OperationModeList(registrationData.RadianOperationMode);
                    RadianContributorOperationWithSoftware radianContributorOperations = _radianAprovedService.ListRadianContributorOperations(radianAdmin.Contributor.RadianContributorId);
                    RadianApprovedOperationModeViewModel radianApprovedOperationModeViewModel = new RadianApprovedOperationModeViewModel()
                    {
                        Contributor = radianAdmin.Contributor,
                        Software = software,
                        OperationModeList = operationModeList,
                        RadianContributorOperations = radianContributorOperations,
                        CreatedBy = software.CreatedBy,
                        SoftwareId = software.Id,
                        SoftwareUrl = software.Url,
                        OperationModes = new SelectList(operationModeList, "Id", "Name")
                    };
                    return View("GetFactorOperationMode", radianApprovedOperationModeViewModel);
                }
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        public void Add(RegistrationDataViewModel registrationData)
        {
            RadianContributor radianContributor = _radianContributorService.CreateContributor(registrationData.ContributorId,
                                                        RadianState.Registrado,
                                                        registrationData.RadianContributorType,
                                                        registrationData.RadianOperationMode,
                                                        User.UserName());

            if (radianContributor.RadianSoftwares.Count > 0)
            {
                RadianSoftware software = radianContributor.RadianSoftwares.FirstOrDefault();
                RadianContributorOperation radianContributorOperation = new RadianContributorOperation()
                {
                    RadianContributorId = radianContributor.Id,
                    SoftwareId = software.Id,
                    SoftwareType = (int)RadianOperationModeTestSet.OwnSoftware,
                    Timestamp = DateTime.Now
                };
                _radianAprovedService.AddRadianContributorOperation(radianContributorOperation, software.Url, software.Name, software.Pin, User.UserName());
            }
                
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
        public ActionResult GetSetTestResult(RadianApprovedViewModel radianApprovedViewModel)
        {
            radianApprovedViewModel.RadianTestSetResult =
                _radianTestSetResultService.GetTestSetResultByNit(radianApprovedViewModel.Nit).FirstOrDefault();
            return View(radianApprovedViewModel);
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
                SoftwareUrl = software.Url,
                OperationModes = new SelectList(operationModeList, "Id", "Name")
            };

            return View(radianApprovedOperationModeViewModel);
        }

        [HttpPost]
        public JsonResult UpdateFactorOperationMode(SetOperationViewModel data)
        {
            int result = -1;
            result = _radianAprovedService.AddRadianContributorOperation(new RadianContributorOperation()
            {
                RadianContributorId = data.RadianContributorId,
                Deleted = false,
                Timestamp = DateTime.Now,
                SoftwareType = data.SoftwareType,
                SoftwareId = data.SoftwareId != null ? new Guid(data.SoftwareId) : Guid.Empty,
            }, data.Url,
            data.SoftwareName,
            data.Pin,
            User.UserName());

            string message;
            if (result == -1)
                message = TextResources.RequiredSoftware;
            else
                message = result == 0 ? TextResources.ExistingSoftware : TextResources.SuccessSoftware;
            return Json(
                new
                {
                    message,
                    success = true,
                }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetTestDetails(RadianApprovedViewModel radianApprovedViewModel)
        {
            radianApprovedViewModel.RadianTestSetResult =
               _radianTestSetResultService.GetTestSetResultByNit(radianApprovedViewModel.Nit).FirstOrDefault();
            return View(radianApprovedViewModel);
        }

        public JsonResult RadianTestResultByNit(string nit)
        {
            RadianTestSetResult testSetResult = _radianAprovedService.RadianTestSetResultByNit(nit);
            return Json(new { data = testSetResult }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteUser(int id, string newState, int radianContributorTypeId, string radianState, string description)
        {
            _radianContributorService.ChangeParticipantStatus(id, newState, radianContributorTypeId, radianState, description);
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
                message = "Modo de operación eliminado.",
                success = true,
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ViewTestSet(int id, int radianTypeId, string softwareId)
        {
            RadianApprovedViewModel radianApprovedViewModel = new RadianApprovedViewModel();
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(id, radianTypeId);
            radianApprovedViewModel.RadianTestSetResult =
               _radianTestSetResultService.GetTestSetResultByNit(radianAdmin.Contributor.Code).FirstOrDefault();

            radianApprovedViewModel.Contributor = radianAdmin.Contributor;
            radianApprovedViewModel.ContributorId = radianAdmin.Contributor.Id;
            radianApprovedViewModel.Name = radianAdmin.Contributor.TradeName;
            radianApprovedViewModel.Nit = radianAdmin.Contributor.Code;
            radianApprovedViewModel.BusinessName = radianAdmin.Contributor.BusinessName;
            radianApprovedViewModel.Email = radianAdmin.Contributor.Email;
            radianApprovedViewModel.Files = radianAdmin.Files;
            radianApprovedViewModel.RadianState = radianAdmin.Contributor.RadianState;
            radianApprovedViewModel.RadianContributorTypeId = radianAdmin.Contributor.RadianContributorTypeId;

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
            List<RadianSoftware> softwares = _radianAprovedService.SoftwareList(radianContributorId);
            List<AutoListModel> filteredItems = softwares.Select(t => new AutoListModel(t.Id.ToString(), t.Name)).ToList();
            return Json(filteredItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomersList(int radianContributorId, string code, RadianState radianState, int page, int pagesize)
        {
            PagedResult<RadianContributor> customers = _radianAprovedService.CustormerList(radianContributorId, code, radianState, page, pagesize);

            List<RadianCustomerViewModel> customerModel = customers.Results.Select(t => new RadianCustomerViewModel()
            {
                BussinessName = t.Contributor.BusinessName,
                Nit = t.Contributor.Code,
                RadianState = t.RadianState,
                Page = page,
                Lenght = pagesize
            }).ToList();

            RadianApprovedViewModel model = new RadianApprovedViewModel()
            {
                CustomerTotalCount = customers.RowCount,
                Customers = customerModel
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

    }
}