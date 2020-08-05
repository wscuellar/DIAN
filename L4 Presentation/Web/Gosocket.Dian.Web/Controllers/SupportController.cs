using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.Role;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class SupportController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

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
        private readonly TableManager dianAuthTableManager = new TableManager("AuthToken");

        private IdentificationTypeService identificationTypeService = new IdentificationTypeService();
        private ContributorService contributorService = new ContributorService();
        private UserService userService = new UserService();

        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> AuthToken(string pk, string rk, string type, string token)
        {
            var model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                IdentificationType = (int)Domain.Common.IdentificationType.CC
            };

            var view = "CompanyLogin";
            var errorField = "CompanyLoginFailed";
            if (type == AuthType.Person.GetDescription())
            {
                view = "PersonLogin";
                errorField = "PersonLoginFailed";
            }

            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            TimeSpan timeSpan = DateTime.UtcNow.Subtract(auth.Timestamp.DateTime);
            if (auth != null && auth.Token == token && timeSpan.TotalMinutes <= 60)
            {
                
                ApplicationUser user = await UserManager.FindByIdAsync(auth.UserId);

                if (user == null)
                {
                    dianAuthTableManager.Delete(auth);
                    ModelState.AddModelError(errorField, "Persona natural no se encuentra registrada.");
                    return View(view, model);
                }

                user.ContributorCode = rk;
                await SignInManager.SignInAsync(user, true, false);
                return RedirectToAction(nameof(HomeController.Dashboard), "Home");
            }

            ModelState.AddModelError(errorField, "Token expirado, por favor intente nuevamente.");
            return View(view, model);
        }

        /// <summary>
        /// Support company login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [ExcludeFilter(typeof(Authorization))]
        public ActionResult CompanyLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            UserLoginViewModel model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                IdentificationType = (int)Domain.Common.IdentificationType.CC
            };
            return View(model);
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> CompanyLogin(UserLoginViewModel model, string returnUrl)
        {
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel
            {
                Id = x.Id,
                Description = x.Description
            }).ToList();

            ClearUnnecessariesModelStateErrorsForAuthentication(false);

            ValidateCaptcha(model);
            if (!ModelState.IsValid) return View("CompanyLogin", model);

            var user = ValidateUser(model, "CompanyLoginFailed", (int)PersonType.Juridic);
            if (!ModelState.IsValid) return View("CompanyLogin", model);

            var contributor = ValidateContributor(model, user, "CompanyLoginFailed", (int)PersonType.Juridic);
            if (!ModelState.IsValid) return View("CompanyLogin", model);

            await ValidateAdmin(model);
            if (!ModelState.IsValid) return View("CompanyLogin", model);

            var pk = $"{model.IdentificationType}|{model.UserCode}";
            var rk = $"{model.CompanyCode}";

            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            if (auth == null)
            {
                auth = new AuthToken(pk, rk) { UserId = user.Id, Email = user.Email, ContributorId = contributor.Id, Type = AuthType.Company.GetDescription(), Token = Guid.NewGuid().ToString(), Status = true };
                dianAuthTableManager.InsertOrUpdate(auth);
            }
            else
            {
                TimeSpan timeSpan = DateTime.UtcNow.Subtract(auth.Timestamp.DateTime);
                if (timeSpan.TotalMinutes > 60 || string.IsNullOrEmpty(auth.Token))
                {
                    auth.UserId = user.Id;
                    auth.Email = user.Email;
                    auth.ContributorId = contributor.Id;
                    auth.Type = AuthType.Company.GetDescription();
                    auth.Token = Guid.NewGuid().ToString();
                    auth.Status = true;
                    dianAuthTableManager.InsertOrUpdate(auth);
                }
            }

            var token = auth.Token;
            return RedirectToAction(nameof(AuthToken), new { pk = auth.PartitionKey, rk = auth.RowKey, type = AuthType.Company.GetDescription(), token });
        }

        /// <summary>
        /// Support person login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [ExcludeFilter(typeof(Authorization))]
        public ActionResult PersonLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            UserLoginViewModel model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                IdentificationType = (int)Domain.Common.IdentificationType.CC
            };
            return View(model);
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> PersonLogin(UserLoginViewModel model, string returnUrl)
        {
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel
            {
                Id = x.Id,
                Description = x.Description
            }).ToList();

            ClearUnnecessariesModelStateErrorsForAuthentication(true);

            ValidateCaptcha(model);
            if (!ModelState.IsValid) return View("PersonLogin", model);

            var user = ValidateUser(model, "PersonLoginFailed", (int)PersonType.Natural);
            if (!ModelState.IsValid) return View("PersonLogin", model);

            var contributor = ValidateContributor(model, user, "PersonLoginFailed", (int)PersonType.Natural);
            if (!ModelState.IsValid) return View("PersonLogin", model);

            await ValidateAdmin(model);
            if (!ModelState.IsValid) return View("PersonLogin", model);

            var pk = $"{model.IdentificationType}|{model.PersonCode}";
            var rk = $"{user.Contributors.FirstOrDefault(c => c.PersonType == (int)PersonType.Natural && c.Name.ToLower() == user.Name.ToLower())?.Code}";

            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            if (auth == null)
            {
                auth = new AuthToken(pk, rk) { UserId = user.Id, Email = user.Email, ContributorId = contributor.Id, Type = AuthType.Person.GetDescription(), Token = Guid.NewGuid().ToString(), Status = true };
                dianAuthTableManager.InsertOrUpdate(auth);
            }
            else
            {
                TimeSpan timeSpan = DateTime.UtcNow.Subtract(auth.Timestamp.DateTime);
                if (timeSpan.TotalMinutes > 60 || string.IsNullOrEmpty(auth.Token))
                {
                    auth.UserId = user.Id;
                    auth.Email = user.Email;
                    auth.ContributorId = contributor.Id;
                    auth.Type = AuthType.Person.GetDescription();
                    auth.Token = Guid.NewGuid().ToString();
                    auth.Status = true;
                    dianAuthTableManager.InsertOrUpdate(auth);
                }
            }

            var token = auth.Token;
            return RedirectToAction(nameof(AuthToken), new { pk = auth.PartitionKey, rk = auth.RowKey, type = AuthType.Person.GetDescription(), token });
        }

        #region Private methods
        private void ClearUnnecessariesModelStateErrorsForAuthentication(bool person = false)
        {
            foreach (var item in ModelState)
            {
                if (person && item.Key.Contains("panyCode"))
                    item.Value.Errors.Clear();
                if (person && item.Key.Contains("UserCode"))
                    item.Value.Errors.Clear();
                if (!person && item.Key.Contains("PersonCode"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("DocumentKey"))
                    item.Value.Errors.Clear();
            }
        }
        private Tuple<bool, string> IsValidCaptcha(string token)
        {

            bool recaptchaIsEnable = Convert.ToBoolean(ConfigurationManager.GetValue("RecaptchaIsEnable"));
            if (recaptchaIsEnable)
            {
                var secret = ConfigurationManager.GetValue("RecaptchaServer");
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(ConfigurationManager.GetValue("RecaptchaUrl") + "?secret=" + secret + "&response=" + token);

                using (var wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string responseFromServer = readStream.ReadToEnd();
                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseFromServer);
                        if (jsonResponse.success.ToObject<bool>() && jsonResponse.score.ToObject<float>() > 0.4)
                            return Tuple.Create(true, "Ok");
                        else if (jsonResponse["error-codes"] != null && jsonResponse["error-codes"].ToObject<List<string>>().Contains("timeout-or-duplicate"))
                            return Tuple.Create(false, "Recaptcha inválido.");
                        else
                            return Tuple.Create(false, "Recaptcha inválido.");
                        //throw new Exception(jsonResponse.ToString());
                    }
                }
            }
            else
            {
                return Tuple.Create(true, "Ok");
            }
        }
        private async Task ValidateAdmin(UserLoginViewModel model)
        {
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    var _user = userService.GetByEmail(model.Email);
                    if (!UserManager.IsInRole(_user.Id, Roles.Administrator))
                        ModelState.AddModelError($"AdminLoginFailed", "Usuario no cuenta con rol de Administrador.");
                    break;
                case SignInStatus.LockedOut:
                    ModelState.AddModelError($"AdminLoginFailed", "Usuario bloqueado.");
                    break;
                case SignInStatus.RequiresVerification:
                    break;
                case SignInStatus.Failure:
                    ModelState.AddModelError($"AdminLoginFailed", "Correo electrónico o contraseña no concuerdan.");
                    break;
                default:
                    ModelState.AddModelError($"AdminLoginFailed", "Correo electrónico o contraseña no concuerdan.");
                    break;
            }
        }
        private void ValidateCaptcha(UserLoginViewModel model)
        {
            var recaptchaValidation = IsValidCaptcha(model.RecaptchaToken);
            if (!recaptchaValidation.Item1) ModelState.AddModelError($"CompanyLoginFailed", recaptchaValidation.Item2);
        }
        private Contributor ValidateContributor(UserLoginViewModel model, ApplicationUser user, string errorField, int personType)
        {
            Contributor contributor = null;
            if (personType == (int)PersonType.Juridic)
            {
                contributor = user.Contributors.FirstOrDefault(c => c.Code == model.CompanyCode);
                if (contributor == null) ModelState.AddModelError(errorField, "Empresa no asociada a representante legal.");
            }
            else
            {
                contributor = user.Contributors.FirstOrDefault(c => c.PersonType == (int)PersonType.Natural && c.Name.ToLower() == user.Name.ToLower());
                if (contributor == null) ModelState.AddModelError(errorField, "Persona natural sin permisos asociados.");
            }

            if (contributor != null && contributor.StatusRut == (int)StatusRut.Cancelled) ModelState.AddModelError(errorField, "Contribuyente tiene RUT en estado cancelado.");
            if (contributor != null && ConfigurationManager.GetValue("Environment") == "Prod" && contributor.AcceptanceStatusId != (int)ContributorStatus.Enabled)
            {
                if (personType == (int)PersonType.Juridic) ModelState.AddModelError(errorField, "Empresa no se encuentra habilitada.");
                else ModelState.AddModelError(errorField, "Usted no se ecuentra habilitado.");
            }
            return contributor;
        }
        private ApplicationUser ValidateUser(UserLoginViewModel model, string errorField, int personType)
        {
            var code = model.UserCode;
            if (personType == (int)PersonType.Natural) code = model.PersonCode;
            var user = userService.GetByCodeAndIdentificationTyte(code, model.IdentificationType);
            if (user == null) ModelState.AddModelError(errorField, "Número de documento y tipo de identificación no coinciden.");
            return user;
        }
        #endregion
    }
}