using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class HomeController : Controller
    {
        readonly ContributorService contributorService = new ContributorService();
        private static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        private static readonly TableManager tableManagerGlobalDocValidatorStats = new TableManager("GlobalDocValidatorStats");
        private static readonly TableManager tableManagerGlobalDocValidatorNewStats = new TableManager("GlobalDocValidatorNewStats");
        private static readonly TableManager dianAuthTableManager = new TableManager("AuthToken");

        // GET: Home
        public ActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        public ActionResult Dashboard()
        {
            var model = new DocumentStatsViewModel();
            if (User.IsInAnyRole("Administrador", "Super"))
            {
                var globalStats = tableManagerGlobalDocValidatorNewStats.FindByPartition<GlobalDocValidatorStats>("STATS");
                model = new DocumentStatsViewModel
                {
                    GlobalTotal = String.Format(CultureInfo.InvariantCulture, "{0:#,#}", globalStats.Sum(g => g.Count)).Replace(",", "."),
                    GlobalTotalAccepted = globalStats.Any(g => g.RowKey == "ACCEPTED") ? globalStats.SingleOrDefault(g => g.RowKey == "ACCEPTED").Count : 0,
                    GlobalTotalRejected = globalStats.Any(g => g.RowKey == "REJECTED") ? globalStats.SingleOrDefault(g => g.RowKey == "REJECTED").Count : 0,
                    GlobalTotalNotification = globalStats.Any(g => g.RowKey == "NOTIFICATION") ? globalStats.SingleOrDefault(g => g.RowKey == "NOTIFICATION").Count : 0
                };

                int arrayMonthCount = 4;
                var arrayMonthDate = new string[arrayMonthCount];
                var arrayMonthAll = new double[arrayMonthCount];
                var arrayMonthAccepted = new double[arrayMonthCount];
                var arrayMonthNotification = new double[arrayMonthCount];
                var arrayMonthRejected = new double[arrayMonthCount];

                int contMonth = 0;
                var currentYear = DateTime.UtcNow.Year;
                var pks = new List<string>();
                for (int i = 3; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddMonths(-i);
                    var month = date.Month.ToString().PadLeft(2, '0');
                    pks.Add($"MONTH|{month}|{date.Year}|STATS");
                }
                var globalMonthlyStats = tableManagerGlobalDocValidatorNewStats.GetRowsContainsInPartitionKeys<GlobalDocValidatorStats>(pks);
                int totalMonthAll = 0;
                for (int i = 3; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddMonths(-i);
                    var month = date.Month.ToString().PadLeft(2, '0');
                    var pk = $"MONTH|{month}|{date.Year}|STATS";
                    var total = globalMonthlyStats.Any(m => m.PartitionKey == pk) ? globalMonthlyStats.Where(m => m.PartitionKey == pk).Sum(m => m.Count) : 0;
                    var accepted = globalMonthlyStats.Any(m => m.PartitionKey == pk && m.RowKey == "ACCEPTED") ? globalMonthlyStats.SingleOrDefault(m => m.PartitionKey == pk && m.RowKey == "ACCEPTED").Count : 0;
                    var notification = globalMonthlyStats.Any(m => m.PartitionKey == pk && m.RowKey == "NOTIFICATION") ? globalMonthlyStats.SingleOrDefault(m => m.PartitionKey == pk && m.RowKey == "NOTIFICATION").Count : 0;
                    var rejected = globalMonthlyStats.Any(m => m.PartitionKey == pk && m.RowKey == "REJECTED") ? globalMonthlyStats.SingleOrDefault(m => m.PartitionKey == pk && m.RowKey == "REJECTED").Count : 0;
                    arrayMonthDate[contMonth] = GetMonthName(date.ToString("MM-yyy").Split('-')[0]) + "-" + date.ToString("MM-yyy").Split('-')[1];
                    arrayMonthAll[contMonth] = total;
                    arrayMonthAccepted[contMonth] = accepted;
                    arrayMonthNotification[contMonth] = notification;
                    arrayMonthRejected[contMonth] = rejected;
                    contMonth++;
                    model.GlobalMonthlyTotals.Add(new Tuple<DateTime, long, long, long, long>(date, total, accepted, notification, rejected));
                    totalMonthAll += (int)total;
                }
                model.JsonMonthSumAll += String.Format(CultureInfo.InvariantCulture, "{0:#,#}", totalMonthAll).Replace(",", ".");
                model.JsonMonthSumAll = !string.IsNullOrWhiteSpace(model.JsonMonthSumAll) ? model.JsonMonthSumAll : "0";

                model.JsonMonthDate = JsonConvert.SerializeObject(arrayMonthDate);
                model.JsonMonthAll = JsonConvert.SerializeObject(arrayMonthAll);
                model.JsonMonthAccepted = JsonConvert.SerializeObject(arrayMonthAccepted);
                model.JsonMonthNotification = JsonConvert.SerializeObject(arrayMonthNotification);
                model.JsonMonthRejected = JsonConvert.SerializeObject(arrayMonthRejected);

                pks = new List<string>();
                for (int i = 13; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddDays(-i);
                    var day = date.Day.ToString().PadLeft(2, '0');
                    var month = date.Month.ToString().PadLeft(2, '0');
                    pks.Add($"DAY|{day}|{month}|{date.Year}|STATS");
                }
                int arrayCount = 14;
                var arrayDate = new string[arrayCount];
                var arrayAll = new double[arrayCount];
                var arrayAccepted = new double[arrayCount];
                var arrayNotification = new double[arrayCount];
                var arrayRejected = new double[arrayCount];
                int cont = 0;
                var globalDailyStats = tableManagerGlobalDocValidatorNewStats.GetRowsContainsInPartitionKeys<GlobalDocValidatorStats>(pks);
                int totalAll = 0;
                for (int i = 13; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddDays(-i);
                    var day = date.Day.ToString().PadLeft(2, '0');
                    var month = date.Month.ToString().PadLeft(2, '0');
                    var pk = $"DAY|{day}|{month}|{date.Year}|STATS";
                    var total = globalDailyStats.Any(d => d.PartitionKey == pk) ? globalDailyStats.Where(d => d.PartitionKey == pk).Sum(m => m.Count) : 0;
                    var accepted = globalDailyStats.Any(d => d.PartitionKey == pk && d.RowKey == "ACCEPTED") ? globalDailyStats.SingleOrDefault(d => d.PartitionKey == pk && d.RowKey == "ACCEPTED").Count : 0;
                    var notification = globalDailyStats.Any(d => d.PartitionKey == pk && d.RowKey == "NOTIFICATION") ? globalDailyStats.SingleOrDefault(d => d.PartitionKey == pk && d.RowKey == "NOTIFICATION").Count : 0;
                    var rejected = globalDailyStats.Any(d => d.PartitionKey == pk && d.RowKey == "REJECTED") ? globalDailyStats.SingleOrDefault(d => d.PartitionKey == pk && d.RowKey == "REJECTED").Count : 0;
                    arrayDate[cont] = date.ToString(@"dd-MM-yyyy");
                    arrayAll[cont] = total;
                    arrayAccepted[cont] = accepted;
                    arrayNotification[cont] = notification;
                    arrayRejected[cont] = rejected;
                    model.GlobalDailyTotals.Add(new Tuple<DateTime, long, long, long, long>(date, total, accepted, notification, rejected));
                    cont++;
                    totalAll += (int)total;
                }
                model.JsonSumAll += string.Format(CultureInfo.InvariantCulture, "{0:#,#}", totalAll).Replace(",", ".");
                model.JsonSumAll = !string.IsNullOrWhiteSpace(model.JsonSumAll) ? model.JsonSumAll : "0";

                model.JsonDate = JsonConvert.SerializeObject(arrayDate);
                model.JsonAll = JsonConvert.SerializeObject(arrayAll);
                model.JsonAccepted = JsonConvert.SerializeObject(arrayAccepted);
                model.JsonNotification = JsonConvert.SerializeObject(arrayNotification);
                model.JsonRejected = JsonConvert.SerializeObject(arrayRejected);

                model.PendingContributors = contributorService.GetCountContributorsByAcceptanceStatusId((int)ContributorStatus.Pending);
                model.RegisteredContributors = contributorService.GetCountContributorsByAcceptanceStatusId((int)ContributorStatus.Registered);
                model.EnabledContributors = contributorService.GetCountContributorsByAcceptanceStatusId((int)ContributorStatus.Enabled);
                int totalContributor = contributorService.GetCountContributorsByAcceptanceStatusId((int)ContributorStatus.Pending) + contributorService.GetCountContributorsByAcceptanceStatusId((int)ContributorStatus.Registered) + contributorService.GetCountContributorsByAcceptanceStatusId((int)ContributorStatus.Enabled);
                model.TotalContributors = string.Format(CultureInfo.InvariantCulture, "{0:#,#}", totalContributor).Replace(",", ".");

                model.ContributorAcceptanceStatusId = (int)ContributorStatus.Pending;
                model.ContributorHabilitationDate = DateTime.UtcNow;
                model.ContributorProductionDate = DateTime.UtcNow;

                var loginMenu = "OFE";
                Session["loginMenu"] = loginMenu;
            }
            else
            {
                var contributorCode = User.ContributorCode();
                var contributorTypeId = User.ContributorTypeId()?.ToString();
                var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributorCode);
                if (testSetResults.Any(t => !t.Deleted && t.ContributorTypeId == contributorTypeId && t.Status == (int)TestSetStatus.InProcess) && User.ContributorAcceptanceStatusId() == (int)ContributorStatus.Registered)
                {
                    var testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.ContributorTypeId == contributorTypeId && t.Status == (int)TestSetStatus.InProcess);
                    var softwareId = testSetResult.SoftwareId;
                    model.ContributorTestSetMessageInfo = $@"
                        <div class='dian-alert dian-alert-info mb-20'>
                            <i class='fa fa-info-circle' style='margin-right: 15px;'></i>
                            <p>
                                <strong>Estimado contribuyente:</strong> <br>
                                Su empresa se encuentra en el proceso de pruebas de validación, el set de pruebas se encuentra <strong>{Domain.Common.EnumHelper.GetEnumDescription(TestSetStatus.InProcess)}</strong> <br>
                                Usted debe proporcionar el identificador del set de pruebas (TestSetId) <strong>{testSetResult.Id}</strong> en el web services para el envío de su set de pruebas. <br>
                                Para dar seguimiento al proceso haga click <a href='{Url.Action("Tracking", "TestSet", new { contributorCode, contributorTypeId, softwareId })}'>aquí</a>.
                            </p>
                        </div>";
                }
                else if (testSetResults.Any(t => !t.Deleted && t.ContributorTypeId == contributorTypeId && t.Status == (int)TestSetStatus.Rejected))
                {
                    var testSetResult = testSetResults.SingleOrDefault(t => !t.Deleted && t.ContributorTypeId == contributorTypeId && t.Status == (int)TestSetStatus.Rejected);
                    var softwareId = testSetResult.SoftwareId;
                    model.ContributorTestSetMessageInfo = $@"
                        <div class='dian-alert dian-alert-danger mb-20'>
                            <i class='fa fa-info-circle' style='margin-right: 15px;'></i>
                            <p>
                                <strong>Estimado contribuyente:</strong> <br>
                                Su empresa se encuentra en el proceso de pruebas de validación, el set de pruebas se encuentra <strong>{Domain.Common.EnumHelper.GetEnumDescription(TestSetStatus.Rejected)}</strong> <br>
                                Usted debe proporcionar el identificador del set de pruebas (TestSetId) <strong>{testSetResult.Id}</strong> en el web services para el envío de su set de pruebas. <br>
                                Para dar seguimiento al proceso haga click <a href='{Url.Action("Tracking", "TestSet", new { contributorCode, contributorTypeId, softwareId })}'>aquí</a>.
                            </p>
                        </div>";
                }

                var contributor = contributorService.Get(User.ContributorId());
                model.ContributorAcceptanceStatusId = contributor.AcceptanceStatusId;
                model.ContributorHabilitationDate = contributor.HabilitationDate ?? DateTime.UtcNow;
                model.ContributorProductionDate = contributor.ProductionDate;
                model.ContributorTypeId = contributor.ContributorTypeId.GetValueOrDefault();

                var identificatioType = User.IdentificationTypeId();

                var pk = identificatioType + "|" + User.UserCode();
                var rk = contributorCode;
                var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
                var loginMenu = "";
                if (auth != null)
                {
                    loginMenu = auth.LoginMenu;
                }
                else
                {
                    auth = dianAuthTableManager.FindPartitionKey<AuthToken>(pk);
                    loginMenu = auth.LoginMenu;
                }

                Session["loginMenu"] = loginMenu;

                if (auth.LoginMenu == "NO OFE")
                {
                    Session["Login_ContributorType"] = "- No OFE";
                }
                else
                {
                    Session["Login_ContributorType"] = "";
                }
            }

            return View(model);
        }

        public async Task<JsonResult> GetEmittedAndreceivedTotal()
        {
            int totalDocumentsEmitted = 0;
            int totalDocumentsReceived = 0;
            var contributorCode = User.ContributorCode();
            try
            {
                var utcNow = DateTime.UtcNow;
                var emmited = new List<GlobalDataDocument>();
                bool hasMoreResults = true;
                var ct = "";
                //do
                //{
                //    var result = await CosmosDBService.Instance(utcNow).ReadDocumentsAsync(ct, utcNow.AddDays(-30), utcNow, 0, "00", contributorCode, null, null, null, 1000, null, "00");
                //    hasMoreResults = result.Item1;
                //    ct = result.Item2;
                //    emmited.AddRange(result.Item3);
                //} while (hasMoreResults && emmited.Count() < 10000);
                //totalDocumentsEmitted = emmited.Count();


                //TODO Analizar después

                totalDocumentsEmitted = 0;

                var received = new List<GlobalDataDocument>();
                hasMoreResults = true;
                ct = "";
                //do
                //{
                //    var result = await CosmosDBService.Instance(utcNow).ReadDocumentsAsync(ct, utcNow.AddDays(-30), utcNow, 0, "00", null, null, contributorCode, null, 1000, null, "00");
                //    hasMoreResults = result.Item1;
                //    ct = result.Item2;
                //    received.AddRange(result.Item3);
                //} while (hasMoreResults && received.Count() < 10000);
                //totalDocumentsReceived = received.Count();

                totalDocumentsReceived = 0;

            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    emitted = totalDocumentsEmitted == 10000 ? $"+ { totalDocumentsEmitted}" : $"{totalDocumentsEmitted}",
                    received = totalDocumentsReceived == 10000 ? $"+ { totalDocumentsReceived}" : $"{totalDocumentsReceived}",
                }, JsonRequestBehavior.AllowGet);
            }

            var json = Json(new
            {
                success = true,
                emitted = totalDocumentsEmitted,
                received = totalDocumentsReceived
            }, JsonRequestBehavior.AllowGet);
            return json;
        }

        private string GetMonthName(string month)
        {
            switch (month)
            {
                case "01":
                    return "Enero";
                case "02":
                    return "Febrero";
                case "03":
                    return "Marzo";
                case "04":
                    return "Abril";
                case "05":
                    return "Mayo";
                case "06":
                    return "Junio";
                case "07":
                    return "Julio";
                case "08":
                    return "Agosto";
                case "09":
                    return "Septiembre";
                case "10":
                    return "Octubre";
                case "11":
                    return "Noviembre";
                case "12":
                    return "Diciembre";
                default:
                    return "Mes inválido";
            }
        }
    }
}