using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Gosocket.Dian.Domain.Common;


namespace Gosocket.Dian.Web.Controllers
{
    [Authorization]
    public class AccountController : Controller
    {
        private ContributorService _contributorService = new ContributorService();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        //
        // GET: /Account/Login
        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl, model.Email);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Intento inválido de login.");
                    return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction(nameof(UserController.Login), "User");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins 
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Login");
        }

        private ActionResult RedirectToLocal(string returnUrl, string userName)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            var user = UserManager.FindByName(userName);
            //if (UserManager.IsInRole(user.Id, "Proveedor"))
            //{
            //    return RedirectToAction("ProviderStatus", "Provider");
            //}
            //else if (UserManager.IsInRole(user.Id, "Facturador"))
            //{
            //    //Check for UserClient
            //    var userClients = _clientService.GetUserClients(user.UserName);
            //    if (userClients.Any())
            //    {
            //        return RedirectToAction("Client", "Dashboard");
            //    }
            //    else
            //    {
            //        return RedirectToAction("RegisterClient", "Client");
            //    }
            //}
            //else
            //{
                return RedirectToAction("Admin", "Dashboard");
            //}
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult RedirectByRole()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.ContributorTypeId() == (int)Domain.Common.ContributorType.Provider || User.ContributorTypeId() == (int)Domain.Common.ContributorType.Provider)
                {
                    return RedirectToAction("Provider", "Dashboard");
                }
                else if (User.ContributorTypeId() == (int)Domain.Common.ContributorType.Biller || User.ContributorTypeId() == null)
                {
                    return RedirectToAction("Client", "Dashboard");
                }
                else if (User.IsInAnyRole("Administrador", "Super"))
                {
                    return RedirectToAction("Admin", "Dashboard");
                }
                else if (User.IsInAnyRole("UsuarioFacturadorGratuito"))
                {
                    return RedirectToAction("RedirectToBiller", "User");
                }
                else
                {
                    

                }
            }
            return RedirectToAction("Login", "AccountTwoFactor");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion


    }
}