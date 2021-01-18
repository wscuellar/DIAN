namespace Gosocket.Dian.Domain.Sql
{
    [System.ComponentModel.DataAnnotations.Schema.Table("UserFreeBillerProfile")]
    public class UsersFreeBillerProfile
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int ProfileFreeBillerId { get; set; }
        public string CompanyCode { get; set; }
        public int CompanyIdentificationType { get; set; }
    }
}
