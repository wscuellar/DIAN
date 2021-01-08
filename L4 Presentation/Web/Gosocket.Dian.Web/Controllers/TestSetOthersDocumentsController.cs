using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Managers;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    /// <summary>
    /// Configuración de Set de Pruebas - Otros Documentos
    /// </summary>
    public class TestSetOthersDocumentsController : Controller
    {
        private static readonly TableManager tableManagerTestSetOtherDocuments = new TableManager("GlobalTestSetOthersDocuments");
        //private static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        private TestSetOthersDocumentsManager testSetOthersDocsManager = new TestSetOthersDocumentsManager();


        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Add(string electronicDocumentId = "", string operationModeId = "")
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSetOtherDocuments;

            ViewBag.listElectronicDocuments = new ElectronicDocumentService().GetElectronicDocuments();
            ViewBag.listOperationModes = new TestSetViewModel().GetOperationModes();

            TestSetOthersDocumentsViewModel model = null;

            if (string.IsNullOrEmpty(electronicDocumentId) && string.IsNullOrEmpty(operationModeId))
            {
                model = new TestSetOthersDocumentsViewModel
                {
                    //TestSetId = null,
                    //StartDate = DateTime.UtcNow,
                    //EndDate = DateTime.UtcNow.AddMonths(3),
                    TotalDocumentRequired = 0,
                    TotalDocumentAcceptedRequired = 0,
                    Date = System.DateTime.UtcNow,
                    OthersDocumentsRequired = 0,
                    OthersDocumentsAcceptedRequired = 0,
                    ElectronicPayrollAjustmentAcceptedRequired = 0,
                    ElectronicPayrollAjustmentRequired = 0,
                    //TotalCreditNotesRequired = 1,
                    //InvoicesTotalRequired = 2,
                    //TotalDebitNotesRequired = 1
                };
            }
            else
            {
                var re = testSetOthersDocsManager.GetTestSet(electronicDocumentId, operationModeId);

                if (re != null)
                {
                    model = new TestSetOthersDocumentsViewModel()
                    {
                        TestSetId = re.TestSetId,
                        ElectronicDocumentId = re.ElectronicDocumentId,
                        OperationModeId = re.OperationModeId,
                        Description = re.Description,
                        TotalDocumentRequired = re.TotalDocumentRequired,
                        OthersDocumentsRequired = re.OthersDocumentsRequired,
                        ElectronicPayrollAjustmentRequired = re.ElectronicPayrollAjustmentRequired,
                        TotalDocumentAcceptedRequired = re.TotalDocumentAcceptedRequired,
                        OthersDocumentsAcceptedRequired = re.OthersDocumentsAcceptedRequired,
                        ElectronicPayrollAjustmentAcceptedRequired = re.ElectronicPayrollAjustmentAcceptedRequired,
                        Date = re.Date,
                        CreatedBy = re.CreatedBy,
                        UpdateBy = re.UpdateBy,
                        Active = re.Active,
                        //EndDate = model.EndDate,
                        //StartDate = model.StartDate,
                        //InvoicesTotalRequired = model.InvoicesTotalRequired,
                        //TotalInvoicesAcceptedRequired = model.TotalInvoicesAcceptedRequired,
                        //TotalDebitNotesRequired = model.TotalDebitNotesRequired,
                        //TotalDebitNotesAcceptedRequired = model.TotalDebitNotesAcceptedRequired,
                        //TotalCreditNotesRequired = model.TotalCreditNotesRequired,
                        //TotalCreditNotesAcceptedRequired = model.TotalCreditNotesAcceptedRequired
                    };
                }
            }

            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Add(TestSetOthersDocumentsViewModel model)
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSetOtherDocuments;

            ViewBag.listElectronicDocuments = new ElectronicDocumentService().GetElectronicDocuments();
            ViewBag.listOperationModes = new ContributorService().GetOperationModes();

            if (!ModelState.IsValid)
                return View("Add", model);

            if (model.ElectronicDocumentId == Gosocket.Dian.Domain.Common.ElectronicsDocuments.ElectronicPayroll.GetHashCode())
            {
                if (model.ElectronicPayrollAjustmentRequired == null)
                    ModelState.AddModelError("validpayrollAjustment", "Por favor digitar el valor");
                    
                if (model.ElectronicPayrollAjustmentAcceptedRequired == null)
                    ModelState.AddModelError("validpayrollAjustmentAccepted", "Por favor digitar el valor");

                if (model.ElectronicPayrollAjustmentRequired == null || model.ElectronicPayrollAjustmentAcceptedRequired == null)
                    return View("Add", model);
            }

            bool result;

            if (string.IsNullOrEmpty(model.TestSetId))
            {
                var testSetExists = testSetOthersDocsManager.GetTestSet(model.ElectronicDocumentId.ToString(), model.OperationModeId.ToString());
                if (testSetExists != null)
                {
                    ViewBag.ErrorExistsTestSet = true;
                    return View("Add", model);
                }

                result = testSetOthersDocsManager.InsertTestSet(
                    new GlobalTestSetOthersDocuments(
                        model.ElectronicDocumentId.ToString(),
                        model.OperationModeId.ToString()
                    )
                    {
                        TestSetId = Guid.NewGuid().ToString(),
                        ElectronicDocumentId = model.ElectronicDocumentId,
                        OperationModeId = model.OperationModeId,
                        Description = model.Description,
                        TotalDocumentRequired = model.TotalDocumentRequired,
                        OthersDocumentsRequired = model.OthersDocumentsRequired,
                        ElectronicPayrollAjustmentRequired = model.ElectronicPayrollAjustmentRequired.HasValue ? 
                            model.ElectronicPayrollAjustmentRequired.Value : 0,
                        TotalDocumentAcceptedRequired = model.TotalDocumentAcceptedRequired,
                        OthersDocumentsAcceptedRequired = model.OthersDocumentsAcceptedRequired,
                        ElectronicPayrollAjustmentAcceptedRequired = model.ElectronicPayrollAjustmentAcceptedRequired.HasValue ? 
                            model.ElectronicPayrollAjustmentAcceptedRequired.Value : 0,
                        Date = DateTime.UtcNow,
                        CreatedBy = User.Identity.Name,
                        Active = true,
                        //EndDate = model.EndDate,
                        //StartDate = model.StartDate,
                        //InvoicesTotalRequired = model.InvoicesTotalRequired,
                        //TotalInvoicesAcceptedRequired = model.TotalInvoicesAcceptedRequired,
                        //TotalDebitNotesRequired = model.TotalDebitNotesRequired,
                        //TotalDebitNotesAcceptedRequired = model.TotalDebitNotesAcceptedRequired,
                        //TotalCreditNotesRequired = model.TotalCreditNotesRequired,
                        //TotalCreditNotesAcceptedRequired = model.TotalCreditNotesAcceptedRequired
                    });
            }
            else
            {
                result = testSetOthersDocsManager.UpdateTestSet(
                    new GlobalTestSetOthersDocuments(
                        model.ElectronicDocumentId.ToString(),
                        model.OperationModeId.ToString()
                    )
                    {
                        TestSetId = model.TestSetId,
                        ElectronicDocumentId = model.ElectronicDocumentId,
                        OperationModeId = model.OperationModeId,
                        Description = model.Description,
                        TotalDocumentRequired = model.TotalDocumentRequired,
                        OthersDocumentsRequired = model.OthersDocumentsRequired,
                        ElectronicPayrollAjustmentRequired = model.ElectronicPayrollAjustmentRequired.HasValue ? model.ElectronicPayrollAjustmentRequired.Value : 0,
                        TotalDocumentAcceptedRequired = model.TotalDocumentAcceptedRequired,
                        OthersDocumentsAcceptedRequired = model.OthersDocumentsAcceptedRequired,
                        ElectronicPayrollAjustmentAcceptedRequired = model.ElectronicPayrollAjustmentAcceptedRequired.HasValue ? model.ElectronicPayrollAjustmentAcceptedRequired.Value : 0,
                        Date = model.Date,
                        CreatedBy = model.CreatedBy,
                        UpdateBy = User.Identity.Name,
                        Active = model.Active
                        //EndDate = model.EndDate,
                        //StartDate = model.StartDate,
                        //InvoicesTotalRequired = model.InvoicesTotalRequired,
                        //TotalInvoicesAcceptedRequired = model.TotalInvoicesAcceptedRequired,
                        //TotalDebitNotesRequired = model.TotalDebitNotesRequired,
                        //TotalDebitNotesAcceptedRequired = model.TotalDebitNotesAcceptedRequired,
                        //TotalCreditNotesRequired = model.TotalCreditNotesRequired,
                        //TotalCreditNotesAcceptedRequired = model.TotalCreditNotesAcceptedRequired
                    });
            }

            if (result)
            {
                return RedirectToAction("List");
            }

            ViewBag.ErrorMessage = "Ocurrio un problema creando el Set de Pruebas";

            return View("Add", model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult List()
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSetOtherDocuments;
            
            var listElectronicDocuments = new ElectronicDocumentService().GetElectronicDocuments()
                .Select(e => new ElectronicDocument
                {
                    Id = e.Id,
                    Name = e.Name
                }).ToList();

            ViewBag.listData = testSetOthersDocsManager.GetAllTestSet(listElectronicDocuments)
                .Select(x => new TestSetOthersDocumentsViewModel
                {
                    TestSetId = x.TestSetId,
                    ElectronicDocumentId = x.ElectronicDocumentId,
                    ElectronicDocumentName = listElectronicDocuments.FirstOrDefault(e => e.Id == x.ElectronicDocumentId).Name,
                    OperationModeId = x.OperationModeId,
                    OperationModeName = new ContributorService().GetOperationMode(int.Parse(x.OperationModeId)).Name,
                    Description = x.Description,
                    Active = x.Active,
                    CreatedBy = x.CreatedBy,
                    Date = x.Date,
                    //Description = x.Description,
                    TotalDocumentRequired = x.TotalDocumentRequired,
                    OthersDocumentsRequired = x.OthersDocumentsRequired,
                    ElectronicPayrollAjustmentRequired = x.ElectronicPayrollAjustmentRequired,
                    TotalDocumentAcceptedRequired = x.TotalDocumentAcceptedRequired,
                    OthersDocumentsAcceptedRequired = x.OthersDocumentsAcceptedRequired,
                    ElectronicPayrollAjustmentAcceptedRequired = x.ElectronicPayrollAjustmentAcceptedRequired,
                    //EndDate = x.EndDate,
                    //StartDate = x.StartDate,
                    //TestSetId = x.TestSetId.ToString(),
                    UpdateBy = x.UpdateBy,
                    //InvoicesTotalRequired = x.InvoicesTotalRequired,
                    //TotalDebitNotesRequired = x.TotalDebitNotesRequired,
                    //TotalCreditNotesRequired = x.TotalCreditNotesRequired,
                }).ToList();

            return View();
        }

        /// <summary>
        /// Agregar un nuevo documento electronico a la tabla
        /// </summary>
        /// <param name="docName">Nombre o descripción del nuevo documento</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddNewElectronicDocument(string docName)
        {
            ElectronicDocument electronicDocument = new ElectronicDocument()
            {
                Name = docName
            };

            int res = new ElectronicDocumentService().InsertElectronicDocuments(electronicDocument);

            return Json(new { res = electronicDocument }, JsonRequestBehavior.AllowGet);
        }

    }
}