using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    

    public class ContributorModel
    {
        public int Id { get; set; }
        public string SelectedFileTypesList { get; set; }

        [Required(ErrorMessage = "El NIT es requerido")]
        [Display(Name = "NIT")]
        public string Nit { get; set; }

        public int ContributorType { get; set; }

        [Display(Name = "Proveedor")]
        public int? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderCode { get; set; }

        [Required(ErrorMessage = "El Razón Social es requerida")]
        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; }

        [Required(ErrorMessage = "El Nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        public int Status { get; set; }

        public string StatusName { get; set; }

        [Required(ErrorMessage = "El Correo Electrónico es requerido")]
        [Display(Name = "Correo Electrónico")]
        [EmailAddress(ErrorMessage = "Debe ingresar un Correo Electrónico válido.")]
        public string Email { get; set; }

        [Display(Name = "Fecha Inicio Autorización")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Fecha Término Autorización")]
        public DateTime? EndDate { get; set; }

        //public List<ProviderFileModel> ProviderFiles { get; set; }
        public List<GlobalTestSet> ProviderTestSets { get; set; }

        public int AcceptanceResult { get; set; }

        [Display(Name = "Software")]
        public string SoftwareId { get; set; }
        [Display(Name = "Nombre")]
        public string SoftwareName { get; set; }

        [Display(Name = "Modo de Opearión")]
        public int? OperationModeId { get; set; }
        public string OperationModeName { get; set; }

        public string Pin { get; set; }
        [Display(Name = "User")]
        public string SoftwareUser { get; set; }
        [Display(Name = "Password")]
        public string SoftwarePassword { get; set; }

        public string Url { get; set; }

        public List<ContributorOperationsViewModel> ContributorOperations { get; set; }

        public ContributorModel()
        {
            //ProviderFiles = new List<ProviderFileModel>();
            ProviderTestSets = new List<GlobalTestSet>();
            ContributorOperations = new List<ContributorOperationsViewModel>();
        }
    }
}