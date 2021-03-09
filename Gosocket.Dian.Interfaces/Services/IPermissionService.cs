using Gosocket.Dian.Domain.Sql;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IPermissionService
    {
        /// <summary>
        /// Obtener All el Menu de la aplicación
        /// </summary>
        /// <returns></returns>
        List<Menu> GetAppMenu(string RolName);

        /// <summary>
        /// Agregar o actualizar los permisos por Usuario externo
        /// </summary>
        /// <param name="permissionList"></param>
        /// <returns></returns>
        int AddOrUpdate(List<Permission> permissionList);
        List<Permission> GetPermissionsByUser(string userId);
        List<SubMenu> GetSubMenusByMenuId(int menuId, string RolName);
    }
}
