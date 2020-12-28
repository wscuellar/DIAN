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
        public string SerieAndNumber { get; set; }
        public string SenderCode { get; set; }
        public string DocumentTypeId { get; set; }
    }
}
