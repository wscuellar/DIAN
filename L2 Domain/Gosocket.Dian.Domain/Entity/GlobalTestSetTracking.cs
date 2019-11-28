using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalTestSetTracking : TableEntity
    {
        public GlobalTestSetTracking() { }

        public GlobalTestSetTracking(string testSetId, string trackId, string documentNumber, string receiverCode, int totalRules, int totalRulesSuccessfully, int totalRulesUnsuccessfully, int totalMandatoryRulesUnsuccessfully) : base($"{testSetId}", $"{trackId}")
        {
            TrackId = trackId;
            TestSetId = testSetId;
            TotalRules = totalRules;
            DocumentNumber = documentNumber;
            ReceiverCode = receiverCode;
            TotalRulesSuccessfully = totalRulesSuccessfully;
            TotalRulesUnsuccessfully = TotalRulesUnsuccessfully;
            TotalMandatoryRulesUnsuccessfully = totalMandatoryRulesUnsuccessfully;
        }

        public string DocumentTypeId { get; set; }
        public bool IsValid { get; set; }
        public string TestSetId { get; set; }
        public string TrackId { get; set; }
        public string DocumentNumber { get; set; }
        public string SenderCode { get; set; }
        public string SoftwareId { get; set; }
        public string ReceiverCode { get; set; }
        public int TotalRules { get; set; }
        public int TotalRulesSuccessfully { get; set; }
        public int TotalRulesUnsuccessfully { get; set; }
        public int TotalMandatoryRulesSuccessfully { get; set; }
        public int TotalMandatoryRulesUnsuccessfully { get; set; }
    }
}
