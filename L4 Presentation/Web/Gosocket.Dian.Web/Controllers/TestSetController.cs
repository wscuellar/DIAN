using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Managers;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class TestSetController : Controller
    {
        ContributorService contributorService = new ContributorService();
        readonly ContributorOperationsService contributorOperationsService = new ContributorOperationsService();
        readonly SoftwareService softwareService = new SoftwareService();
        private static readonly TableManager tableManagerNumberRange = new TableManager("GlobalNumberRange");
        private static readonly TableManager tableManagerTestSet = new TableManager("GlobalTestSet");
        private static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        private static readonly TableManager tableManager = new TableManager("GlobalLogger");
        private TestSetManager testSetManager = new TestSetManager();
        private static Lazy<CloudTableClient> lazyClient = new Lazy<CloudTableClient>(InitializeTableClient);
        public static CloudTableClient tableClient => lazyClient.Value;
        

        private static CloudTableClient InitializeTableClient()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var tableClient = account.CreateCloudTableClient();
            return tableClient;
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Add()
        {
            var model = new TestSetViewModel
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                TotalDocumentRequired = 4,
                TotalDocumentAcceptedRequired = 0,
                TotalCreditNotesRequired = 1,
                InvoicesTotalRequired = 2,
                TotalDebitNotesRequired = 1
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSet;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Add(TestSetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Add", model);
            }
            if (!model.TestSetReplace)
            {
                var testSetExists = testSetManager.GetTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString());
                if (testSetExists != null)
                {
                    ViewBag.ErrorExistsTestSet = true;
                    return View("Add", model);
                }
            }
            var result = testSetManager.InsertTestSet(new GlobalTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString())
            {
                TestSetId = Guid.NewGuid(),
                Active = true,
                CreatedBy = User.Identity.Name,
                Description = model.Description,
                TotalDocumentRequired = model.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = model.TotalDocumentAcceptedRequired,
                EndDate = model.EndDate,
                StartDate = model.StartDate,
                InvoicesTotalRequired = model.InvoicesTotalRequired,
                TotalInvoicesAcceptedRequired = model.TotalInvoicesAcceptedRequired,
                TotalDebitNotesRequired = model.TotalDebitNotesRequired,
                TotalDebitNotesAcceptedRequired = model.TotalDebitNotesAcceptedRequired,
                TotalCreditNotesRequired = model.TotalCreditNotesRequired,
                TotalCreditNotesAcceptedRequired = model.TotalCreditNotesAcceptedRequired
            }
            );
            if (result)
            {
                return RedirectToAction("List"/*, new { contributorId = model.ContributorId }*/);
            }

            ViewBag.ErrorMessage = "Ocurrio un problema creando el Set de Pruebas";
            return View("Add", model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Edit(int operationModeId)
        {
            var testSet = testSetManager.GetTestSet(operationModeId.ToString(), operationModeId.ToString());
            if (testSet == null)
                return RedirectToAction(nameof(List));

            var model = new TestSetViewModel
            {
                StartDate = testSet.StartDate,
                EndDate = testSet.EndDate,
                TotalDocumentRequired = testSet.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = testSet.TotalDocumentAcceptedRequired,
                Description = testSet.Description,
                InvoicesTotalRequired = testSet.InvoicesTotalRequired,
                TotalInvoicesAcceptedRequired = testSet.TotalInvoicesAcceptedRequired,
                TotalDebitNotesRequired = testSet.TotalDebitNotesRequired,
                TotalDebitNotesAcceptedRequired = testSet.TotalDebitNotesAcceptedRequired,
                TotalCreditNotesRequired = testSet.TotalCreditNotesRequired,
                TotalCreditNotesAcceptedRequired = testSet.TotalCreditNotesAcceptedRequired,
                TestSetId = testSet.TestSetId.ToString(),
                OperationModeId = int.Parse(testSet.PartitionKey)
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSet;
            return View(model);
        }

        public ActionResult View(int operationModeId, string contributorId, string contributorCode, string softwareId)
        {
            var software = softwareService.Get(Guid.Parse(softwareId));
            var testSet = testSetManager.GetTestSet(operationModeId.ToString(), operationModeId.ToString());
            var contributorTypeId = User.ContributorTypeId();
            var testSetResult = tableManagerTestSetResult.Find<GlobalTestSetResult>(contributorCode, $"{contributorTypeId}|{softwareId}");
            if (testSetResult == null && contributorTypeId == (int)ContributorType.Provider)
                testSetResult = tableManagerTestSetResult.Find<GlobalTestSetResult>(contributorCode, $"{(int)ContributorType.Biller}|{softwareId}");

            var contributor = contributorService.Get(int.Parse(contributorId));
            if (testSetResult?.Status == (int)TestSetStatus.Accepted && contributor.AcceptanceStatusId == (int)ContributorStatus.Registered && contributor?.ContributorTypeId == (int)ContributorType.Provider)
            {
                contributorService.SetToEnabled(contributor);
                var logger = new GlobalLogger("SetContributorToEnabledForced", contributor.Code)
                {
                    Action = "SetContributorToEnabledForced",
                    Controller = "TestSet",
                    Message = $"Se fuerza habilitación de contribuyente({contributor.ContributorTypeId}) con NIT: {contributor.Code}, set de pruebas con identificador: {testSetResult.Id}, software: {testSetResult.SoftwareId}",
                    RouteData = "",
                    StackTrace = ""
                };
                RegisterLog(logger);
                return RedirectToAction(nameof(ContributorController.View), "Contributor", new { id = contributor.Id });
            }

            if (testSetResult.OperationModeId == (int)OperationMode.Free)
                software = new Domain.Software { Id = Guid.Parse(softwareId), Name = testSetResult.OperationModeName, Pin = ConfigurationManager.GetValue("BillerSoftwarePin") };


            var model = new TestSetViewModel
            {
                SoftwareId = softwareId,
                SoftwareName = software.Name,
                SoftwarePin = software.Pin,
                StartDate = testSet.StartDate,
                EndDate = testSet.EndDate,
                StartDateString = testSet.StartDate.ToString("dd-MM-yyyy"),
                EndDateString = testSet.EndDate.ToString("dd-MM-yyyy"),
                TotalDocumentRequired = testSet.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = testSet.TotalDocumentAcceptedRequired,
                Description = testSet.Description,
                InvoicesTotalRequired = testSet.InvoicesTotalRequired,
                TotalInvoicesAcceptedRequired = testSet.TotalInvoicesAcceptedRequired,
                TotalDebitNotesRequired = testSet.TotalDebitNotesRequired,
                TotalDebitNotesAcceptedRequired = testSet.TotalDebitNotesAcceptedRequired,
                TotalCreditNotesRequired = testSet.TotalCreditNotesRequired,
                TotalCreditNotesAcceptedRequired = testSet.TotalCreditNotesAcceptedRequired,
                TestSetId = testSetResult != null ? testSetResult.Id : "",
                Status = testSetResult != null ? testSetResult.Status : 0,
                OperationModeId = int.Parse(testSet.PartitionKey),
            };

            GlobalNumberRange numberRange = null;
            if (testSetResult.OperationModeId == (int)OperationMode.Free)
                numberRange = tableManagerNumberRange.Find<GlobalNumberRange>("SET", "1");
            if (testSetResult.OperationModeId == (int)OperationMode.Own)
                numberRange = tableManagerNumberRange.Find<GlobalNumberRange>("SET", "2");
            if (testSetResult.OperationModeId == (int)OperationMode.Provider)
                numberRange = tableManagerNumberRange.Find<GlobalNumberRange>("SET", "3");

            if (numberRange != null)
            {
                model.RangePrefix = numberRange.Serie;
                model.RangeResolutionNumber = numberRange.ResolutionNumber;
                model.RangeFromNumber = numberRange.FromNumber;
                model.RangeToNumber = numberRange.ToNumber;
                model.RangeFromDate = DateTime.ParseExact(numberRange.ValidDateNumberFrom.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
                model.RangeToDate = DateTime.ParseExact(numberRange.ValidDateNumberTo.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
                model.RangeTechnicalKey = numberRange.TechnicalKey;
            }

            ViewBag.ContributorId = contributorId;
            ViewBag.ContributorCode = contributorCode;
            ViewBag.SoftwareId = softwareId;
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSet;
            return View(model);
        }


        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Edit(TestSetViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Edit", model);

            var result = testSetManager.InsertTestSet(new GlobalTestSet(model.OperationModeId.ToString(), model.OperationModeId.ToString())
            {
                TestSetId = Guid.Parse(model.TestSetId),//Guid.NewGuid(),
                Active = true,
                CreatedBy = User.Identity.Name,
                Description = model.Description,
                TotalDocumentRequired = model.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = model.TotalDocumentAcceptedRequired,
                EndDate = model.EndDate,
                StartDate = model.StartDate,
                InvoicesTotalRequired = model.InvoicesTotalRequired,
                TotalInvoicesAcceptedRequired = model.TotalInvoicesAcceptedRequired,
                TotalDebitNotesRequired = model.TotalDebitNotesRequired,
                TotalDebitNotesAcceptedRequired = model.TotalDebitNotesAcceptedRequired,
                TotalCreditNotesRequired = model.TotalCreditNotesRequired,
                TotalCreditNotesAcceptedRequired = model.TotalCreditNotesAcceptedRequired,
                UpdateBy = User.Identity.Name,
                Date = DateTime.UtcNow
            });

            if (result)
                return RedirectToAction(nameof(List));

            ViewBag.ErrorMessage = "Ocurrio un problema editando el Set de Pruebas";
            return View("Edit", model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult List()
        {
            var testSetManager = new TestSetManager();
            TestSetTableViewModel model = new TestSetTableViewModel
            {
                TestSets = testSetManager.GetAllTestSet().Select(x => new TestSetViewModel
                {
                    OperationModeName = contributorService.GetOperationMode(int.Parse(x.PartitionKey)).Name,
                    Active = x.Active,
                    CreatedBy = x.CreatedBy,
                    Date = x.Date,
                    Description = x.Description,
                    TotalDocumentRequired = x.TotalDocumentRequired,
                    TotalDocumentAcceptedRequired = x.TotalDocumentAcceptedRequired,
                    EndDate = x.EndDate,
                    StartDate = x.StartDate,
                    TestSetId = x.TestSetId.ToString(),
                    UpdateBy = x.UpdateBy,
                    InvoicesTotalRequired = x.InvoicesTotalRequired,
                    TotalDebitNotesRequired = x.TotalDebitNotesRequired,
                    TotalCreditNotesRequired = x.TotalCreditNotesRequired,
                    OperationModeId = int.Parse(x.PartitionKey)
                }).ToList()
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSet;
            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult ResetTestSetResult(string pk, string rk)
        {
            var testSetResult = tableManagerTestSetResult.Find<GlobalTestSetResult>(pk, rk);
            ResetTestSetResult(testSetResult);
            var json = Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
            return json;
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public JsonResult GetCategoryDocumentTypes(string categoryCode)
        {
            var documentTypeManager = new DocumentTypeManager(categoryCode.ToLower());
            var documentTypes = documentTypeManager.List().Select(x => new { x.Code, x.Name }).ToList();
            return new JsonResult { Data = documentTypes, MaxJsonLength = int.MaxValue };
        }

        public ActionResult Tracking(string contributorCode, string contributorTypeId, string softwareId)
        {
            var testSetTrackingManager = new TestSetManager();

            var model = new TestSetResultViewModel();
            var testSetResult = tableManagerTestSetResult.Find<GlobalTestSetResult>(contributorCode, $"{contributorTypeId}|{softwareId}");
            if (testSetResult == null)
                testSetResult = tableManagerTestSetResult.Find<GlobalTestSetResult>(contributorCode, $"{(int)ContributorType.Zero}|{softwareId}");
            if (testSetResult == null && int.Parse(contributorTypeId) == (int)ContributorType.Provider)
                testSetResult = tableManagerTestSetResult.Find<GlobalTestSetResult>(contributorCode, $"{(int)ContributorType.Biller}|{softwareId}");

            model.ContributorId = testSetResult.ContributorId;
            model.ContributorCode = testSetResult.SenderCode;
            model.OperationModeId = testSetResult.OperationModeId;
            model.OperationModeName = testSetResult.OperationModeName;
            model.SoftwareId = testSetResult.SoftwareId;

            model.TotalDocumentRequired = testSetResult.TotalDocumentRequired;
            model.TotalDocumentAcceptedRequired = testSetResult.TotalDocumentAcceptedRequired;
            model.TotalDocumentSent = testSetResult.TotalDocumentSent;
            model.TotalDocumentAccepted = testSetResult.TotalDocumentAccepted;
            model.TotalDocumentsRejected = testSetResult.TotalDocumentsRejected;

            model.InvoicesTotalRequired = testSetResult.InvoicesTotalRequired;
            model.TotalInvoicesAcceptedRequired = testSetResult.TotalInvoicesAcceptedRequired;
            model.InvoicesTotalSent = testSetResult.InvoicesTotalSent;
            model.TotalInvoicesAccepted = testSetResult.TotalInvoicesAccepted;
            model.TotalInvoicesRejected = testSetResult.TotalInvoicesRejected;

            model.TotalCreditNotesRequired = testSetResult.TotalCreditNotesRequired;
            model.TotalCreditNotesAcceptedRequired = testSetResult.TotalCreditNotesAcceptedRequired;
            model.TotalCreditNotesSent = testSetResult.TotalCreditNotesSent;
            model.TotalCreditNotesAccepted = testSetResult.TotalCreditNotesAccepted;
            model.TotalCreditNotesRejected = testSetResult.TotalCreditNotesRejected;

            model.TotalDebitNotesRequired = testSetResult.TotalDebitNotesRequired;
            model.TotalDebitNotesAcceptedRequired = testSetResult.TotalDebitNotesAcceptedRequired;
            model.TotalDebitNotesSent = testSetResult.TotalDebitNotesSent;
            model.TotalDebitNotesAccepted = testSetResult.TotalDebitNotesAccepted;
            model.TotalDebitNotesRejected = testSetResult.TotalDebitNotesRejected;

            model.Id = testSetResult.Id;
            model.Status = testSetResult.Status;

            if (!User.IsInAnyRole("Administrador", "Super"))
                ViewBag.CurrentPage = Navigation.NavigationEnum.TestSet;
            else
                ViewBag.CurrentPage = Navigation.NavigationEnum.Provider;
            return View(model);
        }

        private static void ResetTestSetResult(GlobalTestSetResult globalTestSetResult)
        {
            var globalTestSet = tableManagerTestSet.Find<GlobalTestSet>(globalTestSetResult.OperationModeId.ToString(), globalTestSetResult.OperationModeId.ToString());
            globalTestSetResult.Status = (int)TestSetStatus.InProcess;
            globalTestSetResult.TotalDocumentRequired = globalTestSet.TotalDocumentRequired;

            globalTestSetResult.TotalDocumentAcceptedRequired = globalTestSet.TotalDocumentAcceptedRequired;
            globalTestSetResult.TotalDocumentSent = 0;
            globalTestSetResult.TotalDocumentAccepted = 0;
            globalTestSetResult.TotalDocumentsRejected = 0;

            globalTestSetResult.InvoicesTotalRequired = globalTestSet.InvoicesTotalRequired;
            globalTestSetResult.TotalInvoicesAcceptedRequired = globalTestSet.TotalInvoicesAcceptedRequired;
            globalTestSetResult.InvoicesTotalSent = 0;
            globalTestSetResult.TotalInvoicesAccepted = 0;
            globalTestSetResult.TotalInvoicesRejected = 0;

            globalTestSetResult.TotalCreditNotesRequired = globalTestSet.TotalCreditNotesRequired;
            globalTestSetResult.TotalCreditNotesAcceptedRequired = globalTestSet.TotalCreditNotesAcceptedRequired;
            globalTestSetResult.TotalCreditNotesSent = 0;
            globalTestSetResult.TotalCreditNotesAccepted = 0;
            globalTestSetResult.TotalCreditNotesRejected = 0;

            globalTestSetResult.TotalDebitNotesRequired = globalTestSet.TotalDebitNotesRequired;
            globalTestSetResult.TotalDebitNotesAcceptedRequired = globalTestSet.TotalDebitNotesAcceptedRequired;
            globalTestSetResult.TotalDebitNotesSent = 0;
            globalTestSetResult.TotalDebitNotesAccepted = 0;
            globalTestSetResult.TotalDebitNotesRejected = 0;

            tableManagerTestSetResult.InsertOrUpdate(globalTestSetResult);

            var projectionQuery = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, globalTestSetResult.Id)).Select(new[] { "RowKey" });
            var table = GetTableRef("GlobalTestSetTracking");
            var entities = table.ExecuteQuery(projectionQuery).ToArray();
            var offset = 0;
            while (offset < entities.Length)
            {
                var batch = new TableBatchOperation();
                var rows = entities.Skip(offset).Take(100);
                foreach (var row in rows)
                    batch.Delete(row);

                var result = table.ExecuteBatch(batch);
                offset += result.Count;
            }
        }

        private static CloudTable GetTableRef(string nameTable)
        {
            CloudTable tableRef = null;            
            tableRef = tableClient.GetTableReference(nameTable);
            return tableRef;
        }

        private void RegisterLog(GlobalLogger logger)
        {            
            tableManager.InsertOrUpdate(logger);
        }

    }
}