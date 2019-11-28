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
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class NumberRangeController : Controller
    {
        private static readonly TableManager tableManagerNumberRangeManager = new TableManager("GlobalNumberRange");

        public ActionResult List()
        {
            return View(new NumberRangeTableViewModel());
        }

        [HttpPost]
        public ActionResult List(NumberRangeTableViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            if (!User.IsInAnyRole("Administrador", "Super"))
                model.Code = User.ContributorCode();

            TableContinuationToken continuationToken = null;
            continuationToken = (TableContinuationToken)Session["NextTableContinuationTokenRangeNumber_" + model.Code + "_" + model.Page];
            if (model.Page == 0)
                continuationToken = null;

            var result = tableManagerNumberRangeManager.GetRangeRows<GlobalNumberRange>(model.Code, model.Length, continuationToken);
            Session["NextTableContinuationTokenRangeNumber_" + model.Code + "_" + (model.Page + 1)] = result.Item2;
            model.NumberRanges = result.Item1.Select(r => new NumberRangeViewModel
            {
                Active = r.Active,
                Serie = r.Serie,
                ResolutionNumber = r.ResolutionNumber,
                ResolutionDate = r.ResolutionDate,
                TechnicalKey = r.TechnicalKey,
                FromNumber = r.FromNumber,
                ToNumber = r.ToNumber,
                ValidDateNumberFrom = DateNumberToString(r.ValidDateNumberFrom.ToString()),
                ValidDateNumberTo = DateNumberToString(r.ValidDateNumberTo.ToString())
            }).ToList();

            model.SearchFinished = true;
            return View(model);
        }

        public ActionResult ListTests(int contributorId, int operationModeId)
        {
            if (!User.IsInAnyRole("Administrador", "Super"))
                contributorId = User.ContributorId();

            NumberRangeTableViewModel model = new NumberRangeTableViewModel();
            var result = tableManagerNumberRangeManager.Find<GlobalNumberRange>("SET", operationModeId.ToString());

            model.NumberRanges = new List<NumberRangeViewModel>()
            {
               new NumberRangeViewModel{
                    Active = result.Active,
                    Serie = result.Serie,
                    ResolutionNumber = result.ResolutionNumber,
                    ResolutionDate = result.ResolutionDate,
                    TechnicalKey = result.TechnicalKey,
                    FromNumber = result.FromNumber,
                    ToNumber = result.ToNumber,
                    ValidDateNumberFrom = DateNumberToString(result.ValidDateNumberFrom.ToString()),
                    ValidDateNumberTo = DateNumberToString(result.ValidDateNumberTo.ToString())
            }};

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.HFE;
            ViewBag.ContributorId = contributorId;
            return View(model);
        }

        private string DateNumberToString(string date)
        {
            return DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
        }
    }
}