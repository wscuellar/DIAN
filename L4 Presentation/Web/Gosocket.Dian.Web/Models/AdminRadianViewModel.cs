using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class AdminRadianViewModel
    {
        public AdminRadianViewModel()
        {
            RadianContributors = new List<RadianContributorsViewModel>();
        }
        public List<RadianContributorsViewModel> RadianContributors { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nit")]
        public string Code { get; set; }
    }

    public class RadianContributorsViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string TradeName { get; set; }
        public string BusinessName { get; set; }
        public string State { get; set; }

        [Display(Name = "Estado de aprobación")]
        public int AcceptanceStatusId { get; set; }

        [Display(Name = "Estado de aprobación")]
        public string AcceptanceStatusName { get; set; }
    }
}