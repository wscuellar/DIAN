﻿using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.RadianApproved;
using System.Collections.Generic;
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

        public RadianApprovedController(IRadianContributorService radianContributorService,
                                        IRadianTestSetService radianTestSetService,
                                        IRadianContributorFileTypeService radianContributorFileTypeService,
                                        IRadianApprovedService radianAprovedService)
        {
            _radianContributorService = radianContributorService;
            _radianTestSetService = radianTestSetService;
            _radianAprovedService = radianAprovedService;
        }

        [HttpGet]
        public ActionResult Index(int Id)
        {
            // LoadSoftwareModeOperation();
            RadianAdmin radianAdmin = _radianAprovedService.ContributorSummary(1704648);
            List<RadianContributorFileType> listFileType = _radianAprovedService.ContributorFileTypeList(radianAdmin.Type.Id);
            RadianApprovedViewModel model = new RadianApprovedViewModel()
            {
                Name = radianAdmin.Contributor.TradeName,
                Nit = radianAdmin.Contributor.Code,
                BusinessName = radianAdmin.Contributor.BusinessName,
                Email = radianAdmin.Contributor.Email,
                Files = radianAdmin.Files,
                FilesRequires = listFileType

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
                var file = Request.Files[i];
            }
            Thread.Sleep(3000);
            return Json(new
            {
                messasge = "Datos actualizados correctamente.",
                success = true,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}