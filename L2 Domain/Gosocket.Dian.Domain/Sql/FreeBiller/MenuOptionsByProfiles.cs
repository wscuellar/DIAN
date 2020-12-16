
namespace Gosocket.Dian.Domain.Sql.FreeBiller
{
    using System.ComponentModel.DataAnnotations;

    public class MenuOptionsByProfiles
    {
        [Key]
        public int Id { get; set; }

        public int ProfileId { get; set; }

        public int MenuOptionId { get; set; }
    }
}
