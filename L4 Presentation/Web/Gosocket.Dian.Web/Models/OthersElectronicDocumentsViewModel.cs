using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class OthersElectronicDocumentsViewModel
    {
        public string Id { get; set; }
        [Display(Name = "Nombre de software")]
        public string SoftwareName { get; set; }

        [Display(Name = "PIN del SW")]
        public string PinSW { get; set; }
        public int ModoOperacionId { get; set; }
        
        [Display(Name= "Configuración modo de operación")]
        public string OperationMode { get; set; }

        [Display(Name = "URL de recepción de eventos")]
        public string UrlEventReception { get; set; }

        [Display(Name = "Nombre de la empresa provedora")]
        public string CompanyName { get; set; }

        [Display(Name = "ID del SW")]
        public string IdSW { get; set; }
    }
}