using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.RadianApproved;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianApprovedController : Controller
    {
        private readonly IRadianContributorService _radianContributorService;
        private readonly IRadianTestSetService _radianTestSetService;
        private readonly IRadianApprovedService _radianAprovedService;
        private readonly IRadianTestSetResultService _radianTestSetResultService;

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
        public ActionResult Index(int Id)
        {
            // LoadSoftwareModeOperation();
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(1704648);
            List<RadianContributorFileType> listFileType = _radianAprovedService.ContributorFileTypeList(radianAdmin.Type.Id);

            RadianApprovedViewModel model = new RadianApprovedViewModel()
            {
                ContributorId = radianAdmin.Contributor.Id,
                Name = radianAdmin.Contributor.TradeName,
                Nit = radianAdmin.Contributor.Code,
                BusinessName = radianAdmin.Contributor.BusinessName,
                Email = radianAdmin.Contributor.Email,
                Files = radianAdmin.Files,
                FilesRequires = listFileType,
                Step = radianAdmin.Contributor.Step

            };
            return View(model);
        }

        // GET: RadianFactor

        [HttpPost]
        public ActionResult Index(RegistrationDataViewModel registrationData)
        {
            _radianContributorService.CreateContributor(registrationData.ContributorId,
                                                        RadianState.Registrado,
                                                        registrationData.RadianContributorType,
                                                        registrationData.RadianOperationMode,
                                                        User.UserName());

            // Lista de Software Modo de Operacion 
            // CA 2.3
            // LoadSoftwareModeOperation();

            // CA 2.4 
            // Software de un Proveedor Electronico
            RadianAdmin radianAdmin = _radianContributorService.ContributorSummary(registrationData.ContributorId);
            RadianApprovedViewModel model = new RadianApprovedViewModel()
            {
                Name = radianAdmin.Contributor.TradeName,
                Nit = radianAdmin.Contributor.Code,
                BusinessName = radianAdmin.Contributor.BusinessName,
                Email = radianAdmin.Contributor.Email,
                Files = radianAdmin.Files
            };
            return View(model);

        }

        private void LoadSoftwareModeOperation()
        {
            List<Domain.RadianOperationMode> list = _radianTestSetService.OperationModeList();
            ViewBag.RadianSoftwareOperationMode = list;
        }

        [HttpPost]
        public JsonResult UploadFiles()
        {
            for (int i = 0; i < Request.Files.Count; i++)
            {
                RadianContributorFile radianContributorFile = new RadianContributorFile();
                RadianContributorFileHistory radianFileHistory = new RadianContributorFileHistory();
                string typeId = Request.Form.Get("TypeId_" + i);
                string nit = Request.Form.Get("nit");
                string email = Request.Form.Get("email");
                string ContributorId = Request.Form.Get("contributorId");
                var file = Request.Files[i];
                radianContributorFile.FileName = file.FileName;
                radianContributorFile.Timestamp = DateTime.Now;
                radianContributorFile.Updated = DateTime.Now;
                radianContributorFile.CreatedBy = email;
                radianContributorFile.RadianContributorId = Convert.ToInt32(ContributorId);
                radianContributorFile.Deleted = false;
                radianContributorFile.FileType = Convert.ToInt32(typeId);
                radianContributorFile.Status = 1;
                radianContributorFile.Comments = "Comentario";
               
                ResponseMessage responseUpload = _radianAprovedService.UploadFile(file.InputStream, nit, radianContributorFile);

                if(responseUpload.Message != ""){
                    radianFileHistory.FileName = file.FileName;
                    radianFileHistory.Comments = "";
                    radianFileHistory.Timestamp = DateTime.Now;
                    radianFileHistory.CreatedBy = email;
                    radianFileHistory.Status = 1;
                    radianFileHistory.RadianContributorFileId = Guid.Parse(responseUpload.Message);
                    ResponseMessage responseUpdateFileHistory = _radianAprovedService.AddFileHistory(radianFileHistory);

                }
            }
            Thread.Sleep(3000);
            return Json(new
            {
                messasge = "Datos actualizados correctamente.",
                success = true,
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetSetTestResult(RadianApprovedViewModel radianApprovedViewModel)
        {
            radianApprovedViewModel.RadianTestSetResult =
                _radianTestSetResultService.GetTestSetResultByNit(radianApprovedViewModel.Nit).FirstOrDefault();
            return PartialView("_setTestResult", radianApprovedViewModel);
        }
    }
}