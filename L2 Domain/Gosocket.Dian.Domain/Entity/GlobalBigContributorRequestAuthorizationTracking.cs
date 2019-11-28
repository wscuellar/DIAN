using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalBigContributorRequestAuthorizationTracking : TableEntity
    {
        public GlobalBigContributorRequestAuthorizationTracking() { }

        public GlobalBigContributorRequestAuthorizationTracking(string contributorCode): base(contributorCode, Guid.NewGuid().ToString())
        {
            ContributorCode = contributorCode;
        }

        public string ContributorCode { get; set; }
        public string User { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string Observations { get; set; }
        public DateTime Date { get; set; }
        public string RequestUrl { get; set; }

    }
}
