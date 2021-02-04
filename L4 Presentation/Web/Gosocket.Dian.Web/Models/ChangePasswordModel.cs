using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class ChangePasswordModel
    {
        [Display(Name = "Contraseña nueva")]
        public string UserPassword { get; set; }

        [Display(Name = "Confirmar contraseña")]
        public string UserPasswordConfirm { get; set; }

        [Display(Name = "Contraseña actual")]
        public string UserOldPassword { get; set; }

        public string UserCode { get; set; }
        public int IdentificationType { get; set; }
    }
}