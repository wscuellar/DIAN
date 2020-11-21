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
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(registrationData.ContributorId);
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
            if (Request.Params.Get("RadianOperationMode") == "Indirect")
            {
                if (model.RadianState == "Habilitado")
                    return View("GetFactorOperationMode", model);
                else
                {
                    model.RadianTestSetResult =
                    _radianTestSetResultService.GetTestSetResultByNit(radianAdmin.Contributor.Code).FirstOrDefault();
                    return View("GetSetTestResult", model);
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
            _radianContributorService.CreateContributor(registrationData.ContributorId,
                                                        RadianState.Registrado,
                                                        registrationData.RadianContributorType,
                                                        registrationData.RadianOperationMode,
                                                        User.UserName());




        }

        private void LoadSoftwareModeOperation()
        {
            List<Domain.RadianOperationMode> list = _radianTestSetService.OperationModeList();
            ViewBag.RadianSoftwareOperationMode = list;
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
            if (Convert.ToInt32(filesNumber) == Request.Files.Count)
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
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(radianApprovedViewModel.ContributorId);
            Software software = _radianAprovedService.SoftwareByContributor(radianApprovedViewModel.ContributorId);
            List<Domain.RadianOperationMode> operationModeList = _radianTestSetService.OperationModeList();
            RadianContributorOperationWithSoftware radianContributorOperations = _radianAprovedService.ListRadianContributorOperations(radianApprovedViewModel.ContributorId);

            foreach (RadianContributorOperation co in radianContributorOperations.RadianContributorOperations)
                co.RadianOperationMode = operationModeList.FirstOrDefault(o => o.Id == co.RadianOperationModeId);

            RadianApprovedOperationModeViewModel radianApprovedOperationModeViewModel = new RadianApprovedOperationModeViewModel()
            {
                Contributor = radianAdmin.Contributor,
                Software = software,
                OperationModeList = operationModeList,
                RadianContributorOperations = radianContributorOperations,
                CreatedBy = software.CreatedBy,
                SoftwareId = software.Id,
                SoftwareUrl = software.Url,
                OperationModeSelected = operationModeList.FirstOrDefault(o => o.Name.Equals("Software propio"))
            };

            return View(radianApprovedOperationModeViewModel);
        }

        [HttpPost]
        public JsonResult UploadFactorOperationMode(RadianApprovedOperationModeViewModel approvedOperModeViewModel)
        {
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(approvedOperModeViewModel.Contributor.Id);
            _radianAprovedService.AddRadianContributorOperation(new RadianContributorOperation()
            {
                RadianContributorId = radianAdmin.Contributor.Id,
                RadianOperationModeId = radianAdmin.Contributor.RadianOperationModeId,
                RadianProviderId = radianAdmin.Contributor.Id,
                Deleted = false,
                Timestamp = DateTime.Now,
                RadianContributorTypeId = radianAdmin.Contributor.RadianContributorTypeId,
                Pin = approvedOperModeViewModel.SoftwarePin,
                SoftwareName = approvedOperModeViewModel.SoftwareName,
                Url = approvedOperModeViewModel.SoftwareUrl,
                SoftwareId = approvedOperModeViewModel.SoftwareId,
            });

            return Json(
                new
                {
                    messasge = "Datos actualizados correctamente.",
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
        public ActionResult DeleteUser(int id, string newState, int radianContributorTypeId, string radianState)
        {
            _radianContributorService.ChangeParticipantStatus(id, newState, radianContributorTypeId, radianState);

            return Json(new
            {
                message = "Datos actualizados",
                success = true,
            }, JsonRequestBehavior.AllowGet);
        }

    }
}