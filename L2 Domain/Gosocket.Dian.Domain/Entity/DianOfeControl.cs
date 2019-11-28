using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class DianOfeControl : TableEntity
    {
        public DianOfeControl() { }
        public DianOfeControl(string pk, string rk) : base(pk, rk)
        { }
        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? LastDocumentDate { get; set; }
        public int? LastDocumentDateNumber { get; set; }
    }
}
