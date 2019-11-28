using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocValidatorStats : TableEntity
    {
        public GlobalDocValidatorStats() { }
        public GlobalDocValidatorStats(string pk, string rk) : base(pk, rk)
        { }

        public long Count { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public DateTime LastDateTimeUpdate { get; set; }
    }
}
