using System;

namespace Gosocket.Dian.Web.Models
{
    public class OtherDocsElectListViewModel
    {
        public int Id { get; set; }
        public int ContributorId { get; set; }
        public string OperationMode { get; set; }
        public string ContributorType { get; set; }
        public string ElectronicDoc { get; set; }
        public string Software { get; set; }

        public string SoftwareId { get; set; }
        
        public string PinSW { get; set; }
        public string StateSoftware { get; set; }

        public string StateContributor { get; set; }
        public string Url { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Length { get; set; }
    }
}