using System;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Web.Utils;
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
            Page = 1;
            Length = 10;
        }
        public List<RadianContributorsViewModel> RadianContributors { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nit Participante")]
        public string Code { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Registro Radian")]
        public DateTime DateInterval { get; set; }

        [Display(Name = "Tipo Participante")]
        public List<RadianContributorTypeViewModel> Type { get; set; }

        [Display(Name = "Estado")]
        public List<RadianContributorStateViewModel> State { get; set; }

        public int Page { get; set; }
        public int Length { get; set; }

        public int Id { get; set; }
        public bool SearchFinished { set; get; }
        public IEnumerable<SelectListItem> RadianType { get; set; }
        public RadianUtil.UserStates? RadianState { get; set; }


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

        [Display(Name = "Estado de aprobación")]
        public int AcceptanceStatusId { get; set; }

        [Display(Name = "Estado de aprobación")]
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
        public RadianUtil.UserApprovalStates? RadianState { get; set; }
        public List<RadianContributorFileViewModel> RadianContributorFiles { get; set; }
        public RadianContributorFileStatusViewModel RadianContributorFileStatus { get; set; }
        public bool CanEdit { get; set; }

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

}