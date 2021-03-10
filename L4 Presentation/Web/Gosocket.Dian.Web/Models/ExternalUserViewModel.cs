using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class ExternalUserViewModel
    {
        public ExternalUserViewModel()
        {
            Id = string.Empty;
            Page = 0;
            Length = 10;
            IdentificationTypes = new List<IdentificationTypeListViewModel>();
            Roles = new List<IdentityUserRole>();
            Users = new List<ExternalUserViewModel>();
        }

        public string Id { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "{0} es requerido.")]
        [MaxLength(100, ErrorMessage = "Solo se permiten 100 caracteres")]
        [RegularExpression(@"^[0-9a-zA-Z\sñÑáéíóú]+$", ErrorMessage = "Solo se permiten caracteres 0-9 y a-z")]
        public string Names { get; set; }

        [Display(Name = "Tipo de documento")]
        [Required(ErrorMessage = "{0} es requerido.")]
        public int IdentificationTypeId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Número de documento")]
        [Required(ErrorMessage = "{0} es requerido.")]
        [MaxLength(30, ErrorMessage = "Solo se permiten 30 caracteres")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Solo se permiten números 0-9")]
        public string IdentificationId { get; set; }

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "{0} es requerido")]
        [MaxLength(100, ErrorMessage = "Solo se permiten 100 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        public string Email { get; set; }

        /// <summary>
        /// El password se genera automaticamente y se le informa al Usuario cual es, en un correo
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indica si el Usuario esta Activo o Inactivo
        /// </summary>
        public byte Active { get; set; }
        public string UpdatedBy { get; set; }
        public string ActiveDescription { get; set; }
        public DateTime LastUpdated { get; set; }
        public string CreatorNit { get; set; }
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Numero
        /// </summary>
        public int Page { get; set; }

        //numero de registros en la tabla/busqueda
        public int Length { get; set; }
        public bool SearchFinished { get; set; }

        public List<IdentificationTypeListViewModel> IdentificationTypes { get; set; }
        
        public List<IdentityUserRole> Roles { get; set; }
        public List<ExternalUserViewModel> Users { get; set; }

    }
}