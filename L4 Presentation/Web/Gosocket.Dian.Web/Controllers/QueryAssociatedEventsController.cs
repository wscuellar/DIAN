using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class QueryAssociatedEventsController : Controller
    {
        private readonly IQueryAssociatedEventsService _Service;
        public QueryAssociatedEventsController(IQueryAssociatedEventsService queryAssociatedEventsService)
        {
            _Service = queryAssociatedEventsService;
        }

        // GET: QueryAssociatedEvents
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult EventsView(string id, string cufe)
        {
            GlobalDocValidatorDocumentMeta eventItem = _Service.DocumentValidation(id);
            SummaryEventsViewModel model = new SummaryEventsViewModel(eventItem);

            model.EventStatus = (EventStatus)Enum.Parse(typeof(EventStatus), eventItem.EventCode);
            model.CUDE = id;
            SetTitles(eventItem, model);

            GlobalDocValidatorDocumentMeta invoice = _Service.DocumentValidation(cufe);
            SetMandate(model, eventItem, invoice);
            SetEndoso(model, eventItem, invoice);
            model.RequestType = TextResources.Event_RequestType;

            GlobalDocValidatorDocument eventVerification = _Service.EventVerification(eventItem.Identifier);
            SetValidations(model, eventItem, eventVerification);

            GlobalDocValidatorDocumentMeta referenceMeta = _Service.DocumentValidation(eventItem.DocumentReferencedKey);
            SetReferences(model, referenceMeta);
            SetEventAssociated(model, eventItem);

            Response.Headers["InjectingPartialView"] = "true";
            return PartialView(model);
        }

        #region Private Methods

        private void SetTitles(GlobalDocValidatorDocumentMeta eventItem, SummaryEventsViewModel model)
        {
            model.Title = _Service.EventTitle(model.EventStatus, eventItem.CustomizationID, eventItem.EventCode);
            model.ValidationTitle = TextResources.Event_ValidationTitle;
            model.ReferenceTitle = TextResources.Event_ReferenceTitle;
        }

        private void SetEventAssociated(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem)
        {
            EventStatus allowEvent = _Service.IdentifyEvent(eventItem);
            if (allowEvent == EventStatus.None) return;

            model.EventTitle = "Eventos de " + Domain.Common.EnumHelper.GetEnumDescription(model.EventStatus);
            List<GlobalDocValidatorDocumentMeta> otherEvents = _Service.OtherEvents(eventItem.DocumentKey, allowEvent);
            if (otherEvents.Any())
                foreach (GlobalDocValidatorDocumentMeta otherEvent in otherEvents)
                {
                    if (_Service.IsVerificated(otherEvent))
                        model.AssociatedEvents.Add(new AssociatedEventsViewModel(otherEvent));
                }
        }

        private static void SetReferences(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta referenceMeta)
        {
            if (referenceMeta == null) return;
            string documentType = string.IsNullOrEmpty(referenceMeta.EventCode) ? TextResources.Event_DocumentType : Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(EventStatus), referenceMeta.EventCode)));
            documentType = string.IsNullOrEmpty(documentType) ? TextResources.Event_DocumentType : documentType;
            model.References.Add(new AssociatedReferenceViewModel(referenceMeta, documentType, string.Empty));
        }

        private void SetValidations(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocument eventVerification)
        {
            if (eventVerification.ValidationStatus == 1)
                model.ValidationMessage = TextResources.Event_ValidationMessage;

            if (eventVerification.ValidationStatus == 10)
            {
                List<GlobalDocValidatorTracking> res = _Service.ListTracking(eventItem.DocumentKey);
                model.Validations = res.Select(t => new AssociatedValidationsViewModel(t)).ToList();
            }
        }

        private static void SetEndoso(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocumentMeta invoice)
        {
            if (model.EventStatus == EventStatus.EndosoGarantia || model.EventStatus == EventStatus.EndosoProcuracion)
                model.Endoso = new EndosoViewModel(eventItem, invoice);
        }

        private void SetMandate(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocumentMeta invoice)
        {
            if (model.EventStatus != EventStatus.Mandato) return;
            model.Mandate = new ElectronicMandateViewModel(eventItem, invoice);
            List<GlobalDocReferenceAttorney> referenceAttorneys = _Service.ReferenceAttorneys(eventItem);
            if (referenceAttorneys.Any())
                model.Mandate.ContractDate = referenceAttorneys.FirstOrDefault().EffectiveDate;
        }

        #endregion
    }
}