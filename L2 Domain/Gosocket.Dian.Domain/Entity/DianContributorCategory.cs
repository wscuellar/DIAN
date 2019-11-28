using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class DianContributorCategory : TableEntity
    {

        public DianContributorCategory() { }

        public DianContributorCategory(string pk, string rk) : base(pk, rk)
        {
            // PartitionKey category id
            // RowKey category id
        }

        public string ActivityCodes { get; set; }
        public DateTime MaximumEmissionDate { get; set; }
        public DateTime MaximumRegistrationDate { get; set; }
        public int ResolutionNumber { get; set; }
        public DateTime ResolutionDate { get; set; }
    }
}
