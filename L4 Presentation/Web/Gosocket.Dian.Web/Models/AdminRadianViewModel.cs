using Gosocket.Dian.Domain;
using Gosocket.Dian.Web.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Models
{
    public class AdminRadianViewModel
    {
        public AdminRadianViewModel()
        {
            RadianContributors = new List<RadianContributorsViewModel>();
            Page = 1;
            Length = 10;
        }
        public List<RadianContributorsViewModel> RadianContributors { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nit")]
        public string Code { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Rango de fechas Registro")]
        public DateTime? DateInterval { get; set; }

        [Display(Name = "Tipo de participante")]
        public int Type { get; set; }
        public string StartDate { get; set;  }
        public string EndDate { get; set; }
        [Display(Name = "Estado")]
        public List<RadianContributorStateViewModel> State { get; set; }

        public int Page { get; set; }
        public int Length { get; set; }

        public int TotalCount { get; set; }

        public int CurrentPage { get; set; }

        public int Id { get; set; }
        public bool SearchFinished { set; get; }
        [Display(Name = "Tipo de participante")]
        public IEnumerable<SelectListItem> RadianType { get; set; }
        [Display(Name = "Estado")]
        public Domain.Common.RadianState? RadianState { get; set; }

    }

    public class RadianContributorsViewModel
    {

        public RadianContributorsViewModel()
        {
            ContributorType = new RadianContributorTypeViewModel();
            Users = new List<UserViewModel>();
            RadianContributorTestSetResults = new List<TestSetResultViewModel>();
            AcceptanceStatuses = new List<RadianContributorAcceptanceStatusViewModel>();
            CanEdit = false;
            IsEdit = false;
        }

        public int Id { get; set; }
        public int ContributorId { get; set; }
        [Display(Name = "Nit")]
        public string Code { get; set; }
        [Display(Name = "Nombre")]
        public string TradeName { get; set; }
        [Display(Name = "Razón Social")]
        public string BusinessName { get; set; }
        public string State { get; set; }

        public int RadianContributorTypeId { get; set; }
        public int RadianOperationModeId { get; set; }
        [Display(Name = "Estado de aprobación")]
        public string RadianState { get; set; }
        public int CreatedBy { get; set; }

        [Display(Name = "Estado de aprobación")]
        public int AcceptanceStatusId { get; set; }

        
        public string AcceptanceStatusName { get; set; }
        public List<RadianContributorAcceptanceStatusViewModel> AcceptanceStatuses { get; set; }


        [Display(Name = "Correo electronico")]
        public string Email { get; set; }

        [Display(Name = "Fecha ingreso de solicitud")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Fecha ultima actualización")]
        public DateTime UpdatedDate { get; set; }

        [Display(Name = "Tipo de participante")]
        public string ContributorTypeName { get; set; }

        public RadianContributorTypeViewModel ContributorType { get; set; }
        public List<UserViewModel> Users { get; set; }
        public List<TestSetResultViewModel> RadianContributorTestSetResults { get; set; }
        [Display(Name = "Estado de aprobación")]
        public RadianUtil.UserApprovalStates? RadianApprovalState { get; set; }
        public List<RadianContributorFileType> RadianContributorFilesType { get; set; }
        public List<RadianContributorFileViewModel> RadianContributorFiles { get; set; }
        public RadianContributorFileStatusViewModel RadianContributorFileStatus { get; set; }
        public bool CanEdit { get; set; }
        public RadianUtil.DocumentStates? RadianFileStatus { get; set; }
        public bool SuccessMessage { get; set; }
        public bool IsEdit { get; set; }
        public List<FilesChangeStateViewModel> ListChangeStateFiles { get; set; }
        public bool IsActive { get; set; }

    }

    public class RadianContributorTypeViewModel
    {
         public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RadianContributorStateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RadianContributorAcceptanceStatusViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class RadianContributorFileViewModel
    {
        public Guid Id { get; set; }

        public ContributorFileTypeViewModel ContributorFileType { get; set; }

        public string FileName { get; set; }

        public bool Deleted { get; set; }

        public RadianContributorFileStatusViewModel ContributorFileStatus { get; set; }

        public string Comments { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime Updated { get; set; }

        public string CreatedBy { get; set; }

        public bool IsNew { get; set; }
    }

    public class RadianContributorFileStatusViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class FilesChangeStateViewModel
    {
        public string Id { get; set; }
        public int NewState { get; set; }
        public string comment { get; set; }

    }

}