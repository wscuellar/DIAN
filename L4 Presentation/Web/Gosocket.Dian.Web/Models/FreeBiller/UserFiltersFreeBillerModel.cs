
namespace Gosocket.Dian.Web.Models.FreeBiller
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.Mvc;

    public class UserFiltersFreeBillerModel
    {
        public UserFiltersFreeBillerModel()
        {
            this.Page = 1;
            this.PageSize = 10;
        }

        [DisplayName("Tipo Documento")]
        public int DocTypeId { get; set; }

        [DisplayName("Número Documento")]
        public string DocNumber { get; set; }

        [DisplayName("Nombre Completo")]
        public string FullName { get; set; }

        [DisplayName("Estado")]
        public int ProfileId { get; set; }

        public List<SelectListItem> DocTypes { get; set; }

        public List<SelectListItem> Profiles { get; set; }

        public UserFreeBillerContainerModel Users { get; set; }


        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}