using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Utils;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.Role;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class UserController : Controller
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
        private readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        private readonly TableManager globalDocValidatorDocumentTableManager = new TableManager("GlobalDocValidatorDocument");
        private readonly TableManager globalDocValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");

        private IdentificationTypeService identificationTypeService = new IdentificationTypeService();
        private ContributorService contributorService = new ContributorService();
        private UserService userService = new UserService();

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult List(string type)
        {
            var model = new UserTableViewModel();
            var users = userService.GetUsers(null, -1, model.Page, model.Length);
            model.Users = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Code = u.Code,
                Name = u.Name,
                Email = u.Email
            }).ToList();

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.LegalRepresentative;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult List(UserTableViewModel model)
        {

            var users = new List<ApplicationUser>();


            if (string.IsNullOrEmpty(model.Code))
            {
                users = userService.GetUsers(null, -1, model.Page, model.Length);
            }
            else
            {
                var user = userService.GetByCode(model.Code);
                if (user != null)
                {
                    users.Add(user);
                }
            }

            model.Users = users.Select(c => new UserViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Email = c.Email
            }).ToList();

            model.SearchFinished = true;
            ViewBag.CurrentPage = Navigation.NavigationEnum.LegalRepresentative;
            return View(model);
        }

        [CustomRoleAuthorization(CustomRoles = "Super")]
        public ActionResult Add()
        {
            UserViewModel model = new UserViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList()
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.LegalRepresentative;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Super")]
        public async Task<ActionResult> Add(UserViewModel model)
        {
            var user = new ApplicationUser
            {
                IdentificationTypeId = model.IdentificationType,
                Code = model.Code,
                Name = model.Name,
                Email = model.Email,
                UserName = model.Email,
                CreatedBy = User.Identity.Name
            };
            var result = await UserManager.CreateAsync(user);
            return RedirectToAction(nameof(View), new { user.Id });
        }

        public JsonResult AddContributor(string id, string code)
        {
            try
            {
                if (!User.IsInAnyRole("Super"))
                    return Json(new
                    {
                        success = false,
                        message = "401 - Unathorized"
                    }, JsonRequestBehavior.AllowGet);

                var contributor = contributorService.GetByCode(code);
                if (contributor == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "NIT ingresado no encontrado en los registros de la DIAN."
                    }, JsonRequestBehavior.AllowGet);
                }

                var userContributor = new UserContributors
                {
                    UserId = id,
                    ContributorId = contributor.Id,
                    CreatedBy = User.UserName(),
                    Timestamp = DateTime.UtcNow
                };

                contributorService.AddUserContributor(userContributor);
                var user = userService.Get(id);

                var authToken = new AuthToken($"{user.IdentificationTypeId}|{user.Code}", contributor.Code)
                {
                    ContributorId = contributor.Id,
                    Email = User.Identity.GetUserName(),
                    UserId = id,
                    Status = false,
                    Token = null
                };
                dianAuthTableManager.InsertOrUpdate(authToken);

                var json = Json(new
                {
                    success = contributor != null,
                    name = contributor.Name,
                    businessName = contributor.BusinessName,
                    email = contributor.Email,
                    contributorTypeId = contributor.ContributorTypeId,
                    id = contributor.Id,
                    code = contributor.Code
                }, JsonRequestBehavior.AllowGet);

                return json;
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public JsonResult RemoveContributor(string id, string code)
        {
            try
            {
                var contributor = contributorService.GetByCode(code);
                if (contributor == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "NIT ingresado no encontrado en los registros de la DIAN."
                    }, JsonRequestBehavior.AllowGet);
                }

                var userContributor = new UserContributors
                {
                    UserId = id,
                    ContributorId = contributor.Id
                };

                contributorService.RemoveUserContributor(userContributor);
                var user = userService.Get(id);

                var auth = dianAuthTableManager.Find<AuthToken>(user.Code, contributor.Code);
                if (auth != null)
                    dianAuthTableManager.Delete(auth);

                var json = Json(new
                {
                    success = contributor != null,
                    code
                }, JsonRequestBehavior.AllowGet);

                return json;
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public new ActionResult View(string id)
        {
            if (!User.IsInAnyRole("Administrador", "Super"))
                id = User.Identity.GetUserId();

            ApplicationUser applicationUser = UserManager.FindById(id);
            if (applicationUser == null)
                return RedirectToAction(nameof(ErrorController.Error400), "Error");

            UserViewModel userViewModel = new UserViewModel
            {
                Id = applicationUser.Id,
                Code = applicationUser.Code,
                Name = applicationUser.Name,
                Email = applicationUser.Email,
                IdentificationType = applicationUser.IdentificationTypeId,
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                Contributors = applicationUser.Contributors.Select(u => new ContributorViewModel
                {
                    Code = u.Code,
                    Id = u.Id,
                    Name = u.Name,
                    BusinessName = u.BusinessName,
                    Email = u.Email,
                    AcceptanceStatusId = u.AcceptanceStatusId,
                    AcceptanceStatusName = u.AcceptanceStatus.Name,
                    StartDate = u.StartDate,
                    EndDate = u.EndDate
                }).ToList()
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.LegalRepresentative;
            return View(userViewModel);
        }

        public ActionResult Edit(string id)
        {
            if (!User.IsInAnyRole("Administrador", "Super"))
                id = User.Identity.GetUserId();

            ApplicationUser applicationUser = UserManager.FindById(id);
            if (applicationUser == null)
                return RedirectToAction(nameof(ErrorController.Error400), "Error");

            UserViewModel userViewModel = new UserViewModel
            {
                Id = applicationUser.Id,
                Code = applicationUser.Code,
                Name = applicationUser.Name,
                Email = applicationUser.Email,
                IdentificationType = applicationUser.IdentificationTypeId,
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                Contributors = applicationUser.Contributors.Select(u => new ContributorViewModel
                {
                    Code = u.Code,
                    Id = u.Id,
                    Name = u.Name,
                    BusinessName = u.BusinessName,
                    Email = u.Email,
                    AcceptanceStatusId = u.AcceptanceStatusId,
                    AcceptanceStatusName = u.AcceptanceStatus.Name,
                    StartDate = u.StartDate,
                    EndDate = u.EndDate
                }).ToList(),
                CanEdit = User.IsInAnyRole("Super")
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.LegalRepresentative;
            return View(userViewModel);
        }

        [HttpPost]
        public ActionResult Edit(UserViewModel model)
        {
            var applicationUser = new ApplicationUser
            {
                Id = model.Id,
                Code = model.Code,
                Name = model.Name,
                Email = model.Email,
                IdentificationTypeId = model.IdentificationType
            };
            string id = userService.AddOrUpdate(applicationUser);

            ViewBag.CurrentPage = Navigation.NavigationEnum.LegalRepresentative;
            return RedirectToAction(nameof(View), new { id });
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Authentication()
        {
            ViewBag.currentTab = Request["currentTab"];
            UserLoginViewModel model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList()
            };
            return View("Login", model);
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CertificateAuthentication(UserLoginViewModel model, string returnUrl)
        {
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList();

            ClearUnnecessariesModelStateErrorsForAuthentication(false);

            var recaptchaValidation = IsValidCaptcha(model.RecaptchaToken);
            if (!recaptchaValidation.Item1)
            {
                ModelState.AddModelError($"CertificateLoginFailed", recaptchaValidation.Item2);
                return View("CertificateLogin", model);
            }
            if (!ModelState.IsValid)
                return View("CertificateLogin", model);

            if (!model.CompanyCode.EncryptSHA256().Equals(model.HashCode))
            {
                ModelState.AddModelError($"CertificateLoginFailed", "NIT de empresa no corresponde al NIT informado en el certificado.");
                return View("CertificateLogin", model);
            }

            var user = userService.GetByCodeAndIdentificationTyte(model.UserCode, model.IdentificationType);
            if (user == null)
            {
                ModelState.AddModelError($"CertificateLoginFailed", "Número de documento y tipo de identificación no coinciden.");
                return View("CertificateLogin", model);
            }

            var contributor = user.Contributors.FirstOrDefault(c => c.Code == model.CompanyCode);
            if (contributor == null)
            {
                model.CompanyCode = model.CompanyCode.Trim();
                model.CompanyCode = model.CompanyCode.Substring(0, model.CompanyCode.Length - 1);
                contributor = user.Contributors.FirstOrDefault(c => c.Code == model.CompanyCode);
                if (contributor == null)
                {
                    ModelState.AddModelError($"CertificateLoginFailed", "Empresa no asociada a representante legal.");
                    return View("CertificateLogin", model);
                }
            }

            if (contributor.StatusRut == (int)Domain.Common.StatusRut.Cancelled)
            {
                ModelState.AddModelError($"CertificateLoginFailed", "Contribuyente tiene RUT en estado cancelado.");
                return View("CertificateLogin", model);
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod" && contributor.AcceptanceStatusId != (int)Domain.Common.ContributorStatus.Enabled)
            {
                ModelState.AddModelError($"CertificateLoginFailed", "Empresa no se encuentra habilitada.");
                return View("CertificateLogin", model);
            }

            var pk = $"{model.IdentificationType}|{model.UserCode}";
            var rk = $"{model.CompanyCode}";

            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            if (auth == null)
            {
                auth = new AuthToken(pk, rk) { UserId = user.Id, Email = user.Email, ContributorId = contributor.Id, Type = AuthType.Certificate.GetDescription(), Token = Guid.NewGuid().ToString(), Status = true };
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
                    auth.Type = AuthType.Certificate.GetDescription();
                    auth.Token = Guid.NewGuid().ToString();
                    auth.Status = true;
                    dianAuthTableManager.InsertOrUpdate(auth);
                }
            }

            user.Code = user.Code;
            user.ContributorCode = contributor.Code;
            await SignInManager.SignInAsync(user, true, false);
            return RedirectToAction(nameof(HomeController.Dashboard), "Home");
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompanyAuthentication(UserLoginViewModel model, string returnUrl)
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
                return View("CompanyLogin", model);
            }
            if (!ModelState.IsValid)
                return View("CompanyLogin", model);

            var pk = $"{model.IdentificationType}|{model.UserCode}";
            var rk = $"{model.CompanyCode}";

            var user = userService.GetByCodeAndIdentificationTyte(model.UserCode, model.IdentificationType);
            if (user == null)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Número de documento y tipo de identificación no coinciden.");
                return View("CompanyLogin", model);
            }

            var contributor = user.Contributors.FirstOrDefault(c => c.Code == model.CompanyCode);
            if (contributor == null)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Empresa no asociada a representante legal.");
                return View("CompanyLogin", model);
            }

            if (contributor.StatusRut == (int)StatusRut.Cancelled)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Contribuyente tiene RUT en estado cancelado.");
                return View("CompanyLogin", model);
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod" && contributor.AcceptanceStatusId != (int)Domain.Common.ContributorStatus.Enabled)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Empresa no se encuentra habilitada.");
                return View("CompanyLogin", model);
            }

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

            //var urlParametersEncrypt = $"{auth.PartitionKey}#{auth.RowKey}#{auth.Token}".Encrypt();
            //var accessUrl = ConfigurationManager.GetValue("CheckUrl") + $"key={urlParametersEncrypt}";

            var accessUrl = ConfigurationManager.GetValue("UserAuthTokenUrl") + $"pk={auth.PartitionKey}&rk={auth.RowKey}&token={auth.Token}";
            if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Prod")
            {
                try
                {
                    auth.Email = user.Email;
                    var emailSenderResponse = await EmailUtil.SendEmailAsync(auth, accessUrl);
                    if (emailSenderResponse.ErrorType != ErrorTypes.NoError)
                    {
                        ModelState.AddModelError($"CompanyLoginFailed", "Autenticación correcta, su solicitud está siendo procesada.");
                        return View("CompanyLogin", model);
                    }
                }
                catch (Exception ex)
                {
                    var requestId = Guid.NewGuid();
                    var logger = new GlobalLogger(requestId.ToString(), requestId.ToString())
                    {
                        Action = "SendEmailAsync",
                        Controller = "User",
                        Message = ex.Message,
                        RouteData = "",
                        StackTrace = ex.StackTrace
                    };
                    var tableManager = new TableManager("GlobalLogger");
                    tableManager.InsertOrUpdate(logger);
                    ModelState.AddModelError($"CompanyLoginFailed", $"Ha ocurrido un error, por favor intente nuevamente. Id: {requestId}");
                    return View("CompanyLogin", model);
                }
            }

            ViewBag.UserEmail = HideUserEmailParts(auth.Email);
            ViewBag.Url = accessUrl;
            ViewBag.currentTab = "confirmed";
            return View("LoginConfirmed", model);
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalUserAuthentication(UserLoginViewModel model, string returnUrl)
        {
            //return RedirectToAction(nameof(HomeController.Dashboard), "Home");

            //return RedirectToAction("RedirectToBiller", "User");
            var redirectUrl = ConfigurationManager.GetValue("BillerAuthUrl") + $"pk=1014556699|999999999&rk=999999999&token=9d15b522-024b-424d-a10a-549fd5c728b1";
            return Redirect(redirectUrl);
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PersonAuthentication(UserLoginViewModel model, string returnUrl)
        {
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList();

            ClearUnnecessariesModelStateErrorsForAuthentication(true);

            var recaptchaValidation = IsValidCaptcha(model.RecaptchaToken);
            if (!recaptchaValidation.Item1)
            {
                ModelState.AddModelError($"PersonLoginFailed", recaptchaValidation.Item2);
                return View("PersonLogin", model);
            }
            if (!ModelState.IsValid)
                return View("PersonLogin", model);

            var user = userService.GetByCodeAndIdentificationTyte(model.PersonCode, model.IdentificationType);
            if (user == null)
            {
                ModelState.AddModelError($"PersonLoginFailed", "Cédula y tipo de indetificación no coinciden.");
                return View("PersonLogin", model);
            }

            var contributor = user.Contributors.FirstOrDefault(c => c.PersonType == (int)PersonType.Natural && c.Name.ToLower() == user.Name.ToLower());
            if (contributor == null)
            {
                ModelState.AddModelError($"PersonLoginFailed", "Persona natural sin permisos asociados.");
                return View("PersonLogin", model);
            }

            if (contributor.StatusRut == (int)StatusRut.Cancelled)
            {
                ModelState.AddModelError($"PersonLoginFailed", "Contribuyente tiene RUT en estado cancelado.");
                return View("PersonLogin", model);
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod" && contributor.AcceptanceStatusId != (int)ContributorStatus.Enabled)
            {
                ModelState.AddModelError($"PersonLoginFailed", "Usted no se ecuentra habilitado.");
                return View("PersonLogin", model);
            }

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

            //var urlParametersEncrypt = $"{auth.PartitionKey}#{auth.RowKey}#{auth.Token}".Encrypt();
            //var accessUrl = ConfigurationManager.GetValue("CheckUrl") + $"key={urlParametersEncrypt}";

            var accessUrl = ConfigurationManager.GetValue("UserAuthTokenUrl") + $"pk={auth.PartitionKey}&rk={auth.RowKey}&token={auth.Token}";
            if (ConfigurationManager.GetValue("Environment") == "Hab" || ConfigurationManager.GetValue("Environment") == "Prod")
            {
                try
                {
                    auth.Email = user.Email;
                    var emailSenderResponse = await EmailUtil.SendEmailAsync(auth, accessUrl);
                    if (emailSenderResponse.ErrorType != ErrorTypes.NoError)
                    {
                        ModelState.AddModelError($"PersonLoginFailed", "Autenticación correcta, su solicitud está siendo procesada.");
                        return View("PersonLogin", model);
                    }
                }
                catch (Exception ex)
                {
                    var requestId = Guid.NewGuid();
                    var logger = new GlobalLogger(requestId.ToString(), requestId.ToString())
                    {
                        Action = "SendEmailAsync",
                        Controller = "User",
                        Message = ex.Message,
                        RouteData = "",
                        StackTrace = ex.StackTrace
                    };
                    var tableManager = new TableManager("GlobalLogger");
                    tableManager.InsertOrUpdate(logger);
                    ModelState.AddModelError($"PersonLoginFailed", $"Ha ocurrido un error, por favor intente nuevamente. Id: {requestId}");
                    return View("CompanyLogin", model);
                }
            }

            ViewBag.UserEmail = HideUserEmailParts(auth.Email);
            ViewBag.Url = accessUrl;
            ViewBag.currentTab = "confirmed";
            return View("LoginConfirmed", model);
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult CheckUrl(string key)
        {
            var urlParametersDecrypted = key.Decrypt();
            var parameters = urlParametersDecrypted.Split('#');
            var pk = parameters[0];
            var rk = parameters[1];
            var token = parameters[2];
            return RedirectToAction(nameof(AuthToken), new { pk, rk, token });
        }

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
                    ModelState.AddModelError($"{auth.Type}LoginFailed", "Persona natural no se encuentra registrada.");
                    return View($"{auth?.Type}Login", model);
                }

                user.ContributorCode = rk;

                await SignInManager.SignInAsync(user, true, false);

                return RedirectToAction(nameof(HomeController.Dashboard), "Home");
            }

            ModelState.AddModelError($"{auth?.Type}LoginFailed", "Token expirado, por favor intente nuevamente.");
            return View($"{auth?.Type}Login", model);
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Login(string returnUrl)
        {
            if (ConfigurationManager.GetValue("LoginType") == "Certificate")
                return RedirectToAction(nameof(CertificateLogin));

            UserLoginViewModel model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList()
            };

            ViewBag.ReturnUrl = returnUrl;
            return View("Login", model);
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult CertificateLogin(string returnUrl)
        {
            if (ConfigurationManager.GetValue("LoginType") == "Certificate")
            {
                UserLoginViewModel model = new UserLoginViewModel
                {
                    IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                    IdentificationType = (int)Domain.Common.IdentificationType.CC
                };

                HttpClientCertificate cert = Request.ClientCertificate;
                if (cert.IsPresent && cert.IsValid)
                {
                    X509Certificate2 X509 = new X509Certificate2(Request.ClientCertificate.Certificate);
                    var parts = GetCertificateSubjectInfo(X509.Subject);

                    var certificateValidator = new CertificateValidator();
                    try
                    {
                        model.CompanyCode = ExtractNitFromCertificate(parts);
                        model.HashCode = model.CompanyCode.EncryptSHA256();

                        if (string.IsNullOrEmpty(model.CompanyCode))
                        {
                            ModelState.AddModelError($"CompanyCode", "No se encontró el NIT de la empresa en el certificado.");
                            return View("CertificateLogin", model);
                        }

                        certificateValidator.Validate(X509);

                        return View("CertificateLogin", model);
                    }
                    catch (Exception ex)
                    {
                        var logger = new GlobalLogger("ZD05-WEB", model.CompanyCode) { Action = "CertificateLogin", Controller = "UserController", Message = ex.Message };
                        var tableManager = new TableManager("GlobalLogger");
                        tableManager.InsertOrUpdate(logger);

                        ModelState.AddModelError($"CertificateLoginFailed", ex.Message);
                        model.CompanyCode = "";
                        model.HashCode = "xxxxxxxxxx".EncryptSHA256();
                        return View("CertificateLogin", model);
                    }
                }
                else
                {
                    ModelState.AddModelError($"CertificateLoginFailed", "No se reconoce el certificado proporcionado.");
                    return View("CertificateLogin", model);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("CertificateLogin");
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult CompanyLogin(string returnUrl)
        {
            if (ConfigurationManager.GetValue("LoginType") == "Certificate")
                return RedirectToAction(nameof(CertificateLogin));

            ViewBag.ReturnUrl = returnUrl;
            UserLoginViewModel model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                IdentificationType = (int)Domain.Common.IdentificationType.CC
            };
            return View("CompanyLogin", model);
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult ExternalUserLogin(string returnUrl)
        {
            if (ConfigurationManager.GetValue("LoginType") == "Certificate")
                return RedirectToAction(nameof(CertificateLogin));

            ViewBag.ReturnUrl = returnUrl;
            UserLoginViewModel model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                IdentificationType = (int)Domain.Common.IdentificationType.CC
            };
            return View("ExternalUserLogin", model); ;
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult PersonLogin(string returnUrl)
        {
            if (ConfigurationManager.GetValue("LoginType") == "Certificate")
                return RedirectToAction(nameof(CertificateLogin));

            ViewBag.ReturnUrl = returnUrl;
            UserLoginViewModel model = new UserLoginViewModel
            {
                IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                IdentificationType = (int)Domain.Common.IdentificationType.CC
            };
            return View("PersonLogin", model);
        }

        public ActionResult ChangePassword()
        {
            var model = new UserChangePasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(UserChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.Identity.GetUserId();
            var user = userService.Get(userId);
            var currentPasswordMatch = UserManager.CheckPassword(user, model.CurrentPassword);
            if (!currentPasswordMatch)
            {
                ModelState.AddModelError("CurrentPassword", "Contraseña actual incorrecta.");
                return View(model);
            }

            UserManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            var result = UserManager.ChangePassword(User.Identity.GetUserId(), model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                ModelState.AddModelError("NewPassword", result.Errors.FirstOrDefault());
            else
                model.Succeeded = true;

            return View(model);
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult SearchDocument(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        [ValidateAntiForgeryToken]
        public ActionResult SearchDocument(UserLoginViewModel model)
        {

            ClearUnnecessariesModelStateErrorsForSearchDocument();
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList();
            var recaptchaValidation = IsValidCaptcha(model.RecaptchaToken);
            if (!recaptchaValidation.Item1)
            {
                ModelState.AddModelError($"DocumentKey", recaptchaValidation.Item2);
                return View("SearchDocument", model);
            }
            if (!ModelState.IsValid)
                return View("SearchDocument", model);

            var documentKey = model.DocumentKey.ToLower();
            var globalDocValidatorDocumentMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(documentKey, documentKey);
            if (globalDocValidatorDocumentMeta == null)
            {
                ModelState.AddModelError("DocumentKey", "Documento no encontrado en los registros de la DIAN.");
                return View(model);
            }

            var identifier = globalDocValidatorDocumentMeta.Identifier;
            var globalDocValidatorDocument = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(identifier, identifier);
            if (globalDocValidatorDocument == null)
            {
                ModelState.AddModelError("DocumentKey", "Documento no encontrado en los registros de la DIAN.");
                return View("SearchDocument", model);
            }

            if (globalDocValidatorDocument.DocumentKey != documentKey)
            {
                ModelState.AddModelError("DocumentKey", "Documento no encontrado en los registros de la DIAN.");
                return View("SearchDocument", model);
            }

            var partitionKey = $"co|{globalDocValidatorDocument.EmissionDateNumber.Substring(6, 2)}|{globalDocValidatorDocument.DocumentKey.Substring(0, 2)}";

            return RedirectToAction(nameof(DocumentController.ShowDocumentToPublic), "Document", new { Id = globalDocValidatorDocument.DocumentKey });
        }

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserLoginViewModel model, string returnUrl)
        {

            ClearUnnecessariesModelStateErrorsForLogin();

            var recaptchaValidation = IsValidCaptcha(model.RecaptchaToken);
            if (!recaptchaValidation.Item1)
            {
                ModelState.AddModelError($"AdminLoginFailed", recaptchaValidation.Item2);
                return View("Login", model);
            }

            if (!ModelState.IsValid)
                return View("Login", model);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = userService.GetByEmail(model.Email);
                    if (!UserManager.IsInRole(user.Id, Roles.Administrator))
                    {
                        ModelState.AddModelError($"AdminLoginFailed", "Usuario no cuenta con rol de Administrador.");
                        return View("Login", model);
                    }
                    return RedirectToAction(nameof(HomeController.Dashboard), "Home");
                case SignInStatus.LockedOut:
                    ModelState.AddModelError($"AdminLoginFailed", "Usuario bloqueado.");
                    return View("Login", model);
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
                case SignInStatus.Failure:
                    ModelState.AddModelError($"AdminLoginFailed", "Correo electrónico o contraseña no concuerdan.");
                    return View("Login", model);
                default:
                    ModelState.AddModelError($"AdminLoginFailed", "Correo electrónico o contraseña no concuerdan.");
                    return View("Login", model);
            }

        }


        [ExcludeFilter(typeof(Authorization))]
        public ActionResult LogOut()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction(nameof(UserController.Login), "User");
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult LogOutBiller(string pk, string rk, string token)
        {
            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            if (auth != null && auth.Token == token)
            {
                auth.Status = false;
                auth.Token = null;
                dianAuthTableManager.InsertOrUpdate(auth);
                if (User.Identity.IsAuthenticated)
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            }
            return RedirectToAction(nameof(UserController.Login), "User");
        }

        [HttpGet]
        [ExcludeFilter(typeof(Authorization))]
        public RedirectResult RedirectToBiller()
        {
            var auth = new AuthToken();
            if (!User.IsInAnyRole("Administrador", "Super"))
            {
                auth = dianAuthTableManager.Find<AuthToken>($"{User.IdentificationTypeId()}|{User.UserCode()}", User.ContributorCode());
            }
            else
            {
                var user = userService.GetByEmail(User.Identity.Name);
                auth = dianAuthTableManager.Find<AuthToken>(user.Id, ConfigurationManager.GetValue("RowKeyDian"));
                if (auth == null)
                {
                    auth = new AuthToken(user.Id, ConfigurationManager.GetValue("RowKeyDian"))
                    {
                        PartitionKey = user.Id,
                        RowKey = ConfigurationManager.GetValue("RowKeyDian"),
                        Email = user.Email,
                        Token = Guid.NewGuid().ToString(),
                        UserId = user.Id,
                        Status = true
                    };
                    dianAuthTableManager.InsertOrUpdate(auth);
                }
                else
                {
                    TimeSpan timeSpan = DateTime.UtcNow.Subtract(auth.Timestamp.DateTime);
                    if (timeSpan.TotalMinutes > 20 || string.IsNullOrEmpty(auth.Token))
                    {
                        auth.PartitionKey = user.Id;
                        auth.RowKey = ConfigurationManager.GetValue("RowKeyDian");
                        auth.Email = user.Email;
                        auth.Token = Guid.NewGuid().ToString();
                        auth.UserId = user.Id;
                        auth.Status = true;
                        dianAuthTableManager.InsertOrUpdate(auth);
                    }
                }
            }

            var redirectUrl = ConfigurationManager.GetValue("BillerAuthUrl") + $"pk={auth.PartitionKey}&rk={auth.RowKey}&token={auth.Token}";
            return Redirect(redirectUrl);
        }

        [ExcludeFilter(typeof(Authorization))]
        public RedirectResult RedirectToBiller2()
        {
            var auth = dianAuthTableManager.Find<AuthToken>($"{User.IdentificationTypeId()}|{User.UserCode()}", User.ContributorCode());
            var redirectUrl = ConfigurationManager.GetValue("BillerAuthUrl2") + $"pk={auth.PartitionKey}&rk={auth.RowKey}&token={auth.Token}";
            return Redirect(redirectUrl);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [ExcludeFilter(typeof(Authorization))]
        public ActionResult Unauthorized()
        {
            return View();
        }

        private string HideUserEmailParts(string email)
        {
            var userEmail = email.Split('@').FirstOrDefault();
            var mid = userEmail.Length / 2;
            if (mid > 0)
            {
                userEmail = userEmail.Remove(userEmail.Length - mid);
                var asterisks = "";
                do
                {
                    asterisks = $"{asterisks}*";
                    mid--;
                } while (mid > 0);
                userEmail = $"{userEmail}{asterisks}";
                userEmail = $"{userEmail}@{email.Split('@').LastOrDefault()}";
            }
            else
                userEmail = $"{userEmail}*@{email.Split('@').LastOrDefault()}";

            return userEmail;
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
        private void ClearUnnecessariesModelStateErrorsForLogin()
        {
            foreach (var item in ModelState)
            {
                if (item.Key.Contains("CompanyCode"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("DocumentKey"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("IdentificationTypes"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("PersonCode"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("UserCode"))
                    item.Value.Errors.Clear();
            }
        }
        private void ClearUnnecessariesModelStateErrorsForSearchDocument()
        {
            foreach (var item in ModelState)
            {
                if (item.Key.Contains("CompanyCode"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("Email"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("Password"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("PersonCode"))
                    item.Value.Errors.Clear();
                if (item.Key.Contains("UserCode"))
                    item.Value.Errors.Clear();
            }
        }
        private ActionResult RedirectToLocal(string returnUrl, string userName)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            var user = UserManager.FindByName(userName);
            return RedirectToAction(nameof(HomeController.Dashboard), "Home");

        }
        private string ExtractNitFromCertificate(Dictionary<string, string> dictionary)
        {
            var companyCode = string.Empty;

            if (dictionary.Keys.Contains("1.3.6.1.4.1.23267.2.3"))
                companyCode = dictionary["1.3.6.1.4.1.23267.2.3"].ExtractNumbers();
            else if (dictionary.Keys.Contains("OID.1.3.6.1.4.1.23267.2.3"))
                companyCode = dictionary["OID.1.3.6.1.4.1.23267.2.3"].ExtractNumbers();
            else if (dictionary.Keys.Contains("SERIALNUMBER"))
                companyCode = dictionary["SERIALNUMBER"].ExtractNumbers();
            else if (dictionary.Keys.Contains("SN"))
                companyCode = dictionary["SN"].ExtractNumbers();
            else if (dictionary.Keys.Contains("1.3.6.1.4.1.31136.1.1.20.2"))
                companyCode = dictionary["1.3.6.1.4.1.31136.1.1.20.2"].ExtractNumbers();
            else if (dictionary.Keys.Contains("2.5.4.97"))
                companyCode = dictionary["2.5.4.97"].ExtractNumbers();
            else if (dictionary.Keys.Contains("OID.2.5.4.97"))
                companyCode = dictionary["OID.2.5.4.97"].ExtractNumbers();

            return companyCode;
        }
        private Dictionary<string, string> GetCertificateSubjectInfo(string subject)
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

        #region ContributorUserLogin

        [HttpPost]
        [ExcludeFilter(typeof(Authorization))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ContributorUserLogin(UserLoginViewModel model)
        {
            model.IdentificationTypes = identificationTypeService.List().Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList();
            ClearUnnecessariesModelStateErrorsForAuthentication(false);

            var recaptchaValidation = IsValidCaptcha(model.RecaptchaToken);
            if (!recaptchaValidation.Item1)
            {
                ModelState.AddModelError($"CompanyLoginFailed", recaptchaValidation.Item2);
                return View("CompanyLogin", model);
            }
            if (!ModelState.IsValid)
                return View("CompanyLogin", model);

            var pk = $"{model.IdentificationType}|{model.UserCode}";
            var rk = $"{model.CompanyCode}";

            var user = userService.GetByCodeAndIdentificationTyte(model.UserCode, model.IdentificationType);

            if (user == null)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Error de ingreso, verifique sus datos");
                return View("CompanyLogin", model);
            }

            var result = await SignInManager.PasswordSignInAsync(user.Email, model.Password, true, shouldLockout: true);

            //var pass = UserManager.PasswordHasher.VerifyHashedPassword(model.Password, user.PasswordHash);
            if (result != SignInStatus.Success)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Contraseña incorrecta");
                return View("CompanyLogin", model);
            }

            if (!Convert.ToBoolean(user.Active))
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Usuario no se encuentra activo");
                return View("CompanyLogin", model);
            }

            UsersFreeBillerProfile freeBiller = userService.GetUserFreeBillerProfile(u => u.CompanyCode == model.CompanyCode && u.CompanyIdentificationType == model.CompanyIdentificationType);

            var contributor = contributorService.GetByCode(model.CompanyCode);
            if (freeBiller == null)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Empresa no asociada a representante legal.");
                return View("CompanyLogin", model);
            }

            if (contributor.StatusRut == (int)StatusRut.Cancelled)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Contribuyente tiene RUT en estado cancelado.");
                return View("CompanyLogin", model);
            }

            if (ConfigurationManager.GetValue("Environment") == "Prod" && contributor.AcceptanceStatusId != (int)Domain.Common.ContributorStatus.Enabled)
            {
                ModelState.AddModelError($"CompanyLoginFailed", "Empresa no se encuentra habilitada.");
                return View("CompanyLogin", model);
            }

            var auth = dianAuthTableManager.Find<AuthToken>(pk, rk);
            if (auth == null)
            {
                auth = new AuthToken(pk, rk) { UserId = user.Id, Email = user.Email, ContributorId = contributor.Id, Type = AuthType.ProfileUser.GetDescription(), Token = Guid.NewGuid().ToString(), Status = true };
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
                    auth.Type = AuthType.ProfileUser.GetDescription();
                    auth.Token = Guid.NewGuid().ToString();
                    auth.Status = true;
                    dianAuthTableManager.InsertOrUpdate(auth);
                }
            }
            UserManager.AddClaim(user.Id, new System.Security.Claims.Claim(AuthType.ProfileUser.GetDescription(), user.Id));
            var redirectUrl = ConfigurationManager.GetValue("BillerAuthUrl") + $"pk={auth.PartitionKey}&rk={auth.RowKey}&token={auth.Token}";
            return Redirect(redirectUrl);
        }

        #endregion
    }
}