using Gosocket.Dian.Domain.Common;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IQueryAssociatedEventsService
    {
        List<GlobalDocValidatorDocumentMeta> CreditAndDebitNotes(string documentReferencedKey);
        GlobalDocValidatorDocumentMeta DocumentValidation(string reference);
        string EventTitle(EventStatus eventStatus, string customizationId, string eventCode);
        Domain.Entity.GlobalDocValidatorDocument EventVerification(string eventItemIdentifier);
        EventStatus IdentifyEvent(GlobalDocValidatorDocumentMeta eventItem);
        bool IsVerificated(GlobalDocValidatorDocumentMeta otherEvent);
        List<Domain.Entity.GlobalDocValidatorTracking> ListTracking(string eventDocumentKey);
        List<GlobalDocValidatorDocumentMeta> OtherEvents(string documentKey, EventStatus eventCode);
        List<GlobalDocReferenceAttorney> ReferenceAttorneys(string documentKey, string documentReferencedKey, string receiverCode, string senderCode);
    }
}
