using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.Role;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class SettingsController : Controller
    {
        private static readonly TableManager tableManagerGlobalContingency = new TableManager("GlobalContingency");
        private UserService userService = new UserService();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult AddContingency()
        {
            var model = new ContingencyViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult AddContingency(ContingencyViewModel model)
        {

            if (!ValidateDates(model.StartDateString, model.EndDateString))
                return View(model);

            ContingencyViewModel.SetDateNumber(ref model);

            if (!ValidateContingency(ref model, true))
                return View(model);

            var guid = Guid.NewGuid().ToString();
            var contingency = new GlobalContingency(guid, guid)
            {
                StartDateNumber = model.StartDateNumber,
                EndDateNumber = model.EndDateNumber,
                Reason = model.Reason,
                CreatedBy = User.Identity.Name
            };
            tableManagerGlobalContingency.InsertOrUpdate(contingency);

            return RedirectToAction(nameof(Contingencies));
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult ChangeStatusContingency(string pk, string rk)
        {
            var globalContingency = tableManagerGlobalContingency.Find<GlobalContingency>(pk, rk);
            globalContingency.Active = !globalContingency.Active;
            globalContingency.UpdatedDate = DateTime.UtcNow;
            globalContingency.UpdatedBy = User.Identity.Name;
            tableManagerGlobalContingency.InsertOrUpdate(globalContingency);
            return RedirectToAction(nameof(Contingencies));
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Contingencies()
        {
            var model = new ContingencyTableViewModel();
            GetContingencies(ref model);

            ViewBag.CurrentPage = Navigation.NavigationEnum.Contingencies;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Contingencies(ContingencyTableViewModel model)
        {
            if (!ValidateDates(model.StartDateString, model.EndDateString))
            {
                model = new ContingencyTableViewModel();
                GetContingencies(ref model);
                return View(model);
            }

            GetContingencies(ref model);

            ViewBag.CurrentPage = Navigation.NavigationEnum.Contingencies;
            return View(model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult DeleteContingency(string pk, string rk)
        {
            var globalContingency = tableManagerGlobalContingency.Find<GlobalContingency>(pk, rk);
            globalContingency.Deleted = true;
            globalContingency.DeletedDate = DateTime.UtcNow;
            globalContingency.DeletedBy = User.Identity.Name;
            tableManagerGlobalContingency.InsertOrUpdate(globalContingency);
            return RedirectToAction(nameof(Contingencies));
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult EditContingency(string pk, string rk)
        {
            var model = new ContingencyViewModel() { PartitionKey = pk, RowKey = rk };
            GetContingency(ref model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult EditContingency(ContingencyViewModel model)
        {
            if (!ValidateDates(model.StartDateString, model.EndDateString))
                return View(model);

            ContingencyViewModel.SetDateNumber(ref model);

            if (!ValidateContingency(ref model))
                return View(model);

            var contingency = new GlobalContingency(model.PartitionKey, model.RowKey)
            {
                Active = model.Active,
                StartDateNumber = model.StartDateNumber,
                EndDateNumber = model.EndDateNumber,
                CreatedBy = model.CreatedBy,
                CreatedDate = model.CreatedDate,
                DeletedBy = model.DeletedBy,
                DeletedDate = model.DeletedDate,
                UpdatedBy = User.Identity.Name,
                Reason = model.Reason
            };
            tableManagerGlobalContingency.InsertOrUpdate(contingency);

            return RedirectToAction(nameof(Contingencies)); ;
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult AddUser()
        {
            UserViewModel model = new UserViewModel { };
            ViewBag.CurrentPage = Navigation.NavigationEnum.LegalRepresentative;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public async Task<ActionResult> AddUser(UserViewModel model)
        {
            var user = userService.GetByEmail(model.Email);

            if (user != null && !UserManager.IsInRole(user.Id, Roles.Administrator))
            {
                await UserManager.AddToRoleAsync(user.Id, Roles.Administrator);
                return RedirectToAction(nameof(Users));
            }

            if (user != null && UserManager.IsInRole(user.Id, Roles.Administrator))
            {
                ModelState.AddModelError("Email", "Usuario ya cuenta con el rol Administrador.");
                return View(model);
            }

            user = new ApplicationUser
            {
                Code = Guid.NewGuid().ToString().Substring(0, 6),
                Name = model.Name,
                Email = model.Email,
                PasswordHash = UserManager.PasswordHasher.HashPassword(model.Email.Split('@')[0]),
                UserName = model.Email,
                CreatedBy = User.Identity.Name
            };

            var result = await UserManager.CreateAsync(user);
            if (result.Succeeded) result = await UserManager.AddToRoleAsync(user.Id, Roles.Administrator);
            if (result.Succeeded) return RedirectToAction(nameof(Users));

            foreach (var item in result.Errors)
                ModelState.AddModelError(string.Empty, item);

            foreach (var item in ModelState)
                if (item.Key.Contains("Code"))
                    item.Value.Errors.Clear();

            return View(model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult UpdateUserRole(string id, string role, bool value)
        {
            var user = userService.Get(id);
            if (value) UserManager.AddToRole(id, role);
            else UserManager.RemoveFromRole(id, role);

            return Json(new { ok = true }, JsonRequestBehavior.AllowGet);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Users()
        {
            var model = new UserTableViewModel();
            var users = userService.GetUsersWithRoles(null, model.Page, model.Length);
            model.Users = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                Email = u.Email,
                IsAdmin = UserManager.IsInRole(u.Id, Roles.Administrator),
            }).ToList();

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.Users;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Users(UserTableViewModel model)
        {
            var users = new List<ApplicationUser>();
            if (string.IsNullOrEmpty(model.Email))
                users = userService.GetUsersWithRoles(null, model.Page, model.Length).ToList();
            else
            {
                var user = userService.GetByEmail(model.Email);
                if (user != null) users.Add(user);
            }

            model.Users = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                Email = u.Email,
                IsAdmin = UserManager.IsInRole(u.Id, Roles.Administrator),
            }).ToList();

            model.SearchFinished = true;
            return View(model);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult UserView(string id)
        {
            ApplicationUser applicationUser = UserManager.FindById(id);
            if (applicationUser == null)
                return RedirectToAction(nameof(ErrorController.Error400), "Error");

            UserViewModel userViewModel = new UserViewModel
            {
                Id = applicationUser.Id,
                Name = applicationUser.Name,
                Email = applicationUser.Email,
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.UserView;
            return View(userViewModel);
        }

        public void GetContingency(ref ContingencyViewModel model)
        {
            var globalContingency = tableManagerGlobalContingency.Find<GlobalContingency>(model.PartitionKey, model.RowKey);
            if (globalContingency != null)
            {
                model.Active = globalContingency.Active;
                model.PartitionKey = globalContingency.PartitionKey;
                model.RowKey = globalContingency.RowKey;
                model.StartDateString = DateTime.ParseExact(globalContingency.StartDateNumber.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy HH:mm:ss");
                model.EndDateString = DateTime.ParseExact(globalContingency.EndDateNumber.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy HH:mm:ss");
                model.Reason = globalContingency.Reason;
                model.CreatedBy = globalContingency.CreatedBy;
                model.CreatedDate = globalContingency.CreatedDate;
                model.DeletedBy = globalContingency.DeletedBy;
                model.DeletedDate = globalContingency.DeletedDate;
                model.UpdatedBy = globalContingency.UpdatedBy;
                model.UpdatedDate = globalContingency.UpdatedDate;
            }
        }
        private void GetContingencies(ref ContingencyTableViewModel model)
        {
            TableContinuationToken continuationToken = null;
            continuationToken = (TableContinuationToken)Session[$"NextTableContinuationTokenContingencies_{model.Page}"];
            if (model.Page == 0)
                continuationToken = null;

            IEnumerable<GlobalContingency> contingencies = new List<GlobalContingency>();

            var result = tableManagerGlobalContingency.GetRangeRows<GlobalContingency>(model.Length, continuationToken);
            contingencies = result.Item1;
            Session[$"NextTableContinuationTokenContingencies_{model.Page + 1}"] = result.Item2;

            var startDateString = $"{model.StartDateString}T00:00";
            var endDateString = $"{model.EndDateString}T23:59";
            var startDateNumber = long.Parse(DateTime.ParseExact(startDateString, "dd-MM-yyyyTHH:mm", CultureInfo.InvariantCulture).ToString("yyyyMMddHHmmss"));
            var endDateNumber = long.Parse(DateTime.ParseExact(endDateString, "dd-MM-yyyyTHH:mm", CultureInfo.InvariantCulture).ToString("yyyyMMddHHmmss"));

            contingencies = contingencies.Where(c => !c.Deleted).ToList();
            model.Contingencies = contingencies.Select(c => new ContingencyViewModel
            {
                Active = c.Active,
                PartitionKey = c.PartitionKey,
                RowKey = c.RowKey,
                StartDateString = DateTime.ParseExact(c.StartDateNumber.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy HH:mm"),
                EndDateString = DateTime.ParseExact(c.EndDateNumber.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy HH:mm"),
                StartDateNumber = c.StartDateNumber,
                EndDateNumber = c.EndDateNumber,
                Reason = c.Reason,
                CreatedBy = c.CreatedBy,
                CreatedDate = c.CreatedDate,
                DeletedBy = c.DeletedBy,
                DeletedDate = c.DeletedDate,
                UpdatedBy = c.UpdatedBy,
                UpdatedDate = c.UpdatedDate,
            }).OrderBy(c => c.StartDateString).ToList();

        }
        private bool ValidateContingency(ref ContingencyViewModel model, bool isNew = false)
        {
            if (20190501000000 > model.StartDateNumber)
            {
                ModelState.AddModelError("StartDateString", "Fecha de inicio no puede ser anterior a 01-05-2019.");
                return false;
            }
            if (model.StartDateNumber > model.EndDateNumber)
            {
                ModelState.AddModelError("EndDateString", "Fecha de termino no puede ser anterior a fecha de inicio.");
                return false;
            }

            if (isNew)
            {
                var globalContingency = tableManagerGlobalContingency.Find<GlobalContingency>(model.StartDateNumber.ToString(), model.EndDateNumber.ToString());
                if (globalContingency != null)
                {
                    ModelState.AddModelError("StartDateString", "Fecha de inicio registrada previamente.");
                    ModelState.AddModelError("EndDateString", "Fecha de termino registrada previamente.");
                    return false;
                }
            }

            return true;
        }
        public bool ValidateDates(string startDateString, string endDateString)
        {
            bool isValid = true;
            if (!ContingencyViewModel.IsValidDate(startDateString))
            {
                ModelState.AddModelError("StartDateString", "Fecha de inicio inválida.");
                isValid = false;
            }
            if (!ContingencyViewModel.IsValidDate(endDateString))
            {
                ModelState.AddModelError("EndDateString", "Fecha final inválida.");
                isValid = false;
            }

            return isValid;

            //if(!DateTime.TryParse(startDateString, out DateTime startDate))
            //{
            //    ModelState.AddModelError("StartDateString", "Fecha de inicio inválida.");
            //    isValid = false;
            //}

            //if (!DateTime.TryParse(endDateString, out DateTime endDate))
            //{
            //    ModelState.AddModelError("EndDateString", "Fecha final inválida.");
            //    isValid = false;
            //}
        }
    }
}