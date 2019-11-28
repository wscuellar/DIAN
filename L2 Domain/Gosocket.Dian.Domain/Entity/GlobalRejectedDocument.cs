using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalRejectedDocument : TableEntity
    {
        public GlobalRejectedDocument() { }
        public GlobalRejectedDocument(string pk, string rk) : base(pk, rk)
        { }

        public string SenderCode { get; set; }
        public string SenderName { get; set; }
        public DateTime GenerationTimeStamp { get; set; }
    }
}
