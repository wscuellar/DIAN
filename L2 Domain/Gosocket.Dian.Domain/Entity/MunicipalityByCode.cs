using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class MunicipalityByCode : TableEntity
    {
        public MunicipalityByCode()
        {

        }

        public MunicipalityByCode(string pk, string rk) : base(pk, rk)
        {

        }

        public DateTime Timestamp { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

    }
}

