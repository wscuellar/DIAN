using Gosocket.Dian.Web.Filters;
using System.Web.Mvc;

namespace Gosocket.Dian.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute() { View = "Error" });
            //filters.Add(new IPFilterAttribute());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
