using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Filters
{
    public class P3PAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext.Current.Response.AddHeader("P3P", "CP=\"C2 IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT CAO CURa ADMa DEVa CONo HISa OUR IND DSP ALL COR NOI ADM DEV COM NAV OUR STP\"");
            //HttpContext.Current.Response.AddHeader("p3p", "CP=\"C2 NOI CURa ADMa DEVa TAIa OUR BUS IND UNI COM NAV INT\"");
            base.OnActionExecuting(filterContext);
        }
    }
}