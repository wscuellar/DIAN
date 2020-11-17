using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
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

        public PartialViewResult EventsView(string id,  string cufe)
        {
            try
            {
                GlobalDocValidatorDocumentMeta invoice = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(cufe, cufe);

                var eventItem = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(id, id);
                GlobalDocValidatorDocument eventVerification = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier);
                //----------------------------------------------------------------------------------------------------------------Encabezado

                // eventItem.CustomizationID == 361 y 362 = solicitud de primera disponibilizacion
                //eventItem.CustomizationID == 363 y 364 = solicitud disponibilizacion posterior
                string eventtitle = string.Empty;
                if (eventItem.EventCode == "036" && eventItem.CustomizationID == "361" || eventItem.CustomizationID == "362")
                    eventtitle = "Solicitud de Primera Disponibilización";

                if (eventItem.EventCode == "036" && eventItem.CustomizationID == "363" || eventItem.CustomizationID == "364")
                    eventtitle = "Solicitud de Disponibilización Posterior";
                else
                    eventtitle = EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), eventItem.EventCode)));

                string CUDE = id;
                string prefijo = eventItem.Serie;
                string number = eventItem.Number;
                DateTime emmisionDate = eventItem.SigningTimeStamp.Date;
                //----------------------------------------------------------------------------------------------------------------Encabezado


                //----------------------------------------------------------------------------------General
                string emisorCode = eventItem.SenderCode;
                string emisorName = eventItem.SenderName;
                string receptorCode = eventItem.ReceiverCode;
                string receptorName = eventItem.ReceiverName;
                //----------------------------------------------------------------------------------General


                //------------------------------------------------------------------------------------Especializado 
                //--------------para mandato
                List<GlobalDocReferenceAttorney> referenceAttorneys = documentMetaTableManager.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(eventItem.DocumentKey, eventItem.DocumentReferencedKey, eventItem.ReceiverCode, eventItem.SenderCode);
                if (referenceAttorneys.Any())
                {
                    var fechacontrato = referenceAttorneys.FirstOrDefault().EffectiveDate;
                }

                string nitmandatario = eventItem.ReceiverCode;
                string nameMandatario = eventItem.ReceiverName;
                string nitmandante = invoice.SenderCode;
                string nameMandante = invoice.SenderName;
                string tipoMandato = "Mandato por documento";
                //--------------



                //--------------tipo de solicitud de disponibilizacion
                string requestType = "Solicitud de Disponibilización para Negociaciación General";
                //--------------

                //endosante=  eventItem.SenderCode de la factura
                //endosatorio = evento de receiver.

                //---------------endoso enk propiedad
                string nitEndosante = invoice.SenderCode;
                string nameEndosante = invoice.SenderName;
                string nitEndosatario = eventItem.ReceiverCode;
                string nameEndosatario = eventItem.ReceiverName;
                string tipoEndoso = EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), eventItem.EventCode)));

                //---------------endoso enk propiedad ---045

                //---------------notificacion de pago total o parcila
                //--eventItem.CustomizationID == 452 //pago total -- sino parcial
                /*
                        IVA
                       TOTAl
                */
                //---------------

                
                //--------------------------------------------------------Validacion de eventos
                string valNotificacion = string.Empty;
                if (eventVerification.ValidationStatus == 1)
                {
                    valNotificacion = "Documento validado por la DIAN";
                }
                if (eventVerification.ValidationStatus == 10)
                {
                    List<GlobalDocValidatorTracking> res = globalDocValidatorTrackingTableManager.FindByPartition<GlobalDocValidatorTracking>(eventItem.DocumentKey);
                    res.Select(t => new
                    {
                        Nombre = t.RuleName,
                        Estado = "Notificación",
                        Message = t.ErrorMessage
                    });
                }
                //---------------------------------------------------------------

                //------------------------------------------------------------Referencias del evento
                GlobalDocValidatorDocumentMeta referenceMeta = documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(eventItem.DocumentReferencedKey, eventItem.DocumentReferencedKey);
                if (referenceMeta != null)
                {
                    string eventcodetext2 = string.IsNullOrEmpty(referenceMeta.EventCode) ? "Factura Electronica" : EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), referenceMeta.EventCode)));
                    eventcodetext2 = string.IsNullOrEmpty(eventcodetext2) ? "Factura Electronica" : eventcodetext2;
                    var emissionDate = referenceMeta.SigningTimeStamp;
                    var Description = string.Empty;
                    var nitEmisor = referenceMeta.SenderCode;
                    var emisor = referenceMeta.SenderName;
                    var nitReceptor = referenceMeta.ReceiverCode;
                    var receptor = referenceMeta.ReceiverName;
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
                    var otherEvents = documentMetaTableManager.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(eventItem.DocumentKey, eventCode2);
                    if (otherEvents.Any())
                    {

                        foreach (var eventItem3 in otherEvents)
                        {
                            if (!string.IsNullOrEmpty(eventItem3.EventCode))
                            {
                                GlobalDocValidatorDocument eventVerification2 = globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier);
                                string eventcodetext3 = EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.EventStatus), eventItem.EventCode)));
                                //model.Events.Add(new EventsViewModel()
                                var t = new
                                {
                                    EventCode = eventItem.EventCode,
                                    Description = eventcodetext3,
                                    EventDate = eventItem.SigningTimeStamp,
                                    SenderCode = eventItem.ReceiverCode,
                                    Sender = eventItem.SenderName,
                                    ReceiverCode = eventItem.ReceiverCode,
                                    Receiver = eventItem.ReceiverName
                                };
                            }
                        }
                    }

                }
                //-------------------------------------------------------------------------------------------------------------------

                Response.Headers["InjectingPartialView"] = "true";
                return PartialView();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}