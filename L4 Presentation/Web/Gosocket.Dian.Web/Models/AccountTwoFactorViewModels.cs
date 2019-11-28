using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class LoginTwoFactorViewModel
    {
        [Required(ErrorMessage = "El NIT de empresa es requerido.")]
        [Display(Name = "NIT Empresa")]
        public string CompanyCode{ get; set; }

        [Required(ErrorMessage = "El NIT de representante legal es requerido.")]
        [Display(Name = "NIT Representante Legal")]
        public string UserCode { get; set; }

        ////Para el login de persona natural
        [Required(ErrorMessage = "Seleccione tipo de identificación.")]
        [Display(Name = "Tipo de identificación")]
        public int IdentificationType { get; set; }
        public List<IdentificationTypeListViewModel> IdentificationTypes { get; set; }
    }

    public class IdentificationTypeListViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
