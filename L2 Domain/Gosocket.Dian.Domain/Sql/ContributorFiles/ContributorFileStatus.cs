using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("ContributorFileStatus")]
    public class ContributorFileStatus
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
