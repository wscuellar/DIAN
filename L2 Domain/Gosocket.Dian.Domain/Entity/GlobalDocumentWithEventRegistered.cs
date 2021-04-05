using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocumentWithEventRegistered : TableEntity
    {
        public GlobalDocumentWithEventRegistered() { }

        public GlobalDocumentWithEventRegistered(string pk, string rk) : base(pk, rk)
        {

        }

        public string QueryAttempt { get; set; }
        public DateTime? EndAttempt { get; set; }

    }
}
