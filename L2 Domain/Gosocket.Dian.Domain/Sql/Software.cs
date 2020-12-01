using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [Table("Software")]
    public class Software
    {
        [Key]
        public Guid Id { get; set; }

        public int ContributorId { get; set; }
        public virtual Contributor Contributor { get; set; }

        public string Pin { get; set; }

        public string Name { get; set; }

        public DateTime? SoftwareDate { get; set; }

        public string SoftwareUser { get; set; }

        public string SoftwarePassword { get; set; }

        public string Url { get; set; }

        public bool Status { get; set; }

        public int AcceptanceStatusSoftwareId { get; set; }
        public virtual AcceptanceStatusSoftware AcceptanceStatusSoftware { get; set; }

        public bool Deleted { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime Updated { get; set; }

        public string CreatedBy { get; set; }


    }
}
