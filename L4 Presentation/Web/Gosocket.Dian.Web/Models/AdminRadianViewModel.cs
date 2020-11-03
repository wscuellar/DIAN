using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
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
        [Display(Name = "Nit Participante")]
        public string Code { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Registro Radian")]
        public DateTime DateInterval { get; set; }

        [Display(Name = "Tipo Participante")]
        public List<RadianContributorType> Type { get; set; }

        [Display(Name = "Estado")]
        public List<RadianContributorState> State { get; set; }

        public int Id { get; set; }
        public bool SearchFinished { set; get; }
        public IEnumerable<SelectListItem> RadianType { get; set; }
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
        public int ContributorId { get; set; }

    }

    public class RadianContributorType
    {
        public RadianContributorType()
        {

        }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RadianContributorState
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}