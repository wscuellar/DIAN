using Gosocket.Dian.Domain.Sql.FreeBiller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Models.FreeBiller
{
    public class UserFreeBillerModel
    {
        public string Id { get; set; }

        [DisplayName("Nombre")]
        public string Name { get; set; }

        [DisplayName("Apellidos")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Nombres y apellidos requerido")]
        [MaxLength(50, ErrorMessage = "Máximo {1} carácteres")]
        [DisplayName("Nombres y apellidos")]
        public string FullName { get; set; }

        [DisplayName("Tipo de documento")]
        [Required(ErrorMessage = "El tipo de documento es requerido")]
        public string TypeDocId { get; set; }

        public List<SelectListItem> TypesDoc { get; set; }

        [Required(ErrorMessage = "El número de documento es requerido")]
        [MaxLength(30, ErrorMessage = "Máximo {1} carácteres")]
        [DisplayName("Número de documento")]
        public string NumberDoc { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [MaxLength(50, ErrorMessage = "Máximo {1} carácteres")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [DisplayName("Correo electrónico")]
        public string Email { get; set; }

        [DisplayName("Contraseña")]
        public string Password { get; set; }

        [DisplayName("Perfil")]
        public int ProfileId { get; set; }

        public List<int> ProfileIds { get; set; }

        public List<SelectListItem> Profiles { get; set; }

        public string DescriptionTypeDoc { get; set; }

        public string DescriptionProfile { get; set; }

        public DateTime? LastUpdate { get; set; }

        public bool IsActive { get; set; }

        public bool IsEdit { get; set; }

        public List<MenuOptions> MenuOptionsByProfile { get; set; }
    }


    public class UserFreeBillerContainerModel
    {
        public int TotalCount
        {
            get; set;
        }

        public List<UserFreeBillerModel> Users
        {
            get; set;
        }
    }

}