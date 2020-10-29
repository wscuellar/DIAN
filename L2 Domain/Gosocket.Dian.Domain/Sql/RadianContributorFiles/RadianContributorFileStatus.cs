using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("RadianContributorFileStatus")]
    public class RadianContributorFileStatus
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
