using Gosocket.Dian.Plugin.Functions.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DateTime SigningTime { get; set; }
        public string EndDate { get; set; }
        public string SenderParty { get; set; }
        public string ReceiverParty { get; set; }
        public string DocumentTypeIdRef { get; set; }
        public string IssuerPartyCode { get; set; }
        public string IssuerPartyName { get; set; }        

        public EventRadianModel() { }

        public EventRadianModel(string trackId, string trackIdCude, string eventCode, 
            string documentTypeId, string listId, 
            string customizationId, DateTime signingTime, 
            string endDate, string senderParty, 
            string receiverParty, string documentTypeIdRef, 
            string issuerPartyCode, string issuerPartyName)
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
            IssuerPartyCode = issuerPartyCode;
            IssuerPartyName = issuerPartyName;
        }

        public static void SetValuesEventPrev(ref EventRadianModel eventRadian, ValidateEmitionEventPrev.RequestObject eventPrev)
        {
            eventPrev.TrackId = eventRadian.TrackId;
            eventPrev.EventCode = eventRadian.EventCode;
            eventPrev.DocumentTypeId = eventRadian.DocumentTypeId;
            eventPrev.TrackIdCude = eventRadian.TrackIdCude;
            eventPrev.ListId = eventRadian.ListId;
            eventPrev.CustomizationID = eventRadian.CustomizationId;
        }
    }
}
