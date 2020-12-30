using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        public PermissionService(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public List<Menu> GetAppMenu(string RolName)
        {
            return _permissionRepository.GetAppMenu(RolName);
        }

        public int AddOrUpdate(List<Permission> permissionList)
        {
            return _permissionRepository.AddOrUpdate(permissionList);
        }

        public List<Permission> GetPermissionsByUser(string userId)
        {
            return _permissionRepository.GetPermissionsByUser(userId);
        }

        public List<SubMenu> GetSubMenusByMenuId(int menuId, string RolName)
        {
            return _permissionRepository.GetSubMenusByMenuId(menuId, RolName);
        }
    }
}