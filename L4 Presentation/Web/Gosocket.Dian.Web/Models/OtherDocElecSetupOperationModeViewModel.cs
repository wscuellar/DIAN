using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Models
{
    public class OtherDocElecSetupOperationModeViewModel
    {
        public int Id { get; set; }
        public int ContributorId { get; set; }
        public string OperationMode { get; set; }


        [Display(Name = "Configuración modo de operación")]
        public int OperationModeId { get; set; }
        public string ElectronicDoc { get; set; }
        public int ElectronicDocId { get; set; }
        public int ContributorTypeId { get; set; }
        public string ContributorType { get; set; }
        [Display(Name = "Nombre de software")]
        [Required(ErrorMessage = "Nombre de software es requerido")]
        public string SoftwareName { get; set; }

        [Required(ErrorMessage = "PIN del Software es requerido")]
        [Display(Name = "PIN del Software")]
        [RegularExpression(@"\d{5}", ErrorMessage = "El PIN no cuenta con el formato correcto")]
        public string SoftwarePin { get; set; }
        public string SoftwareUrl { get; set; }

        public int ProviderId { get; set; }
        public string Provider { get; set; }
        public Guid SoftwareId { get; set; }
        public Guid SoftwareIdBase { get; set; }
        public List<SelectListItem> OperationModeList { get; set; }
        public string CreatedBy { get; set; }
        public List<OtherDocsElectListViewModel> ListTable { get; set; }
    }
}