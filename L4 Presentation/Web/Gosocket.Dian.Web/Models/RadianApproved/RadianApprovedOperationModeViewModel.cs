using Gosocket.Dian.Domain;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models.RadianApproved
{
    public class RadianApprovedOperationModeViewModel
    {
        [Display(Name = "Configuración modo de Operación")]
        public RadianOperationMode OperationModeSelected { get; set; }

        [Display(Name = "URL de recepción de eventos")]
        public string Url { get; set; }

        [Display(Name = "Nombre de software")]
        public string SoftwareName { get; set; }

        [Display(Name = "PIN del SW")]
        public string SoftwarePin { get; set; }
    }
}