using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [Authorize]
    public class OthersElectronicDocNumberRangeController : Controller
    {
        public ActionResult Index(int otherDocElecContributorId)
        {
            var cosmosManager = new CosmosDbManagerNumberingRange();
            NumberRangeTableViewModel model = new NumberRangeTableViewModel();
            model.NumberRanges = new List<NumberRangeViewModel>();

            var result = cosmosManager.GetNumberingRangeByOtherDocElecContributor(otherDocElecContributorId);
            var data = result;
            if(data != null)
            {
                model.NumberRanges
                    .Add(
                        new NumberRangeViewModel
                        {
                            Serie = data.Prefix,
                            ResolutionNumber = data.ResolutionNumber,
                            FromNumber = data.NumberFrom,
                            ToNumber = data.NumberTo,
                            ValidDateNumberFrom = data.CreationDate.ToString("dd-MM-yyyy"),
                            ValidDateNumberTo = data.ExpirationDate.ToString("dd-MM-yyyy")
                        }
                    );
            }

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.HFE;
            ViewBag.ContributorId = User.ContributorId();
            return View(model);
        }
    }
}