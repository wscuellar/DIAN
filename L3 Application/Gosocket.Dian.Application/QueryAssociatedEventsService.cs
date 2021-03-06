﻿using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class QueryAssociatedEventsService : IQueryAssociatedEventsService
    {
        private readonly IGlobalDocValidationDocumentMetaService _radianGlobalDocValidationDocumentMeta;
        private readonly IGlobalDocValidatorDocumentService _globalDocValidatorDocument;
        private readonly IGlobalDocValidatorTrackingService _globalDocValidatorTracking;
        private readonly IGlobalDocPayrollService _globalDocPayrollService;

        const string CONS361 = "361";
        const string CONS362 = "362";
        const string CONS363 = "363";
        const string CONS364 = "364";

        const string TITULOVALORCODES = "030, 032, 033, 034";
        const string DISPONIBILIZACIONCODES = "036";
        const string PAGADACODES = "045";
        const string ENDOSOCODES = "037,038,039";
        const string LIMITACIONCODES = "041";
        const string ANULACIONENDOSOCODES = "040";
        const string ANULACIONLIMITACIONCODES = "042";
        const string MANDATOCODES = "043";

        const string CREDITNOTE = "91";
        const string DEBITNOTE = "92";

        public QueryAssociatedEventsService(IGlobalDocValidationDocumentMetaService radianGlobalDocValidationDocumentMeta, IGlobalDocValidatorDocumentService globalDocValidatorDocument, IGlobalDocValidatorTrackingService globalDocValidatorTracking, IGlobalDocPayrollService globalDocPayrollService)
        {
            _radianGlobalDocValidationDocumentMeta = radianGlobalDocValidationDocumentMeta;
            _globalDocValidatorDocument = globalDocValidatorDocument;
            _globalDocValidatorTracking = globalDocValidatorTracking;
            _globalDocPayrollService = globalDocPayrollService;
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

        public List<GlobalDocValidatorDocumentMeta> OtherEvents(string documentKey, EventStatus eventCode)
        {
            string code = ((int)eventCode).ToString();
            return _radianGlobalDocValidationDocumentMeta.GetAssociatedDocuments(documentKey, code);
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
                case EventStatus.SolicitudDisponibilizacion:
                    if (customizationId == CONS361 || customizationId == CONS362)
                        title = TextResources.SolicitudDisponibilizacion;
                    if (customizationId == CONS363 || customizationId == CONS364)
                        title = TextResources.SolicitudDisponibilizacion1;
                    break;
                default:
                    title = EnumHelper.GetEnumDescription(Enum.Parse(typeof(EventStatus), eventCode));
                    break;
            }

            return title;
        }

        public bool IsVerificated(GlobalDocValidatorDocumentMeta otherEvent)
        {
            //otherEvent.Identifier
            if (string.IsNullOrEmpty(otherEvent.EventCode))
                return false;

            GlobalDocValidatorDocument eventVerification = EventVerification(otherEvent.Identifier);

            return eventVerification != null
                && (eventVerification.ValidationStatus == 0 || eventVerification.ValidationStatus == 1 || eventVerification.ValidationStatus == 10);
        }

        public List<GlobalDocValidatorTracking> ListTracking(string eventDocumentKey)
        {
            return _globalDocValidatorTracking.ListTracking(eventDocumentKey);
        }

        public EventStatus IdentifyEvent(GlobalDocValidatorDocumentMeta eventItem)
        {
            if (ENDOSOCODES.Contains(eventItem.EventCode.Trim()))
                return EventStatus.InvoiceOfferedForNegotiation;

            if (MANDATOCODES.Contains(eventItem.EventCode.Trim()))
                return EventStatus.TerminacionMandato;

            if (LIMITACIONCODES.Contains(eventItem.EventCode.Trim()))
                return EventStatus.AnulacionLimitacionCirculacion;

            return EventStatus.None;
        }

        public Dictionary<int, string> IconType(List<GlobalDocValidatorDocumentMeta> allReferencedDocuments, string documentKey = "")
        {
            Dictionary<int, string> statusValue = new Dictionary<int, string>();
            int securityTitleCounter = 0;
            int index = 3;

            statusValue.Add(1, $"{RadianDocumentStatus.ElectronicInvoice.GetDescription()}");

            if (documentKey != "")
            {
                allReferencedDocuments = _radianGlobalDocValidationDocumentMeta.FindDocumentByReference(documentKey);
                allReferencedDocuments = allReferencedDocuments.Where(t => t.EventCode != null).ToList();
            }

            allReferencedDocuments = allReferencedDocuments.OrderBy(t => t.Timestamp).ToList();
            var events = eventListByTimestamp(allReferencedDocuments).OrderBy(t => t.Timestamp) .ToList();


            events = removeEvents(events, EventStatus.InvoiceOfferedForNegotiation, new List<string>() { $"0{(int)EventStatus.EndosoProcuracion}", $"0{ (int)EventStatus.EndosoGarantia}" });
            events = removeEvents(events, EventStatus.AnulacionLimitacionCirculacion, new List<string>() { $"0{(int)EventStatus.NegotiatedInvoice}" });

            foreach (GlobalDocValidatorDocumentMeta documentMeta in events)
            {
                if (TITULOVALORCODES.Contains(documentMeta.EventCode.Trim()))
                    securityTitleCounter++;

                if (!statusValue.Values.Contains(RadianDocumentStatus.SecurityTitle.GetDescription()) && securityTitleCounter >= 3)
                    statusValue.Add(2, $"{RadianDocumentStatus.SecurityTitle.GetDescription()}");//5

                if (DISPONIBILIZACIONCODES.Contains(documentMeta.EventCode.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Readiness.GetDescription()}");
                    index++;
                }

                if (ENDOSOCODES.Contains(documentMeta.EventCode.Trim())  )
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Endorsed.GetDescription()}");
                    index++;
                }

                if (PAGADACODES.Contains(documentMeta.EventCode.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Paid.GetDescription()}");
                    index++;
                }

                if (LIMITACIONCODES.Contains(documentMeta.EventCode.Trim())  )
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Limited.GetDescription()}");
                    index++;
                }
            }

            Dictionary<int, string> cleanDictionary = statusValue.GroupBy(pair => pair.Value)
                         .Select(group => group.Last())
                         .ToDictionary(pair => pair.Key, pair => pair.Value);

            if (cleanDictionary.ContainsValue(RadianDocumentStatus.Readiness.GetDescription()) || cleanDictionary.ContainsValue(RadianDocumentStatus.Limited.GetDescription()))
                cleanDictionary.Remove(1);

            return cleanDictionary;
        }



        private List<GlobalDocValidatorDocumentMeta> removeEvents(List<GlobalDocValidatorDocumentMeta> events, EventStatus conditionalStatus, List<string> removeData)
        {
            if (events.Count > 0 && events.Last().EventCode == $"0{(int)conditionalStatus }")
            {
                events.Remove(events.Last());
                foreach (var item in events.OrderByDescending(x => x.Timestamp))
                {
                    if (removeData.Contains(item.EventCode.Trim()))
                    {
                        events.Remove(item);
                        break;
                    }
                }
                return removeEvents(events, conditionalStatus, removeData);
            }
            return events;
        }

        //Pass Information to DocumentController for Debit And Credit Notes
        public Tuple<GlobalDocValidatorDocument, List<GlobalDocValidatorDocumentMeta>, Dictionary<int, string>> InvoiceAndNotes(string documentKey)
        {
            List<GlobalDocValidatorDocumentMeta> allReferencedDocuments = _radianGlobalDocValidationDocumentMeta.FindDocumentByReference(documentKey);
            Dictionary<int, string> icons = new Dictionary<int, string>();

            GlobalDocValidatorDocument globalDocValidatorDocument = GlobalDocValidatorDocumentByGlobalId(documentKey);

            if (!string.IsNullOrEmpty(documentKey) && globalDocValidatorDocument.DocumentTypeId == "01")
                icons = IconType(allReferencedDocuments);

            Tuple<GlobalDocValidatorDocument, List<GlobalDocValidatorDocumentMeta>, Dictionary<int, string>> tuple = Tuple.Create(globalDocValidatorDocument, CreditAndDebitNotes(allReferencedDocuments), icons);

            return tuple;
        }

        //Join Credit and Debit Notes in one list
        public List<GlobalDocValidatorDocumentMeta> CreditAndDebitNotes(List<GlobalDocValidatorDocumentMeta> allReferencedDocuments)
        {
            List<GlobalDocValidatorDocumentMeta> creditDebitNotes = FindAllNotes(allReferencedDocuments);
            return creditDebitNotes.OrderBy(n => n.EmissionDate).ToList();
        }

        //
        public List<GlobalDocValidatorDocumentMeta> FindAllNotes(List<GlobalDocValidatorDocumentMeta> allReferencedDocuments)
        {
            List<GlobalDocValidatorDocumentMeta> notes = allReferencedDocuments.Where(c => c.DocumentTypeId == CREDITNOTE || c.DocumentTypeId == DEBITNOTE).ToList();

            List<GlobalDocValidatorDocumentMeta> validateNotes = new List<GlobalDocValidatorDocumentMeta>();

            foreach (var note in notes)
            {
                if (IsVerifiedNote(note.DocumentKey))
                    validateNotes.Add(note);
            }

            return validateNotes;
        }

        public GlobalDocValidatorDocument GlobalDocValidatorDocumentByGlobalId(string globalDocumentId)
        {
            return _globalDocValidatorDocument.FindByGlobalDocumentId(globalDocumentId);
        }

        private bool IsVerifiedNote(string documentKey)
        {
            if (_globalDocValidatorDocument.FindByGlobalDocumentId(documentKey) != null)
                return true;

            return false;
        }

        private List<GlobalDocValidatorDocumentMeta> eventListByTimestamp(List<GlobalDocValidatorDocumentMeta> originalList)
        {
            List<GlobalDocValidatorDocumentMeta> resultList = new List<GlobalDocValidatorDocumentMeta>();

            foreach (var item in originalList)
            {
                if (!string.IsNullOrEmpty(item.EventCode))
                {
                    resultList.Add(item);
                }
            }

            return resultList.Where(e => TITULOVALORCODES.Contains(e.EventCode.Trim()) || DISPONIBILIZACIONCODES.Contains(e.EventCode.Trim()) || PAGADACODES.Contains(e.EventCode.Trim()) || ENDOSOCODES.Contains(e.EventCode.Trim()) || DISPONIBILIZACIONCODES.Contains(e.EventCode.Trim()) || ANULACIONENDOSOCODES.Contains(e.EventCode.Trim()) || LIMITACIONCODES.Contains(e.EventCode.Trim()) || ANULACIONLIMITACIONCODES.Contains(e.EventCode.Trim())).ToList();
        }

        private bool IsAnulation(int counterAnulations, int counterInformation)
        {
            if (counterInformation > counterAnulations)
                return false;

            return true;
        }

        public GlobalDocPayroll GetPayrollById(string partitionKey)
        {
            return this._globalDocPayrollService.Find(partitionKey);
        }
    }
}
