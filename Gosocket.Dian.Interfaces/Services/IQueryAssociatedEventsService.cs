using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IQueryAssociatedEventsService
    {
        List<GlobalDocValidatorDocumentMeta> CreditAndDebitNotes(List<GlobalDocValidatorDocumentMeta> allReferencedDocuments);
        GlobalDocValidatorDocumentMeta DocumentValidation(string reference); 
        string EventTitle (EventStatus eventStatus, string customizationId, string eventCode, string SchemeID);
        Domain.Entity.GlobalDocValidatorDocument EventVerification(GlobalDocValidatorDocumentMeta eventItem);
        Domain.Entity.GlobalDocValidatorDocument GlobalDocValidatorDocumentByGlobalId(string globalDocumentId);
        Dictionary<int, string> IconType(List<GlobalDocValidatorDocumentMeta> allReferencedDocuments, string documentKey = "");
        EventStatus IdentifyEvent(GlobalDocValidatorDocumentMeta eventItem);
        bool IsVerificated(GlobalDocValidatorDocumentMeta otherEvent);
        List<Domain.Entity.GlobalDocValidatorTracking> ListTracking(string eventDocumentKey);
        List<GlobalDocValidatorDocumentMeta> OtherEvents(string documentKey, EventStatus eventCode);
        List<GlobalDocReferenceAttorney> ReferenceAttorneys(string documentKey, string documentReferencedKey, string receiverCode, string senderCode);
        GlobalDocPayroll GetPayrollById(string partitionKey);
        Tuple<List<GlobalDocValidatorDocumentMeta>, Dictionary<int, string>> InvoiceAndNotes(List<DocumentTag> documentTags, string documentKey, string documentTypeId);
    }
}
