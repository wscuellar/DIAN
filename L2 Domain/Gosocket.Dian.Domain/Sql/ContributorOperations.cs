using System;

namespace Gosocket.Dian.Domain
{
    //[Table("ContributorOperations")]
    public class ContributorOperations
    {
        //[Key, Column(Order = 0)]
        public int Id { get; set; }
        public bool Deleted { get; set; }
        public int ContributorId { get; set; }
        public Contributor Contributor { get; set; }

        public int? ContributorTypeId { get; set; }

        //[Key, Column(Order = 1)]
        public int OperationModeId { get; set; }
        public OperationMode OperationMode { get; set; }

        public int? ProviderId { get; set; }
        public Contributor Provider { get; set; }

        public Guid? SoftwareId { get; set; }
        public Software Software { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
