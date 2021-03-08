using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("ContributorFile")]
    public class ContributorFile
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Contributor")]
        public int ContributorId { get; set; }
        public virtual Contributor Contributor { get; set; }

        [ForeignKey("ContributorFileType")]
        public int FileType { get; set; }
        public virtual ContributorFileType ContributorFileType { get; set; }

        public string FileName { get; set; }
        
        public bool Deleted { get; set; }

        [ForeignKey("ContributorFileStatus")]
        public int Status { get; set; }
        public virtual ContributorFileStatus ContributorFileStatus { get; set; }

        public string Comments { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime Updated { get; set; }

        public string CreatedBy { get; set; }
    }
}
