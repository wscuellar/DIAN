using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocRegisterProviderAR : TableEntity
    {
        public GlobalDocRegisterProviderAR() { }

        public GlobalDocRegisterProviderAR(string pk, string rk) : base(pk, rk)
        {

        }

        public string trackId { get; set; }
        public string providerCode { get; set; }
        public string serieAndNumber { get; set; }
        public string senderCode { get; set; }
        public string docTypeCode { get; set; }
    }
}
