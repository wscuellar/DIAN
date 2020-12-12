using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain
{
    [System.ComponentModel.DataAnnotations.Schema.Table("RadianContributorOperations")]
    public class RadianContributorOperation
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("RadianContributor")]
        public int RadianContributorId { get; set; }
        public RadianContributor RadianContributor { get; set; }
        public int OperationStatusId { get; set; }
        public int SoftwareType { get; set; }
        public Guid SoftwareId { get; set; }
        public RadianSoftware Software { get; set; }
        public bool Deleted { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
