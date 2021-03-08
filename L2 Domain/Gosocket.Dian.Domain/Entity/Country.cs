using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class Country : TableEntity
    {
        public Country()
        {

        }

        public Country(string pk, string rk) : base(pk, rk)
        {

        }

        public string Code { get; set; }
        public string Description { get; set; }
    }
}