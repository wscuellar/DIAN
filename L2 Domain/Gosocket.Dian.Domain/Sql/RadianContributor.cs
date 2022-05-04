using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [System.ComponentModel.DataAnnotations.Schema.Table("RadianContributor")]
    public class RadianContributor
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public int ContributorId { get; set; }
        public Contributor Contributor { get; set; }
        public int RadianContributorTypeId { get; set; }
        public RadianContributorType RadianContributorType { get; set; }
        public int RadianOperationModeId { get; set; }
        public RadianOperationMode RadianOperationMode { get; set; }
        public string RadianState { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime Update { get; set; }
        public int Step { get; set; }
        public string CreatedBy { get; set; }
        public virtual ICollection<RadianContributorFile> RadianContributorFile { get; set; }
        [NotMapped]
        public ICollection<RadianSoftware> RadianSoftwares { get; set; }
        public virtual ICollection<RadianContributorOperation> RadianContributorOperations { get; set; }
        public bool IsActive { get; set; }
        
    }

}
