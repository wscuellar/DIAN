using System.Security.Claims;
using System.Security.Principal;

namespace Gosocket.Dian.Web.Services.Validator
{
    public static class PrincipalExtensions
    {
        public static string AuthCode(this IPrincipal principal)
        {
            ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)principal;
            if (claimsPrincipal?.FindFirst(CustomClaimTypes.AuthCode) == null) return null;
            return claimsPrincipal?.FindFirst(CustomClaimTypes.AuthCode).Value;
        }

        public static string AuthEmail(this IPrincipal principal)
        {
            ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)principal;
            if (claimsPrincipal?.FindFirst(CustomClaimTypes.AuthEmail) == null) return null;
            return claimsPrincipal?.FindFirst(CustomClaimTypes.AuthEmail).Value;
        }
    }
}