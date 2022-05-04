using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [Authorize]
    public class OthersElectronicDocNumberRangeController : Controller
    {
        public ActionResult Index(int otherDocElecContributorId)
        {
            var tableManagerNumberRangeManager = new TableManager("GlobalNumberRange");
            //var cosmosManager = new CosmosDbManagerNumberingRange();
            NumberRangeTableViewModel model = new NumberRangeTableViewModel();
            model.NumberRanges = new List<NumberRangeViewModel>();

            //var result = cosmosManager.GetNumberingRangeByOtherDocElecContributor(otherDocElecContributorId);
            var data = tableManagerNumberRangeManager.Find<GlobalNumberRange>(User.ContributorCode(), "SEDS|05|18760000001");
            if(data != null)
            {
                model.NumberRanges
                    .Add(
                        new NumberRangeViewModel
                        {
                            Serie = data.Serie,
                            ResolutionNumber = data.ResolutionNumber,
                            FromNumber = data.FromNumber,
                            ToNumber = data.ToNumber,
                            ValidDateNumberFrom = DateTime.ParseExact(data.ValidDateNumberFrom.ToString(), "yyyyMMdd", null).ToString("dd-MM-yyyy"),
                            ValidDateNumberTo = DateTime.ParseExact(data.ValidDateNumberTo.ToString(), "yyyyMMdd", null).ToString("dd-MM-yyyy"),
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