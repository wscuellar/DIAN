using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gosocket.Dian.Web.Utils;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianApprovedViewModel
    {
        public RadianApprovedViewModel()
        {
            RadianFileList = new List<RadianContributorFileTypeTableViewModel>();
            PageTable = 1;
        }
        public int Step { get; set; }

        public int ContributorId { get; set; }

        public RedianContributorWithTypes Contributor { get; set; }

        [Display(Name = "Tipo de participante")]
        public int RadianContributorTypeId { get; set; }

        public List<RadianContributorFileType> FilesRequires { get; set; }

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

        public RadianTestSetResult RadianTestSetResult { get; set; }

        [Display(Name = "Estado de aprobación")]
        public string RadianState { get; set; }

        public RadianSoftware Software { get; set; }

        public RadianContributorOperationWithSoftware RadianContributorOperations { get; set; }

        public List<string> LegalRepresentativeIds { get; set; }

        public List<UserViewModel> LegalRepresentativeList { get; set; }

       
        [Display(Name = "Nit Facturador")]
        public string NitSearch { get; set; }

        [Display(Name = "Nombre de archivo")]
        public string NameSearch { get; set; }

        [Display(Name = "Estado de aprobación")]
        public RadianUtil.UserStates RadianStateSelect { get; set; }
        public int PageTable { get; set; }
        public int CustomerTotalCount { get; internal set; }
        public FileHistoryListViewModel FileHistories { get; internal set; } 
        public int FileHistoriesRowCount { get; internal set; }
        public List<RadianContributorFileType> FileTypes { get; internal set; }

        public int SoftwareType { get; set; }

        public int RadianOperationId { get; set; }
    }
}