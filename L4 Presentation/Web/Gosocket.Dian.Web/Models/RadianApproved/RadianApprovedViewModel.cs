using Gosocket.Dian.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianApprovedViewModel
    {
        public int Step { get; set; }

        public int CurrentlyStep { get; set; }

        public int RadianContributorTypeId { get; set; }

        [Display(Name = "NIT")]
        public string Nit { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Razón Social")]
        public string BusinessName { get; set; }

        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        public List<RadianContributorFileTypeTableViewModel> RadianFileList { get; set; }

        public List<RadianContributorFile> Files { get; set; }

        public List<RadianCustomerViewModel> Customers { get; set; }

        public RadianApprovedViewModel()
        {
            RadianFileList = new List<RadianContributorFileTypeTableViewModel>();
        }
    }
}