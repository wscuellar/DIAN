using Gosocket.Dian.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class OthersElectronicDocumentsViewModel
    {
        public OthersElectronicDocumentsViewModel()
        {
            OperationModesAsociatedInElectronicBiller = new List<OperationModeElectronicBillerViewModel>();
        }

        public string Id { get; set; }

        public int  OtherDocElecContributorId { get; set; }
        public int ElectronicDocumentId { get; set; }
        public int OperationModeId { get; set; }
        public int ContributorIdType { get; set; }
		//[Required(ErrorMessage = "{0} es requerido")]
		//[Display(Name = "Proveedor tecnológico")]
		public int ProviderId { get; set; }

        //[Required(ErrorMessage = "{0} es requerido")] 
        [Display(Name = "ID del Software")]

        public string SoftwareId { get; set; }

        
        [Display(Name = "ID del Software")]

        public string SoftwareIdPr { get; set; }


        [Display(Name = "Nombre de Software")] 
   
        //Es requerido cuando es softwarepropio
        public string SoftwareName { get; set; }

        [Display(Name = "PIN del Software")]
        //[Required(ErrorMessage = "{0} es requerido")]
        public string PinSW { get; set; }
        public int ModoOperacionId { get; set; }

        //[Required(ErrorMessage = "{0} es requerido")]
        [Display(Name= "Configuración modo de operación")]
        public string OperationMode { get; set; }

        [Required(ErrorMessage = "{0} es requerido")]
        [Display(Name = "URL de recepción de documentos")]
        public string UrlEventReception { get; set; }   

        public int SoftwareType { get; set; }

        public List<OtherDocsElectListViewModel> ListTable { get; set; }

        [Display(Name = "Seleccione el modo de operación")]
        public string OperationModeSelectedId { get; set; }

        [Display(Name = "Nombre empresa proveedora")]
        public string ContributorName { get; set; }

        public bool ContributorIsOfe { get; set; }
        public bool ElectronicDocumentIsSupport => ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument;
        public bool ElectronicDocumentIsEquivalent => ElectronicDocumentId == (int)ElectronicsDocuments.ElectronicEquivalent;
        public List<OperationModeElectronicBillerViewModel> OperationModesAsociatedInElectronicBiller { get; set; }

        public string FreeBillerSoftwareId { get; set; }
    }

    public class OperationModeElectronicBillerViewModel
    {
        public int OperationModeId { get; set; }
        public string OperationModeName { get; set; }
        public OperationModeElectronicBillerDataViewModel Data { get; set; }
    }
    public class OperationModeElectronicBillerDataViewModel
    {
        public string ProviderCompanyName { get; set; }
        public string SoftwareName { get; set; }
        public string SoftwarePin { get; set; }
        public string SoftwareId { get; set; }
    }
}