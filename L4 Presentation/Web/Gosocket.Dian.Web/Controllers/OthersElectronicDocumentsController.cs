using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private readonly IOthersElectronicDocumentsService _othersElectronicDocumentsService;
        private readonly IOthersDocsElecContributorService _othersDocsElecContributorService;
        private readonly IContributorService _contributorService;

        public OthersElectronicDocumentsController(IOthersElectronicDocumentsService othersElectronicDocumentsService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IContributorService contributorService)
        {
            _othersElectronicDocumentsService = othersElectronicDocumentsService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _contributorService = contributorService;
        }

        /// <summary>
        /// Listado de los otros documentos que se encuentran en la BD de SQLServer ElectronicDocument
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.UserCode = User.UserCode();
            ViewBag.CurrentPage = Navigation.NavigationEnum.OthersEletronicDocuments;
            ViewBag.ListElectronicDocuments = new ElectronicDocumentService().GetElectronicDocuments()
                .Select(t => new AutoListModel(t.Id.ToString(), t.Name)).ToList();
            ViewBag.ContributorId = User.ContributorId();

            NameValueCollection result = _othersDocsElecContributorService.Summary(User.UserCode());
            ViewBag.ContributorId = result["ContributorId"];
            ViewBag.ElectronicInvoice_OtherDocElecContributorTypeId = result["ElectronicInvoice_OtherDocElecContributorTypeId"];
            ViewBag.ElectronicInvoice_OtherDocElecOperationModeId = result["ElectronicInvoice_OtherDocElecOperationModeId"];
            ViewBag.TechnologyProvider_OtherDocElecContributorTypeId = result["TechnologyProvider_OtherDocElecContributorTypeId"];
            ViewBag.TechnologyProvider_OtherDocElecOperationModeId = result["TechnologyProvider_OtherDocElecOperationModeId"];
            ViewBag.TradingSystem_OtherDocElecContributorTypeId = result["TradingSystem_OtherDocElecContributorTypeId"];
            ViewBag.TradingSystem_OtherDocElecOperationModeId = result["TradingSystem_OtherDocElecOperationModeId"];
            ViewBag.Factor_OtherDocElecContributorTypeId = result["Factor_OtherDocElecContributorTypeId"];
            ViewBag.Factor_OtherDocElecOperationModeId = result["Factor_OtherDocElecOperationModeId"];
            return View(); 
        }

        public ActionResult AddOrUpdate(int electronicDocumentId = 0, int operationModeId = 0, int ContributorIdType = 0 )
        {
            List<ElectronicDocument> listED = new ElectronicDocumentService().GetElectronicDocuments();
            List<OperationModeViewModel> listOM = new TestSetViewModel().GetOperationModes();
            OthersElectronicDocumentsViewModel model = new OthersElectronicDocumentsViewModel();
            List<ContributorViewModel> listContri = new List<ContributorViewModel>()
            {
                new ContributorViewModel() { Id=0, Name= "Seleccione..."}
            };

            ViewBag.ListSoftwares = new List<SoftwareViewModel>()
            {
                new SoftwareViewModel() { Id = System.Guid.Empty, Name = "Seleccione..."}
            };

            var listCont = _contributorService.GetContributors(ContributorType.Provider.GetHashCode(), 
                ContributorStatus.Enabled.GetHashCode()).ToList();

            if(listCont != null)
            {
                listContri.AddRange(listCont.Select(c => new ContributorViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList());
            }

            ViewBag.ListTechnoProviders = listContri;

            var opeMode = listOM.FirstOrDefault(o => o.Id == operationModeId);
            if (opeMode != null)
                model.OperationMode = opeMode.Name;

            ViewBag.Title = $"Asociar modo de operación {model.OperationMode}";


            return View(model);
        }

        public ActionResult AddParticipants(int electronicDocumentId)
        {
            ViewBag.UserCode = User.UserCode();
            ViewBag.electronicDocumentId = electronicDocumentId;

            IEnumerable<SelectListItem> OperationsModes = _othersDocsElecContributorService.GetOperationModes()
             .Select(c => new SelectListItem
             {
                 Value = c.Id.ToString(),
                 Text = c.Name
             });
            ViewBag.ListOperationMode = OperationsModes;
            
            return View();
        }

        [HttpPost]
        public JsonResult Validation(ValidacionOtherElectronicDocumentsViewModel ValidacionOtherElectronicDocuments)
        {
            ResponseMessage validation =
                _othersElectronicDocumentsService.Validation(ValidacionOtherElectronicDocuments.UserCode.ToString(),
                ValidacionOtherElectronicDocuments.Accion,
                ValidacionOtherElectronicDocuments.ElectronicDocument,
                   ValidacionOtherElectronicDocuments.ComplementoTexto,
                   ValidacionOtherElectronicDocuments.ContributorIdType
                );

            return Json(validation, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetSoftwaresByContributorId(int id)
        {
            List<SoftwareViewModel> softwareList=new List<SoftwareViewModel>();
            var softs = new SoftwareService().GetSoftwaresByContributorAndState(id, true);

            if(softs != null)
            {
                softwareList = softs.Select(s => new SoftwareViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();
            }

            return Json(new { res = softwareList }, JsonRequestBehavior.AllowGet);
        }

    }
}