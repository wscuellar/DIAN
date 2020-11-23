using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
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
        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;

        public QueryAssociatedEventsController(IQueryAssociatedEventsService queryAssociatedEventsService)
        {
            _queryAssociatedEventsService = queryAssociatedEventsService;
        }

        // GET: QueryAssociatedEvents
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult EventsView(string id, string cufe)
        {
            GlobalDocValidatorDocumentMeta eventItem = _queryAssociatedEventsService.DocumentValidation(id);

            SummaryEventsViewModel model = new SummaryEventsViewModel(eventItem);

            model.EventStatus = (EventStatus)Enum.Parse(typeof(EventStatus), eventItem.EventCode);

            model.CUDE = id;
            SetTitles(eventItem, model);

            GlobalDocValidatorDocumentMeta invoice = _queryAssociatedEventsService.DocumentValidation(cufe);

            SetMandate(model, eventItem, invoice);
            SetEndoso(model, eventItem, invoice);
            model.RequestType = TextResources.Event_RequestType;

            GlobalDocValidatorDocument eventVerification = _queryAssociatedEventsService.EventVerification(eventItem.Identifier);
            SetValidations(model, eventItem, eventVerification);

            GlobalDocValidatorDocumentMeta referenceMeta = _queryAssociatedEventsService.DocumentValidation(eventItem.DocumentReferencedKey);
            SetReferences(model, referenceMeta);
            
            SetEventAssociated(model, eventItem);

            Response.Headers["InjectingPartialView"] = "true";
            return PartialView(model);
        }

        #region Private Methods
        private void SetTitles(GlobalDocValidatorDocumentMeta eventItem, SummaryEventsViewModel model)
        {
            model.Title = _queryAssociatedEventsService.EventTitle(model.EventStatus, eventItem.CustomizationID, eventItem.EventCode);
            model.ValidationTitle = TextResources.Event_ValidationTitle;
            model.ReferenceTitle = TextResources.Event_ReferenceTitle;
        }

        private void SetEventAssociated(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem)
        {
            EventStatus allowEvent = _queryAssociatedEventsService.IdentifyEvent(eventItem);

            if (allowEvent != EventStatus.None)
            {
                model.EventTitle = "Eventos de " + Domain.Common.EnumHelper.GetEnumDescription(model.EventStatus);
                List<GlobalDocValidatorDocumentMeta> otherEvents = _queryAssociatedEventsService.OtherEvents(eventItem.DocumentKey, allowEvent);
                if (otherEvents.Any())
                {
                    foreach (GlobalDocValidatorDocumentMeta otherEvent in otherEvents)
                    {
                        if (_queryAssociatedEventsService.IsVerificated(otherEvent))
                            model.AssociatedEvents.Add(new AssociatedEventsViewModel(otherEvent));
                    }
                }
            }
        }

        private static void SetReferences(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta referenceMeta)
        {
            if (referenceMeta != null)
            {
                string documentType = string.IsNullOrEmpty(referenceMeta.EventCode) ? TextResources.Event_DocumentType : Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), referenceMeta.EventCode)));
                documentType = string.IsNullOrEmpty(documentType) ? TextResources.Event_DocumentType : documentType;
                model.References.Add(new AssociatedReferenceViewModel(referenceMeta, documentType, string.Empty));
            }
        }

        private void SetValidations(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocument eventVerification)
        {
            if (eventVerification.ValidationStatus == 1)
                model.ValidationMessage = TextResources.Event_ValidationMessage;

            if (eventVerification.ValidationStatus == 10)
            {
                List<GlobalDocValidatorTracking> res = _queryAssociatedEventsService.ListTracking(eventItem.DocumentKey);

                model.Validations = res.Select(t => new AssociatedValidationsViewModel(t)).ToList();
            }
        }

        private static void SetEndoso(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocumentMeta invoice)
        {
            if (model.EventStatus == Gosocket.Dian.Domain.Common.EventStatus.EndosoGarantia || model.EventStatus == Gosocket.Dian.Domain.Common.EventStatus.EndosoProcuracion)
                model.Endoso = new EndosoViewModel(eventItem, invoice);
        }

        private void SetMandate(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocumentMeta invoice)
        {
            if (model.EventStatus == Gosocket.Dian.Domain.Common.EventStatus.Mandato)
            {
                model.Mandate = new ElectronicMandateViewModel(eventItem, invoice);

                List<GlobalDocReferenceAttorney> referenceAttorneys = _queryAssociatedEventsService.ReferenceAttorneys(eventItem.DocumentKey, eventItem.DocumentReferencedKey, eventItem.ReceiverCode, eventItem.SenderCode);

                if (referenceAttorneys.Any())
                    model.Mandate.ContractDate = referenceAttorneys.FirstOrDefault().EffectiveDate;
            }
        }
        #endregion
    }
}