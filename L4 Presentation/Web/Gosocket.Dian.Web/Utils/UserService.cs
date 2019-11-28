using Gosocket.Dian.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Web.Utils
{
    public class UserService
    {
        ApplicationDbContext _sqlDBContext;
        public UserService()
        {
            if (_sqlDBContext == null)
            {
                _sqlDBContext = new ApplicationDbContext();
            }
        }

        public IEnumerable<ApplicationUser> GetUsers(List<string> ids)
        {
            return _sqlDBContext.Users.Where(u => ids.Contains(u.Id));
        }
        public List<ApplicationUser> GetUsers(string code, int status, int page, int length)
        {
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

    }
}