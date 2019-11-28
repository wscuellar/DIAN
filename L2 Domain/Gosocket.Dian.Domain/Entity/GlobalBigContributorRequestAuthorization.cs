using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalBigContributorRequestAuthorization : TableEntity
    {
        public GlobalBigContributorRequestAuthorization() { }

        public GlobalBigContributorRequestAuthorization(string contributorCode) : base(contributorCode, contributorCode)
        {
            ContributorCode = contributorCode;
        }

        public DateTime RequestDate { get; set; }
        public string ContributorCode { get; set; }
        public string ContributorName { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string Justification { get; set; }
        public int MinimumInvoices { get; set; }
        public int MaximumInvoices { get; set; }
        public string Observations { get; set; }
    }
}
