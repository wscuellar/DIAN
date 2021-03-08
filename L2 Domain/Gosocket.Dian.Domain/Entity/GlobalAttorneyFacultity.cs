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
