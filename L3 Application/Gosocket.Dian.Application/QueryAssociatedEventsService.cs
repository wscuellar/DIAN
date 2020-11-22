using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class QueryAssociatedEventsService : IQueryAssociatedEventsService
    {
        private readonly IGlobalDocValidationDocumentMetaService _radianGlobalDocValidationDocumentMeta;
        private readonly IGlobalDocValidatorDocumentService _globalDocValidatorDocument;
        private readonly IGlobalDocValidatorTrackingService _globalDocValidatorTracking;

        const string CONS361 = "361";
        const string CONS362 = "362";
        const string CONS363 = "363";
        const string CONS364 = "364";

        public QueryAssociatedEventsService(IGlobalDocValidationDocumentMetaService radianGlobalDocValidationDocumentMeta, IGlobalDocValidatorDocumentService globalDocValidatorDocument, IGlobalDocValidatorTrackingService globalDocValidatorTracking)
        {
            _radianGlobalDocValidationDocumentMeta = radianGlobalDocValidationDocumentMeta;
            _globalDocValidatorDocument = globalDocValidatorDocument;
            _globalDocValidatorTracking = globalDocValidatorTracking;
        }

        public GlobalDocValidatorDocumentMeta DocumentValidation(string reference)
        {
            return _radianGlobalDocValidationDocumentMeta.DocumentValidation(reference);
        }

        public GlobalDocValidatorDocument EventVerification(string eventItemIdentifier)
        {
            return _globalDocValidatorDocument.EventVerification(eventItemIdentifier);
        }

        public List<GlobalDocReferenceAttorney> ReferenceAttorneys(string documentKey, string documentReferencedKey, string receiverCode, string senderCode)
        {
            return _radianGlobalDocValidationDocumentMeta.ReferenceAttorneys(documentKey, documentReferencedKey, receiverCode, senderCode);
        }

        public List<GlobalDocValidatorDocumentMeta> OtherEvents(string documentKey, string eventCode)
        {
            return _radianGlobalDocValidationDocumentMeta.OtherEvents(documentKey, eventCode);
        }

        public string EventTitle(EventStatus eventStatus, string customizationId, string eventCode)
        {
            string title = string.Empty;            

            switch (eventStatus)
            {
                case EventStatus.Received:
                    title = TextResources.Received;
                    break;
                case EventStatus.Receipt:
                    title = TextResources.Receipt;
                    break;
                case EventStatus.Accepted:
                    title = TextResources.Accepted;
                    break;
                case EventStatus.Mandato:
                    title = TextResources.Mandato;
                    break;
                case EventStatus.SolicitudDisponibilizacion:
                    if (customizationId == CONS361 || customizationId == CONS362)
                        title = TextResources.SolicitudDisponibilizacion;
                    if (customizationId == CONS363 || customizationId == CONS364)
                        title = TextResources.SolicitudDisponibilizacion1;
                    break;
                case EventStatus.EndosoGarantia:
                case EventStatus.EndosoProcuracion:
                case EventStatus.EndosoPropiedad:
                    title = TextResources.Endoso;
                    break;
                default:
                    title = EnumHelper.GetEnumDescription(Enum.Parse(typeof(EventStatus), eventCode));
                    break;
            }

            return title;
        }

        public bool IsVerificated(string identifier)
        {
            GlobalDocValidatorDocument eventVerification = EventVerification(identifier);

            return eventVerification != null 
                && (eventVerification.ValidationStatus == 1 || eventVerification.ValidationStatus == 10);
        }

        public List<GlobalDocValidatorTracking> ListTracking(string eventDocumentKey)
        {
            return _globalDocValidatorTracking.ListTracking(eventDocumentKey);
        }

        //public List<GlobalDocReferenceAttorney> IsMandato(EventStatus eventStatus, GlobalDocValidatorDocumentMeta eventitem)
        //{
            
        //}
    }
}
