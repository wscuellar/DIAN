﻿using Gosocket.Dian.Domain.Sql;
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
                        var permissions = context.Permissions.Where<Permission>(p => p.UserId == userId).ToList();

                        if (permissions != null)
                        {
                            foreach (var item in permissions)
                            {
                                //((Permission)item).State = System.Data.Entity.EntityState.Deleted.ToString();
                                //((Permission)item).UpdatedBy = System.Data.Entity.EntityState.Deleted;
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
                                    //Si la actualizacón fue exitosa, eliminar los aneriores
                                    //context.Permissions.RemoveRange(context.Permissions.Where(p => p.State == System.Data.Entity.EntityState.Deleted.ToString()).ToList());
                                    context.Permissions.RemoveRange(permissions);
                                    int rePerDeleted = context.SaveChanges();

                                }
                                else //si no fue exitoso la Actualización/Inserción de los nuevos permisos, quitar la marcación de Eliminados
                                {
                                    result = -2;//No se pudo actualizarInsertar los nuevos permisos

                                    foreach (var item in permissions)
                                    {
                                        item.State = null;
                                        item.UpdatedBy = permissionList.ElementAt(0).UpdatedBy;
                                    }

                                    int rollbackDelete = context.SaveChanges();

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