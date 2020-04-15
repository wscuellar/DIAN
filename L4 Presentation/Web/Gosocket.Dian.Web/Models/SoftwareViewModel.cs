using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class SoftwareTableViewModel
    {
        public SoftwareTableViewModel()
        {
            Page = 0;
            Length = 10;
            Contributors = new List<ContributorViewModel>();
            Softwares = new List<SoftwareViewModel>();
        }

        public int? ContributorId { get; set; }
        [Display(Name = "NIT Facturador")]
        public string ContributorCode { get; set; }
        [Display(Name = "Nombre Facturador")]
        public string ContributorName { get; set; }
        public int Status { get; set; }
        public int Page { get; set; }
        public int Length { get; set; }

        [Display(Name = "Pin")]
        public string Pin { get; set; }

        [Display(Name = "Proveedores")]
        public List<ContributorViewModel> Contributors { get; set; }

        [Display(Name = "Prefijo")]
        public List<NumberRangeViewModel> Prefixes { get; set; }

        public List<NumberRangeViewModel> AssociatedRanges { get; set; }

        public string NumberRangeRowKey { get; set; }
        public Guid SoftwareId { get; set; }
        [Display(Name = "Proveedor - Software")]
        public List<SoftwareViewModel> Softwares { get; set; }

    }
    public class SoftwareViewModel
    {
        [Display(Name = "Identificador")]
        public Guid Id { get; set; }

        [Display(Name = "NIT de proveedor")]
        public int ContributorId { get; set; }

        [Display(Name = "Proveedor")]
        public string ContributorName { get; set; }

        [Display(Name = "NIT de proveedor")]
        public string ContributorCode { get; set; }

        [Required(ErrorMessage = "El Pin es requerido.")]
        [RegularExpression(@"\d{5}",ErrorMessage = "Debe ser numérico y tener 5 dígitos.")]
        [Display(Name = "Pin")]
        public string Pin { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Fecha")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "El Usuario es requerido.")]
        [Display(Name = "Usuario")]
        public string SoftwareUser { get; set; }

        [Required(ErrorMessage = "La Contraseña es requerida.")]
        [Display(Name = "Contraseña")]
        public string SoftwarePassword { get; set; }

        [Required(ErrorMessage = "La Url es requerida.")]
        [Display(Name = "Url")]
        public string Url { get; set; }

        [Display(Name = "Status")]
        public int AcceptanceStatusSoftwareId { get; set; }
        public List<AcceptanceStatusSoftwareViewModel> Statuses { get; set; }

        [Display(Name = "Estado")]
        public string StatusName { get; set; }


        [Display(Name = "Deleted")]
        public bool Deleted { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }

        [Display(Name = "Updated")]
        public DateTime Updated { get; set; }

        [Display(Name = "Creado por")]
        public string CreatedBy { get; set; }

    }

    public class AcceptanceStatusSoftwareViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [Obsolete]
    public class ListSoftwareViewModel
    {
        public string ProviderId { get; set; }
        public int Status { get; set; }
        public int Page { get; set; }
        public bool IsNextPage { get; set; }
        public string Nit { get; set; }
        public string Pin { get; set; }
        public List<SoftwareViewModel> Softwares { get; set; }
        public List<AcceptanceStatusSoftware> StatusList { get; set; }

        public ListSoftwareViewModel()
        {
            IsNextPage = false;
            Page = 1;
            Status = -1;
            Nit = "";
            Pin = "";
            ProviderId = "";
            Softwares = new List<SoftwareViewModel>();
            StatusList = new SoftwareService().GetAcceptanceStatusesSoftware();
        }
    }
}