using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Models.FreeBiller
{
    public class UserFreeBillerModel
    {
        public int Id { get; set; }

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
    }
}