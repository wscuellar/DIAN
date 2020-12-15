using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Domain.Sql
{
    [System.ComponentModel.DataAnnotations.Schema.Table("OtherDocElecSoftware")]
    public class OtherDocElecSoftware
    {
        [Key]
        public Guid Id { get; set; }
        public int OtherDocElecContributorId { get; set; }
        public virtual OtherDocElecContributor OtherDocElecContributor { get; set; }
        public string Name { get; set; }
        public string Pin { get; set; }
        public DateTime? SoftwareDate { get; set; }
        public string SoftwarePassword { get; set; }
        public string SoftwareUser { get; set; }
        public string Url { get; set; }
        public bool Status { get; set; }
        public int OtherDocElecSoftwareStatusId { get; set; }
        public bool Deleted { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime Updated { get; set; }
        public string CreatedBy { get; set; }
        public virtual ICollection<OtherDocElecContributorOperations> OtherDocElecContributorOperations { get; set; } 

    }
}
