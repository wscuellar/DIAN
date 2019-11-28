using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalContingency: TableEntity
    {
        public GlobalContingency(){ }
        public GlobalContingency(string pk, string rk) : base(pk, rk)
        {
            Active = true;
            CreatedDate = DateTime.UtcNow;
            DeletedDate = DateTime.UtcNow.AddYears(-1);
            UpdatedDate = DateTime.UtcNow;
            Deleted = false;
        }

        public bool Active { get; set; }
        public long StartDateNumber { get; set; }
        public long EndDateNumber { get; set; }
        public string Reason { get; set; }
        public bool Deleted { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
