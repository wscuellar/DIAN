using Gosocket.Dian.Web.Common;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gosocket.Dian.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomContributorAuthorization : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext.User;
            if (!user.IsInAnyRole("Administrador", "Super"))
            {
                int.TryParse(HttpContext.Current.Request.QueryString["id"], out int id);
                if (user.ContributorId() != id)
                    ReturnUnauthorized(filterContext);
            }
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