using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gosocket.Dian.Domain.Sql
{
    [Table("OtherDocElecContributorOperations")]
    public class OtherDocElecContributorOperations
    {
        public int Id { get; set; }
        [ForeignKey("OtherDocElecContributor")]
        public int OtherDocElecContributorId { get; set; }  
        public  OtherDocElecContributor OtherDocElecContributor { get; set; }
        public int OperationStatusId { get; set; }
        public int SoftwareType { get; set; }
        public Guid SoftwareId { get; set; }
        public OtherDocElecSoftware Software { get; set; }
        public bool Deleted { get; set; }
        public DateTime Timestamp { get; set; }
         
    }
}
