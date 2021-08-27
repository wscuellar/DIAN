using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Utils;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.Web.Utils
{
    public class UserService
    {
        ApplicationDbContext _sqlDBContext;
        readonly SqlDBContext _sqlAspDBContext;
        private static readonly TableManager globalLogger = new TableManager("GlobalLogger");
        public UserService()
        {
            if (_sqlDBContext == null)
            {
                _sqlDBContext = new ApplicationDbContext();
            }
            if (_sqlAspDBContext == null)
            {
                _sqlAspDBContext = new SqlDBContext();
            }
        }

        #region RELACION DE USUARIO CON PERFIL PARA FACTURADOR GRATUITO.

        public List<ApplicationUser> UserFreeBillerProfile(Expression<Func<ApplicationUser, bool>> expression,  string companyCode, int profileId = 0)
        {
            var query = from pr in _sqlDBContext.UserFreeBillerProfile.Where(t=> t.CompanyCode == companyCode && ( profileId == 0 || t.ProfileFreeBillerId ==  profileId))
                        join u in _sqlDBContext.Users.Where(expression) on pr.UserId equals u.Id
                        select u;

            return query.ToList();
        }


        public int UserFreeBillerUpdate(UsersFreeBillerProfile usersFreeBiller)
        {
            UsersFreeBillerProfile usersInstance = _sqlDBContext.UserFreeBillerProfile.FirstOrDefault(c => c.UserId == usersFreeBiller.UserId && c.ProfileFreeBillerId == usersFreeBiller.ProfileFreeBillerId);

            if (usersInstance != null)
            {
                usersInstance.UserId = usersFreeBiller.UserId;
                usersInstance.ProfileFreeBillerId = usersFreeBiller.ProfileFreeBillerId;
                _sqlDBContext.Entry(usersInstance).State = System.Data.Entity.EntityState.Modified;
            }
            else
                _sqlDBContext.Entry(usersFreeBiller).State = System.Data.Entity.EntityState.Added;

            _sqlDBContext.SaveChanges();

            return usersInstance ==  null ? usersFreeBiller.Id : usersInstance.Id;
        }


        public int UserFreeBillerDeleteAll(string userId)
        {
            List<UsersFreeBillerProfile> usersInstance = _sqlDBContext.UserFreeBillerProfile.Where(c => c.UserId == userId).ToList();

            if (usersInstance != null)
            {
                foreach (var profileuser in usersInstance)
                {
                    _sqlDBContext.Entry (profileuser).State = System.Data.Entity.EntityState.Deleted;
                }
            }
            
            return _sqlDBContext.SaveChanges();
        }

        public UsersFreeBillerProfile GetUserFreeBillerProfile(Expression<Func<UsersFreeBillerProfile, bool>> expression)
        {
             return _sqlDBContext.UserFreeBillerProfile.FirstOrDefault(expression);
        }

        #region Actualizar Claims para usuarios de facturador gratuito

        public int UpdateUserClaim(ClaimsDb usersFreeBiller)
        {
            ClaimsDb usersInstance = _sqlAspDBContext.ClaimsDbs.FirstOrDefault(c => c.UserId.Equals(usersFreeBiller.UserId));

            if (usersInstance != null)
            {
                usersInstance.UserId = usersFreeBiller.UserId;
                usersInstance.ClaimValue = usersFreeBiller.ClaimValue.ToString();
                _sqlAspDBContext.Entry(usersInstance).State = System.Data.Entity.EntityState.Modified;
            }

            _sqlAspDBContext.SaveChanges();

            return usersInstance == null ? usersFreeBiller.Id : usersInstance.Id;
        }

        #endregion

        #region DeleteUserClaims

        public int DeleteUserClaims(string userId)
        {
            List<ClaimsDb> usersInstance = _sqlAspDBContext.ClaimsDbs.Where(c => c.UserId == userId).ToList();

            if (usersInstance != null)
            {
                foreach (var profileuser in usersInstance)
                {
                    _sqlAspDBContext.Entry(profileuser).State = System.Data.Entity.EntityState.Deleted;
                }
            }

            return _sqlAspDBContext.SaveChanges();
        }

        #endregion


        #endregion

        public IEnumerable<ApplicationUser> GetUsers(List<string> ids)
        {
            return _sqlDBContext.Users.Where(u => ids.Contains(u.Id));
        }
        public List<ApplicationUser> GetUsers(string code, int status, int page, int length)
        {
            //TODO Para revisar
            return new List<ApplicationUser>();
            var query = _sqlDBContext.Users.Where(c =>
                         (string.IsNullOrEmpty(code) || c.Code == code)
                         ).OrderByDescending(c => c.Code).Skip(page * length).Take(length);

            return query.ToList();
        }

        public List<ApplicationUser> GetUsersWithRoles(string email, int page, int length)
        {
            var query = _sqlDBContext.Users.Where(c =>
                        c.Roles.Count > 0 &&
                         (string.IsNullOrEmpty(email) || c.Email == email)
                         ).OrderByDescending(c => c.Email).Skip(page * length).Take(length);

            return query.ToList();
        }

        public ApplicationUser Get(string id)
        {
            return _sqlDBContext.Users.FirstOrDefault(u => u.Id == id);
        }

        public ApplicationUser GetByCode(string code)
        {
            return _sqlDBContext.Users.FirstOrDefault(u => u.Code == code);
        }

        public ApplicationUser GetByCodeAndIdentificationTyte(string code, int identificatioTypeId)
        {
            return _sqlDBContext.Users.FirstOrDefault(u => u.Code == code && u.IdentificationTypeId == identificatioTypeId);
        }


        public ApplicationUser GetByCodePasswordAndIdentificationTyte(string code, int identificatioTypeId, string password)
        {
            return _sqlDBContext.Users.FirstOrDefault(u => u.Code == code && u.IdentificationTypeId == identificatioTypeId && u.PasswordHash == password);
        }

        public ApplicationUser GetByEmail(string email)
        {
            return _sqlDBContext.Users.FirstOrDefault(u => u.Email == email);
        }

        public string AddOrUpdate(ApplicationUser user)
        {
            using (var context = new ApplicationDbContext())
            {
                var userInstance = context.Users.FirstOrDefault(c => c.Code == user.Code);

                if (userInstance != null)
                {
                    userInstance.IdentificationTypeId = user.IdentificationTypeId;
                    userInstance.Name = user.Name;
                    userInstance.Email = user.Email;
                    context.Entry(userInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(user).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();

                return userInstance != null ? userInstance.Id : user.Id;
            }
        }

        public string GetRolName(string id)
        {
            return _sqlDBContext.Roles.FirstOrDefault(r => r.Id == id)?.Name;
        }

        /// <summary>
        /// Activando o Inactivando al Usario externo
        /// </summary>
        /// <param name="userId">Id del Usuario externo a actualizar</param>
        /// <param name="active">1: Activar, 0: Inactivar</param>
        /// <param name="updatedBy">Usuario que realiza la acción</param>
        /// <param name="activeDescription">Motivo por el cual se realiza la acción</param>
        /// <returns></returns>
        public int UpdateActive(string userId, byte active, string updatedBy, string activeDescription)
        {
            int result = 0;
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);
                    if (user != null)
                    {
                        user.Active = active;
                        user.LastUpdated = DateTime.Now;
                        user.UpdatedBy = updatedBy;
                        user.ActiveDescription = activeDescription;
                        result = db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                result = -1;
                System.Diagnostics.Debug.WriteLine("UserService:UpdateActive: " + ex);
            }

            return result;
        }

        /// <summary>
        /// Actualizar los campos basicos del Usuario Externo
        /// </summary>
        /// <param name="user"><see cref="ExternalUserViewModel"/></param>
        /// <returns></returns>
        public int UpdateExternalUser(ExternalUserViewModel user)
        {
            int result = 0;
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userDB = db.Users.FirstOrDefault(u => u.Id == user.Id);
                    if (userDB != null)
                    {
                        userDB.IdentificationTypeId = user.IdentificationTypeId;
                        userDB.IdentificationId = user.IdentificationId;
                        userDB.Name = user.Names;
                        userDB.Email = user.Email;
                        userDB.LastUpdated = user.LastUpdated;
                        userDB.UpdatedBy = user.UpdatedBy;

                        result = db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                result = -1;
                System.Diagnostics.Debug.WriteLine("UserService:UpdateExternalUser: " + ex);
            }

            return result;
        }

        /// <summary>
        /// Registrar trazabilidad de Usuarios externos
        /// </summary>
        /// <param name="message">Datos del usuario</param>
        /// <param name="actionName">Crear,Actualizar, Asignar/Actualizar permisos</param>
        public void RegisterExternalUserTrazability(string message, string actionName)
        {
            var requestId = Guid.NewGuid();

            var logger = new GlobalLogger(requestId.ToString(), requestId.ToString())
            {
                Action = actionName,
                Controller = "ExternalUsers",
                Message = message,
                RouteData = null,
                StackTrace = null
            };

            globalLogger.InsertOrUpdate(logger);
        }

        public ApplicationUser FindUserByIdentificationAndTypeId(string Id, int identificationTypeId, string identificationId)
        {
            if (string.IsNullOrEmpty(Id))
                return _sqlDBContext.Users.FirstOrDefault(u => u.IdentificationTypeId == identificationTypeId && u.IdentificationId == identificationId);
            else
                return _sqlDBContext.Users.
                FirstOrDefault(u => u.IdentificationTypeId == identificationTypeId
                            && u.IdentificationId == identificationId
                            && u.Id != Id);
        }

        public ApplicationUser FindUserByEmail(string Id, string Email)
        {
            if (string.IsNullOrEmpty(Id))
                return _sqlDBContext.Users.FirstOrDefault(u => u.Email == Email);
            else
                return _sqlDBContext.Users.
                FirstOrDefault(u => u.Email == Email
                            && u.Id != Id);
        }


        /// <summary>
        /// Buscar/Listar los Usuarios Externos
        /// </summary>
        /// <param name="creatorNit">Nit de empresa o persona natural registrada en el Rut que Creo a los Usuarios</param>
        /// <param name="page">Número de pagina</param>
        /// <param name="length">Número re registros</param>
        /// <returns></returns>
        public List<ApplicationUser> GetExternalUsersPaginated(string creatorNit, int page, int length)
        {
            var query = _sqlDBContext.Users.Where(c =>
                         (string.IsNullOrEmpty(creatorNit) || c.CreatorNit == creatorNit)
                         ).OrderByDescending(c => c.CreationDate).Skip(page * length).Take(length);

            return query.ToList();
        }

    }
}