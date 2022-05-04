using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalRadianContributorEnabled : TableEntity
    {
        public GlobalRadianContributorEnabled() { }

        public GlobalRadianContributorEnabled(string pk, string rk ) : base(pk,rk)
        {

        }

        public bool IsActive { get; set; }
        public string UpdateBy { get; set; }

    }
}
