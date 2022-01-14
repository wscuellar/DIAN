using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Gosocket.Dian.Web.Models
{
    public class ExportDocumentTableViewModel
    {
        public ExportDocumentTableViewModel()
        {
            StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1); ;
            EndDate = DateTime.Now;
            Tasks = new List<ExportDocumentViewModel>();
            AmountAdmin = Int32.Parse(ConfigurationManager.GetValue("AdminDocsToExport"));
            AmountContributor1 = Int32.Parse(ConfigurationManager.GetValue("ContributorsDocsToExport1"));
            AmountContributor2 = Int32.Parse(ConfigurationManager.GetValue("ContributorsDocsToExport2"));
        }
        [Display(Name = "NIT emisor")]
        public string SenderCode { get; set; }
        [Display(Name = "NIT receptor")]
        public string ReceiverCode { get; set; }
        public int Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GroupCode { get; set; }
        public List<ExportDocumentViewModel> Tasks { get; set; }        
        public int AmountAdmin { get; set; }
        public int AmountContributor1 { get; set; }
        public int AmountContributor2 { get; set; }
    }
    public class ExportDocumentViewModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
        public int Type { get; set; }
        public string TypeDescription { get; set; }
        public int Status { get; set; }
        public string StatusDescription { get; set; }
        public string FilterDate { get; set; }
        public string FilterGroup { get; set; }
        public string TotalResult { get; set; }
    }
}