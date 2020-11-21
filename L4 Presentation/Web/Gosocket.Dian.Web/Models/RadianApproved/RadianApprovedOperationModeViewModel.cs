using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianApprovedOperationModeViewModel
    {
        [Display(Name = "Configuración modo de Operación")]
        public RadianOperationMode OperationModeSelected { get; set; }

        [Display(Name = "Nombre de software")]
        [Required(ErrorMessage = "Nombre de software es requerido")]
        public string SoftwareName { get; set; }

        public Guid SoftwareId { get; set; }

        [Display(Name = "PIN del Software")]
        [RegularExpression(@"\d{5}", ErrorMessage = "El PIN no cuenta con el formato correcto")]
        [Required(ErrorMessage = "PIN del Software es requerido")]
        public string SoftwarePin { get; set; }

        public string SoftwareUrl { get; set; }

        public string CreatedBy { get; set; }

        public RedianContributorWithTypes Contributor { get; set; }

        public Software Software { get; set; }

        public RadianContributorOperationWithSoftware RadianContributorOperations { get; set; }

        public List<RadianOperationMode> OperationModeList { get; set; }
    }
}