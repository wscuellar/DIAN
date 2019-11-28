using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalExchangeEmail : TableEntity
    {
        public GlobalExchangeEmail() { }

        public GlobalExchangeEmail(string pk, string rk) : base(pk, rk)
        {
        }

        public string Email { get; set; }
    }
}
