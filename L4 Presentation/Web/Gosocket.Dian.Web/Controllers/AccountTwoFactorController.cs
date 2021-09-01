using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Utils;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Authorization = Gosocket.Dian.Web.Filters.Authorization;

namespace Gosocket.Dian.Web.Controllers
{
    [Authorization]
    public class AccountTwoFactorController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private static readonly TableManager dianAuthTableManager = new TableManager("DianAuthToken");
        private IdentificationTypeService identificationTypeService = new IdentificationTypeService();
        private UserService userService = new UserService();


        public AccountTwoFactorController()
        {

        }

        public AccountTwoFactorController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        public RedirectResult RedirectToBiller()
        {
            var auth = dianAuthTableManager.Find<AuthToken>($"{User.IdentificationTypeId()}|{User.UserCode()}", User.ContributorCode());
            var redirectUrl = ConfigurationManager.GetValue("BillerAuthUrl") + $"pk={auth.PartitionKey}&rk={auth.RowKey}&token={auth.Token}";
            return Redirect(redirectUrl);
        }

        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> Login(string returnUrl)
        {
            List<string> listErrors = new List<string>();
            if (System.Configuration.ConfigurationManager.AppSettings["LoginType"] == "Certificate")
            {
                HttpClientCertificate cert = Request.ClientCertificate;
                if (cert.IsPresent && cert.IsValid)
                {
                    X509Certificate2 X509 = new X509Certificate2(Request.ClientCertificate.Certificate);
                    Dictionary<string, string> subject = GetSubjectProccesed(X509.Subject);
                    var certValidator = new CertificateValidator();
                    certValidator.Validate(X509);

                    ApplicationUser user = await SignInManager.UserManager.FindByEmailAsync(subject["E"]);
                    Contributor contributor = user.Contributors.SingleOrDefault(x => x.Code == subject["SERIALNUMBER"]);
                    if (user != null && user.Contributors != null && contributor != null)
                    {
                        user.Code = user.Code;
                        user.ContributorCode = contributor.Code;
                        await SignInManager.SignInAsync(user, true, false);
                        return RedirectToAction("Dashboard", "Home");
                    }

                }
                listErrors.Add("No se reconoce el certificado proporcionado");
            }

            ViewBag.ListErrors = listErrors;
            ViewBag.ReturnUrl = returnUrl;
            LoginTwoFactorViewModel model = new LoginTwoFactorViewModel();
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList();
            return View(model);


        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult LoginNaturalPerson()
        {
            LoginTwoFactorViewModel model = new LoginTwoFactorViewModel();
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList();
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Authentication(LoginTwoFactorViewModel model, string returnUrl)
        {
            if (String.IsNullOrEmpty(model.CompanyCode)) model.CompanyCode = model.UserCode;
            var auth = dianAuthTableManager.Find<AuthToken>($"{model.IdentificationType}|{model.UserCode}", model.CompanyCode);
            if (auth == null)
                ViewBag.ErrorMessage = "No se ha encontrado un Representante Legal con el NIT especificado.";
            else
            {
                TimeSpan timeSpan = DateTime.UtcNow.Subtract(auth.Timestamp.DateTime);
                if (timeSpan.TotalMinutes > 60 || string.IsNullOrEmpty(auth.Token))
                {
                    auth.Token = Guid.NewGuid().ToString();
                    auth.Status = true;
                    dianAuthTableManager.InsertOrUpdate(auth);
                }

                var accessUrl = ConfigurationManager.GetValue("AccountTwoFactorAuthTokenUrl") + $"pk={auth.PartitionKey}&rk={auth.RowKey}&token={auth.Token}";
                if (ConfigurationManager.GetValue("Environment") == "Production")
                {
                    var emailSenderResponse = await EmailUtil.SendEmailAsync(auth, accessUrl);
                    if (emailSenderResponse.ErrorType != ErrorTypes.NoError)
                    {
                        ViewBag.ErrorMessage = "Ha ocurrido un error al enviar el correo electrónico, por favor intente nuevamente.";
                    }
                }

                ViewBag.Url = accessUrl;
            }
            return View("LoginConfirmed", model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pk"></param>
        /// <param name="rk"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> AuthToken(string pk, string rk, string token)
        {
            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            TimeSpan timeSpan = DateTime.UtcNow.Subtract(auth.Timestamp.DateTime);
            if (auth != null && auth.Token == token && timeSpan.TotalMinutes <= 60)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(auth.UserId);
                user.Code = pk.Split('|')[1];
                user.ContributorCode = rk;
                await SignInManager.SignInAsync(user, true, false);
                return RedirectToAction(nameof(HomeController.Dashboard), "Home");
            }

            List<string> listErrors = new List<string>();
            ViewBag.ListErrors = new List<string>() { "Token expirado, por favor intente nuevamente." };
            return View("Login");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private ActionResult RedirectToLocal(string returnUrl, string userName)
        {
            var user = UserManager.FindByName(userName);
            if (UserManager.IsInRole(user.Id, "Proveedor"))
                return RedirectToAction(nameof(ContributorController.Check), "Contributor");
            else return RedirectToAction(nameof(HomeController.Dashboard), "Home");
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

        private Dictionary<string, string> GetSubjectProccesed(string subject)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                string[] subjectSplited = subject.Split(',');
                foreach (var item in subjectSplited)
                {
                    string[] itemSplit = item.Split('=');
                    result.Add(itemSplit[0].Trim(), itemSplit[1]);
                }
            }
            catch (Exception)
            {
                return result;
            }
            return result;
        }

        #endregion
    }
}