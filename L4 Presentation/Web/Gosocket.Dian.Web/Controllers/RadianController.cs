using System.Collections.Generic;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Gosocket.Dian.Web.Common;
using System.Linq;
using System;
using Gosocket.Dian.Infrastructure;
using System.Diagnostics;
using Gosocket.Dian.Domain;
using System.Collections.Specialized;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using System.Text;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Common.Resources;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {
        private readonly IRadianContributorService _radianContributorService;
        private readonly UserService userService = new UserService();


        public RadianController(IRadianContributorService radianContributorService)
        {
            _radianContributorService = radianContributorService;
        }


        #region MODOS DE OPERACION RADIAN

        /// <summary>
        /// Action GET encargada de inicializar la vista de ingreso a RADIAN, Consulta la informacion del contribuyente postulante.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            NameValueCollection result = _radianContributorService.Summary(User.UserCode());
            ViewBag.ContributorId = result["ContributorId"];
            ViewBag.ElectronicInvoice_RadianContributorTypeId = result["ElectronicInvoice_RadianContributorTypeId"];
            ViewBag.ElectronicInvoice_RadianOperationModeId = result["ElectronicInvoice_RadianOperationModeId"];
            ViewBag.TechnologyProvider_RadianContributorTypeId = result["TechnologyProvider_RadianContributorTypeId"];
            ViewBag.TechnologyProvider_RadianOperationModeId = result["TechnologyProvider_RadianOperationModeId"];
            ViewBag.TradingSystem_RadianContributorTypeId = result["TradingSystem_RadianContributorTypeId"];
            ViewBag.TradingSystem_RadianOperationModeId = result["TradingSystem_RadianOperationModeId"];
            ViewBag.Factor_RadianContributorTypeId = result["Factor_RadianContributorTypeId"];
            ViewBag.Factor_RadianOperationModeId = result["Factor_RadianOperationModeId"];
            return View();
        }

        /// <summary>
        /// Action GET encargada de inicializar la vista de ingreso a RADIAN, Consulta la informacion del contribuyente postulante, para los modos de Facturador Electronico.
        /// </summary>
        /// <returns></returns>
        public ActionResult ElectronicInvoiceView()
        {
            return Index();
        }

        /// <summary>
        /// Metodo POST, encargado de realizar las validaciones de registro en la seleccion de modos de operacion de RADIAN.
        /// </summary>
        /// <param name="registrationData">Estructura con la informacion a validar</param>
        /// <returns>Json con la respuesta de la validacion</returns>
        [HttpPost]
        public JsonResult RegistrationValidation(RegistrationDataViewModel registrationData)
        {
            ResponseMessage validation = _radianContributorService.RegistrationValidation(User.UserCode(), registrationData.RadianContributorType, registrationData.RadianOperationMode);
            if (validation.MessageType == "redirect")
                validation.RedirectTo = Url.Action("Index", "RadianApproved", registrationData);
            return Json(validation, JsonRequestBehavior.AllowGet);
        }

        #endregion

        /// <summary>
        /// Metodo GET para la consulta de participantes en la habilitacion RADIAN
        /// </summary>
        /// <returns>Modelo con la informacion para graficar la vista de Habilitacion</returns>
        public ActionResult AdminRadianView()
        {
            int page = 1, size = 10;
            RadianAdmin radianAdmin = _radianContributorService.ListParticipants(page, size);

            AdminRadianViewModel model = new AdminRadianViewModel()
            {
                TotalCount = radianAdmin.RowCount,
                CurrentPage = radianAdmin.CurrentPage,
                RadianContributors = radianAdmin.Contributors.Select(c => new RadianContributorsViewModel()
                {
                    Id = c.Id,
                    Code = c.Code,
                    TradeName = c.TradeName,
                    BusinessName = c.BusinessName,
                    AcceptanceStatusName = c.AcceptanceStatusName,
                    RadianState = c.RadianState

                }).ToList(),
                RadianType = radianAdmin.Types.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name

                }).ToList(),
                SearchFinished = true
            };


            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AdminRadianView(AdminRadianViewModel model)
        {
            AdminRadianFilter filter = new AdminRadianFilter()
            {
                Id = model.Id,
                Code = model.Code,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Type = model.Type,
                RadianState = model.RadianState != null ? model.RadianState.Value.GetDescription() : null
            };
            RadianAdmin radianAdmin = _radianContributorService.ListParticipantsFilter(filter, model.Page, model.Length);

            AdminRadianViewModel result = new AdminRadianViewModel()
            {
                TotalCount = radianAdmin.RowCount,
                CurrentPage = radianAdmin.CurrentPage,
                Page = model.Page,
                RadianContributors = radianAdmin.Contributors.Select(c => new RadianContributorsViewModel()
                {
                    Id = c.RadianContributorId,
                    Code = c.Code,
                    TradeName = c.TradeName,
                    BusinessName = c.BusinessName,
                    AcceptanceStatusName = c.AcceptanceStatusName,
                    RadianState = c.RadianState
                }).ToList(),
                RadianType = radianAdmin.Types.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name

                }).ToList(),
                SearchFinished = true
            };
            return View(result);
        }

        public ActionResult ViewDetails(int id)
        {
            RadianAdmin radianAdmin = _radianContributorService.ContributorSummary(id);
            if (radianAdmin.Contributor.RadianState == "Cancelado")
            {
                return RedirectToAction("AdminRadianView");
            }
            RadianContributorsViewModel model = new RadianContributorsViewModel
            {
                Id = radianAdmin.Contributor.RadianContributorId,
                Code = radianAdmin.Contributor.Code,
                TradeName = radianAdmin.Contributor.TradeName,
                BusinessName = radianAdmin.Contributor.BusinessName,
                Email = radianAdmin.Contributor.Email,
                ContributorTypeName = radianAdmin.Type?.Name,
                AcceptanceStatusId = radianAdmin.Contributor.AcceptanceStatusId,
                AcceptanceStatusName = radianAdmin.Contributor.AcceptanceStatusName,
                CreatedDate = radianAdmin.Contributor.CreatedDate,
                UpdatedDate = radianAdmin.Contributor.Update,
                RadianState = radianAdmin.Contributor.RadianState,
                RadianContributorFilesType = radianAdmin.FileTypes,
                RadianContributorFiles = radianAdmin.Files.Count > 0 ? radianAdmin.Files.Select(f => new RadianContributorFileViewModel
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
                Users = userService.GetUsers(radianAdmin.LegalRepresentativeIds).Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Code = u.Code,
                    Name = u.Name,
                    Email = u.Email
                }).ToList(),

            };

            model.RadianContributorTestSetResults = radianAdmin.Tests.Select(t => new TestSetResultViewModel
            {
                Id = t.Id,
                OperationModeName = t.OperationModeName,
                SoftwareId = t.SoftwareId,
                Status = t.Status,
                StatusDescription = t.StatusDescription
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
        public JsonResult ViewDetails(List<FilesChangeStateViewModel> data, int id, string approveState, string radianState, string description)
        {
            try
            {
                if (data != null)
                {
                    RadianContributorFile radianContributorFileInstance = null;
                    foreach (var n in data)
                    {
                        RadianContributorFileHistory radianFileHistory = new RadianContributorFileHistory();
                        radianContributorFileInstance = _radianContributorService.RadianContributorFileList(n.Id).FirstOrDefault();
                        radianContributorFileInstance.Status = n.NewState;
                        radianContributorFileInstance.Comments = n.comment;

                        _ = _radianContributorService.UpdateRadianContributorFile(radianContributorFileInstance).ToString();

                        radianFileHistory.FileName = radianContributorFileInstance.FileName;
                        radianFileHistory.Comments = n.comment;
                        radianFileHistory.CreatedBy = radianContributorFileInstance.CreatedBy;
                        radianFileHistory.Status = n.NewState;
                        radianFileHistory.RadianContributorFileId = radianContributorFileInstance.Id;
                        _ = _radianContributorService.AddFileHistory(radianFileHistory);
                    }
                }

                RadianAdmin radianAdmin = _radianContributorService.ContributorSummary(id);
                RadianState stateProcess = approveState == "1" ? RadianState.Cancelado : RadianState.Test;
                if (radianAdmin.Contributor.RadianState == RadianState.Test.GetDescription() && stateProcess == RadianState.Cancelado)
                    return Json(new { message = TextResources.TestNotRemove, success = true, id = radianAdmin.Contributor.RadianContributorId }, JsonRequestBehavior.AllowGet);

                if (stateProcess == RadianState.Test && radianAdmin.Files.Any(n => n.Status != 2 && n.RadianContributorFileType.Mandatory))
                    return Json(new { message = TextResources.AllSoftware, success = true, id = radianAdmin.Contributor.RadianContributorId }, JsonRequestBehavior.AllowGet);

                if (radianAdmin.Contributor.RadianState == RadianState.Habilitado.GetDescription())
                {
                    string clientsData = _radianContributorService.GetAssociatedClients(radianAdmin.Contributor.RadianContributorId);
                    if (!string.IsNullOrEmpty(clientsData))
                        return Json(new { message = clientsData, success = true, id = radianAdmin.Contributor.RadianContributorId, html = "html" }, JsonRequestBehavior.AllowGet);
                }

                _ = _radianContributorService.ChangeParticipantStatus(radianAdmin.Contributor.Id, stateProcess.GetDescription(), radianAdmin.Contributor.RadianContributorTypeId, radianState, description);
                _ = SendMail(radianAdmin);

                return Json(new { message = TextResources.SuccessSoftware, success = true, id = radianAdmin.Contributor.RadianContributorId }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { messasge = "Tenemos problemas al actualizar los datos.", success = false, error = ex }, JsonRequestBehavior.AllowGet);
            }
        }

        public bool SendMail(RadianAdmin radianAdmin, string observations = "")
        {
            var emailService = new Gosocket.Dian.Application.EmailService();
            StringBuilder message = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            message.Append("A continuacion encontrara el resultado de validación de los documentos requisitos que la DIAN verificó en su proceso RADIAN");

            foreach (RadianContributorFile file in radianAdmin.Files)
            {
                message.AppendFormat("</br> Nombre del Archivo: {0}", file.FileName);
                message.AppendFormat("</br> Estado: {0}", file.Status.GetDescription());
            }

            message.AppendFormat("</br> Observaciones: {0}", radianAdmin.Contributor.RadianState);

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            //emailService.SendEmail(radianAdmin.Contributor.Email, "Resultado Validación Documentos Requisitos RADIAN", dic);

            return true;
        }

        [HttpPost]
        public JsonResult GetSetTestByContributor(string code, string softwareId, string softwareType)
        {
            RadianTestSetResult result = _radianContributorService.GetSetTestResult(code, softwareId, softwareType);
            List<EventCountersViewModel> events = new List<EventCountersViewModel>();
            for(int i=0; i<14; i++)
            {
                events.Add(new EventCountersViewModel() { EventName = "Event name " + i.ToString(), Counter1 = i, Counter2 = i, Counter3 = 3 });
            }
            return Json(events, JsonRequestBehavior.AllowGet);
        }


    }

    public class EventCountersViewModel
    {
        public  string EventName { get; set; }
        public int Counter1 { get; set; }
        public int Counter2 { get; set; }
        public int Counter3 { get; set; }
    }
}