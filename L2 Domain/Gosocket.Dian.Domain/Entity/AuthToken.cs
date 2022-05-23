using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class AuthToken : TableEntity
    {

        public AuthToken() { }

        public AuthToken(string pk, string rk) : base(pk, rk) { }

        public string Email { get; set; }
        public int ContributorId { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Type { get; set; }
        public string LoginMenu { get; set; }
        public bool Status { get; set; }
    }
}
