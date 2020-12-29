using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain.Sql
{
    [Table("MenuRole")]
    public class MenuRole
    {
        [Key]
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int? SubMenuId { get; set; }
        public string RoleId { get; set; }
        public int Order { get; set; } 
    }
}
