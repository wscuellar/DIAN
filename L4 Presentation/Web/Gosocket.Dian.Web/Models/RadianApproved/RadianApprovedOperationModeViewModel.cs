using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianApprovedOperationModeViewModel
    {
        [Display(Name = "Seleccione el modo de operación")]
        public string OperationModeSelectedId { get; set; }

        public SelectList OperationModes { get; set; }

        [Display(Name = "Nombre del software")]
        [Required(ErrorMessage = "Nombre de software es requerido")]
        [MaxLength(50, ErrorMessage = "Máximo {1} carácteres")]
        public string SoftwareName { get; set; }

        public Guid SoftwareId { get; set; }

        [Required(ErrorMessage = "PIN del Software es requerido")]
        [Display(Name = "PIN del Software")]
        [RegularExpression(@"\d{5}", ErrorMessage = "El PIN no cuenta con el formato correcto")]
        public string SoftwarePin { get; set; }

        public string SoftwareUrl { get; set; }

        public string CreatedBy { get; set; }

        public RedianContributorWithTypes Contributor { get; set; }

        public Software Software { get; set; }

        public RadianContributorOperationWithSoftware RadianContributorOperations { get; set; }

        public List<RadianOperationMode> OperationModeList { get; set; }
        [Display(Name = "name")]
        public string CustomerName { get; set; }
        public int CustomerID { get; set; }

        public int SoftwareSelectedId { get; set; }
        public List<AutoListModel> SupplierCompany { get; set; }
    }
}