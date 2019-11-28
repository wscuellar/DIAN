using Gosocket.Dian.Web.Common;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gosocket.Dian.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class Authorization : AuthorizeAttribute
    {
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //base.OnAuthorization(filterContext);
            var user = filterContext.HttpContext.User;

            if (((System.Security.Claims.ClaimsPrincipal)HttpContext.Current.User).FindFirst(CustomClaimTypes.ContributorName) == null && !user.IsInAnyRole("Administrador", "Super"))
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                ReturnToLogin(filterContext);
                return;
            }

            if (((System.Security.Claims.ClaimsPrincipal)HttpContext.Current.User).FindFirst(CustomClaimTypes.UserFullName) == null && user.IsInAnyRole("Administrador", "Super"))
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                ReturnToLogin(filterContext);
                return;
            }

            if (!user.Identity.IsAuthenticated)
            {
                ReturnToLogin(filterContext);
                return;
            }

            //if (!AuthorizeCore(filterContext.HttpContext))
            //{
            //    ReturnUnauthorized(filterContext);
            //    return;
            //}
        }

        public void ReturnToLogin(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "Controller", "User"},
                    { "Action", "Login"}
                });
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