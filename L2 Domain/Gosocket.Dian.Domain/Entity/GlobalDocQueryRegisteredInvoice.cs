using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocQueryRegisteredInvoice : TableEntity
    {
        public GlobalDocQueryRegisteredInvoice() { }

        public GlobalDocQueryRegisteredInvoice(string pk, string rk) : base(pk, rk)
        {

        }

        public string QueryAttempt { get; set; }
        public DateTime? EndAttempt { get; set; }

    }
}
