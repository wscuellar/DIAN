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

        [DisplayName("Nombres")]
        public string Nombre { get; set; }

        [DisplayName("Apellidos")]
        public string Apellido { get; set; }

        [DisplayName("Nombres y apellidos")]
        public string NombreCopleto { get; set; }

        [DisplayName("Tipo documento")]
        public string TipoDocId { get; set; }

        public List<SelectListItem> TiposDoc { get; set; }

        [DisplayName("Número documento")]
        public string Numerodoc { get; set; }

        [DisplayName("Correo electrónico")]
        public string Email { get; set; }

        [DisplayName("Contraseña")]
        public string Password { get; set; }

        [DisplayName("Perfil")]
        public int PerfilId { get; set; }

        public List<SelectListItem> Perfiles { get; set; }
    }
}