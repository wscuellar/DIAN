using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {

        public List<Menu> GetAppMenu(string rolsearch)
        {
            try
            {
                using (var context = new SqlDBContext())
                {
                    var query = (from m in context.Menus
                                 join mrol in context.MenuRoles on m.Id equals mrol.MenuId
                                 join rol in context.Roles on mrol.RoleId equals rol.Id
                                 where rol.Name == rolsearch && mrol.SubMenuId == null
                                 select new
                                 {
                                     m.Id,
                                     m.Name,
                                     m.Description,
                                     m.Title,
                                     m.Icon,
                                     mrol.Order
                                 }).Distinct().ToList()
                                 .Select(x => new Menu()
                                 {
                                     Id = x.Id,
                                     Name = x.Name,
                                     Description = x.Description,
                                     Title = x.Title,
                                     Icon = x.Icon,
                                     Order = x.Order
                                 }).OrderBy(x => x.Order).ToList();
                    return query;

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PermissionRepository:GetAppMenu: " + ex);
            }

            return new List<Menu>();
        }

        public List<SubMenu> GetSubMenusByMenuId(int menuId, string rolname)
        {
            try
            {
                using (var context = new SqlDBContext())
                {
                    var query = (from sm in context.SubMenus
                                 join mrol in context.MenuRoles on sm.Id equals mrol.SubMenuId
                                 join rol in context.Roles on mrol.RoleId equals rol.Id
                                 where rol.Name == rolname
                                 && sm.MenuId == menuId
                                 && sm.MenuId == mrol.MenuId
                                 select new
                                 {
                                     sm.Id,
                                     sm.MenuId,
                                     sm.Name,
                                     sm.Description,
                                     sm.Title,
                                     mrol.Order,
                                 }).Distinct().ToList()
                                 .Select(x => new SubMenu()
                                 {
                                     Id = x.Id,
                                     MenuId = x.MenuId,
                                     Name = x.Name,
                                     Description = x.Description,
                                     Title = x.Title,
                                     Order = x.Order
                                 }).OrderBy(x => x.Order).ToList();

                    return query;

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PermissionRepository:GetSubMenus: " + ex);
            }

            return new List<SubMenu>();
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
                        var permissions = context.Permissions.Where<Permission>(p => p.UserId == userId).ToList();

                        if (permissions != null)
                        {
                            foreach (var item in permissions)
                            {
                                item.State = System.Data.Entity.EntityState.Deleted.ToString();
                                item.UpdatedBy = permissionList.ElementAt(0).UpdatedBy;
                            }

                            int ru = context.SaveChanges();//se marcan para eliminar los permisos anteriores
                            if (ru >= 0)
                            {
                                //Insertar los nuevos perrmisoss
                                context.Permissions.AddRange(permissionList);
                                result = context.SaveChanges();

                                if (result > 0)//Inserto los nuevos permisos exitosamente
                                {
                                    context.Permissions.RemoveRange(permissions);
                                    context.SaveChanges();
                                }
                                else //si no fue exitoso la Actualización/Inserción de los nuevos permisos, quitar la marcación de Eliminados
                                {
                                    result = -2;//No se pudo actualizarInsertar los nuevos permisos

                                    foreach (var item in permissions)
                                    {
                                        item.State = null;
                                        item.UpdatedBy = permissionList.ElementAt(0).UpdatedBy;
                                    }
                                    context.SaveChanges();
                                }
                            }
                            else
                                result = -2;//no se pudo marcar para eliminar los permisos actuales
                        }
                        else //el Usuario actualmente no tiene permisos asignados. Entonces Insertar los nuevos perimisos
                        {
                            context.Permissions.AddRange(permissionList);
                            result = context.SaveChanges();
                        }
                    }
                    else //no hay ningunos permisos asignados en la tabla de la BD
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