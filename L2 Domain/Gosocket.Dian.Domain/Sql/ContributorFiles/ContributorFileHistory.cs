using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("ContributorFileHistory")]
    public class ContributorFileHistory
    {
        [System.ComponentModel.DataAnnotations.Key]
        public Guid Id { get; set; }

        public Guid ContributorFileId { get; set; }
        public virtual ContributorFile ContributorFile { get; set; }

        public string FileName { get; set; }

        [ForeignKey("ContributorFileStatus")]
        public int Status { get; set; }
        public virtual ContributorFileStatus ContributorFileStatus { get; set; }

        public string Comments { get; set; }

        public DateTime Timestamp { get; set; }

        public string CreatedBy { get; set; }
    }
}
