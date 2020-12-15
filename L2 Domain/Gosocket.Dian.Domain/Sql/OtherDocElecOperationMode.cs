using System.Collections.Generic;

namespace Gosocket.Dian.Domain.Sql
{
    [System.ComponentModel.DataAnnotations.Schema.Table("OtherDocElecOperationMode")]
    public class OtherDocElecOperationMode
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<OtherDocElecContributor> OtherDocElecContributor { get; set; }
    }
}
