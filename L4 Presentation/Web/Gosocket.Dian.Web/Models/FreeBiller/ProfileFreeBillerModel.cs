
namespace Gosocket.Dian.Web.Models.FreeBiller
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public class ProfileFreeBillerModel
    {

        public int ProfileId { get; set; }

        [DisplayName("Perfil")]
        public string Name { get; set; }

        public List<MenuOptionsModel> MenuOptionsByProfile { get; set; }
    }
}