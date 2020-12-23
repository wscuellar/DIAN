using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalContributor : TableEntity
    {
        public GlobalContributor() { }
        public GlobalContributor(string pk, string rk) : base(pk, rk)
        { }

        public int StatusId { get; set; }
        public string Code { get; set; }
        public int? TypeId { get; set; }
        public int StatusRut { get; set; }
    }
}
