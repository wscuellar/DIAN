using ExpressiveAnnotations.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class OthersElectronicDocumentsViewModel
    {
        public string Id { get; set; }

        public int  OtherDocElecContributorId { get; set; }
        public int ElectronicDocumentId { get; set; }
        public int OperationModeId { get; set; }
        public int ContributorIdType { get; set; }
        [Required(ErrorMessage = "{0} es requerido")]
        [Display(Name = "Proveedor tecnológico")]
        public int ProviderId { get; set; }

        [Required(ErrorMessage = "{0} es requerido")] 
        [Display(Name = "Nombre de software")]

        public string SoftwareId { get; set; }

        [Display(Name = "Nombre de software")] 
        [RequiredIf("OperationModeId == 1", ErrorMessage = "{0}1 es requerido")]
        //Es requerido cuando es softwarepropio
        public string SoftwareName { get; set; }

        [Display(Name = "PIN del SW")]
        [RequiredIf("OperationModeId == 1", ErrorMessage = "{0}2 es requerido")]
        public string PinSW { get; set; }
        public int ModoOperacionId { get; set; }

        [Required(ErrorMessage = "{0}es requerido")]
        [Display(Name= "Configuración modo de operación")]
        public string OperationMode { get; set; }

        [Required(ErrorMessage = "{0} es requerido")]
        [Display(Name = "URL de recepción de eventos")]
        public string UrlEventReception { get; set; }   

        public int SoftwareType { get; set; }

        public List<OtherDocsElectListViewModel> ListTable { get; set; }
    }
}