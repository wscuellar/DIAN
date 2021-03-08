using System;

namespace Gosocket.Dian.Web.Models
{
    public class OtherDocElecSoftwareViewModel
    {
        public Guid Id { get; set; } 
        public string Name { get; set; }
        public string Pin { get; set; }
        public DateTime? SoftwareDate { get; set; }
        public string SoftwarePassword { get; set; }
        public string SoftwareUser { get; set; }
        public string Url { get; set; }
        public bool Status { get; set; }
        public int OtherDocElecSoftwareStatusId { get; set; }
        public string OtherDocElecSoftwareStatusName { get; set; }
        public int ProviderId { get; set; }

        public Guid SoftwareId { get; set; }
         
    }
}