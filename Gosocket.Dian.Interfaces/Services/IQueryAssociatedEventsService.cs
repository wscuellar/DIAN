using Gosocket.Dian.Domain.Common;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IQueryAssociatedEventsService
    {
        List<GlobalDocValidatorDocumentMeta> CreditAndDebitNotes(List<GlobalDocValidatorDocumentMeta> allReferencedDocuments);
        GlobalDocValidatorDocumentMeta DocumentValidation(string reference);
        string EventTitle(EventStatus eventStatus, string customizationId, string eventCode);
        Domain.Entity.GlobalDocValidatorDocument EventVerification(string eventItemIdentifier);
        Domain.Entity.GlobalDocValidatorDocument GlobalDocValidatorDocumentByGlobalId(string globalDocumentId);
        Dictionary<int, string> IconType(List<GlobalDocValidatorDocumentMeta> allReferencedDocuments);
        EventStatus IdentifyEvent(GlobalDocValidatorDocumentMeta eventItem);
        Tuple<Domain.Entity.GlobalDocValidatorDocument, List<GlobalDocValidatorDocumentMeta>, Dictionary<int, string>> InvoiceAndNotes(string documentKey);
        bool IsVerificated(GlobalDocValidatorDocumentMeta otherEvent);
        List<Domain.Entity.GlobalDocValidatorTracking> ListTracking(string eventDocumentKey);
        List<GlobalDocValidatorDocumentMeta> OtherEvents(string documentKey, EventStatus eventCode);
        List<GlobalDocReferenceAttorney> ReferenceAttorneys(string documentKey, string documentReferencedKey, string receiverCode, string senderCode);
    }
}
