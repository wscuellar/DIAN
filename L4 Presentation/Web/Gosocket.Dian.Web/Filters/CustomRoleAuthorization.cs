using Gosocket.Dian.Domain.Common;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Gosocket.Dian.Web.Common;

namespace Gosocket.Dian.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomRoleAuthorization : AuthorizeAttribute
    {
        public string CustomRoles { get; set; }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext.User;

            var currentContributorTypeId = ((System.Security.Claims.ClaimsPrincipal)HttpContext.Current.User).FindFirst(CustomClaimTypes.ContributorTypeId);

            if (currentContributorTypeId != null && CustomRoles != null && !user.IsInAnyRole("Administrador", "Super"))
            {
                int contributorTypeId = 0;
                if (!string.IsNullOrEmpty(currentContributorTypeId.Value))
                    contributorTypeId = int.Parse(currentContributorTypeId.Value);
                var roleDescription = EnumHelper.GetEnumDescription((ContributorType)contributorTypeId);
                if (!CustomRoles.Contains(roleDescription))
                    ReturnUnauthorized(filterContext);
                return;
            }

            //if (user.IsInRole("Administrador") && CustomRoles.Contains("Administrador"))
            //    return;
            //if (user.IsInRole("Super") && CustomRoles.Contains("Super"))
            //    return;

            //if (user.IsInRole("Administrador") && CustomRoles.Contains("Super"))
            //    ReturnUnauthorized(filterContext);
        }

        public void ReturnUnauthorized(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "Controller", "User"},
                    { "Action", "Unauthorized"}
                });
        }
    }
}