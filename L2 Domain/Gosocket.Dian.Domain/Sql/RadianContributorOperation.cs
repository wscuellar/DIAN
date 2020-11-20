using System;

namespace Gosocket.Dian.Domain
{
    [System.ComponentModel.DataAnnotations.Schema.Table("RadianContributorOperations")]
    public class RadianContributorOperation
    {
        public int Id { get; set; }
        
        public bool Deleted { get; set; }

        public int RadianContributorId { get; set; }
        public RadianContributor RadianContributor { get; set; }

        public int? RadianContributorTypeId { get; set; }

        //[Key, Column(Order = 1)]
        public int RadianOperationModeId { get; set; }
        public RadianOperationMode RadianOperationMode { get; set; }

        public int? RadianProviderId { get; set; }
        public RadianContributor RadianProvider { get; set; }

        public string SoftwareId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
