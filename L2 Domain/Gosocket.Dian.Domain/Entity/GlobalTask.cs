using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalTask : TableEntity
    {
        public GlobalTask() { }

        public GlobalTask(string pk, string rk) : base(pk, rk)
        { }

        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }
        public string User { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string FilterDate { get; set; }
        public string FilterGroupCode { get; set; }
        public string FilterGroup { get; set; }
        public string TotalResult { get; set; }
    }
}
