using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalAuthorization : TableEntity
    {
        public GlobalAuthorization() { }
        public GlobalAuthorization(string pk, string rk) : base(pk, rk)
        { }
        public string LegalEntity { get; set; }
        public string SenderCode { get; set; }
    }
}
