using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain.Sql
{
    [Table("EquivalentElectronicDocument")]
    public class EquivalentElectronicDocument
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}