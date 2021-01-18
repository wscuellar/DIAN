using Gosocket.Dian.Domain.Sql.FreeBiller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Models.FreeBiller
{
    public class UserFreeBillerModel
    {
        public UserFreeBillerModel()
        {
            IsEdit = true;
        }
        public string Id { get; set; }

        [DisplayName("Nombre")]
        public string Name { get; set; }

        [DisplayName("Apellidos")]
        public string LastName { get; set; }

        [DisplayName("Nombres y apellidos")]
        public string FullName { get; set; }

        [DisplayName("Tipo documento")]
        public string TypeDocId { get; set; }

        public List<SelectListItem> TypesDoc { get; set; }

        [DisplayName("Número documento")]
        public string NumberDoc { get; set; }

        [DisplayName("Correo electrónico")]
        public string Email { get; set; }

        [DisplayName("Contraseña")]
        public string Password { get; set; }

        [DisplayName("Perfil")]
        public int ProfileId { get; set; }

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