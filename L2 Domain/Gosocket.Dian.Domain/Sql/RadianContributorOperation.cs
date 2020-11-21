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
                
        public int RadianOperationModeId { get; set; }
        public RadianOperationMode RadianOperationMode { get; set; }

        public int? RadianProviderId { get; set; }
        public RadianContributor RadianProvider { get; set; }

        public Guid SoftwareId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Pin { get; set; }
        public string SoftwareName { get; set; }
        public string Url { get; set; }
    }
}
