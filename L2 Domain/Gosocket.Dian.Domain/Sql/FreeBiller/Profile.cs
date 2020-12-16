namespace Gosocket.Dian.Domain.Sql.FreeBiller
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    [Table("ProfilesFreeBiller")]
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        [StringLength(70)]
        public string Name { get; set; }

        public bool IsEditable { get; set; }
    }
}
