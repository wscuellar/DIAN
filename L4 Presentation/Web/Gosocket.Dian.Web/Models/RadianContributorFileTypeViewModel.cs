using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class RadianContributorFileTypeTableViewModel
    {
        public RadianContributorFileTypeTableViewModel()
        {
            Page = 0;
            Length = 10;
            RadianContributorFileTypes = new List<RadianContributorFileTypeViewModel>();
        }
        public int Page { get; set; }
        public int Length { get; set; }

        public bool SearchFinished { get; set; }
        public List<RadianContributorFileTypeViewModel> RadianContributorFileTypes { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nombre")]
        public string Name { get; set; }
    }

    public class RadianContributorFileTypeViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime Updated { get; set; }

        [Display(Name = "Obligatorio")]
        public bool Mandatory { get; set; }
    }
}