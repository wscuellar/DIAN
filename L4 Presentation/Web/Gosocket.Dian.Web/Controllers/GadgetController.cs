using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Utils;
using System;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class GadgetController : Controller
    {
        private UserService userService = new UserService();
        private readonly TableManager dianAuthTableManager = new TableManager("AuthToken");

        // GET: Gadget
        public ActionResult Open(string url, string pageName, Navigation.NavigationEnum navigation)
        {
            if (!User.IsInAnyRole("Administrador", "Super"))
                return RedirectToAction(nameof(UserController.Unauthorized), "User");
            if (navigation != Navigation.NavigationEnum.StatisticsBI)
                ViewBag.Url = $"{url}?token={GeToken()}";
            else
                ViewBag.Url = url;
            ViewBag.CurrentPage = navigation;
            ViewBag.PageName = pageName;

            if (navigation == Navigation.NavigationEnum.DocumentValidator)
                ViewBag.PageHeight = 1000;
            else
                ViewBag.PageHeight = 700;

            return View();
        }

        private string GeToken()
        {
            var user = userService.GetByEmail(User.Identity.Name);
            var auth = dianAuthTableManager.Find<AuthToken>(user.Id, user.Id);
            if (auth == null)
            {
                auth = new AuthToken(user.Id, user.Id) { UserId = user.Id, Email = user.Email, Token = Guid.NewGuid().ToString(), Status = true };
                dianAuthTableManager.InsertOrUpdate(auth);
            }
            else
            {
                TimeSpan timeSpan = DateTime.UtcNow.Subtract(auth.Timestamp.DateTime);
                if (timeSpan.TotalMinutes > 20 || string.IsNullOrEmpty(auth.Token))
                {
                    auth.UserId = user.Id;
                    auth.Email = user.Email;
                    auth.Token = Guid.NewGuid().ToString();
                    auth.Status = true;
                    dianAuthTableManager.InsertOrUpdate(auth);
                }
            }
            return $"{auth.UserId}|{auth.Token}";
        }
    }
}