using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain;
using System.Collections.Generic;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Gosocket.Dian.Web.Common;
using System.Linq;
using System;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces;
using System.Diagnostics;
using System.Data.Entity;
using Gosocket.Dian.Application.Managers;
using Microsoft.AspNet.Identity;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {

        private readonly IContributorService _ContributorService;
        private readonly IRadianContributorService _RadianContributorService;
        private readonly UserService userService = new UserService();
        private static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        private readonly RadianTestSetResultManager radianTestSetManager = new RadianTestSetResultManager();


        public RadianController(IContributorService contributorService, IRadianContributorService radianContributorService)
        {

            _ContributorService = contributorService;
            _RadianContributorService = radianContributorService;
        }


        private void SetContributorInfo()
        {
            string userCode = User.UserCode();
            Domain.Contributor contributor = _ContributorService.GetByCode(userCode);
            if (contributor == null) return;

            ViewBag.ContributorId = contributor.Id;
            ViewBag.ContributorTypeId = contributor.ContributorTypeId;
            ViewBag.Active = contributor.Status;
            ViewBag.WithSoft = contributor.Softwares?.Count > 0;

            List<Domain.RadianContributor> radianContributor = _RadianContributorService.List(t => t.ContributorId == contributor.Id && t.RadianState != "Cancelado");
            string rcontributorTypes = radianContributor?.Aggregate("", (current, next) => current + ", " + next.RadianContributorTypeId.ToString());
            ViewBag.ExistInRadian = rcontributorTypes;
        }
        
        // GET: Radian
        public ActionResult Index()
        {
            SetContributorInfo();
            return View();
        }

        public ActionResult ElectronicInvoiceView()
        {
            SetContributorInfo();
            return View();
        }

        public ActionResult AdminRadianView()
        {
            var radianContributors = _RadianContributorService.List(t => true);
            var radianContributorType = _RadianContributorService.GetRadianContributorTypes(t => true);
            var radianFileStatus = _RadianContributorService.GetRadianContributorFileStatus(t => true);
            var model = new AdminRadianViewModel();
            var model2 = new RadianContributorsViewModel();
            model.RadianContributors = radianContributors.Select(c =>
            new RadianContributorsViewModel()
            {
                Id = c.Contributor.Id,
                Code = c.Contributor.Code,
                TradeName = c.Contributor.Name,
                BusinessName = c.Contributor.BusinessName,
                AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name  

            }).ToList();

            //model2.RadianFileStatus = radianFileStatus.Select(c => new SelectListItem
            //{
            //    Value = c.Id.ToString(),
            //    Text = c.Name

            //}).ToList();

            model.RadianType = radianContributorType.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name

            }).ToList();
            model.SearchFinished = true;
            return View(model);
        }

        [HttpPost]
        public ActionResult AdminRadianView(AdminRadianViewModel model)
        {
            var radianContributorType = _RadianContributorService.GetRadianContributorTypes(t => true);
            DateTime? startDate = string.IsNullOrEmpty(model.StartDate) ? null : (DateTime?)Convert.ToDateTime(model.StartDate).Date;
            DateTime? endDate = string.IsNullOrEmpty(model.EndDate) ? null : (DateTime?)Convert.ToDateTime(model.EndDate).Date;
            var radianContributors = _RadianContributorService.List(t => 
            (t.Contributor.Code == model.Code || model.Code == null) && 
            (t.RadianContributorTypeId == model.Type || model.Type == 0) && 
            ( t.RadianState == model.RadianState.ToString() || model.RadianState == null) &&
            (DbFunctions.TruncateTime(t.CreatedDate) >= startDate || !startDate.HasValue) && 
            (DbFunctions.TruncateTime(t.CreatedDate) <= endDate || !endDate.HasValue), 
            model.Page, model.Length);
            
            model.RadianType = radianContributorType.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name

            }).ToList();

            model.RadianContributors = radianContributors.Select(c => new RadianContributorsViewModel
            {
                Id = c.Contributor.Id,
                Code = c.Contributor.Code,
                TradeName = c.Contributor.Name,
                BusinessName = c.Contributor.BusinessName,
                AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name

            }).ToList();

            model.SearchFinished = true;
            return View(model);
        }

        public ActionResult ViewDetails(int id)
        {

            var radianContributor = _RadianContributorService.List(t => t.ContributorId == id).FirstOrDefault();
            var userIds = _ContributorService.GetUserContributors(id).Select(u => u.UserId);
            var testSet = radianTestSetManager.GetAllTestSetResultByContributor(id);

            var model = new RadianContributorsViewModel
            {
                Id = radianContributor.Id,
                Code = radianContributor.Contributor.Code,
                TradeName = radianContributor.Contributor.Name,
                BusinessName = radianContributor.Contributor.BusinessName,
                Email = radianContributor.Contributor.Email,
                ContributorTypeName = radianContributor.Contributor.ContributorTypeId != null ? radianContributor.Contributor.ContributorType.Name : "",
                AcceptanceStatusId = radianContributor.Contributor.AcceptanceStatusId,
                AcceptanceStatusName = radianContributor.Contributor.AcceptanceStatus.Name,
                CreatedDate = radianContributor.CreatedDate,
                UpdatedDate = radianContributor.Update,
                RadianState = radianContributor.RadianState,
                RadianContributorFiles = radianContributor.RadianContributorFile.Count > 0 ? radianContributor.RadianContributorFile.Select(f => new RadianContributorFileViewModel
                {
                    Id = f.Id,
                    Comments = f.Comments,
                    ContributorFileStatus = new RadianContributorFileStatusViewModel
                    {
                        Id = f.RadianContributorFileStatus.Id,
                        Name = f.RadianContributorFileStatus.Name,
                    },
                    ContributorFileType = new ContributorFileTypeViewModel
                    {
                        Id = f.RadianContributorFileType.Id,
                        Mandatory = f.RadianContributorFileType.Mandatory,
                        Name = f.RadianContributorFileType.Name,
                        Timestamp = f.RadianContributorFileType.Timestamp,
                        Updated = f.RadianContributorFileType.Updated
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

            };


            model.RadianContributorTestSetResults = testSet.Select(t => new TestSetResultViewModel
            {
                Id = t.Id,
                OperationModeName = t.OperationModeName,
                SoftwareId = t.SoftwareId,
                Status = t.Status,
                StatusDescription = t.StatusDescription,
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

        public ActionResult DownloadContributorFile(string code, string fileName)
        {
            try
            {
                string fileNameURL = code + "/" + StringTools.MakeValidFileName(fileName);
                var fileManager = new FileManager(ConfigurationManager.GetValue("GlobalStorage"));
                var result = fileManager.GetBytes("radiancontributor-files", fileNameURL, out string contentType);
                return File(result, contentType, $"{fileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return File(new byte[1], "application/pdf", $"error");
            }

        }

        [HttpPost]
        public JsonResult ViewDetails(List<FilesChangeStateViewModel> data, int id, string approveState)
        {
            try
            {
                var sendEmail = false;
                var radianContributorFileInstance = new RadianContributorFile();
                var fileStateUpdated = "0";
                var radianContributor = _RadianContributorService.List(t => t.Id == id).FirstOrDefault();
                var radianUpdateContributor = new RadianContributor();
                radianUpdateContributor = radianContributor;
                if (data != null)
                {
                    foreach (var n in data)
                    {
                        radianContributorFileInstance = _RadianContributorService.GetRadianContributorFile(t => t.Id.ToString() == n.Id.ToString()).FirstOrDefault();
                        radianContributorFileInstance.Status = n.NewState;
                        fileStateUpdated = _RadianContributorService.UpdateRadianContributorFile(radianContributorFileInstance).ToString();
                    }
                }

                radianContributor = _RadianContributorService.List(t => t.Id == id).FirstOrDefault();

                foreach(var n in radianContributor.RadianContributorFile)
                {
                    if(n.Status != 2)
                    {
                        sendEmail = false;
                        return Json(new
                        {
                            messasge = "Todos los archivos deben estar en estado 'Aceptado' para poder cambiar el estado del participante.",
                            success = true,
                            id = radianContributor.ContributorId
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        sendEmail = true;
                    }
                }
                if (sendEmail)
                {
                    var isUpdate = UpdateTime(radianUpdateContributor, approveState);
                    var emailSended = SendMail();
                }
                return Json(new
                {
                    messasge = "Datos actualizados correctamente.",
                    success = true,
                    id = radianContributor.ContributorId
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new {
                    messasge = "Tenemos problemas al actualizar los datos.",
                    success = false,
                    error = ex
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public bool UpdateTime(RadianContributor model, string approveState)
        {
            try
            {
                _RadianContributorService.AddOrUpdate(model, approveState);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendMail()
        {
            try
            {
                var emailService = new Gosocket.Dian.Application.EmailService();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("##CONTENT##", "Este es un mensaje de prueba");
                emailService.SendEmail("camilo.lizarazo87@gmail.com", "Esta es una prueba", dic);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
           
        }
    }
}