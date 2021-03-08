namespace Gosocket.Dian.Domain.Utils
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("AspNetUserClaims")]
    public class ClaimsDb
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }
    }
}
