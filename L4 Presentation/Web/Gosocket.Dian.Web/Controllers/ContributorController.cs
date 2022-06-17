using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Helpers;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [IPFilter]
    [Authorization]
    public class ContributorController : Controller
    {
        ContributorService contributorService = new ContributorService();
        private readonly UserService userService = new UserService();
        private readonly ContributorFileTypeService contributorFileTypeService = new ContributorFileTypeService();
        SoftwareService softwareService = new SoftwareService();
        ContributorOperationsService contributorOperationsService = new ContributorOperationsService();
        private ApplicationUserManager userManager;

        private static readonly TableManager tableManagerGlobalAuthorization = new TableManager("GlobalAuthorization");
        private static readonly TableManager tableManagerDianContributorCategory = new TableManager("DianContributorCategory");
        private static readonly TableManager tableManagerTestSet = new TableManager("GlobalTestSet");
        private static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        private static readonly TableManager tableManagerNumberRangeManager = new TableManager("GlobalNumberRange");
        private static readonly TableManager tableManagerGlobalContributor = new TableManager("GlobalContributor");
        private static readonly TableManager tableManagerGlobalExchangeEmail = new TableManager("GlobalExchangeEmail");
        private static readonly TableManager tableManagerGlobalSoftware = new TableManager("GlobalSoftware");
        private static readonly TableManager dianAuthTableManager = new TableManager("AuthToken");
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                userManager = value;
            }
        }

        [CustomRoleAuthorization(CustomRoles = "Super")]
        public ActionResult Activation()
        {
            var model = new ContributorViewModel { ContributorTestSetResults = new List<TestSetResultViewModel>() };
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Super")]
        public ActionResult Activation(ContributorViewModel model)
        {
            Contributor contributor = contributorService.GetByCode(model.Code);
            if (contributor == null) return View(model);
            model.AcceptanceStatusId = contributor.AcceptanceStatusId;
            model.AcceptanceStatusName = Domain.Common.EnumHelper.GetEnumDescription((ContributorStatus)contributor.AcceptanceStatusId);
            var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);
            model.ContributorTestSetResults = testSetResults.Where(t => !t.Deleted).Select(t => new TestSetResultViewModel
            {
                Id = t.Id,
                OperationModeName = t.OperationModeName,
                SoftwareId = t.SoftwareId,
                Status = t.Status,
                StatusDescription = Domain.Common.EnumHelper.GetEnumDescription((TestSetStatus)t.Status),
                TotalInvoicesAcceptedRequired = t.TotalInvoicesAcceptedRequired,
                TotalInvoicesAccepted = t.TotalInvoicesAccepted,
                TotalInvoicesRejected = t.TotalInvoicesRejected,
                TotalCreditNotesAcceptedRequired = t.TotalCreditNotesAcceptedRequired,
                TotalCreditNotesAccepted = t.TotalCreditNotesAccepted,
                TotalCreditNotesRejected = t.TotalCreditNotesRejected,
                TotalDebitNotesAcceptedRequired = t.TotalDebitNotesAcceptedRequired,
                TotalDebitNotesAccepted = t.TotalDebitNotesAccepted,
                TotalDebitNotesRejected = t.TotalDebitNotesRejected
            }).ToList();
            return View(model);
        }

        [NonAction]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        public ActionResult Add(string type)
        {
            SetView(type);
            int.TryParse(type, out int contributorType);

            var model = new ContributorViewModel
            {
                ContributorTypeId = contributorType
            };

            if (contributorType == (int)Domain.Common.ContributorType.Biller)
            {
                var providers = contributorService.GetContributors((int)Domain.Common.ContributorType.Provider, (int)ContributorStatus.Enabled);
                model.Providers = providers.Select(p => new ProviderViewModel
                {
                    Id = p.Id,
                    Name = p.Name

                }).ToList();
            }

            return View(model);
        }

        [NonAction]
        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ContributorViewModel model)
        {
            SetView(model.ContributorTypeId.ToString());

            var contributor = new Contributor
            {
                Code = model.Code,
                Name = model.Name,
                BusinessName = model.BusinessName,
                Email = model.Email,
                ContributorTypeId = model.ContributorTypeId,
                AcceptanceStatusId = 1,
                CreatedBy = User.Identity.Name,
                Updated = DateTime.UtcNow,
                PrincipalActivityCode = "479"
            };

            switch (model.ContributorTypeId)
            {
                case (int)Domain.Common.ContributorType.Biller:
                    contributor.OperationModeId = model.OperationModeId;
                    if (model.OperationModeId == (int)Domain.Common.OperationMode.Provider)
                    {
                        contributor.ProviderId = model.ProviderId;
                    }

                    break;
                case (int)Domain.Common.ContributorType.Provider:
                    contributor.ContributorFiles = DataUtils.GetMandatoryContributorFileTypes().Select(c => new ContributorFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = c.Name,
                        Status = (int)Domain.Common.ContributorFileStatus.Pending,
                        Comments = "",
                        Deleted = false,
                        FileType = c.Id,
                        ContributorId = contributor.Id,
                        CreatedBy = User.Identity.Name,
                        Timestamp = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    }).ToList();
                    break;
                case (int)Domain.Common.ContributorType.AuthorizedProvider:
                    break;
                default:
                    contributor.ContributorTypeId = null;
                    break;
            }

            int id = contributorService.AddOrUpdate(contributor);
            if (model.ContributorTypeId == (int)Domain.Common.ContributorType.Biller && model.OperationModeId == (int)Domain.Common.ContributorType.Provider)
            {
                var software = new Software
                {
                    Id = Guid.NewGuid(),
                    Pin = Guid.NewGuid().ToString(),
                    Name = model.Software.Name,
                    ContributorId = id,
                    Status = true,
                    SoftwareUser = model.Software.SoftwareUser,
                    SoftwarePassword = model.Software.SoftwarePassword,
                    Url = model.Software.Url,
                    SoftwareDate = DateTime.UtcNow,
                    Timestamp = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    CreatedBy = User.Identity.Name,
                    AcceptanceStatusSoftwareId = (int)SoftwareStatus.Test
                };
                softwareService.AddOrUpdate(software);

                //
                var globalSoftware = new GlobalSoftware(software.Id.ToString(), software.Id.ToString()) { Id = software.Id, Deleted = software.Deleted, Pin = software.Pin, StatusId = software.AcceptanceStatusSoftwareId };
                tableManagerGlobalSoftware.InsertOrUpdate(globalSoftware);
            }

            return RedirectToAction(nameof(View), new { id });
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
        [ValidateAntiForgeryToken]
        public ActionResult CancelProvider(int id)
        {
            var contributor = contributorService.Get(id);
            contributor.AcceptanceStatusId = (int)ContributorStatus.Enabled;
            contributor.ContributorTypeId = (int)Domain.Common.ContributorType.Biller;

            // TODO:
            // Update contributor

            // Delete associated files
            // Delete test set for provider


            return RedirectToAction(nameof(View), new { id });
        }

        public ActionResult Check()
        {
            Contributor contributor = contributorService.Get(User.ContributorId());
            var contributorId = contributor.Id;
            var contributorAcceptanceStatusId = contributor.AcceptanceStatusId;
            var contributorTypeId = contributor.ContributorTypeId;

            AuthToken auth = GetAuthData();
            var contributorLoggedByOfe = auth.LoginMenu == "OFE";

            if (contributorId == 0)
                return RedirectToAction(nameof(HomeController.Dashboard), "Home");

            if (contributorTypeId == null || contributorTypeId == (int)Domain.Common.ContributorType.Zero)
                return RedirectToAction(nameof(Register));

            if (/*contributorTypeId == (int)Domain.Common.ContributorType.Biller &&*/ contributorLoggedByOfe && contributorAcceptanceStatusId == (int)ContributorStatus.Pending)
                return RedirectToAction(nameof(Register));

            if (contributorTypeId == (int)Domain.Common.ContributorType.Provider && contributorAcceptanceStatusId != (int)ContributorStatus.Enabled)
                return RedirectToAction(nameof(Wizard));

            if (contributorTypeId == (int)Domain.Common.ContributorType.Provider && contributorAcceptanceStatusId == (int)ContributorStatus.Enabled)
                return RedirectToAction(nameof(View), new { id = contributorId });

            if (contributorLoggedByOfe/*contributorTypeId == (int)Domain.Common.ContributorType.Biller*/)
                return RedirectToAction(nameof(View), new { id = contributorId });

            return RedirectToAction(nameof(HomeController.Dashboard), "Home");
        }

        public ActionResult CheckContributorRegister()
        {
            Contributor contributor = contributorService.Get(User.ContributorId());
            var contributorAcceptanceStatusId = contributor.AcceptanceStatusId;
            var contributorTypeId = contributor.ContributorTypeId;
            
            var auth = Session["loginMenu"].ToString();

            var contributorLoggedByOfe = (auth == "OFE");

            if (contributorAcceptanceStatusId == (int)ContributorStatus.Pending)
            {
                if (contributorLoggedByOfe)
                {
                    return RedirectToAction(nameof(Check));
                }
                else
                {
                    return RedirectToAction(nameof(RegisterNoOfe));
                }
            }
            else
            {
                return RedirectToAction(nameof(DocumentController.ElectronicDocuments), "Document");
            }
        }

        [CustomRoleAuthorization(CustomRoles = "Facturador, Proveedor")]
        public ActionResult Configuration()
        {
            Contributor contributor = contributorService.Get(User.ContributorId());
            var model = new ContributorViewModel { Id = contributor.Id, ExchangeEmail = contributor.ExchangeEmail };
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Facturador, Proveedor")]
        public ActionResult Configuration(ContributorViewModel model)
        {
            Contributor contributor = contributorService.Get(model.Id);
            contributor.ExchangeEmail = model.ExchangeEmail?.ToLower();
            contributorService.AddOrUpdateConfiguration(contributor);

            if (!string.IsNullOrEmpty(contributor.ExchangeEmail))
            {
                var globalExchangeEmail = new GlobalExchangeEmail(contributor.Code, contributor.Code) { Email = contributor.ExchangeEmail?.ToLower() };
                tableManagerGlobalExchangeEmail.InsertOrUpdate(globalExchangeEmail);

                if (ConfigurationManager.GetValue("Environment") == "Hab")
                {
                    // insert or update in production.
                    var globalStorageConnectionStringProduction = ConfigurationManager.GetValue("GlobalStorageProduction");
                    var tableManagerExchangeEmailProdcution = new TableManager("GlobalExchangeEmail", globalStorageConnectionStringProduction);
                    tableManagerExchangeEmailProdcution.InsertOrUpdate(globalExchangeEmail);
                }
            }

            return RedirectToAction(nameof(Configuration));
        }

        [CustomRoleAuthorization(CustomRoles = "Super")]
        public ActionResult Edit(int id, int type)
        {
            Contributor contributor = contributorService.ObsoleteGet(id);
            SetView(contributor.ContributorTypeId.ToString());
            var model = new ContributorViewModel
            {
                Id = contributor.Id,
                Code = contributor.Code,
                Name = contributor.Name,
                PrincipalActivityCode = contributor.PrincipalActivityCode,
                BusinessName = contributor.BusinessName,
                Email = contributor.Email,
                ContributorTypeId = (contributor.ContributorTypeId != null && contributor.ContributorTypeId != 0) ? contributor.ContributorTypeId.Value : type,
                OperationModeId = contributor.OperationModeId,
                ProviderId = contributor.ProviderId,
                AcceptanceStatusId = contributor.AcceptanceStatusId,
                AcceptanceStatusName = contributor.AcceptanceStatus?.Name,
                AcceptanceStatuses = contributorService.GetAcceptanceStatuses().Select(s => new ContributorAcceptanceStatusViewModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name
                }).ToList(),
                Softwares = contributor.Softwares?.Select(s => new SoftwareViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Pin = s.Pin,
                    SoftwarePassword = s.SoftwarePassword,
                    SoftwareUser = s.SoftwareUser,
                    Url = s.Url,
                    CreatedBy = s.SoftwareUser
                }).ToList(),
                ContributorFiles = contributor.ContributorFiles.Count > 0 ? contributor.ContributorFiles.Select(f => new ContributorFileViewModel
                {
                    Id = f.Id,
                    Comments = f.Comments,
                    ContributorFileStatus = new ContributorFileStatusViewModel
                    {
                        Id = f.ContributorFileStatus.Id,
                        Name = f.ContributorFileStatus.Name,
                    },
                    ContributorFileType = new ContributorFileTypeViewModel
                    {
                        Id = f.ContributorFileType.Id,
                        Mandatory = f.ContributorFileType.Mandatory,
                        Name = f.ContributorFileType.Name,
                        Timestamp = f.ContributorFileType.Timestamp,
                        Updated = f.ContributorFileType.Updated
                    },
                    CreatedBy = f.CreatedBy,
                    Deleted = f.Deleted,
                    FileName = f.FileName,
                    Timestamp = f.Timestamp,
                    Updated = f.Updated
                }).ToList() : null,
                ContributorFileTypes = contributor.ContributorTypeId == (int)Domain.Common.ContributorType.Provider || contributor.ContributorTypeId == (int)Domain.Common.ContributorType.AuthorizedProvider ?
                                        contributorService.GetNotRequiredContributorFileTypes().Select(f => new ContributorFileTypeViewModel { Id = f.Id, Name = f.Name }).ToList()
                                        : null,
                CanEdit = true,
                FileStatuses = contributorService.GetContributorFileStatuses().Select(st => new ContributorFileStatusViewModel
                {
                    Id = st.Id,
                    Name = st.Name
                }).ToList()
            };
            model.ContributorFileTypes = (model.ContributorTypeId == (int)Domain.Common.ContributorType.Provider || model.ContributorTypeId == (int)Domain.Common.ContributorType.AuthorizedProvider) && model.ContributorFiles != null ?
                                        model.ContributorFileTypes.Where(f => !model.ContributorFiles.Select(c => c.ContributorFileType.Id).Contains(f.Id)).ToList()
                                        : null;

            var providers = contributorService.GetContributors((int)Domain.Common.ContributorType.Provider, (int)ContributorStatus.Enabled);
            if (contributor.OperationModeId == (int)Domain.Common.OperationMode.Own && model.Softwares != null)
            {
                model.Software = model.Softwares.FirstOrDefault();
            }
            model.Providers = providers.Select(p => new ProviderViewModel
            {
                Id = p.Id,
                Name = p.Name

            }).ToList();
            FillContributorCategory(model);
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Super")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ContributorViewModel model)
        {
            SetView(model.ContributorTypeId.ToString());
            Contributor contributor = new Contributor
            {
                Id = model.Id,
                Code = model.Code,
                Name = model.Name,
                BusinessName = model.BusinessName,
                Email = model.Email,
                ContributorTypeId = model.ContributorTypeId,
                OperationModeId = model.OperationModeId,
                ProviderId = model.OperationModeId != (int)Domain.Common.OperationMode.Provider ? null : model.ProviderId,
                AcceptanceStatusId = model.AcceptanceStatusId
            };

            contributor.ContributorFiles = model.ContributorFiles.Where(f => f.IsNew || f.Deleted).Select(f => new ContributorFile
            {
                Id = !f.Deleted ? Guid.NewGuid() : f.Id,
                FileName = f.FileName,
                Status = 0,
                Comments = "",
                Deleted = f.Deleted,
                FileType = f.ContributorFileType.Id,
                ContributorId = contributor.Id,
                CreatedBy = User.Identity.Name,
                Timestamp = DateTime.Now,
                Updated = DateTime.Now
            }).ToList();

            int contributorId = contributorService.AddOrUpdate(contributor);
            if (model.OperationModeId == (int)Domain.Common.OperationMode.Own)
            {
                var software = new Software
                {
                    Id = model.Software.Id != Guid.Parse("00000000-0000-0000-0000-000000000000") ? model.Software.Id : Guid.NewGuid(),
                    Pin = !string.IsNullOrEmpty(model.Software.Pin) ? model.Software.Pin : Guid.NewGuid().ToString(),
                    Name = model.Software.Name,
                    ContributorId = contributor.Id,
                    Status = true,
                    SoftwareUser = model.Software.SoftwareUser,
                    SoftwarePassword = model.Software.SoftwarePassword,
                    Url = model.Software.Url,
                    SoftwareDate = DateTime.UtcNow,
                    Timestamp = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    CreatedBy = User.Identity.Name,
                    AcceptanceStatusSoftwareId = model.Software.AcceptanceStatusSoftwareId != 0 ? model.Software.AcceptanceStatusSoftwareId : 1
                };
                softwareService.AddOrUpdate(software);

                //
                var globalSoftware = new GlobalSoftware(software.Id.ToString(), software.Id.ToString()) { Id = software.Id, Deleted = software.Deleted, Pin = software.Pin, StatusId = software.AcceptanceStatusSoftwareId };
                tableManagerGlobalSoftware.InsertOrUpdate(globalSoftware);
            }

            return RedirectToAction(nameof(View), new { id = model.Id });
        }

        public JsonResult Get(string code)
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

        [CustomRoleAuthorization(CustomRoles = "Administrador, Proveedor, Super")]
        public JsonResult GetContributorFileHistories(Guid id)
        {
            var history = new List<ContributorFileHistoryViewModel>();
            var result = false;
            try
            {
                var fileHistories = contributorService.GetContributorFileHistories(id);
                history = fileHistories.Select(h => new ContributorFileHistoryViewModel
                {
                    FileName = h.FileName,
                    Status = h.ContributorFileStatus.Name,
                    User = h.CreatedBy,
                    Date = h.Timestamp.ToString("yyyy-MM-dd hh:mm:ss"),
                    Comments = h.Comments
                }).ToList();
                result = true;
            }
            catch { }

            return Json(new { Success = result, history }, JsonRequestBehavior.AllowGet);
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador, Proveedor, Super")]
        public ActionResult List(string type)
        {
            SetView(type);
            var model = new ContributorTableViewModel
            {
                Type = type
            };
            int.TryParse(type, out int contributorType);
            if (string.IsNullOrEmpty(type) || contributorType == 0)
            {
                contributorType = -1;
                model.Type = "-1";
            }
            var contributors = new List<Contributor>();
            if (User.ContributorTypeId() == (int)Domain.Common.ContributorType.Provider || User.ContributorTypeId() == (int)Domain.Common.ContributorType.AuthorizedProvider)
            {
                var operations = contributorOperationsService.GetContributorOperations(int.Parse(User.ContributorId().ToString()), null);
                contributors = operations.Where(o => !o.Deleted).Select(c => new Contributor
                {
                    Id = c.Contributor.Id,
                    Code = c.Contributor.Code,
                    Name = c.Contributor.Name,
                    BusinessName = c.Contributor.BusinessName,
                    Status = c.Contributor.Status,
                    StartDate = c.Contributor.StartDate,
                    EndDate = c.Contributor.EndDate,
                    AcceptanceStatusId = c.Contributor.AcceptanceStatusId,
                    AcceptanceStatus = c.Contributor.AcceptanceStatus,
                }).ToList();
            }
            else
            {
                //if (contributorType == (int)Domain.Common.ContributorType.Biller)
                //    contributors = contributorService.GetBillerContributors(model.Page, model.Length);
                //if (contributorType == (int)Domain.Common.ContributorType.Provider)
                //    contributors = contributorService.GetProviderContributors(model.Page, model.Length);
                if (contributorType == (int)Domain.Common.ContributorType.Biller || contributorType == (int)Domain.Common.ContributorType.Provider)
                    contributors = contributorService.GetContributors(contributorType, model.Page, model.Length);
                else if (contributorType == -1)
                    contributors = contributorService.GetParticipantContributors(model.Page, model.Length);
            }
            model.Contributors = contributors.Select(c => new ContributorViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                BusinessName = c.BusinessName,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                AcceptanceStatusId = c.AcceptanceStatusId,
                AcceptanceStatusName = c.AcceptanceStatus.Name,
            }).ToList();

            model.SearchFinished = true;
            return View(model);
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador, Proveedor, Super")]
        public ActionResult List(ContributorTableViewModel model)
        {
            SetView(model.Type);

            var contributors = new List<Contributor>();
            int.TryParse(model.Type, out int contributorType);

            if (model.Type == "-1")
                contributorType = -1;

            if (User.ContributorTypeId() == (int)Domain.Common.ContributorType.Provider || User.ContributorTypeId() == (int)Domain.Common.ContributorType.AuthorizedProvider)
            {
                var operations = contributorOperationsService.GetContributorOperations(int.Parse(User.ContributorId().ToString()), model.Code);
                contributors = operations.Where(o => !o.Deleted).Select(c => new Contributor
                {
                    Id = c.Contributor.Id,
                    Code = c.Contributor.Code,
                    Name = c.Contributor.Name,
                    BusinessName = c.Contributor.BusinessName,
                    Status = c.Contributor.Status,
                    StartDate = c.Contributor.StartDate,
                    EndDate = c.Contributor.EndDate,
                    AcceptanceStatusId = c.Contributor.AcceptanceStatusId,
                    AcceptanceStatus = c.Contributor.AcceptanceStatus,
                }).ToList();
            }
            else
            {
                if (string.IsNullOrEmpty(model.Code))
                {
                    //if (contributorType == (int)Domain.Common.ContributorType.Biller)
                    //    contributors = contributorService.GetBillerContributors(model.Page, model.Length);
                    //if (contributorType == (int)Domain.Common.ContributorType.Provider)
                    //    contributors = contributorService.GetProviderContributors(model.Page, model.Length);
                    if (contributorType == (int)Domain.Common.ContributorType.Biller || contributorType == (int)Domain.Common.ContributorType.Provider)
                        contributors = contributorService.GetContributors(contributorType, model.Page, model.Length);
                    else if (contributorType == -1)
                        contributors = contributorService.GetParticipantContributors(model.Page, model.Length);
                }
                else
                {
                    Contributor contributor = null;
                    if (model.Type == "-1")
                        contributor = contributorService.GetByCode(model.Code);
                    else
                        contributor = contributorService.GetByCode(model.Code, int.Parse(model.Type));
                    if (contributor != null)
                        contributors.Add(contributor);
                }
            }

            model.Contributors = contributors.Select(c => new ContributorViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                BusinessName = c.BusinessName,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                AcceptanceStatusId = c.AcceptanceStatusId,
                AcceptanceStatusName = c.AcceptanceStatus != null ? c.AcceptanceStatus.Name : "",
            }).ToList();

            model.SearchFinished = true;
            return View(model);
        }

        public ActionResult Register()
        {
            if (ConfigurationManager.GetValue("Environment") == "Prod")
                return RedirectToAction(nameof(UserController.Unauthorized));

            Contributor contributor = contributorService.Get(User.ContributorId());
            SetView(contributor.ContributorTypeId.ToString());

            var model = new ContributorViewModel
            {
                Id = contributor.Id,
                Code = contributor.Code,
                Name = contributor.Name,
                BusinessName = contributor.BusinessName,
                Email = contributor.Email?.Trim(),
                ExchangeEmail = contributor.ExchangeEmail,
                AcceptanceStatusId = contributor.AcceptanceStatusId,
                AcceptanceStatusName = Domain.Common.EnumHelper.GetEnumDescription((ContributorStatus)contributor.AcceptanceStatusId),
                PrincipalActivityCode = contributor.PrincipalActivityCode
            };
            FillContributorCategory(model);

            if (string.IsNullOrEmpty(model.PrincipalActivityCode))
                ModelState.AddModelError("PrincipalActivityCode", "");

            return View(model);
        }

        [HttpPost]
        public ActionResult Register(ContributorViewModel model)
        {
            if (ConfigurationManager.GetValue("Environment") == "Prod")
                return RedirectToAction(nameof(UserController.Unauthorized));

            if (User.ContributorId() == 0)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction(nameof(UserController.Login), "User");
            }

            if (User.ContributorId() != model.Id)
                return RedirectToAction(nameof(UserController.Unauthorized), "User");

            if (!ModelState.IsValid)
                return View(model);

            Contributor contributor = contributorService.ObsoleteGet(model.Id);
            contributor.ContributorTypeId = (int)Domain.Common.ContributorType.Biller;
            contributor.AcceptanceStatusId = (int)ContributorStatus.Registered;
            contributor.ExchangeEmail = model.ExchangeEmail?.ToLower();
            contributorService.AddOrUpdate(contributor);

            var globalContributor = new GlobalContributor(contributor.Code, contributor.Code) { Code = contributor.Code, StatusId = contributor.AcceptanceStatusId, TypeId = contributor.ContributorTypeId };
            tableManagerGlobalContributor.InsertOrUpdate(globalContributor);

            if (!string.IsNullOrEmpty(contributor.ExchangeEmail))
            {
                var globalExchangeEmail = new GlobalExchangeEmail(contributor.Code, contributor.Code) { Email = contributor.ExchangeEmail?.ToLower() };
                tableManagerGlobalExchangeEmail.InsertOrUpdate(globalExchangeEmail);

                // insert in production.
                if (ConfigurationManager.GetValue("Environment") == "Hab")
                {
                    var globalStorageConnectionStringProduction = ConfigurationManager.GetValue("GlobalStorage");
                    var tableManagerExchangeEmailProdcution = new TableManager("GlobalExchangeEmail", globalStorageConnectionStringProduction);
                    tableManagerExchangeEmailProdcution.InsertOrUpdate(globalExchangeEmail);
                }
            }

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction(nameof(UserController.Login), "User");
        }

        public ActionResult RegisterNoOfe()
        {
            if (ConfigurationManager.GetValue("Environment") == "Prod")
                return RedirectToAction(nameof(UserController.Unauthorized));

            Contributor contributor = contributorService.Get(User.ContributorId());
            SetView(contributor.ContributorTypeId.ToString());

            var model = new ContributorViewModel
            {
                Id = contributor.Id,
                Code = contributor.Code,
                Name = contributor.Name,
                BusinessName = contributor.BusinessName,
                Email = contributor.Email?.Trim(),
                ExchangeEmail = contributor.ExchangeEmail ?? "",
                AcceptanceStatusId = contributor.AcceptanceStatusId,
                AcceptanceStatusName = Domain.Common.EnumHelper.GetEnumDescription((ContributorStatus)contributor.AcceptanceStatusId),
                PrincipalActivityCode = contributor.PrincipalActivityCode
            };
            FillContributorCategory(model);

            if (string.IsNullOrEmpty(model.PrincipalActivityCode))
                ModelState.AddModelError("PrincipalActivityCode", "");

            return View(model);
        }

        [HttpPost]
        public ActionResult RegisterNoOfe(ContributorViewModel model)
        {
            if (ConfigurationManager.GetValue("Environment") == "Prod")
                return RedirectToAction(nameof(UserController.Unauthorized));

            if (User.ContributorId() == 0)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction(nameof(UserController.Login), "User");
            }

            if (User.ContributorId() != model.Id)
                return RedirectToAction(nameof(UserController.Unauthorized), "User");

            if (!ModelState.IsValid)
                return View(model);

            Contributor contributor = contributorService.ObsoleteGet(model.Id);
            contributor.ContributorTypeId = (int)Domain.Common.ContributorType.BillerNoObliged;
            contributor.AcceptanceStatusId = (int)ContributorStatus.Registered;
            contributor.ExchangeEmail = model.ExchangeEmail?.ToLower();
            contributorService.AddOrUpdate(contributor);

            var globalContributor = new GlobalContributor(contributor.Code, contributor.Code) { Code = contributor.Code, StatusId = contributor.AcceptanceStatusId, TypeId = contributor.ContributorTypeId };
            tableManagerGlobalContributor.InsertOrUpdate(globalContributor);

            if (!string.IsNullOrEmpty(contributor.ExchangeEmail))
            {
                var globalExchangeEmail = new GlobalExchangeEmail(contributor.Code, contributor.Code) { Email = contributor.ExchangeEmail?.ToLower() };
                tableManagerGlobalExchangeEmail.InsertOrUpdate(globalExchangeEmail);

                // insert in production.
                if (ConfigurationManager.GetValue("Environment") == "Hab")
                {
                    var globalStorageConnectionStringProduction = ConfigurationManager.GetValue("GlobalStorageProduction");
                    var tableManagerExchangeEmailProdcution = new TableManager("GlobalExchangeEmail", globalStorageConnectionStringProduction);
                    tableManagerExchangeEmailProdcution.InsertOrUpdate(globalExchangeEmail);
                }
            }

            return RedirectToAction(nameof(RegisterNoOfe), "Contributor");
        }

        public ActionResult ConfigureOperationModes(int id)
        {
            if (ConfigurationManager.GetValue("Environment") == "Prod")
                return RedirectToAction(nameof(UserController.Unauthorized));

            Contributor contributor = contributorOperationsService.GetContributor(User.ContributorId());

            if (contributor.ContributorTypeId == (int)Domain.Common.ContributorType.Provider)
            {
                var c = contributorService.GetContributorFiles(User.ContributorId());
                if (c.ContributorFiles.Any(f => f.ContributorFileStatus.Id != (int)Domain.Common.ContributorFileStatus.Approved))
                    return RedirectToAction(nameof(Wizard));
            }

            ContributorViewModel model = new ContributorViewModel { Id = contributor.Id, ContributorTypeId = contributor.ContributorTypeId.Value, Name = contributor.Name, Code = contributor.Code };
            model.ContributorTypeId = contributor.ContributorTypeId.Value;
            model.Software = new SoftwareViewModel { Url = ConfigurationManager.GetValue("WebServiceUrl"), Id = Guid.NewGuid() };

            var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);

            var contributorOperations = contributor.ContributorOperations.Where(o => !o.Deleted);

            var contributorOperationsModel = new List<ContributorOperationsViewModel>();
            foreach (var item in contributorOperations)
            {
                var testSet = GetTestSetResult(testSetResults, item, model);
                if (testSet == null)
                {
                    item.Deleted = true;
                    contributorOperationsService.AddOrUpdate(item);

                    var softwareId = GetSoftwareIdByOperationType(item.SoftwareId, item.OperationModeId);
                    var software = softwareService.Get(Guid.Parse(softwareId));

                    if (software != null && item.OperationModeId == (int)Domain.Common.OperationMode.Own)
                    {
                        software.Deleted = true;
                        software.Updated = DateTime.UtcNow;
                        softwareService.AddOrUpdate(software);
                    }

                    continue;
                }

                contributorOperationsModel.Add(new ContributorOperationsViewModel
                {
                    Id = item.Id,
                    OperationMode = item.OperationMode != null ? new OperationModeViewModel { Id = item.OperationModeId, Name = item.OperationMode.Name } : null,
                    OperationModeId = item.OperationModeId,
                    Provider = item.Provider != null ? new ContributorViewModel { Name = item.Contributor.Name, AcceptanceStatusName = item.Contributor.AcceptanceStatus.Name } : null,
                    ProviderId = item.ProviderId,
                    Deleted = item.Deleted,
                    Timestamp = item.Timestamp,
                    StatusId = testSet.Status,
                    Status = Domain.Common.EnumHelper.GetEnumDescription((TestSetStatus)testSet.Status),
                    Software = item.Software != null ? new SoftwareViewModel
                    {
                        Id = item.Software.Id,
                        Pin = item.Software.Pin,
                        Name = item.Software.Name,
                        Url = item.Software.Url,
                        AcceptanceStatusSoftwareId = item.Software.AcceptanceStatusSoftwareId,
                        StatusName = item.Software.AcceptanceStatusSoftware.Name
                    } : null,
                    SoftwareId = item.SoftwareId
                });
            }
            model.ContributorOperations = contributorOperationsModel;

            var providers = contributorService.GetContributorsByType((int)Domain.Common.ContributorType.Provider).Where(x => x.AcceptanceStatusId == (int)ContributorStatus.Enabled && x.Softwares.Count > 0);
            model.Providers = providers.Select(z => new ProviderViewModel
            {
                Id = z.Id,
                Code = z.Code,
                Name = z.Name,
                Softwares = z.Softwares.Where(s => !s.Deleted && s.AcceptanceStatusSoftwareId == (int)SoftwareStatus.Production).Select(s => new SoftwareViewModel
                {
                    Id = s.Id,
                    ContributorId = s.ContributorId,
                    Name = s.Name
                }).ToList()
            }).ToList();

            foreach (var p in model.Providers)
            {
                // se seleccionan solo los softwares que tenga aceptado su set de pruebas
                var softwareIds = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(p.Code).Where(t => !t.Deleted && t.Status == (int)TestSetStatus.Accepted).Select(t => t.SoftwareId);
                p.Softwares = p.Softwares.Where(s => softwareIds.Contains(s.Id.ToString())).ToList();

                // se excluyen softwares que ya se encuentran asociados
                p.Softwares = p.Softwares.Where(s => !model.ContributorOperations.Select(o => o.SoftwareId).Contains(s.Id)).ToList();
            }

            // se elimina proveedor que no tenga softwares con set de pruebas aceptado y agregados como modo de operación PT
            model.Providers.RemoveAll(p => p.Softwares.Count == 0);

            foreach (var o in model.OperationModes)
            {

            }

            // modo operación software gratuito aceptado
            var operation = model.ContributorOperations.FirstOrDefault(co => !co.Deleted && co.SoftwareId == Guid.Parse(ConfigurationManager.GetValue("BillerSoftwareId")) && co.StatusId == (int)TestSetStatus.Accepted);

            if (model.ContributorOperations.Any(c => !c.Deleted && (c.StatusId == (int)TestSetStatus.InProcess || c.StatusId == (int)TestSetStatus.Rejected)))
            {
                if (model.ContributorOperations.Any(c => !c.Deleted && c.StatusId == (int)TestSetStatus.Rejected))
                {
                    var testSetResultRejected = testSetResults.FirstOrDefault(t => !t.Deleted && t.Status == (int)TestSetStatus.Rejected);
                    model.TestSetResultRejected = new ContributorTestSetResultViewModel { PartitionKey = testSetResultRejected.PartitionKey, RowKey = testSetResultRejected.RowKey, Status = testSetResultRejected.Status };
                }

                var operations = model.ContributorOperations.Where(c => !c.Deleted && (c.StatusId == (int)TestSetStatus.InProcess || c.StatusId == (int)TestSetStatus.Rejected));
                var ids = operations.Select(o => o.OperationModeId);
                model.OperationModes = model.GetOperationModes().Where(o => !ids.Contains(o.Id)).ToList();
            }
            else
                model.OperationModes = model.GetOperationModes();

            // se eliminar software gratuito si este se encuentra aceptado
            model.OperationModes = model.OperationModes.Where(o => o.Id != operation?.OperationModeId).ToList();

            ViewBag.CurrentPage = Navigation.NavigationEnum.HFE;
            return View(model);
        }

        public ActionResult OperationModes(int id)
        {
            //if (ConfigurationManager.GetValue("Environment") != "Prod")
            //    return RedirectToAction(nameof(UserController.Unauthorized));

            Contributor contributor = contributorOperationsService.GetContributor(User.ContributorId());
            ContributorViewModel model = new ContributorViewModel { Id = contributor.Id, ContributorTypeId = contributor.ContributorTypeId.Value, Name = contributor.Name, Code = contributor.Code };
            model.Software = new SoftwareViewModel { Url = ConfigurationManager.GetValue("WebServiceUrl") };

            var operations = contributor.ContributorOperations.Where(o => !o.Deleted);
            model.ContributorOperations = operations.Select(x => new ContributorOperationsViewModel
            {
                Id = x.Id,
                OperationMode = x.OperationMode != null ? new OperationModeViewModel { Id = x.OperationModeId, Name = x.OperationMode.Name } : null,
                OperationModeId = x.OperationModeId,
                Provider = x.Provider != null ? new ContributorViewModel { Name = x.Provider.Name, AcceptanceStatusName = x.Provider.AcceptanceStatus.Name } : null,
                ProviderId = x.ProviderId,
                Timestamp = x.Timestamp,
                Software = x.Software != null ? new SoftwareViewModel
                {
                    Id = x.Software.Id,
                    Pin = x.Software.Pin,
                    Name = x.Software.Name,
                    Url = x.Software.Url,
                    AcceptanceStatusSoftwareId = x.Software.AcceptanceStatusSoftwareId,
                    StatusName = x.Software.AcceptanceStatusSoftware.Name
                } : null,
                SoftwareId = x.SoftwareId
            }).ToList();


            return View(model);
        }

        [Obsolete]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OldAddContributorOperations(ContributorViewModel model)
        {
            if (ConfigurationManager.GetValue("Environment") == "Prod")
                return RedirectToAction(nameof(UserController.Unauthorized));

            if (User.ContributorId() != model.Id)
                return RedirectToAction(nameof(UserController.Unauthorized), "User");
            var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(model.Code).ToList();
            var testSetResultOk = testSetResults.Where(x => x.Status == (int)TestSetStatus.InProcess && !x.Deleted).ToList();
            if (testSetResultOk.Count() > 0)
            {
                TempData["ErrorMessage"] = "No puede tener un modo de operación en pruebas.";
                return RedirectToAction("ConfigureOperationModes", new { id = model.Id });
            }
            ContributorOperations contributorOperation = new ContributorOperations
            {
                ContributorId = model.Id,
                OperationModeId = model.OperationModeId.Value,
                ProviderId = model.OperationModeId == (int)Domain.Common.OperationMode.Provider ? model.ProviderId : null,
                SoftwareId = model.SoftwareId,
                //SoftwareId = model.OperationModeId == (int)Domain.Common.OperationMode.Provider ? model.SoftwareId : softwareId,
                Deleted = false,
                Timestamp = DateTime.UtcNow
            };

            if (contributorOperation.OperationModeId == (int)Domain.Common.OperationMode.Free)
            {
                contributorOperation.SoftwareId = Guid.Parse(ConfigurationManager.GetValue("BillerSoftwareId"));
                var software = softwareService.Get(contributorOperation.SoftwareId.Value);
                contributorOperation.ProviderId = software.ContributorId;
            }

            var contributorOperationSearch = contributorOperationsService.Get(model.Id, contributorOperation.OperationModeId, contributorOperation.ProviderId, contributorOperation.SoftwareId);

            if (model.OperationModeId == (int)Domain.Common.OperationMode.Provider && model.SoftwareId == Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                TempData["ErrorMessage"] = "Debe especificar un software para el proveedor.";
                return RedirectToAction("ConfigureOperationModes", new { id = model.Id });
            }

            if (contributorOperationSearch == null)
            {
                if (model.OperationModeId == (int)Domain.Common.OperationMode.Own)
                {
                    var software = new Software
                    {
                        Id = model.Software.Id != Guid.Parse("00000000-0000-0000-0000-000000000000") ? model.Software.Id : Guid.NewGuid(),
                        Pin = !string.IsNullOrEmpty(model.Software.Pin) ? model.Software.Pin : Guid.NewGuid().ToString(),
                        Name = model.Software.Name,
                        ContributorId = model.Id,
                        Status = true,
                        SoftwareUser = model.Software.SoftwareUser,
                        SoftwarePassword = model.Software.SoftwarePassword,
                        Url = ConfigurationManager.GetValue("WebServiceUrl"),
                        SoftwareDate = DateTime.UtcNow,
                        Timestamp = DateTime.UtcNow,
                        Updated = DateTime.UtcNow,
                        CreatedBy = User.Identity.Name,
                        AcceptanceStatusSoftwareId = model.Software.AcceptanceStatusSoftwareId != 0 ? model.Software.AcceptanceStatusSoftwareId : 1
                    };
                    softwareService.AddOrUpdate(software);

                    //
                    var globalSoftware = new GlobalSoftware(software.Id.ToString(), software.Id.ToString()) { Id = software.Id, Deleted = software.Deleted, Pin = software.Pin, StatusId = software.AcceptanceStatusSoftwareId };
                    tableManagerGlobalSoftware.InsertOrUpdate(globalSoftware);

                    contributorOperation.SoftwareId = software.Id;
                    model.SoftwareId = software.Id;

                    var authorization = new GlobalAuthorization(User.UserCode(), model.Code);
                    tableManagerGlobalAuthorization.InsertOrUpdate(authorization);
                    authorization = new GlobalAuthorization(model.Code, model.Code);
                    tableManagerGlobalAuthorization.InsertOrUpdate(authorization);
                }

                var globalTestSet = tableManagerTestSet.Find<GlobalTestSet>(model.OperationModeId.ToString(), model.OperationModeId.ToString());
                contributorOperationsService.AddOrUpdate(contributorOperation);
                var testSetResultSoftwareId = GetSoftwareIdByOperationType(model.SoftwareId, model.OperationModeId.Value);

                if (model.ContributorTypeId == (int)Domain.Common.ContributorType.Zero)
                    model.ContributorTypeId = (int)Domain.Common.ContributorType.Biller;

                var testResult = new GlobalTestSetResult(model.Code, $"{model.ContributorTypeId}|{testSetResultSoftwareId}")
                {
                    ContributorId = contributorOperation.ContributorId,
                    SenderCode = model.Code,
                    SoftwareId = testSetResultSoftwareId,
                    ContributorTypeId = model.ContributorTypeId.ToString(),
                    OperationModeId = contributorOperation.OperationModeId,
                    OperationModeName = Domain.Common.EnumHelper.GetEnumDescription((Domain.Common.OperationMode)contributorOperation.OperationModeId),
                    ProviderId = model.OperationModeId == (int)Domain.Common.OperationMode.Provider ? model.ProviderId : null,
                    TestSetReference = model.OperationModeId.Value.ToString(),
                    TotalDocumentsRejected = 0,
                    Status = (int)TestSetStatus.InProcess,
                    Deleted = false,
                    Id = Guid.NewGuid().ToString(),

                    TotalDocumentRequired = globalTestSet.TotalDocumentRequired,
                    TotalDocumentAcceptedRequired = globalTestSet.TotalDocumentAcceptedRequired,
                    TotalDocumentSent = 0,
                    TotalDocumentAccepted = 0,

                    InvoicesTotalRequired = globalTestSet.InvoicesTotalRequired,
                    TotalInvoicesAcceptedRequired = globalTestSet.TotalInvoicesAcceptedRequired,
                    InvoicesTotalSent = 0,
                    TotalInvoicesAccepted = 0,

                    TotalCreditNotesRequired = globalTestSet.TotalCreditNotesRequired,
                    TotalCreditNotesAcceptedRequired = globalTestSet.TotalCreditNotesAcceptedRequired,
                    TotalCreditNotesSent = 0,
                    TotalCreditNotesAccepted = 0,

                    TotalDebitNotesRequired = globalTestSet.TotalDebitNotesRequired,
                    TotalDebitNotesAcceptedRequired = globalTestSet.TotalDebitNotesAcceptedRequired,
                    TotalDebitNotesSent = 0,
                    TotalDebitNotesAccepted = 0,
                };
                tableManagerTestSetResult.InsertOrUpdate(testResult);
            }
            else if (!contributorOperationSearch.Deleted)
            {
                TempData["ErrorMessage"] = "Ya especificó un set de pruebas con estas características.";
            }
            else
            {
                contributorOperationSearch.Deleted = false;
                contributorOperationsService.AddOrUpdate(contributorOperationSearch);
                var rkSoftwareId = GetSoftwareIdByOperationType(contributorOperation.SoftwareId, contributorOperation.OperationModeId);
                var rk = $"{User.ContributorTypeId()}|{rkSoftwareId}";
                var testSetResult = testSetResults.SingleOrDefault(x => x.RowKey == rk);
                testSetResult.Deleted = false;
                tableManagerTestSetResult.InsertOrUpdate(testSetResult);

                if (model.OperationModeId == (int)Domain.Common.OperationMode.Provider)
                {
                    if (new string[] { "Dev", "Hab", "Test" }.Contains(ConfigurationManager.GetValue("Environment")))
                    {
                        if (model.OperationModeId == (int)Domain.Common.OperationMode.Provider)
                        {
                            var provider = contributorService.Get(contributorOperation.ProviderId.Value);
                            if (provider != null)
                            {
                                var authorization = new GlobalAuthorization(provider.Code, model.Code);
                                tableManagerGlobalAuthorization.InsertOrUpdate(authorization);
                            }
                        }
                    }
                }
            }
            return RedirectToAction("ConfigureOperationModes", new { id = model.Id });
        }

        [HttpPost]
        public ActionResult AddContributorOperations(ContributorViewModel model)
        {
            if (ConfigurationManager.GetValue("Environment") == "Prod")
                return RedirectToAction(nameof(UserController.Unauthorized));

            if (User.ContributorId() != model.Id)
                return RedirectToAction(nameof(UserController.Unauthorized), "User");

            if (model.ContributorTypeId == (int)Domain.Common.ContributorType.Provider)
            {
                var c = contributorService.GetContributorFiles(model.Id);
                if (c.ContributorFiles.Any(f => f.ContributorFileStatus.Id != (int)Domain.Common.ContributorFileStatus.Approved))
                    return RedirectToAction(nameof(Wizard));
            }

            var contributorCode = User.ContributorCode();
            var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributorCode);
            if (testSetResults.Any(t => !t.Deleted && t.Status == (int)TestSetStatus.InProcess))
                return RedirectToAction(nameof(ConfigureOperationModes), new { id = model.Id });

            // instancia del objeto contributorOperation
            ContributorOperations contributorOperation = new ContributorOperations
            {
                ContributorId = model.Id,
                ContributorTypeId = User.ContributorTypeId(),
                OperationModeId = model.OperationModeId.Value,
                ProviderId = model.OperationModeId == (int)Domain.Common.OperationMode.Provider ? model.ProviderId : null,
                SoftwareId = model.SoftwareId,
                Deleted = false,
                Timestamp = DateTime.UtcNow
            };

            var operation = contributorOperationsService.Get(contributorOperation.ContributorId, contributorOperation.OperationModeId, contributorOperation.ProviderId, contributorOperation.SoftwareId);
            if (operation != null)
                return RedirectToAction(nameof(ConfigureOperationModes), new { id = model.Id });

            if (model.ContributorTypeId == (int)Domain.Common.ContributorType.Zero)
                model.ContributorTypeId = (int)Domain.Common.ContributorType.Biller;

            // facturador gratuito
            if (contributorOperation.OperationModeId == (int)Domain.Common.OperationMode.Free)
            {
                contributorOperation.SoftwareId = Guid.Parse(ConfigurationManager.GetValue("BillerSoftwareId"));
                var software = softwareService.Get(contributorOperation.SoftwareId.Value);
                contributorOperation.ProviderId = software.ContributorId;
                model.SoftwareId = contributorOperation.SoftwareId.Value;
            }

            // software propio
            if (model.OperationModeId == (int)Domain.Common.OperationMode.Own)
            {
                var software = softwareService.Get(model.Software.Id);
                if (software == null)
                {
                    var softwareInstance = new Software
                    {
                        Id = model.Software.Id,
                        Pin = !string.IsNullOrEmpty(model.Software.Pin) ? model.Software.Pin : Guid.NewGuid().ToString().Substring(0, 5),
                        Name = model.Software.Name,
                        ContributorId = model.Id,
                        Status = true,
                        SoftwareUser = model.Software.SoftwareUser,
                        SoftwarePassword = model.Software.SoftwarePassword,
                        Url = ConfigurationManager.GetValue("WebServiceUrl"),
                        SoftwareDate = DateTime.UtcNow,
                        Timestamp = DateTime.UtcNow,
                        Updated = DateTime.UtcNow,
                        CreatedBy = User.Identity.Name,
                        AcceptanceStatusSoftwareId = model.Software.AcceptanceStatusSoftwareId != 0 ? model.Software.AcceptanceStatusSoftwareId : 1
                    };
                    softwareService.AddOrUpdate(softwareInstance);

                    //
                    var globalSoftware = new GlobalSoftware(softwareInstance.Id.ToString(), softwareInstance.Id.ToString()) { Id = softwareInstance.Id, Deleted = softwareInstance.Deleted, Pin = softwareInstance.Pin, StatusId = softwareInstance.AcceptanceStatusSoftwareId };
                    tableManagerGlobalSoftware.InsertOrUpdate(globalSoftware);

                    contributorOperation.SoftwareId = softwareInstance.Id;
                    model.SoftwareId = softwareInstance.Id;
                }
            }

            // software propio
            if (model.OperationModeId == (int)Domain.Common.OperationMode.Provider)
                contributorOperation.SoftwareId = model.SoftwareId;

            // se agrega modo de operación en bd
            contributorOperationsService.AddOrUpdate(contributorOperation);

            // se obtiene set de pruebas para modo de operación
            var globalTestSet = tableManagerTestSet.Find<GlobalTestSet>(model.OperationModeId.ToString(), model.OperationModeId.ToString());

            // se crea test set result object
            var testResult = new GlobalTestSetResult(model.Code, $"{model.ContributorTypeId}|{model.SoftwareId}")
            {
                ContributorId = contributorOperation.ContributorId,
                SenderCode = model.Code,
                SoftwareId = model.SoftwareId.ToString(),
                ContributorTypeId = model.ContributorTypeId.ToString(),
                OperationModeId = contributorOperation.OperationModeId,
                OperationModeName = Domain.Common.EnumHelper.GetEnumDescription((Domain.Common.OperationMode)contributorOperation.OperationModeId),
                ProviderId = model.OperationModeId == (int)Domain.Common.OperationMode.Provider ? model.ProviderId : null,
                TestSetReference = model.OperationModeId.Value.ToString(),
                TotalDocumentsRejected = 0,
                Status = (int)TestSetStatus.InProcess,
                Deleted = false,
                Id = Guid.NewGuid().ToString(),

                TotalDocumentRequired = globalTestSet.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = globalTestSet.TotalDocumentAcceptedRequired,
                TotalDocumentSent = 0,
                TotalDocumentAccepted = 0,

                InvoicesTotalRequired = globalTestSet.InvoicesTotalRequired,
                TotalInvoicesAcceptedRequired = globalTestSet.TotalInvoicesAcceptedRequired,
                InvoicesTotalSent = 0,
                TotalInvoicesAccepted = 0,

                TotalCreditNotesRequired = globalTestSet.TotalCreditNotesRequired,
                TotalCreditNotesAcceptedRequired = globalTestSet.TotalCreditNotesAcceptedRequired,
                TotalCreditNotesSent = 0,
                TotalCreditNotesAccepted = 0,

                TotalDebitNotesRequired = globalTestSet.TotalDebitNotesRequired,
                TotalDebitNotesAcceptedRequired = globalTestSet.TotalDebitNotesAcceptedRequired,
                TotalDebitNotesSent = 0,
                TotalDebitNotesAccepted = 0,
            };
            tableManagerTestSetResult.InsertOrUpdate(testResult);

            // asociación de permisos
            if (model.OperationModeId == (int)Domain.Common.OperationMode.Own || model.OperationModeId == (int)Domain.Common.OperationMode.Provider)
            {
                var authorization = new GlobalAuthorization(User.UserCode(), model.Code);
                tableManagerGlobalAuthorization.InsertOrUpdate(authorization);
                authorization = new GlobalAuthorization(model.Code, model.Code);
                tableManagerGlobalAuthorization.InsertOrUpdate(authorization);

                // solo para proveedor tecnológico
                if (model.OperationModeId == (int)Domain.Common.OperationMode.Provider)
                {
                    if (new string[] { "Dev", "Hab", "Test" }.Contains(ConfigurationManager.GetValue("Environment")))
                    {
                        var provider = contributorService.Get(contributorOperation.ProviderId.Value);
                        if (provider != null)
                        {
                            authorization = new GlobalAuthorization(provider.Code, model.Code);
                            tableManagerGlobalAuthorization.InsertOrUpdate(authorization);
                        }
                    }
                }
            }

            NotificationsController notification = new NotificationsController();
            notification.EventNotificationsAsyncOperationMode("04", User.UserCode(), (int)model.OperationModeId);

            return RedirectToAction("ConfigureOperationModes", new { id = model.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveContributorOperation(int id)
        {
            ContributorOperations contributorOperation = contributorOperationsService.GetForDelete(id);
            contributorOperation.Id = id;
            contributorOperation.Deleted = true;

            var softwareId = GetSoftwareIdByOperationType(contributorOperation.SoftwareId, contributorOperation.OperationModeId);

            var rk = $"{contributorOperation.Contributor.ContributorTypeId}|{softwareId}";
            var testSetResult = tableManagerTestSetResult.Find<GlobalTestSetResult>(contributorOperation.Contributor.Code, rk);
            if (testSetResult == null)
            {
                if (contributorOperation.Contributor.ContributorTypeId == 1)
                    rk = $"2|{softwareId}";
                else if(contributorOperation.Contributor.ContributorTypeId == 2)
                    rk = $"1|{softwareId}";
                testSetResult = tableManagerTestSetResult.Find<GlobalTestSetResult>(contributorOperation.Contributor.Code, rk);
            }
            if (testSetResult == null)
            {
                return Json(new
                {
                    id,
                    messasge = "No se encontró la información del modo de operación.",
                    success = false
                }, JsonRequestBehavior.AllowGet);
            }
            if (testSetResult.Status == (int)TestSetStatus.Accepted)
            {
                return Json(new
                {
                    id,
                    messasge = "No puede eliminar un set de pruebas aceptado.",
                    success = false
                }, JsonRequestBehavior.AllowGet);
            }

            contributorOperationsService.AddOrUpdate(contributorOperation);

            var software = softwareService.Get(Guid.Parse(softwareId));
            if (software != null && contributorOperation.OperationModeId == (int)Domain.Common.OperationMode.Own)
            {
                software.Deleted = true;
                software.Updated = DateTime.UtcNow;
                softwareService.AddOrUpdate(software);
            }

            testSetResult.Deleted = true;
            tableManagerTestSetResult.InsertOrUpdate(testSetResult);

            if (User.UserCode() != contributorOperation.Contributor?.Code)
            {
                var authorization = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(User.UserCode(), contributorOperation.Contributor?.Code);
                if (authorization != null) tableManagerGlobalAuthorization.Delete(authorization);
            }

            var provider = contributorService.Get(software.ContributorId);
            if (provider != null && provider.Code != contributorOperation.Contributor?.Code && contributorOperation.OperationModeId == (int)Domain.Common.OperationMode.Provider)
            {
                var authorization = tableManagerGlobalAuthorization.Find<GlobalAuthorization>(provider.Code, contributorOperation.Contributor?.Code);
                if (authorization != null) tableManagerGlobalAuthorization.Delete(authorization);
            }

            var json = Json(new
            {
                success = contributorOperation != null,
                id
            }, JsonRequestBehavior.AllowGet);

            return json;
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Facturador")]
        [ValidateAntiForgeryToken]
        public JsonResult SetToProvider(int id)
        {
            var contributor = contributorService.ObsoleteGet(id);
            contributor.ContributorTypeId = (int)Domain.Common.ContributorType.Provider;
            contributor.AcceptanceStatusId = (int)ContributorStatus.Pending;
            contributor.OperationModeId = null;
            contributor.ProviderId = null;

            contributor.ContributorFiles = DataUtils.GetMandatoryContributorFileTypes().Select(c => new ContributorFile
            {
                Id = Guid.NewGuid(),
                FileName = c.Name,
                Status = 0,
                Comments = "",
                Deleted = false,
                FileType = c.Id,
                ContributorId = contributor.Id,
                CreatedBy = User.Identity.Name,
                Timestamp = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            }).ToList();
            contributorService.AddOrUpdate(contributor);

            CreateTestSetResultForProvider(contributor);

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            var json = Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
            return json;
        }


        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Facturador, Proveedor")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SyncToProduction(int id)
        {
            try
            {
                var response = await ApiHelpers.ExecuteRequestAsync<GlobalContributorActivation>(ConfigurationManager.GetValue("SendToActivateContributorUrl"), new { contributorId = id });
                if (!response.Success)
                    return Json(new
                    {
                        success = false,
                        message = response.Message
                    }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    message = response.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        //[CustomRoleAuthorization(CustomRoles = "Facturador, Proveedor")]
        public async Task<JsonResult> SetHabilitationAndProductionDates(string habilitationDate, string productionDate)
        {

            DateTime.TryParseExact(habilitationDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _habilitationDate);
            DateTime.TryParseExact(productionDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _productionDate);
            var contributor = contributorService.Get(User.ContributorId());

            if (contributor == null)
                return Json(new
                {
                    success = false,
                    message = "No se encontró contribuyente."
                }, JsonRequestBehavior.AllowGet);

            if (contributor.AcceptanceStatusId == (int)ContributorStatus.Enabled && contributor.ProductionDate != null)
                return Json(new
                {
                    success = false,
                    message = "Contribuyente ya registró fecha de inicio en producción."
                }, JsonRequestBehavior.AllowGet);

            contributor.HabilitationDate = contributor.HabilitationDate ?? _habilitationDate;
            contributor.ProductionDate = contributor.ProductionDate ?? _productionDate;

            var status = false;
            var statusDescription = string.Empty;
            if (new string[] { "Hab", "Test" }.Contains(ConfigurationManager.GetValue("Environment")))
            {
                try
                {
                    // Get token
                    var result = await GetAssignResponsabilityToken();
                    // Assign responsoabilty
                    var response = await AssignResponsability(contributor, result);
                    int.TryParse(response.Code, out int statusCode);
                    if (statusCode == (int)HttpStatus.Success)
                    {
                        // Update data in db.
                        contributorService.SetHabilitationAndProductionDates(contributor);
                        // Update data in production db.
                        if (ConfigurationManager.GetValue("Environment") == "Hab")
                        {
                            var sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");
                            contributorService.SetHabilitationAndProductionDates(contributor, sqlConnectionStringProd);
                        }

                        status = true;
                        statusDescription = Domain.Common.EnumHelper.GetEnumDescription(HttpStatus.Success);
                    }
                    else
                        statusDescription = response.StatusDescription;
                }
                catch (Exception ex)
                {
                    statusDescription = ex.Message;
                }
            }
            else
                statusDescription = Domain.Common.EnumHelper.GetEnumDescription(HttpStatus.InvalidEnvironment);

            var json = Json(new
            {
                success = status,
                message = statusDescription
            }, JsonRequestBehavior.AllowGet);
            return json;
        }

        private static void CreateTestSetResultForProvider(Contributor contributor)
        {
            var globalTestSet = tableManagerTestSet.Find<GlobalTestSet>(((int)Domain.Common.OperationMode.Own).ToString(), ((int)Domain.Common.OperationMode.Own).ToString());
            var globalTestSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);
            var globalTestSetResult = globalTestSetResults.FirstOrDefault(x => !x.Deleted && x.OperationModeId == (int)Domain.Common.OperationMode.Own && x.Status == (int)TestSetStatus.Accepted);

            globalTestSetResult.RowKey = $"{contributor.ContributorTypeId}|{globalTestSetResult.SoftwareId}";
            globalTestSetResult.Status = (int)TestSetStatus.InProcess;
            globalTestSetResult.Id = Guid.NewGuid().ToString();
            globalTestSetResult.ContributorTypeId = contributor.ContributorTypeId.ToString();
            globalTestSetResult.TotalDocumentRequired = globalTestSet.TotalDocumentRequired;

            globalTestSetResult.TotalDocumentAcceptedRequired = globalTestSet.TotalDocumentAcceptedRequired;
            globalTestSetResult.TotalDocumentSent = 0;
            globalTestSetResult.TotalDocumentAccepted = 0;
            globalTestSetResult.TotalDocumentsRejected = 0;

            globalTestSetResult.InvoicesTotalRequired = globalTestSet.InvoicesTotalRequired;
            globalTestSetResult.TotalInvoicesAcceptedRequired = globalTestSet.TotalInvoicesAcceptedRequired;
            globalTestSetResult.InvoicesTotalSent = 0;
            globalTestSetResult.TotalInvoicesAccepted = 0;
            globalTestSetResult.TotalInvoicesRejected = 0;

            globalTestSetResult.TotalCreditNotesRequired = globalTestSet.TotalCreditNotesRequired;
            globalTestSetResult.TotalCreditNotesAcceptedRequired = globalTestSet.TotalCreditNotesAcceptedRequired;
            globalTestSetResult.TotalCreditNotesSent = 0;
            globalTestSetResult.TotalCreditNotesAccepted = 0;
            globalTestSetResult.TotalCreditNotesRejected = 0;

            globalTestSetResult.TotalDebitNotesRequired = globalTestSet.TotalDebitNotesRequired;
            globalTestSetResult.TotalDebitNotesAcceptedRequired = globalTestSet.TotalDebitNotesAcceptedRequired;
            globalTestSetResult.TotalDebitNotesSent = 0;
            globalTestSetResult.TotalDebitNotesAccepted = 0;
            globalTestSetResult.TotalDebitNotesRejected = 0;

            tableManagerTestSetResult.InsertOrUpdate(globalTestSetResult);
        }

        [CustomRoleAuthorization(CustomRoles = "Proveedor")]
        public ActionResult UploadFile(int id, string code, Guid fileId, int fileTypeId, string fileTypeName)
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.Provider;
            var model = new ContributorUploadFileViewModel
            {
                Id = id,
                Code = code,
                FileId = fileId,
                FileTypeId = fileTypeId,
                FileTypeName = fileTypeName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadContributorFile(ContributorUploadFileViewModel model)
        {
            var fileName = "";
            var result = false;
            try
            {
                if (Request.Files.Count > 0)
                {
                    var postedFile = Request.Files[0];

                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        fileName = StringTools.MakeValidFileName(postedFile.FileName);
                        var fileManager = new FileManager();
                        result = fileManager.Upload("contributor-files", model.Code.ToLower() + "/" + fileName, postedFile.InputStream);

                        if (result)
                        {
                            result = contributorService.AddOrUpdateContributorFile(new ContributorFile
                            {
                                Id = model.FileId,
                                FileName = fileName,
                                Updated = DateTime.Now,
                                CreatedBy = User.Identity.Name,
                                Status = 1,
                                Comments = ""
                            });

                            contributorService.AddContributorFileHistory(new ContributorFileHistory
                            {
                                Id = Guid.NewGuid(),
                                ContributorFileId = model.FileId,
                                FileName = fileName,
                                Timestamp = DateTime.Now,
                                CreatedBy = User.Identity.Name,
                                Status = 1,
                                Comments = ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return Json(new { Success = result });
        }

        [HttpPost]
        [CustomRoleAuthorization(CustomRoles = "Administrador")]
        public ActionResult UpdateContributorFile(Guid contributorFileId, string comments, int statusId)
        {
            ContributorFile contributorFile = contributorService.GetContributorFile(contributorFileId);
            contributorFile.Comments = comments;
            contributorFile.Status = statusId;
            contributorFile.Timestamp = DateTime.Now;
            contributorFile.CreatedBy = User.Identity.Name;
            var result = contributorService.AddOrUpdateContributorFile(contributorFile);

            contributorService.AddContributorFileHistory(new ContributorFileHistory
            {
                Id = Guid.NewGuid(),
                ContributorFileId = contributorFile.Id,
                FileName = contributorFile.FileName,
                Timestamp = DateTime.Now,
                CreatedBy = User.Identity.Name,
                Status = 1,
                Comments = comments
            });
            return RedirectToAction("Edit", new { id = contributorFile.ContributorId, type = 2 });
        }

        [CustomRoleAuthorization(CustomRoles = "Administrador")]
        public ActionResult DownloadContributorFile(string code, string fileName)
        {
            try
            {
                string fileNameURL = code + "/" + StringTools.MakeValidFileName(fileName);
                var fileManager = new FileManager();
                var result = fileManager.GetBytes("contributor-files", fileNameURL, out string contentType);
                return File(result, contentType, $"{fileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return File(new byte[1], "application/pdf", $"error");
            }

        }

        public ActionResult DownloadResolutions()
        {
            try
            {
                var resolutionZipFileName = ConfigurationManager.GetValue("ResolutionFileName");
                string fileNameURL = "normative" + "/" + StringTools.MakeValidFileName(resolutionZipFileName);
                var fileManager = new FileManager();
                var result = fileManager.GetBytes("dian", fileNameURL, out string contentType);
                return File(result, contentType, $"{resolutionZipFileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return File(new byte[1], "application/zip", $"error");
            }

        }

        public ActionResult View(int id)
        {
            if (!User.IsInAnyRole("Administrador", "Super"))
                id = User.ContributorId();

            Contributor contributor = contributorService.ObsoleteGet(id);
            SetView(contributor.ContributorTypeId.ToString());

            var userIds = contributorService.GetUserContributors(id).Select(u => u.UserId);
            var testSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);
            //var result = contributorOperationsService.GetContributorOperations(id);
            var contributorOperations = contributorOperationsService.GetContributorOperations(id).Where(co => !co.Deleted).Select(co => new ContributorOperationsViewModel
            {
                OperationModeId = co.OperationModeId,
                Software = co.Software == null ? new SoftwareViewModel() : new SoftwareViewModel { AcceptanceStatusSoftwareId = co.Software.AcceptanceStatusSoftwareId }
            }).ToList();

            var model = new ContributorViewModel
            {
                Id = contributor.Id,
                Code = contributor.Code,
                Name = contributor.Name,
                BusinessName = contributor.BusinessName,
                Email = contributor.Email,
                ExchangeEmail = contributor.ExchangeEmail,
                ProductionDate = contributor.ProductionDate.HasValue ? contributor.ProductionDate.Value.ToString("dd-MM-yyyy") : "Sin registrar",
                PrincipalActivityCode = contributor.PrincipalActivityCode,
                ContributorTypeId = contributor.ContributorTypeId != null ? contributor.ContributorTypeId.Value : 0,
                OperationModeId = contributor.OperationModeId != null ? contributor.OperationModeId.Value : 0,
                ProviderId = contributor.ProviderId != null ? contributor.ProviderId.Value : 0,
                AcceptanceStatusId = contributor.AcceptanceStatusId,
                AcceptanceStatusName = contributor.AcceptanceStatus.Name,
                AcceptanceStatuses = contributorService.GetAcceptanceStatuses().Select(s => new ContributorAcceptanceStatusViewModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name
                }).ToList(),
                Softwares = contributor.Softwares?.Select(s => new SoftwareViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Pin = s.Pin,
                    SoftwarePassword = s.SoftwarePassword,
                    SoftwareUser = s.SoftwareUser,
                    Url = s.Url,
                    CreatedBy = s.SoftwareUser,
                    AcceptanceStatusSoftwareId = s.AcceptanceStatusSoftwareId
                }).ToList(),
                ContributorFiles = contributor.ContributorFiles.Count > 0 ? contributor.ContributorFiles.Select(f => new ContributorFileViewModel
                {
                    Id = f.Id,
                    Comments = f.Comments,
                    ContributorFileStatus = new ContributorFileStatusViewModel
                    {
                        Id = f.ContributorFileStatus.Id,
                        Name = f.ContributorFileStatus.Name,
                    },
                    ContributorFileType = new ContributorFileTypeViewModel
                    {
                        Id = f.ContributorFileType.Id,
                        Mandatory = f.ContributorFileType.Mandatory,
                        Name = f.ContributorFileType.Name,
                        Timestamp = f.ContributorFileType.Timestamp,
                        Updated = f.ContributorFileType.Updated
                    },
                    CreatedBy = f.CreatedBy,
                    Deleted = f.Deleted,
                    FileName = f.FileName,
                    Timestamp = f.Timestamp,
                    Updated = f.Updated

                }).ToList() : null,
                Users = userService.GetUsers(userIds.ToList()).Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Code = u.Code,
                    Name = u.Name,
                    Email = u.Email
                }).ToList(),
                ContributorOperations = contributorOperations
            };

            model.ContributorTestSetResults = testSetResults.Where(t => !t.Deleted).Select(t => new TestSetResultViewModel
            {
                Id = t.Id,
                OperationModeName = t.OperationModeName,
                SoftwareId = t.SoftwareId,
                Status = t.Status,
                StatusDescription = Domain.Common.EnumHelper.GetEnumDescription((TestSetStatus)t.Status),
                TotalInvoicesAcceptedRequired = t.TotalInvoicesAcceptedRequired,
                TotalInvoicesAccepted = t.TotalInvoicesAccepted,
                TotalInvoicesRejected = t.TotalInvoicesRejected,
                TotalCreditNotesAcceptedRequired = t.TotalCreditNotesAcceptedRequired,
                TotalCreditNotesAccepted = t.TotalCreditNotesAccepted,
                TotalCreditNotesRejected = t.TotalCreditNotesRejected,
                TotalDebitNotesAcceptedRequired = t.TotalDebitNotesAcceptedRequired,
                TotalDebitNotesAccepted = t.TotalDebitNotesAccepted,
                TotalDebitNotesRejected = t.TotalDebitNotesRejected
            }).ToList();


            return View(model);
        }

        [CustomRoleAuthorization(CustomRoles = "Proveedor")]
        public ActionResult Wizard()
        {
            var contributorId = User.ContributorId();
            var contributor = contributorService.ObsoleteGet(contributorId);
            //contributor.Softwares
            var globalTestSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);
            var globalTestSetResult = globalTestSetResults.FirstOrDefault(g => !g.Deleted && g.OperationModeId == (int)Domain.Common.OperationMode.Own && g.RowKey.StartsWith(contributor.ContributorTypeId.ToString()));
            if (globalTestSetResult == null)
            {
                CreateTestSetResultForProvider(contributor);
                globalTestSetResults = tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(contributor.Code);
                globalTestSetResult = globalTestSetResults.FirstOrDefault(g => !g.Deleted && g.OperationModeId == (int)Domain.Common.OperationMode.Own && g.RowKey.StartsWith(contributor.ContributorTypeId.ToString()));
            }

            var softwareId = globalTestSetResult != null ? globalTestSetResult.SoftwareId : Guid.Empty.ToString();
            var software = contributor.Softwares.SingleOrDefault(s => s.Id == Guid.Parse(softwareId));

            var model = new ContributorViewModel
            {
                Id = contributor.Id,
                Code = contributor.Code,
                Name = contributor.Name,
                OperationModeId = (int)Domain.Common.OperationMode.Own,
                BusinessName = contributor.BusinessName,
                Email = contributor.Email,
                ContributorTypeId = contributor.ContributorTypeId != null ? contributor.ContributorTypeId.Value : 0,
                AcceptanceStatusId = contributor.AcceptanceStatusId,
                Status = contributor.Status,
                TestSetStatus = globalTestSetResult?.Status,
                ContributorFiles = contributor.ContributorFiles.Count > 0 ? contributor.ContributorFiles.Select(f => new ContributorFileViewModel
                {
                    Id = f.Id,
                    Comments = f.Comments,
                    ContributorFileStatus = new ContributorFileStatusViewModel
                    {
                        Id = f.ContributorFileStatus.Id,
                        Name = f.ContributorFileStatus.Name,
                    },
                    ContributorFileType = new ContributorFileTypeViewModel
                    {
                        Id = f.ContributorFileType.Id,
                        Mandatory = f.ContributorFileType.Mandatory,
                        Name = f.ContributorFileType.Name,
                        Timestamp = f.ContributorFileType.Timestamp,
                        Updated = f.ContributorFileType.Updated
                    },
                    CreatedBy = f.CreatedBy,
                    Deleted = f.Deleted,
                    FileName = f.FileName,
                    Timestamp = f.Timestamp,
                    Updated = f.Updated

                }).ToList() : null
            };
            if (software != null)
            {
                model.Software = new SoftwareViewModel
                {
                    Id = software.Id,
                    Url = software.Url,
                    AcceptanceStatusSoftwareId = software.AcceptanceStatusSoftwareId,
                    Date = software.SoftwareDate,
                    Name = software.Name,
                    Pin = software.Pin,
                };
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ResetCertification(int id)
        {
            var contributor = contributorService.ObsoleteGet(id);
            //contributor.ContributorTypeId = (int)Domain.Common.ContributorType.Provider;
            //contributor.AcceptanceStatusId = (int)ContributorStatus.Registered;
            //contributor.OperationModeId = null;
            //contributor.ProviderId = null;
            //contributor.ContributorFiles = null;

            //contributorService.AddOrUpdate(contributor);
            CreateTestSetResultForProvider(contributor);
            var json = Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
            return json;
        }

        #region Private methods
        private void CreateGlobalTestSetResult(int operationModeId)
        {

        }
        private string GetSoftwareId(Guid? softwareId)
        {
            return softwareId != null ? softwareId.ToString() : ConfigurationManager.GetValue("BillerSoftwareId");
        }

        private string GetSoftwareIdByOperationType(Guid? softwareId, int operationModeid)
        {
            return operationModeid != (int)Domain.Common.OperationMode.Free ? softwareId.Value.ToString() : ConfigurationManager.GetValue("BillerSoftwareId");
        }
        private GlobalTestSetResult GetTestSetResult(List<GlobalTestSetResult> testSetResults, ContributorOperations operation, ContributorViewModel model)
        {

            var softwareId = GetSoftwareId(operation.SoftwareId);

            var testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{operation.ContributorTypeId}|{softwareId}" && t.OperationModeId == operation.OperationModeId);
            if (testSetResult != null) return testSetResult;

            if (testSetResult == null)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{model.ContributorTypeId}|{softwareId}" && t.OperationModeId == operation.OperationModeId);
            if (testSetResult == null)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)Domain.Common.ContributorType.Zero}|{operation.SoftwareId}" && t.OperationModeId == operation.OperationModeId);

            if (testSetResult == null && model.ContributorTypeId == (int)Domain.Common.ContributorType.Provider)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && int.Parse(t.ContributorTypeId) == (int)Domain.Common.ContributorType.Provider && t.SoftwareId == softwareId && t.OperationModeId == operation.OperationModeId);

            if (testSetResult == null && model.ContributorTypeId == (int)Domain.Common.ContributorType.Provider)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && int.Parse(t.ContributorTypeId) == (int)Domain.Common.ContributorType.Biller && t.SoftwareId == softwareId && t.OperationModeId == operation.OperationModeId);

            return testSetResult;
        }
        private void FillContributorCategory(ContributorViewModel model)
        {
            if (model.PrincipalActivityCode != null)
            {
                model.PrincipalActivityCode = model.PrincipalActivityCode.Trim();
                var result = tableManagerDianContributorCategory.GetRangeRows<DianContributorCategory>(100, null);
                DianContributorCategory contributoCategory = null;
                if (model.PrincipalActivityCode.Trim().Length > 2)
                {
                    string category2digits = model.PrincipalActivityCode.Trim().Substring(0, 2);
                    string category3digits = model.PrincipalActivityCode.Trim().Substring(0, 3);
                    Regex regex2digits = new Regex($@"\|{category2digits}\||^{category2digits}\||\|{category2digits}$", RegexOptions.IgnoreCase);
                    Regex regex3digits = new Regex($@"\|{category3digits}\||^{category3digits}\||\|{category3digits}$", RegexOptions.IgnoreCase);
                    var contributoCategories = result.Item1.Where(c => regex2digits.IsMatch(c.ActivityCodes) || regex3digits.IsMatch(c.ActivityCodes));
                    if (contributoCategories.Count() > 0)
                        contributoCategory = contributoCategories.First();
                    else
                        contributoCategory = result.Item1.FirstOrDefault(c => c.PartitionKey == "14");
                }
                if (contributoCategory != null)
                {
                    model.BillerType = contributoCategory.MaximumRegistrationDate > DateTime.UtcNow ? Domain.Common.EnumHelper.GetEnumDescription(BillerType.Voluntary) : Domain.Common.EnumHelper.GetEnumDescription(BillerType.Obliged);
                    model.MaxRegisterDate = contributoCategory.MaximumRegistrationDate.ToString("dd-MM-yyyy");
                    model.MaxStartDate = contributoCategory.MaximumEmissionDate.ToString("dd-MM-yyyy");
                    model.ResolutionNumber = contributoCategory.ResolutionNumber;
                    model.ResolutionDate = contributoCategory.ResolutionDate.ToString("dd-MM-yyyy");
                }
                else
                    model.BillerType = Domain.Common.EnumHelper.GetEnumDescription(BillerType.None);
            }
            else
                model.BillerType = Domain.Common.EnumHelper.GetEnumDescription(BillerType.None);
        }
        private string GetAcceptanceStatusStyle(int statusId)
        {
            switch (statusId)
            {
                case 1:
                    return "text-warning";
                case 2:
                    return "text-yellow";
                case 3:
                    return "text-info";
                case 4:
                    return "text-gosocket";
                default:
                    return "text-red";
            }
        }
        private async Task<ResponseAssignResponsabilityToken> GetAssignResponsabilityToken()
        {
            var loginUrl = ConfigurationManager.GetValue("AssignResponsabilityLoginUrl").Replace("#", "&");
            //loginUrl = loginUrl.Replace("#", "&");
            var obj = new { };
            var response = await ApiHelpers.ExecuteRequestAsync<ResponseAssignResponsabilityToken>(loginUrl, obj);
            return response;
        }
        private async Task<ResponseAssignResponsability> AssignResponsability(Contributor contributor, ResponseAssignResponsabilityToken responseAssignResponsabilityToken)
        {
            var headers = new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "ClientId", responseAssignResponsabilityToken.ClientId },
                    { "Authorization", $"{responseAssignResponsabilityToken.TokenType} {responseAssignResponsabilityToken.TokenId}" }
                };

            var assignResponsabilityUrl = ConfigurationManager.GetValue("AssignResponsabilityUrl");
            var request = new { idTransaccion = StringUtil.GenerateRandomString(), nroIdentificacion = contributor.Code, fechaCambio = contributor.ProductionDate?.ToString("yyyyMMdd") };
            var response = await ApiHelpers.ExecuteRequestWithHeaderAsync<ResponseAssignResponsability>(assignResponsabilityUrl, request, headers);
            return response;
        }
        private void SetView(string type)
        {
            ViewBag.Type = type;
            switch (type)
            {
                case "1":
                    ViewBag.CurrentPage = Navigation.NavigationEnum.HFE;
                    break;
                case "2":
                    ViewBag.CurrentPage = Navigation.NavigationEnum.Provider;
                    break;
                case "3":
                    ViewBag.CurrentPage = Navigation.NavigationEnum.ProviderAuthorized;
                    break;
                default:
                    ViewBag.CurrentPage = Navigation.NavigationEnum.Client;
                    ViewBag.Type = 0;
                    break;
            }
        }
        #endregion

        /************************/
        private AuthToken GetAuthData()
        {
            var userIdentificationType = User.IdentificationTypeId();
            var userCode = User.UserCode();
            var partitionKey = $"{userIdentificationType}|{userCode}";
            var contributorCode = User.ContributorCode();
            var auth = dianAuthTableManager.Find<AuthToken>(partitionKey, contributorCode);
            return auth;
        }
        /************************/
    }
}