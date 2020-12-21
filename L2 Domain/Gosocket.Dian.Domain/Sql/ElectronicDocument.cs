using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain.Sql
{
    [Table("ElectronicDocument")]
    public class ElectronicDocument
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<OtherDocElecContributor> OtherDocElecContributors { get; set; }
    }
}