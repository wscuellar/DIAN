using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {

        public List<Menu> GetAppMenu()
        {
            List<Menu> list = null;

            try
            {
                using (var context = new SqlDBContext())
                {
                    list = context.Menus.ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PermissionRepository:GetAppMenu: " + ex);
            }

            return list;
        }

        public List<SubMenu> GetSubMenusByMenuId(int menuId)
        {
            List<SubMenu> list = null;

            try
            {
                using (var context = new SqlDBContext())
                {
                    list = context.SubMenus.Where(s => s.MenuId == menuId).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PermissionRepository:GetSubMenus: " + ex);
            }

            return list;
        }

        public int AddOrUpdate(List<Permission> permissionList)
        {
            int result = 0;
            string userId = permissionList.ElementAt(0).UserId;

            try
            {
                using (var context = new SqlDBContext())
                {
                    if (context.Permissions.Count() > 0)
                    {
                        var permissions = context.Permissions.Where<Permission>(p => p.UserId == userId);

                        if (permissions != null)
                        {
                            foreach (var item in permissions)
                            {
                                //((Permission)item).State = System.Data.Entity.EntityState.Deleted.ToString();
                                //((Permission)item).UpdatedBy = System.Data.Entity.EntityState.Deleted;
                                item.State = System.Data.Entity.EntityState.Deleted.ToString();
                                item.UpdatedBy = permissionList.ElementAt(0).UpdatedBy;
                            }

                            int ru = context.SaveChanges();
                            if (ru > 0)
                            {
                                //Si la actualizacón fue exitosa, eliminar los aneriores
                                context.Permissions.RemoveRange(context.Permissions.Where(p => p.State == System.Data.Entity.EntityState.Deleted.ToString()));
                                result = context.SaveChanges();
                            }
                        }
                        else
                        {
                            context.Permissions.AddRange(permissionList);
                            result = context.SaveChanges();
                        }
                    }
                    else
                    {
                        context.Permissions.AddRange(permissionList);
                        result = context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                result = -1;
                System.Diagnostics.Debug.WriteLine("PermissionRepository:AddOrUpdate: " + ex);
            }

            return result;
        }

        public List<Permission> GetPermissionsByUser(string userId)
        {
            List<Permission> list = null;

            try
            {
                using (var context = new SqlDBContext())
                {
                    list = context.Permissions.Where<Permission>(p => p.UserId == userId).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PermissionRepository:GetPermissionsByUser: " + ex);
            }

            return list;
        }

    }
}
