
namespace Gosocket.Dian.Web.Models.FreeBiller
{
    using Gosocket.Dian.Domain.Sql.FreeBiller;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class ProfileFreeBillerModel
    {

        public int ProfileId { get; set; }

        [Required]
        [DisplayName("Perfil")]
        public string Name { get; set; }

        public List<MenuOptions> MenuOptionsByProfile { get; set; }

        public string[] ValuesSelected { get; set; }
    }
}