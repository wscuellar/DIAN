using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalAttorneyFacultity : TableEntity
    {
        public GlobalAttorneyFacultity()
        {
            
        }

        public GlobalAttorneyFacultity(string pk, string rk) : base(pk, rk)
        {

        }

        public bool Active { get; set; }
        public string Actor { get; set; }
        public string Description { get; set; }
    
    }
}
