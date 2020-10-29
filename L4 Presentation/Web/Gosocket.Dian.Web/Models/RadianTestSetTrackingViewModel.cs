using System;

namespace Gosocket.Dian.Web.Models
{
    public class RadianTestSetTrackingViewModel
    {
        public Guid RadianTestSetId { get; set; }
        public Guid TrackId { get; set; }
        public string DocumentNumber { get; set; }
        public string ReceiverCode { get; set; }
        public int TotalRules { get; set; }
        public int TotalRulesSuccessfully { get; set; }
        public int TotalRulesUnsuccessfully { get; set; }
        public int TotalMandatoryRulesUnsuccessfully { get; set; }
    }
}