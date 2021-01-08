using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Web.Models;
using Microsoft.AspNet.Identity;

namespace Gosocket.Dian.TestProject2.Controllers.FreeBiller
{
    public class FakeRepository
    {
        public ApplicationUser GetCompany(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            else
            {
                return new ApplicationUser
                {
                    Code = "999999999",
                };
            }
        }

        public ApplicationUser FindUserByIdentificationAndTypeId(int typeId, string numberId) 
        {
            return new ApplicationUser
            {
                IdentificationTypeId = typeId,
                Code = numberId,
            };
        }

        public IdentityResult CreateFailing(ApplicationUser applicationUser, string password) 
        {
            return new IdentityResult(new string[] { "Error simulado" });
        }

        public IdentityResult CreateOk(ApplicationUser applicationUser, string password) 
        {
            return new IdentityResult();
        }

        public IdentityResult CreateFail(ApplicationUser applicationUser, string password)
        {
            return new IdentityResult(new string[] { "Error simulado" });
        }

        public IdentityResult AddToRoleOk(string id, string role)
        {
            return new IdentityResult();
        }

        public IdentityResult AddToRoleFail(string id, string role)
        {
            return new IdentityResult(new string[] { "Error simulado" });
        }
    }
}
