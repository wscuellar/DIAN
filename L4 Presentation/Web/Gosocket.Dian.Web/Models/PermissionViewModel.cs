namespace Gosocket.Dian.Web.Models
{
    public class PermissionViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Id del Usuario de la tabla AspNetUsers
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Menu al que pertenece el SubMenu
        /// </summary>
        public int MenuId { get; set; }
        /// <summary>
        /// Id de la opción de Menu a la cual se tendra acceso
        /// </summary>
        public int SubMenuId { get; set; }
        public string State { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}