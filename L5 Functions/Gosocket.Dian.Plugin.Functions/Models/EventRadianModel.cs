using Gosocket.Dian.Plugin.Functions.Cufe;
using Gosocket.Dian.Plugin.Functions.Event;
using Gosocket.Dian.Plugin.Functions.EventApproveCufe;
using Gosocket.Dian.Plugin.Functions.SigningTime;
using Gosocket.Dian.Plugin.Functions.ValidateParty;
using System;

namespace Gosocket.Dian.Plugin.Functions.Models
{
    public class EventRadianModel
    {
        public string TrackId { get; set; }
        public string TrackIdCude { get; set; }
        public string EventCode { get; set; }
        public string DocumentTypeId { get; set; }
        public string ListId { get; set; }
        public string CustomizationId { get; set; }
        public string SigningTime { get; set; }
        public string EndDate { get; set; }
        public string SenderParty { get; set; }
        public string ReceiverParty { get; set; }
        public string DocumentTypeIdRef { get; set; }
        public string DocumentIdReference { get; set; }
        public string IssuerPartyCode { get; set; }
        public string IssuerPartyName { get; set; }
        public bool SendTestSet { get; set; }

        public EventRadianModel() { }

        public EventRadianModel(string trackId, string trackIdCude, string eventCode, 
            string documentTypeId, string listId, 
            string customizationId, string signingTime, 
            string endDate, string senderParty, 
            string receiverParty, string documentTypeIdRef, string documentIdReference,
            string issuerPartyCode, string issuerPartyName, bool sendTestSet)
        {
            TrackId = trackId;
            TrackIdCude = trackIdCude;
            EventCode = eventCode;
            DocumentTypeId = documentTypeId;
            ListId = listId;
            CustomizationId = customizationId;
            SigningTime = signingTime;
            EndDate = endDate;
            SenderParty = senderParty;
            ReceiverParty = receiverParty;
            DocumentTypeIdRef = documentTypeIdRef;
            DocumentIdReference = documentIdReference;
            IssuerPartyCode = issuerPartyCode;
            IssuerPartyName = issuerPartyName;
            SendTestSet = sendTestSet;
        }

        public static void SetValuesEventPrev(ref EventRadianModel eventRadian, RequestObjectEventPrev eventPrev)
        {
            eventPrev.TrackId = eventRadian.TrackId;
            eventPrev.EventCode = eventRadian.EventCode;
            eventPrev.DocumentTypeId = eventRadian.DocumentTypeId;
            eventPrev.TrackIdCude = eventRadian.TrackIdCude;
            eventPrev.ListId = eventRadian.ListId;
            eventPrev.CustomizationID = eventRadian.CustomizationId;
        }

        public static void SetValuesValidateParty(ref EventRadianModel eventRadian, RequestObjectParty validateParty)
        {
            validateParty.TrackId = eventRadian.TrackId;
            validateParty.SenderParty = eventRadian.SenderParty;
            validateParty.ReceiverParty = eventRadian.ReceiverParty;
            validateParty.ResponseCode = eventRadian.EventCode;
            validateParty.CustomizationID = eventRadian.CustomizationId;
            validateParty.TrackIdCude = eventRadian.TrackIdCude;
            validateParty.ListId = eventRadian.ListId;
            validateParty.SendTestSet = eventRadian.SendTestSet;
        }
        
        public static void SetValueEventAproveCufe(ref EventRadianModel eventRadian, RequestObjectEventApproveCufe eventApproveCufe)
        {
            eventApproveCufe.TrackId = eventRadian.TrackId;
            eventApproveCufe.ResponseCode = eventRadian.EventCode;
            eventApproveCufe.DocumentTypeId = eventRadian.DocumentTypeId;
        }

        public static void SetValuesDocReference(ref EventRadianModel eventRadian, RequestObjectDocReference docReference)
        {
            docReference.TrackId = eventRadian.TrackId;
            docReference.IdDocumentReference = eventRadian.DocumentIdReference;
            docReference.EventCode = eventRadian.EventCode;
            docReference.DocumentTypeIdRef = eventRadian.DocumentTypeIdRef;
            docReference.IssuerPartyCode = eventRadian.IssuerPartyCode;
            docReference.IssuerPartyName = eventRadian.IssuerPartyName;
        }

        public static void SetValuesSigningTime(ref EventRadianModel eventRadian, RequestObjectSigningTime signingTime)
        {
            signingTime.TrackId = eventRadian.TrackId;
            signingTime.EventCode = eventRadian.EventCode;
            signingTime.SigningTime = eventRadian.SigningTime;
            signingTime.DocumentTypeId = eventRadian.DocumentTypeId;
            signingTime.CustomizationID = eventRadian.CustomizationId;
            signingTime.EndDate = eventRadian.EndDate;
            signingTime.TrackIdcude = eventRadian.TrackIdCude;
        }
    }
}
