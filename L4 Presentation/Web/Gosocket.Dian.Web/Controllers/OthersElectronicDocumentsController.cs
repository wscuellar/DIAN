using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    /// <summary>
    /// Controlador utilizado para la opción de menu, Otros Documentos. HU DIAN-HU-070_3_ODC_HabilitacionParticipanteOD
    /// </summary>
    [Authorize]
    public class OthersElectronicDocumentsController : Controller
    {
        /// <summary>
        /// Listado de los otros documentos que se encuentran en la BD de SQLServer ElectronicDocument
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddOrUpdate (int electronicDocumentId = 0, int operationModeId=0)
        {
            List<ElectronicDocument> listED = new ElectronicDocumentService().GetElectronicDocuments();
            List<OperationModeViewModel> listOM = new TestSetViewModel().GetOperationModes();
            OthersElectronicDocumentsViewModel model = new OthersElectronicDocumentsViewModel();

            var opeMode = listOM.FirstOrDefault(o => o.Id == operationModeId);
            if (opeMode != null)
                model.OperationMode = opeMode.Name;
            
            ViewBag.Title = $"Asociar modo de operación {model.OperationMode}";


            return View(model);
        }

    }
}