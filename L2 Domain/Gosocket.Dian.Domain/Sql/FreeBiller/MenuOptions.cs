
namespace Gosocket.Dian.Domain.Sql.FreeBiller
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MenuOptionsFreeBiller")]
    public class MenuOptions
    {
        [Key]
        public int Key { get; set; }

        [StringLength(50)]
        public string MyProperty { get; set; }

        public int ParentId { get; set; }

        public bool IsActive { get; set; }

    }
}
