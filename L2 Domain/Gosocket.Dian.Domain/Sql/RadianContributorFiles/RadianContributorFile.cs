using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("RadianContributorFile")]
    public class RadianContributorFile
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("RadianContributor")]
        public int RadianContributorId { get; set; }
        public virtual RadianContributor RadianContributor { get; set; }

        [ForeignKey("RadianContributorFileType")]
        public int FileType { get; set; }
        public virtual RadianContributorFileType RadianContributorFileType { get; set; }

        public string FileName { get; set; }
        
        public bool Deleted { get; set; }

        [ForeignKey("RadianContributorFileStatus")]
        public int Status { get; set; }
        public virtual RadianContributorFileStatus RadianContributorFileStatus { get; set; }

        public string Comments { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime Updated { get; set; }

        public string CreatedBy { get; set; }
    }
}
