using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class QueryAssociatedEventsController : Controller
    {
        private readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        private readonly TableManager globalDocValidatorDocumentTableManager = new TableManager("GlobalDocValidatorDocument");
        private readonly TableManager globalDocValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");

        // GET: QueryAssociatedEvents
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult EventsView(string id, string cufe)
        {
            try
            {
                SummaryEventsViewModel model = new SummaryEventsViewModel();
                GlobalDocValidatorDocumentMeta invoice = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(cufe, cufe);

                var eventItem = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(id, id);
                GlobalDocValidatorDocument eventVerification = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier);

                model.EventStatus = (EventStatus)Enum.Parse(typeof(EventStatus), eventItem.EventCode);
                //----------------------------------------------------------------------------------------------------------------Encabezado

                switch (model.EventStatus)
                {
                    case EventStatus.Received:
                        model.Title = "Acuse de Recibo de Factura Electrónica";
                        break;
                    case EventStatus.Receipt:
                        model.Title = "Constancia de Recibo de mercancia";
                        break;
                    case EventStatus.Accepted:
                        model.Title = "Aceptación expresa de la Factura  Electrónica";
                        break;
                    case EventStatus.Mandato:
                        model.Title = "Inscripción de Mandato Electrónico";
                        break;
                    case EventStatus.SolicitudDisponibilizacion:
                        if (eventItem.CustomizationID == "361" || eventItem.CustomizationID == "362")
                            model.Title = "Solicitud de Primera Disponibilización";
                        if (eventItem.CustomizationID == "363" || eventItem.CustomizationID == "364")
                            model.Title = "Solicitud de Disponibilización Posterior";
                        break;
                    case EventStatus.EndosoGarantia:
                    case EventStatus.EndosoProcuracion:
                    case EventStatus.EndosoPropiedad:
                        model.Title = "Endoso Electronico";
                        break;
                    default:
                        model.Title = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), eventItem.EventCode)));
                        break;
                }
                ViewBag.ValidationTitle = "Validaciones del Evento";
                ViewBag.ReferenceTitle = "Referencias del Evento";

                model.CUDE = id;
                model.Prefix = eventItem.Serie;
                model.Number = eventItem.Number;
                model.DateOfIssue = eventItem.SigningTimeStamp.Date;
                //----------------------------------------------------------------------------------------------------------------Encabezado


                //----------------------------------------------------------------------------------General
                model.SenderCode = eventItem.SenderCode;
                model.SenderName = eventItem.SenderName;
                model.ReceiverCode = eventItem.ReceiverCode;
                model.ReceiverName = eventItem.ReceiverName;

                //----------------------------------------------------------------------------------General


                //------------------------------------------------------------------------------------Especializado 
                //--------------para mandato
                if (model.EventStatus == Gosocket.Dian.Domain.Common.EventStatus.Mandato)
                {
                    List<GlobalDocReferenceAttorney> referenceAttorneys = documentMetaTableManager.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(eventItem.DocumentKey, eventItem.DocumentReferencedKey, eventItem.ReceiverCode, eventItem.SenderCode);
                    if (referenceAttorneys.Any())
                    {
                        model.Mandate.ContractDate = referenceAttorneys.FirstOrDefault().EffectiveDate;
                    }

                    model.Mandate.ReceiverCode = eventItem.ReceiverCode;
                    model.Mandate.ReceiverName = eventItem.ReceiverName;
                    model.Mandate.SenderCode = invoice.SenderCode;
                    model.Mandate.SenderName = invoice.SenderName;
                    model.Mandate.MandateType = "Mandato por documento";
                }
                //--------------

                //--------------tipo de solicitud de disponibilizacion
                model.RequestType = "Solicitud de Disponibilización para Negociaciación General";
                //--------------

                //endosante=  eventItem.SenderCode de la factura
                //endosatorio = evento de receiver.

                //---------------endoso enk propiedad
                if (model.EventStatus == Gosocket.Dian.Domain.Common.EventStatus.EndosoGarantia || model.EventStatus == Gosocket.Dian.Domain.Common.EventStatus.EndosoProcuracion)
                {
                    model.Endoso = new EndosoViewModel()
                    {
                        ReceiverCode = eventItem.ReceiverCode,
                        ReceiverName = eventItem.ReceiverName,
                        SenderCode = invoice.SenderCode,
                        SenderName = invoice.SenderName,
                        EndosoType = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), eventItem.EventCode)))
                    };
                }

                //---------------endoso enk propiedad ---045

                //---------------notificacion de pago total o parcila
                //--eventItem.CustomizationID == 452 //pago total -- sino parcial
                /*
                        IVA
                       TOTAl
                */
                //---------------


                //--------------------------------------------------------Validacion de eventos

                if (eventVerification.ValidationStatus == 1)
                {
                    model.ValidationMessage = "Documento validado por la DIAN";
                }
                if (eventVerification.ValidationStatus == 10)
                {
                    List<GlobalDocValidatorTracking> res = globalDocValidatorTrackingTableManager.FindByPartition<GlobalDocValidatorTracking>(eventItem.DocumentKey);
                    model.Validations = res.Select(t => new AssociatedValidationsViewModel()
                    {
                        RuleName = t.RuleName,
                        Status = "Notificación",
                        Message = t.ErrorMessage //esto va en el tooltip.
                    }).ToList();
                }
                //---------------------------------------------------------------

                //------------------------------------------------------------Referencias del evento
                GlobalDocValidatorDocumentMeta referenceMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(eventItem.DocumentReferencedKey, eventItem.DocumentReferencedKey);
                if (referenceMeta != null)
                {
                    string documentType = string.IsNullOrEmpty(referenceMeta.EventCode) ? "Factura Electronica" : Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), referenceMeta.EventCode)));
                    documentType = string.IsNullOrEmpty(documentType) ? "Factura Electronica" : documentType;
                    model.References.Add(new AssociatedReferenceViewModel()
                    {
                        Document = documentType,
                        DateOfIssue = referenceMeta.EmissionDate.Date,
                        Description = string.Empty,
                        SenderCode = referenceMeta.SenderCode,
                        SenderName = referenceMeta.SenderName,
                        ReceiverCode = referenceMeta.ReceiverCode,
                        ReceiverName = referenceMeta.ReceiverName
                    });
                }

                //------------------------------------------------------------------

                //----------------------------------------------------------------------------Eventos asociados
                //eventos que se se puede desprender 
                string endosoCodes = "037,038,039";
                string limitacionCodes = "041";
                string mandatoCodes = "043";
                string eventCode2 = endosoCodes.Contains(eventItem.EventCode.Trim()) ? "040" :
                                    mandatoCodes.Contains(eventItem.EventCode.Trim()) ? "044" :
                                    limitacionCodes.Contains(eventItem.EventCode.Trim()) ? "042" :
                                    string.Empty;

                if (!string.IsNullOrEmpty(eventCode2))
                {
                    model.EventTitle = "Eventos de " + Domain.Common.EnumHelper.GetEnumDescription(model.EventStatus);
                    var otherEvents = documentMetaTableManager.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(eventItem.DocumentKey, eventCode2);
                    if (otherEvents.Any())
                    {

                        foreach (var eventItem3 in otherEvents)
                        {
                            if (!string.IsNullOrEmpty(eventItem3.EventCode))
                            {
                                GlobalDocValidatorDocument eventVerification2 = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(eventItem3.Identifier, eventItem3.Identifier);
                                if (eventVerification2 != null && (eventVerification2.ValidationStatus == 1 || eventVerification.ValidationStatus == 10))
                                {
                                    string documentName = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.EventStatus), eventItem3.EventCode)));
                                    //model.Events.Add(new EventsViewModel()
                                    AssociatedEventsViewModel newEvent = new AssociatedEventsViewModel()
                                    {
                                        EventCode = eventItem.EventCode,
                                        Document = documentName,
                                        EventDate = eventItem.SigningTimeStamp,
                                        SenderCode = eventItem.ReceiverCode,
                                        Sender = eventItem.SenderName,
                                        ReceiverCode = eventItem.ReceiverCode,
                                        Receiver = eventItem.ReceiverName
                                    };
                                    model.AssociatedEvents.Add(newEvent);
                                }

                            }
                        }

                    }
                }
                //-------------------------------------------------------------------------------------------------------------------

                Response.Headers["InjectingPartialView"] = "true";
                return PartialView(model);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}