using Gosocket.Dian.Domain.Sql;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IPermissionRepository
    {
        List<Menu> GetAppMenu(string rolsearch);

        int AddOrUpdate(List<Permission> permissionList);

        List<Permission> GetPermissionsByUser(string userId);
        List<SubMenu> GetSubMenusByMenuId(int menuId, string rolname);
    }
}
