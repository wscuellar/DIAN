using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{
    public class FileHistoryFilterViewModel
    {

        public int RadianContributorId { get; set; }
        public string FileName { get; set; }
        public string Initial { get; set; }
        public string End { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class FileHistoryListViewModel
    {
        public int Page { get; set; }
        public int RowCount { get; set; }
        public List<FileHistoryItemViewModel> Customers { get; set; }
    }

    public class FileHistoryItemViewModel
    {
        public string FileName { get; set; }
        public string Updated { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }

    }
}
