using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Domain.Sql
{
    [System.ComponentModel.DataAnnotations.Schema.Table("OtherDocElecContributor")]
    public class OtherDocElecContributor
    {

        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public int ContributorId { get; set; }
        public Contributor Contributor { get; set; }
        public int OtherDocElecContributorTypeId { get; set; }
        public OtherDocElecContributorType OtherDocElecContributorType { get; set; }
        public int OtherDocElecOperationModeId { get; set; }
        public OtherDocElecOperationMode OtherDocElecOperationMode { get; set; } 
        public int ElectronicDocumentId { get; set; }
        public ElectronicDocument ElectronicDocument { get; set; }
        public string State { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Step { get; set; }
        public DateTime Update { get; set; }
        public string CreatedBy { get; set; }
        public string Description { get; set; }
        public virtual ICollection<OtherDocElecSoftware> OtherDocElecSoftwares { get; set; }
        public virtual ICollection<OtherDocElecContributorOperations> OtherDocElecContributorOperations { get; set; }
    }

}
