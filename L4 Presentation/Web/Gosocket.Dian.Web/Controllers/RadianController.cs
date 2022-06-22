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
using Gosocket.Dian.Application;

namespace Gosocket.Dian.Web.Controllers
{
    public class RadianController : Controller
    {
        private readonly IRadianContributorService _radianContributorService;
        private readonly IGlobalRadianContributorEnabledService _globalRadianContributorEnabledService = new GlobalRadianContributorEnabledService();        
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
            int contributorId = User.ContributorId();
            NameValueCollection result = _radianContributorService.Summary(contributorId);
            ViewBag.ContributorId = result["ContributorId"];
            ViewBag.ElectronicInvoice_RadianContributorTypeId = result["ElectronicInvoice_RadianContributorTypeId"];
            ViewBag.ElectronicInvoice_RadianOperationModeId = result["ElectronicInvoice_RadianOperationModeId"];
            ViewBag.TechnologyProvider_RadianContributorTypeId = result["TechnologyProvider_RadianContributorTypeId"];
            ViewBag.TechnologyProvider_RadianOperationModeId = result["TechnologyProvider_RadianOperationModeId"];
            ViewBag.TradingSystem_RadianContributorTypeId = result["TradingSystem_RadianContributorTypeId"];
            ViewBag.TradingSystem_RadianOperationModeId = result["TradingSystem_RadianOperationModeId"];
            ViewBag.Factor_RadianContributorTypeId = result["Factor_RadianContributorTypeId"];
            ViewBag.Factor_RadianOperationModeId = result["Factor_RadianOperationModeId"];
            ViewBag.CurrentPage = Navigation.NavigationEnum.RADIAN;

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
            int contributorId = User.ContributorId();
            ResponseMessage validation = _radianContributorService.RegistrationValidation(contributorId, registrationData.RadianContributorType, registrationData.RadianOperationMode);
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
                    RadianContributorTypeId = c.RadianContributorTypeId,
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

            ViewBag.CurrentPage = Navigation.NavigationEnum.AdminRadian;

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
                    RadianContributorTypeId = c.RadianContributorTypeId,
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
            RadianContributorsViewModel model = new RadianContributorsViewModel
            {
                Id = radianAdmin.Contributor.RadianContributorId,
                Code = radianAdmin.Contributor.Code,
                RadianContributorTypeId = radianAdmin.Contributor.RadianContributorTypeId,
                TradeName = radianAdmin.Contributor.TradeName,
                BusinessName = radianAdmin.Contributor.BusinessName,
                Email = radianAdmin.Contributor.Email,
                ContributorTypeName = radianAdmin.Type?.Name,
                AcceptanceStatusId = radianAdmin.Contributor.AcceptanceStatusId,
                AcceptanceStatusName = radianAdmin.Contributor.AcceptanceStatusName,
                CreatedDate = radianAdmin.Contributor.CreatedDate.AddHours(-5),
                UpdatedDate = radianAdmin.Contributor.Update.AddHours(-5),
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
                IsActive = radianAdmin.Contributor.IsActive

            };

            model.RadianContributorTestSetResults = radianAdmin.Tests.Select(t => new TestSetResultViewModel
            {
                Id = t.Id,
                OperationModeName = t.OperationModeName,
                SoftwareId = t.SoftwareId,
                Status = t.Status,
                StatusDescription = t.State
            }).ToList();


            return View(model);
        }

        public ActionResult DownloadContributorFile(string code, string fileName)
        {
            try
            {
                byte[] result = _radianContributorService.DownloadContributorFile(code, fileName, out string contentType);
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
                
                //Requisitos vencidos
                if(approveState == "2")
                {
                    RadianContributor result = _radianContributorService.ChangeContributorActiveRequirement(radianAdmin.Contributor.RadianContributorId);
                    if(result == null)
                        return Json(new { messasge = "Error al actualizar estado requisitos contribuyente.", success = false, error = "" }, JsonRequestBehavior.AllowGet);
                    else if (result.IsActive)
                    {
                        string guid = Guid.NewGuid().ToString();
                        GlobalRadianContributorEnabled item = new GlobalRadianContributorEnabled(radianAdmin.Contributor.Code, guid)
                        {
                            IsActive = true,
                            UpdateBy = User.Identity.Name
                        };
                        _ = SendMail(radianAdmin);

                        if(_globalRadianContributorEnabledService.Insert(item))
                            return Json(new { message = "Actualiza estado requisitos contribuyente a activos.", success = true, id = radianAdmin.Contributor.RadianContributorId }, JsonRequestBehavior.AllowGet);
                    }                        
                    else if (!result.IsActive)
                    {
                        string guid = Guid.NewGuid().ToString();
                        GlobalRadianContributorEnabled item = new GlobalRadianContributorEnabled(radianAdmin.Contributor.Code, guid)
                        {
                            IsActive = false,
                            UpdateBy = User.Identity.Name
                        };
                        _ = SendMail(radianAdmin);

                        if(_globalRadianContributorEnabledService.Insert(item))                        
                            return Json(new { message = "Actualiza estado requisitos contribuyente a inactivos.", success = true, id = radianAdmin.Contributor.RadianContributorId }, JsonRequestBehavior.AllowGet);
                    }
                    
                }
                else
                {
                    RadianState stateProcess = approveState == "1" ? RadianState.Cancelado : RadianState.Test;

                    //Participantes en pruebas no pueden ser cancelados
                    if (radianAdmin.Contributor.RadianState == RadianState.Test.GetDescription() && stateProcess == RadianState.Cancelado)
                        return Json(new { message = TextResources.TestNotRemove, success = false, id = radianAdmin.Contributor.RadianContributorId }, JsonRequestBehavior.AllowGet);

                    //Todos los archivos soporte deben estar aceptados 
                    if (stateProcess == RadianState.Test && radianAdmin.Files.Any(n => n.Status != 2 && n.RadianContributorFileType.Mandatory))
                        return Json(new { message = TextResources.AllSoftware, success = false, id = radianAdmin.Contributor.RadianContributorId }, JsonRequestBehavior.AllowGet);

                    //El SW de la operacion tiene clientes asociados
                    if (radianAdmin.Contributor.RadianState == RadianState.Habilitado.GetDescription() && stateProcess == RadianState.Cancelado)
                    {
                        int counter = _radianContributorService.GetAssociatedClients(radianAdmin.Contributor.RadianContributorId);
                        if (counter > 0)
                        {
                            string message = string.Format(TextResources.WithCustomerList, counter);
                            return Json(new { message, success = false, id = radianAdmin.Contributor.RadianContributorId }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    _ = _radianContributorService.ChangeParticipantStatus(radianAdmin.Contributor.Id, stateProcess.GetDescription(), radianAdmin.Contributor.RadianContributorTypeId, radianState, description);

                    if (stateProcess == RadianState.Test && radianAdmin.Contributor.RadianOperationModeId == (int)Domain.Common.RadianOperationMode.Direct)
                        _radianContributorService.UpdateRadianOperation(radianAdmin.Contributor.RadianContributorId, (int)Domain.Common.RadianOperationModeTestSet.OwnSoftware);

                }

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
                message.Append("<br/>");
                message.Append("<div>");
                message.AppendFormat("Nombre del requisito: <strong>{0}</strong>", file.FileName);
                message.Append("<ul>");
                message.AppendFormat("<li>Estado del requisito: {0}</li>", ReturnStatus(file.Status));
                message.AppendFormat("<li>Observación del requisito: {0}</li>", file.Comments);
                message.Append("</ul>");
                message.Append("</div>");
            }

            message.Append("<div>");
            message.AppendFormat("Observaciones: {0}", radianAdmin.Contributor.RadianState);
            message.Append("</div>");

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            emailService.SendEmail(radianAdmin.Contributor.Email, "Resultado Validación Documentos Requisitos RADIAN", dic);

            return true;
        }

        [HttpPost]
        public JsonResult GetSetTestByContributor(string code, string softwareId, int type)
        {
            RadianTestSetResult result = _radianContributorService.GetSetTestResult(code, softwareId, type);

            List<EventCountersViewModel> events = new List<EventCountersViewModel>();
            if (result == null)
                return Json(events, JsonRequestBehavior.AllowGet); ;

            // Acuse de recibo
            events.Add(new EventCountersViewModel() { EventName = EventStatus.Received.GetDescription(), Counter1 = result.ReceiptNoticeTotalAcceptedRequired, Counter2 = result.ReceiptNoticeAccepted, Counter3 = result.ReceiptNoticeRejected });

            //Recibo del bien
            events.Add(new EventCountersViewModel() { EventName = EventStatus.Receipt.GetDescription(), Counter1 = result.ReceiptServiceTotalAcceptedRequired, Counter2 = result.ReceiptServiceAccepted, Counter3 = result.ReceiptServiceRejected });

            // Aceptación expresa
            events.Add(new EventCountersViewModel() { EventName = EventStatus.Accepted.GetDescription(), Counter1 = result.ExpressAcceptanceTotalAcceptedRequired, Counter2 = result.ExpressAcceptanceAccepted, Counter3 = result.ExpressAcceptanceRejected });

            //Manifestación de aceptación -- aceptacion Tacita
            events.Add(new EventCountersViewModel() { EventName = EventStatus.AceptacionTacita.GetDescription(), Counter1 = result.AutomaticAcceptanceTotalAcceptedRequired, Counter2 = result.AutomaticAcceptanceAccepted, Counter3 = result.AutomaticAcceptanceRejected });

            //Rechazo factura electrónica
            events.Add(new EventCountersViewModel() { EventName = EventStatus.Rejected.GetDescription(), Counter1 = result.RejectInvoiceTotalAcceptedRequired, Counter2 = result.RejectInvoiceAccepted, Counter3 = result.RejectInvoiceRejected });

            // Solicitud disponibilización
            events.Add(new EventCountersViewModel() { EventName = EventStatus.SolicitudDisponibilizacion.GetDescription(), Counter1 = result.ApplicationAvailableTotalAcceptedRequired, Counter2 = result.ApplicationAvailableAccepted, Counter3 = result.ApplicationAvailableRejected });

            // Endoso en Propiedad
            events.Add(new EventCountersViewModel() { EventName = EventStatus.EndosoPropiedad.GetDescription(), Counter1 = result.EndorsementPropertyTotalAcceptedRequired, Counter2 = result.EndorsementPropertyAccepted, Counter3 = result.EndorsementPropertyRejected });

            // Endoso en Procuracion
            events.Add(new EventCountersViewModel() { EventName = EventStatus.EndosoProcuracion.GetDescription(), Counter1 = result.EndorsementProcurementTotalAcceptedRequired, Counter2 = result.EndorsementProcurementAccepted, Counter3 = result.EndorsementProcurementRejected });

            // Endoso en Garantia
            events.Add(new EventCountersViewModel() { EventName = EventStatus.EndosoGarantia.GetDescription(), Counter1 = result.EndorsementGuaranteeTotalAcceptedRequired, Counter2 = result.EndorsementGuaranteeAccepted, Counter3 = result.EndorsementGuaranteeRejected });

            // Cancelación de endoso
            events.Add(new EventCountersViewModel() { EventName = EventStatus.InvoiceOfferedForNegotiation.GetDescription(), Counter1 = result.EndorsementCancellationTotalAcceptedRequired, Counter2 = result.EndorsementCancellationAccepted, Counter3 = result.EndorsementCancellationRejected });

            // Avales
            events.Add(new EventCountersViewModel() { EventName = EventStatus.Avales.GetDescription(), Counter1 = result.GuaranteeTotalAcceptedRequired, Counter2 = result.GuaranteeAccepted, Counter3 = result.GuaranteeRejected });

            // Mandato electrónico
            events.Add(new EventCountersViewModel() { EventName = EventStatus.Mandato.GetDescription(), Counter1 = result.ElectronicMandateTotalAcceptedRequired, Counter2 = result.ElectronicMandateAccepted, Counter3 = result.ElectronicMandateRejected });

            // Terminación mandato
            events.Add(new EventCountersViewModel() { EventName = EventStatus.TerminacionMandato.GetDescription(), Counter1 = result.EndMandateTotalAcceptedRequired, Counter2 = result.EndMandateAccepted, Counter3 = result.EndMandateRejected });

            // Notificación de pago
            events.Add(new EventCountersViewModel() { EventName = EventStatus.NotificacionPagoTotalParcial.GetDescription(), Counter1 = result.PaymentNotificationTotalAcceptedRequired, Counter2 = result.PaymentNotificationAccepted, Counter3 = result.PaymentNotificationRejected });

            // Limitación de circulación
            events.Add(new EventCountersViewModel() { EventName = EventStatus.NegotiatedInvoice.GetDescription(), Counter1 = result.CirculationLimitationTotalAcceptedRequired, Counter2 = result.CirculationLimitationAccepted, Counter3 = result.CirculationLimitationRejected });

            // Terminación limitación  
            events.Add(new EventCountersViewModel() { EventName = EventStatus.AnulacionLimitacionCirculacion.GetDescription(), Counter1 = result.EndCirculationLimitationTotalAcceptedRequired, Counter2 = result.EndCirculationLimitationAccepted, Counter3 = result.EndCirculationLimitationRejected });


            events.Add(new EventCountersViewModel() { EventName = EventStatus.ValInfoPago.GetDescription(), Counter1 = result.ReportForPaymentTotalAcceptedRequired, Counter2 = result.ReportForPaymentAccepted, Counter3 = result.ReportForPaymentRejected });

            //Endoso con efectos de cesión ordinaria
            events.Add(new EventCountersViewModel() { EventName = EventStatus.EndorsementWithEffectOrdinaryAssignment.GetDescription(), Counter1 = result.EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired, Counter2 = result.EndorsementWithEffectOrdinaryAssignmentAccepted, Counter3 = result.EndorsementWithEffectOrdinaryAssignmentRejected });

            //Protesto
            events.Add(new EventCountersViewModel() { EventName = EventStatus.Objection.GetDescription(), Counter1 = result.ObjectionTotalAcceptedRequired, Counter2 = result.ObjectionAccepted, Counter3 = result.ObjectionRejected });

            //Transferencia de los derechos económicos 
            events.Add(new EventCountersViewModel() { EventName = EventStatus.TransferEconomicRights.GetDescription(), Counter1 = result.TransferEconomicRightsTotalAcceptedRequired, Counter2 = result.TransferEconomicRightsAccepted, Counter3 = result.TransferEconomicRightsRejected });

            //Notificación al deudor sobre la transferencia de los derechos económicos 
            events.Add(new EventCountersViewModel() { EventName = EventStatus.NotificationDebtorOfTransferEconomicRights.GetDescription(), Counter1 = result.NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired, Counter2 = result.NotificationDebtorOfTransferEconomicRightsAccepted, Counter3 = result.NotificationDebtorOfTransferEconomicRightsRejected });

            //Pago de la transferencia de los derechos económicos 
            events.Add(new EventCountersViewModel() { EventName = EventStatus.PaymentOfTransferEconomicRights.GetDescription(), Counter1 = result.PaymentOfTransferEconomicRightsTotalAcceptedRequired, Counter2 = result.PaymentOfTransferEconomicRightsAccepted, Counter3 = result.PaymentOfTransferEconomicRightsRejected });

            return Json(events, JsonRequestBehavior.AllowGet);
        }

        private string ReturnStatus(int idStatus)
        {
            switch (idStatus)
            {
                case 0:
                    return "Pendiente";
                case 1:
                    return "Cargado y en revisión";
                case 2:
                    return "Aprobado";
                case 3:
                    return "Rechazado";
                case 4:
                    return "Observaciones";
                default:
                    return "Pendiente";
            }
        }
    }


}