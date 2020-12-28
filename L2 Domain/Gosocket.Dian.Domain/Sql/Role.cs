using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain.Sql
{
    [Table("AspNetRoles")]
    public class Role
    {
        [Key]
        public string Id { get; set; } 
        public string Name { get; set; } 
    }
}
