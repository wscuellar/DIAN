using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocAssociate : TableEntity
    {
        public GlobalDocAssociate() { }

        public GlobalDocAssociate(string pk, string rk) : base(pk, rk)
        {

        }

        public bool Active { get; set; }
        public string Identifier { get; set; }

    }
}
