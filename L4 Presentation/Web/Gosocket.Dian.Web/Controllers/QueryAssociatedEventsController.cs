using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class QueryAssociatedEventsController : Controller
    {
        #region Fields and Constructor

        private readonly IQueryAssociatedEventsService _queryAssociatedEventsService;
        private readonly IContributorService _contributorService;
        private readonly IAssociateDocuments _associateDocuments;

        public QueryAssociatedEventsController(IQueryAssociatedEventsService queryAssociatedEventsService, IContributorService contributorService, IAssociateDocuments associateDocuments)
        {
            _queryAssociatedEventsService = queryAssociatedEventsService;
            _contributorService = contributorService;
            _associateDocuments = associateDocuments;
        }

        #endregion

        #region Views

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult EventsView(string id, string cufe)
        {
            List<Task> arrayTasks = new List<Task>();
            GlobalDocValidatorDocumentMeta eventItem = _queryAssociatedEventsService.DocumentValidation(id);

            SummaryEventsViewModel model = new SummaryEventsViewModel(eventItem);

            model.EventStatus = (EventStatus)Enum.Parse(typeof(EventStatus), eventItem.EventCode);

            model.CUDE = id;
            model.RequestType = TextResources.Event_RequestType;
            GlobalDocValidatorDocumentMeta invoice = _queryAssociatedEventsService.DocumentValidation(cufe);
            Task operation1 = Task.Run(() =>
            {
                SetMandate(model, eventItem, invoice);
                SetTitles(eventItem, model);
            });
            Task operation2 = Task.Run(() =>
            {
                SetEndoso(model, eventItem, invoice);
            });

            Task operation3 = Task.Run(() =>
            {
                GlobalDocValidatorDocument eventVerification = _queryAssociatedEventsService.EventVerification(eventItem);
                SetValidations(model, eventItem, eventVerification);
            });
            Task operation4 = Task.Run(() =>
            { 
                GlobalDocValidatorDocumentMeta referenceMeta = _queryAssociatedEventsService.DocumentValidation(eventItem.DocumentReferencedKey);
                SetReferences(model, referenceMeta); 
            });
            Task operation5 = Task.Run(() =>
            { 
                SetEventAssociated(model, eventItem); 
            });

            arrayTasks.Add(operation1);
            arrayTasks.Add(operation2);
            arrayTasks.Add(operation3);
            arrayTasks.Add(operation4);
            arrayTasks.Add(operation5);

            Task.WhenAll(arrayTasks).Wait();

            Response.Headers["InjectingPartialView"] = "true";
            return PartialView(model);
        }

        #endregion

        #region Private Methods

        private void SetTitles(GlobalDocValidatorDocumentMeta eventItem, SummaryEventsViewModel model)
        {
            model.Title = _queryAssociatedEventsService.EventTitle(model.EventStatus, eventItem.CustomizationID, eventItem.EventCode, model.Mandate?.SchemeID);
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
            if (eventVerification.ValidationStatus == 1 || eventVerification.ValidationStatus == 0)
                model.ValidationMessage = TextResources.Event_ValidationMessage;

            if (eventVerification.ValidationStatus == 10)
            {
                List<GlobalDocValidatorTracking> res = _queryAssociatedEventsService.ListTracking(eventItem.DocumentKey);

                model.Validations = res.Select(t => new AssociatedValidationsViewModel(t)).ToList();
            }
        }

        private static void SetEndoso(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocumentMeta invoice)
        {
            model.Endoso = new EndosoViewModel(eventItem, invoice);
            if (model.EventStatus == EventStatus.EndosoPropiedad)
            {
                if (eventItem.CustomizationID == "371")
                    model.Endoso.EndosoType = "Con responsabilidad";
                if (eventItem.CustomizationID == "372")
                    model.Endoso.EndosoType = "Sin responsabilidad";
            }
        }

        private void SetMandate(SummaryEventsViewModel model, GlobalDocValidatorDocumentMeta eventItem, GlobalDocValidatorDocumentMeta invoice)
        {
            if (model.EventStatus == EventStatus.Mandato)
            {
                model.Mandate = new ElectronicMandateViewModel(eventItem, invoice);
                Contributor contributor = _contributorService.GetByCode(model.Mandate.TechProviderCode);
                if (contributor != null)
                    model.Mandate.TechProviderName = contributor.BusinessName;
                List<GlobalDocReferenceAttorney> referenceAttorneys = _queryAssociatedEventsService.ReferenceAttorneys(eventItem.DocumentKey, eventItem.DocumentReferencedKey, eventItem.ReceiverCode, eventItem.SenderCode);

                if (referenceAttorneys.Any())
                {
                    var Mandate = referenceAttorneys.FirstOrDefault();
                    model.Mandate.ContractDate = Mandate.EffectiveDate;
                    model.Mandate.SchemeID = Mandate.SchemeID;
                }
            }
        }

        #endregion

    }
}