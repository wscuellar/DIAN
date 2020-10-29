using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("RadianContributorFileHistory")]
    public class RadianContributorFileHistory
    {
        [System.ComponentModel.DataAnnotations.Key]
        public Guid Id { get; set; }

        public Guid RadianContributorFileId { get; set; }
        public virtual RadianContributorFile RadianContributorFile { get; set; }

        public string FileName { get; set; }

        [ForeignKey("RadianContributorFileStatus")]
        public int Status { get; set; }
        public virtual RadianContributorFileStatus RadianContributorFileStatus { get; set; }

        public string Comments { get; set; }

        public DateTime Timestamp { get; set; }

        public string CreatedBy { get; set; }
    }
}
