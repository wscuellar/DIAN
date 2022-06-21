using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
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
        private readonly IAssociateDocuments _associateDocuments;

        const string CONS361 = "361";
        const string CONS362 = "362";
        const string CONS363 = "363";
        const string CONS364 = "364";

        const string TITULOVALORCODES = "030, 032, 033, 034";
        const string DISPONIBILIZACIONCODES = "036";
        const string PAGADACODES = "045";
        const string ENDOSOCODES = "037,038,039,047";
        const string LIMITACIONCODES = "041";
        const string ANULACIONENDOSOCODES = "040";
        const string ANULACIONLIMITACIONCODES = "042";
        const string MANDATOCODES = "043";
        const string PROTESTADACODES = "048";
        const string TRANSFERENCIACODES = "049";
        const string NOTIFICACIONTRANSFERENCIA = "050";
        const string PAGOTRANSFERENCIA = "051";
        const string CREDITNOTE = "91";
        const string DEBITNOTE = "92";

        public QueryAssociatedEventsService(IGlobalDocValidationDocumentMetaService radianGlobalDocValidationDocumentMeta, IGlobalDocValidatorDocumentService globalDocValidatorDocument, IGlobalDocValidatorTrackingService globalDocValidatorTracking, IGlobalDocPayrollService globalDocPayrollService, IAssociateDocuments associateDocuments)
        {
            _radianGlobalDocValidationDocumentMeta = radianGlobalDocValidationDocumentMeta;
            _globalDocValidatorDocument = globalDocValidatorDocument;
            _globalDocValidatorTracking = globalDocValidatorTracking;
            _globalDocPayrollService = globalDocPayrollService;
            _associateDocuments = associateDocuments;
        }

        public GlobalDocValidatorDocumentMeta DocumentValidation(string reference)
        {
            return _radianGlobalDocValidationDocumentMeta.DocumentValidation(reference);
        }

        public GlobalDocValidatorDocument EventVerification(GlobalDocValidatorDocumentMeta eventItem)
        {
            return _globalDocValidatorDocument.EventVerification(eventItem);
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

        public string EventTitle(EventStatus eventStatus, string CustomizationID, string EventCode, string SchemeID)
        {
            string Title = string.Empty;

            switch (eventStatus)
            {
                case EventStatus.Received:
                    Title = EnumHelper.GetDescription(EventStatus.Received);
                    break;
                case EventStatus.Receipt:
                    Title = EnumHelper.GetDescription(EventStatus.Receipt);
                    break;
                case EventStatus.AceptacionTacita:
                    Title = EnumHelper.GetDescription(EventStatus.AceptacionTacita);
                    break;
                case EventStatus.Accepted:
                    Title = EnumHelper.GetDescription(EventStatus.Accepted);
                    break;
                case EventStatus.SolicitudDisponibilizacion:
                    if (CustomizationID.Equals("361"))
                        Title = EnumHelper.GetDescription(SubEventStatus.PrimeraSolicitudDisponibilizacion);
                    if (CustomizationID.Equals("362"))
                        Title = EnumHelper.GetDescription(SubEventStatus.SolicitudDisponibilizacionDirecta);
                    if (CustomizationID.Equals("363"))
                        Title = EnumHelper.GetDescription(SubEventStatus.SolicitudDisponibilizacionPosterior);
                    if (CustomizationID.Equals("364"))
                        Title = EnumHelper.GetDescription(SubEventStatus.SolicitudDisponibilizacionPosteriorDirecta);
                    break;
                case EventStatus.EndosoGarantia:
                    Title = EnumHelper.GetDescription(EventStatus.EndosoGarantia);
                    break;
                case EventStatus.EndosoPropiedad:
                    if (CustomizationID.Equals("371"))
                        Title = EnumHelper.GetDescription(SubEventStatus.ConResponsabilidad);
                    else
                        Title = EnumHelper.GetDescription(SubEventStatus.SinResponsabilidad);
                    break;
                case EventStatus.EndosoProcuracion:
                    Title = EnumHelper.GetDescription(EventStatus.EndosoProcuracion);
                    break;
                case EventStatus.InvoiceOfferedForNegotiation:
                    Title = eventStatus.GetDescription();
                    if (CustomizationID.Equals("401"))
                        Title = EnumHelper.GetDescription(SubEventStatus.CancelacionEndoso);
                    if (CustomizationID.Equals("402"))
                        Title = EnumHelper.GetDescription(SubEventStatus.CancelacionEndosoProcuracion);
                    if (CustomizationID.Equals("403"))
                        Title = EnumHelper.GetDescription(SubEventStatus.TachaEndosoRetorno);
                    break;
                case EventStatus.Avales:
                    Title = "Aval";
                    break;
                case EventStatus.Mandato:
                    if (!string.IsNullOrEmpty(SchemeID))
                    {
                        if (CustomizationID.Equals("431"))
                            Title = EnumHelper.GetDescription(SchemeID == "1" ? SubEventStatus.MandatoGenerarlIlimitadoEspecial : SubEventStatus.MandatoGenerarlLimitado);
                        if (CustomizationID.Equals("432"))
                            Title = EnumHelper.GetDescription(SchemeID == "1" ? SubEventStatus.MandatoGenerarlLimitadoEspecial : SubEventStatus.MandatoGenerarlLimitado);
                        if (CustomizationID.Equals("433"))
                            Title = EnumHelper.GetDescription(SchemeID == "1" ? SubEventStatus.MandatoGenerarlTiempoEspecial : SubEventStatus.MandatoGenerarlTiempo);
                        if (CustomizationID.Equals("434"))
                            Title = EnumHelper.GetDescription(SchemeID == "1" ? SubEventStatus.MandatoGenerarlTiempoIlimitadoEspecial : SubEventStatus.MandatoGenerarlTiempoIlimitado);
                    }
                    else
                        Title = EnumHelper.GetEnumDescription(Enum.Parse(typeof(EventStatus), EventCode));
                    break;
                case EventStatus.TerminacionMandato:
                    if (CustomizationID.Equals("441"))
                        Title = EnumHelper.GetDescription(SubEventStatus.TerminacionRevocatoria);
                    if (CustomizationID.Equals("442"))
                        Title = EnumHelper.GetDescription(SubEventStatus.TerminacionRenuncia);
                    break;
                case EventStatus.ValInfoPago:
                    Title = EnumHelper.GetDescription(EventStatus.ValInfoPago);
                    break;
                case EventStatus.NotificacionPagoTotalParcial:
                    if (CustomizationID.Equals("451"))
                        Title = EnumHelper.GetDescription(SubEventStatus.PagoParcial);
                    if (CustomizationID.Equals("452"))
                        Title = EnumHelper.GetDescription(SubEventStatus.PagoTotal);
                    break;
                case EventStatus.NegotiatedInvoice:
                    if (CustomizationID.Equals("411"))
                        Title = EnumHelper.GetDescription(SubEventStatus.MedidaCautelarEmbargo);
                    if (CustomizationID.Equals("412"))
                        Title = EnumHelper.GetDescription(SubEventStatus.MedidaCautelarMandamiento);
                    break;
                case EventStatus.AnulacionLimitacionCirculacion:
                    if (CustomizationID.Equals("421"))
                        Title = EnumHelper.GetDescription(SubEventStatus.TerminacionSentencia);
                    if (CustomizationID.Equals("422"))
                        Title = EnumHelper.GetDescription(SubEventStatus.TerminacionAnticipada);
                    break;
                case EventStatus.Objection:
                case EventStatus.TransferEconomicRights:
                case EventStatus.PaymentOfTransferEconomicRights:
                    Title = EnumHelper.GetDescription((SubEventStatus)int.Parse(CustomizationID));
                    break;
                default:
                    Title = EnumHelper.GetEnumDescription(Enum.Parse(typeof(EventStatus), EventCode));
                    break;
            }
            return Title;
        }

        /*
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
        */
        public bool IsVerificated(GlobalDocValidatorDocumentMeta otherEvent)
        {
            //otherEvent.Identifier
            if (string.IsNullOrEmpty(otherEvent.EventCode))
                return false;

            GlobalDocValidatorDocument eventVerification = EventVerification(otherEvent);

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
                List<InvoiceWrapper> invoiceWrapper = _associateDocuments.GetEventsByTrackId(documentKey);
                allReferencedDocuments = (invoiceWrapper.Any() && invoiceWrapper[0].Documents.Any()) ? invoiceWrapper[0].Documents.Select(x => x.DocumentMeta).ToList() : null;
                if (allReferencedDocuments == null)
                    return statusValue;
                allReferencedDocuments = allReferencedDocuments.Where(t => int.Parse(t.DocumentTypeId) == (int)DocumentType.ApplicationResponse).ToList();
                //allReferencedDocuments = _radianGlobalDocValidationDocumentMeta.FindDocumentByReference(documentKey);
            }

              
            allReferencedDocuments = allReferencedDocuments.Where(t => t.EventCode != null && _radianGlobalDocValidationDocumentMeta.EventValidator(t) != null).ToList();

            allReferencedDocuments = allReferencedDocuments.OrderBy(t => t.SigningTimeStamp).ToList();
            var events = eventListByTimestamp(allReferencedDocuments).OrderBy(t => t.SigningTimeStamp).ToList();


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

                if (ENDOSOCODES.Contains(documentMeta.EventCode.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Endorsed.GetDescription()}");
                    index++;
                }

                if (PAGADACODES.Contains(documentMeta.EventCode.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Paid.GetDescription()}");
                    index++;
                }

                if (LIMITACIONCODES.Contains(documentMeta.EventCode.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Limited.GetDescription()}");
                    index++;
                }

                if (PROTESTADACODES.Contains(documentMeta.EventCode.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.Objection.GetDescription()}");
                    index++;
                }

                if (TRANSFERENCIACODES.Contains(documentMeta.EventCode.Trim()))
                {
                    statusValue.Add(index, $"{RadianDocumentStatus.TransferOfEconomicRights.GetDescription()}");
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

            return resultList.Where(e => TITULOVALORCODES.Contains(e.EventCode.Trim()) || DISPONIBILIZACIONCODES.Contains(e.EventCode.Trim()) 
            || PAGADACODES.Contains(e.EventCode.Trim()) || ENDOSOCODES.Contains(e.EventCode.Trim()) || DISPONIBILIZACIONCODES.Contains(e.EventCode.Trim()) 
            || ANULACIONENDOSOCODES.Contains(e.EventCode.Trim()) || LIMITACIONCODES.Contains(e.EventCode.Trim()) || ANULACIONLIMITACIONCODES.Contains(e.EventCode.Trim()) 
            || PROTESTADACODES.Contains(e.EventCode.Trim()) || TRANSFERENCIACODES.Contains(e.EventCode.Trim())
            || NOTIFICACIONTRANSFERENCIA.Contains(e.EventCode.Trim()) || PAGOTRANSFERENCIA.Contains(e.EventCode.Trim())).ToList();
        }

        public GlobalDocPayroll GetPayrollById(string partitionKey)
        {
            return this._globalDocPayrollService.Find(partitionKey);
        }

        //Pass Information to DocumentController for Debit And Credit Notes
        public Tuple<List<GlobalDocValidatorDocumentMeta>, Dictionary<int, string>> InvoiceAndNotes(List<DocumentTag> documentTags, string documentKey, string documentTypeId)
        {
            List<GlobalDocValidatorDocumentMeta> allReferencedDocuments = new List<GlobalDocValidatorDocumentMeta>();
            Dictionary<int, string> icons = new Dictionary<int, string>();

            foreach (var item in documentTags)
            {
                var GlobalDocValidationDocumentMeta = _radianGlobalDocValidationDocumentMeta.DocumentValidation(item.Value);

                allReferencedDocuments.Add(GlobalDocValidationDocumentMeta);
            }

            if (!string.IsNullOrEmpty(documentKey) && documentTypeId == "01")
                icons = IconType(allReferencedDocuments, documentKey);

            Tuple<List<GlobalDocValidatorDocumentMeta>, Dictionary<int, string>> tuple = Tuple.Create(allReferencedDocuments, icons);

            return tuple;
        }
    }
}
