using Gosocket.Dian.Domain.Sql;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IPermissionRepository
    {
        List<Menu> GetAppMenu();

        int AddOrUpdate(List<Permission> permissionList);

        List<Permission> GetPermissionsByUser(string userId);
    }
}
