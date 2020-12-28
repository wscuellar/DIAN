using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocEvent : TableEntity
    {
        public GlobalDocEvent() { }

        public GlobalDocEvent(string pk, string rk) : base(pk, rk)
        { }

        public string Description { get; set; }
        public string DocumentTypeId { get; set; }
        public bool Active { get; set; }
    }
}
