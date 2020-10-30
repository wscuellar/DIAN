using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Managers;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianTestSetController : Controller
    {
        readonly ContributorService contributorService = new ContributorService();

        // GET: RadianSetTest
        public ActionResult Index()
        {
            var testSetManager = new RadianTestSetManager();
            RadianTestSetTableViewModel model = new RadianTestSetTableViewModel
            {
                RadianTestSets = testSetManager.GetAllTestSet().Select(x => new RadianTestSetViewModel
                {
                    OperationModeName = contributorService.GetOperationMode(int.Parse(x.PartitionKey)).Name,
                    Active = x.Active,
                    CreatedBy = x.CreatedBy,
                    Date = x.Date,
                    Description = x.Description,
                    TotalDocumentRequired = x.TotalDocumentRequired,
                    TotalDocumentAcceptedRequired = x.TotalDocumentAcceptedRequired,
                    //EndDate = x.EndDate,
                    //StartDate = x.StartDate,
                    TestSetId = x.TestSetId.ToString(),
                    UpdateBy = x.UpdateBy,
                    //InvoicesTotalRequired = x.InvoicesTotalRequired,
                    //TotalDebitNotesRequired = x.TotalDebitNotesRequired,
                    //TotalCreditNotesRequired = x.TotalCreditNotesRequired,
                    OperationModeId = int.Parse(x.PartitionKey)
                }).ToList()
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.TestSet;
            return View(model);

        }
    }
}