using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class UserTableViewModel
    {
        public UserTableViewModel()
        {
            Page = 0;
            Length = 10;
            Users = new List<UserViewModel>();
        }
        public int Page { get; set; }
        public int Length { get; set; }

        public bool SearchFinished { get; set; }
        public List<UserViewModel> Users { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "NIT representante legal")]
        public string Code { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
    }

    public class UserChangePasswordViewModel
    {
        public UserChangePasswordViewModel()
        {
            Succeeded = false;
        }
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        [Required(ErrorMessage = "Contraseña actual es requerida.")]
        public string CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        [Required(ErrorMessage = "Nueva contraseña es requerida.")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirme nueva contraseña")]
        [Required(ErrorMessage = "Confirmación de nueva contraseña es requerida.")]
        [Compare("NewPassword", ErrorMessage = "Confirmación no coincide con nueva contraseña.")]
        public string ConfirmNewPassowrd { get; set; }

        public bool Succeeded { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "Contraseña actualizada con éxito.")]
        public string SuccessMessage { get; set; }
    }

    public class UserLoginViewModel
    {
        public UserLoginViewModel()
        {
            IdentificationTypes = new List<IdentificationTypeListViewModel>();
        }

        public string RecaptchaToken { get; set; }
        public string AdminLoginFailed { get; set; }
        public string CertificateLoginFailed { get; set; }
        public string CompanyLoginFailed { get; set; }
        public string PersonLoginFailed { get; set; }

        [Required(ErrorMessage = "El NIT de empresa es requerido.")]
        [Display(Name = "NIT Empresa")]
        public string CompanyCode { get; set; }

        public string HashCode { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Ingrese CUFE o UUID")]
        [Required(ErrorMessage = "CUFE o UUID es requerido.")]
        public string DocumentKey { get; set; }

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Cédula del contribuyente es requerido.")]
        [Display(Name = "Cédula del contribuyente")]
        public string PersonCode { get; set; }

        [Required(ErrorMessage = "El NIT de representante legal es requerido.")]
        [Display(Name = "NIT Representante Legal")]
        public string UserCode { get; set; }


        ////Para el login de persona natural
        [Display(Name = "Tipo de identificación")]
        public int IdentificationType { get; set; }

        public List<IdentificationTypeListViewModel> IdentificationTypes { get; set; }
    }

    public class UserViewModel
    {
        public UserViewModel()
        {
            CanEdit = false;
            Contributors = new List<ContributorViewModel>();
        }

        public string Id { get; set; }
        
        [DataType(DataType.Text)]
        [Display(Name = "NIT")]
        [Required(ErrorMessage = "NIT representante legal es requerido.")]
        public string Code { get; set; }


        [DataType(DataType.Text)]
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Nombre representante legal es requerido.")]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Correo electrónico representante legal es requerido.")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string Email { get; set; }

        public string Roles { get; set; }

        public string ContributorCode { get; set; }

        public bool CanEdit { get; set; }

        [Required(ErrorMessage = "Seleccione tipo de identificación.")]
        [Display(Name = "Tipo de identificación")]
        public int IdentificationType { get; set; }

        public List<IdentificationTypeListViewModel> IdentificationTypes { get; set; }

        public List<ContributorViewModel> Contributors { get; set; }
    }
}