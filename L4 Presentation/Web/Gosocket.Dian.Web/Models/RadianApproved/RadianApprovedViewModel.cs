using Gosocket.Dian.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianApprovedViewModel
    {
        public RadianApprovedViewModel()
        {
            RadianFileList = new List<RadianContributorFileTypeTableViewModel>();
        }
        public int Step { get; set; }

        public int CurrentlyStep { get; set; }

        public int RadianContributorTypeId { get; set; }

        [Display(Name = "NIT")]
        public int Nit { get; set; }

        [Display(Name = "Nombre")]
        public int Name { get; set; }

        [Display(Name = "Razón Social")]
        public int BusinessName { get; set; }

        [Display(Name = "Correo electrónico")]
        public int Email { get; set; }

        public string IsoCertificate { get; set; }

        public string SarlafCertificate { get; set; }

        public List<RadianContributorFileTypeTableViewModel> RadianFileList { get; set; }
        public List<RadianContributorFile> Files { get; set; }
        public List<RadianCustomerViewModel> Customers { get; set; }

    }
}