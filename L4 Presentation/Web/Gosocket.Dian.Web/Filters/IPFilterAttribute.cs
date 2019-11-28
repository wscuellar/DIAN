using Gosocket.Dian.Infrastructure;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gosocket.Dian.Web.Filters
{
    public class IPFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Retrieve user IP
            string userIpAddress = GetIP();

            if (!CheckIp(userIpAddress))
                ReturnUnauthorized(filterContext);

            base.OnActionExecuting(filterContext);
        }

        public static bool CheckIp(string userIpAddress)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.GetValue("IpAccess"))) return true;
            bool.TryParse(ConfigurationManager.GetValue("IpAccess"), out bool ipAccess);
            if (!ipAccess) return true;

            //get allowedIps Setting from Web.Config file and remove whitespaces from int
            string allowedIps = ConfigurationManager.GetValue("AllowedIPs").Replace(" ", "").Trim();

            //convert allowedIPs string to table by exploding string with ';' delimiter
            string[] ips = allowedIps.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            //iterate ips table
            if (ips.Contains(userIpAddress)) return true;

            //if we get to this point, that means that user's address is not allowed, therefore returning false
            return false;
        }

        public static string GetIP()
        {
            string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            return ip.Split(':').FirstOrDefault();
        }

        public void ReturnUnauthorized(ActionExecutingContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "Controller", "Error"},
                    { "Action", "Unauthorized"}
                });
        }
    }
}