using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("ContributorFileType")]
    public class ContributorFileType
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime Updated { get; set; }
        public string CreatedBy { get; set; }
        public bool Mandatory { get; set; }
        public bool Deleted { get; set; }
    }
}
