using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    /// <summary>
    /// Controlador Otros Documentos en estado asociado. HU DIAN-HU-070_3_ODC_HabilitacionParticipanteOD
    /// </summary>
    [Authorize]
    public class OthersElectronicDocAssociatedController : Controller
    {
        // GET: OthersElectronicDocAssociated
        public ActionResult Index(int electronicDocumentId = 0, int operationModeId = 0, int ContributorIdType = 0)
        {

            OthersElectronicDocAssociatedViewModel model = new OthersElectronicDocAssociatedViewModel()
            {
                ContributorId = 1,
                Name = "Datos Quemados",
                Nit = "Nit000",
                BusinessName = "BusinessName",
                Email = "Email",
                Step = 1,
                State = "",
            };

            return View(model);
        }


        [HttpPost]
        public ActionResult CancelRegister(int ContributorId, int ContributorTypeId, string State, string description)
        {
            ResponseMessage response = new ResponseMessage();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

    }
}