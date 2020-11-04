using Gosocket.Dian.Web.App_Start;
using Gosocket.Dian.Web.Filters;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Gosocket.Dian.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            UnityConfig.RegisterComponents();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            IFilterProvider[] providers = FilterProviders.Providers.ToArray();
            FilterProviders.Providers.Clear();
            FilterProviders.Providers.Add(new ExcludeFilterProvider(providers));

            MvcHandler.DisableMvcResponseHeader = true;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (sender is HttpApplication app && app.Context != null)
                app.Context.Response.Headers.Remove("Server");
            HttpContext.Current.Response.AddHeader("P3P", "CP=\"NOI CURa ADMa DEVa TAIa OUR BUS IND UNI COM NAV INT\"");
        }
    }
}
