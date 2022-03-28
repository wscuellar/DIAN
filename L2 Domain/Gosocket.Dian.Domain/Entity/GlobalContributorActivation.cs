using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalContributorActivation : TableEntity
    {
        public GlobalContributorActivation() { }
        public GlobalContributorActivation(string pk, string rk) : base(pk, rk)
        { }

        public DateTime SendDate { get; set; }
        public string ContributorCode { get; set; }
        public int? ContributorTypeId { get; set; }
        public int  OperationModeId { get; set; }
        public string OperationModeName { get; set; }
        public string SentToActivateBy { get; set; }
        public string SoftwareId { get; set; }
        public bool Success { get; set; }
        public string TestSetId { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
        public string Trace { get; set; }
        public string Request { get; set; }
        public int OtherDocContributorId { get; set; }
    }
}
