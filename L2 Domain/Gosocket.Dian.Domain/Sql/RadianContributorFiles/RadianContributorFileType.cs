using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("RadianContributorFileType")]
    public class RadianContributorFileType
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime Updated { get; set; }
        public string CreatedBy { get; set; }
        public bool Mandatory { get; set; }
        public bool Deleted { get; set; }
        public int RadianContributorTypeId { get; set; }
        public virtual RadianContributorType RadianContributorType { get; set; }
        [NotMapped]
        public bool HideDelete { get; set; }
    }
}
