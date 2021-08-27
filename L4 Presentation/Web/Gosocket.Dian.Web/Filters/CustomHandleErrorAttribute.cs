using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Filters
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        private static readonly TableManager tableManager = new TableManager("GlobalLogger");
        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="filterContext">The action-filter context.</param>
        /// <remarks>
        /// <author> YMO /José Yadiel Montalvan Horta (Gosocket)
        /// <rol>Ing.Analista</rol>
        /// </author>
        /// <version>0A YMO</version>
        /// <datetime> 3/6/2014 - 11:05 AM</datetime>
        /// </remarks>
        public override void OnException(ExceptionContext filterContext)
        {
            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
                return;


            var userEmail = "";

            var routeData = "RouteData: " + ToDebugString(filterContext.RouteData.Values) + "\r\n" +
                            "FormParameters:" + ToDebugString(filterContext.HttpContext.Request.Form) + "\r\n" +
                            "User: " + userEmail + "\r\n" +
                            "IP: " + GetDireccionIp(filterContext.RequestContext.HttpContext.Request) + "\r\n" +
                            "Browser: " + filterContext.RequestContext.HttpContext.Request.UserAgent + "\r\n" +
                            "Header: " + filterContext.RequestContext.HttpContext.Request.Headers;


            var controllerName = (string)filterContext.RouteData.Values["controller"];
            var actionName = (string)filterContext.RouteData.Values["action"];

            var requestId = Guid.NewGuid();

            HttpContext.Current.Session["RequestId"] = requestId;

            //TrelloUtils trelloUtils = new TrelloUtils();
            //trelloUtils.AddCardAsync("gtpa-dian", "Errors", controllerName + " - " + requestId, filterContext.Exception.Message + "\r\n" + filterContext.Exception.StackTrace);

            var logger = new GlobalLogger(requestId.ToString(), requestId.ToString())
            {
                Action = actionName,
                Controller = controllerName,
                Message = filterContext.Exception.Message,
                RouteData = routeData,
                StackTrace = filterContext.Exception.StackTrace
            };
            RegisterException(logger);

            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                ReturnJsonError(filterContext);
            }
            else
            {
                ReturnViewError(filterContext);
            }

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

        private void ReturnJsonError(ExceptionContext filterContext)
        {
            filterContext.Result = new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    error = true,
                    message = filterContext.Exception.Message
                }
            };
        }

        private void ReturnViewError(ExceptionContext filterContext)
        {
            var controllerName = (string)filterContext.RouteData.Values["controller"];
            var actionName = (string)filterContext.RouteData.Values["action"];
            var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);

            filterContext.Result = new ViewResult
            {
                ViewName = View,
                MasterName = Master,
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                TempData = filterContext.Controller.TempData
            };
        }

        private static string ToDebugString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key.ToString() + "=" + kv.Value.ToString()).ToArray()) + "}";
        }

        private static string ToDebugString(NameValueCollection collection)
        {
            return "{" + string.Join(",", collection.AllKeys.Select(k => k + "=" + collection[k]).ToArray()) + "}";
        }

        /// <summary>
        /// Recupera la dirección ip del visitante de la web
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>IP del usuario</returns>
        private static string GetDireccionIp(HttpRequestBase request)
        {
            // Recuperamos la IP de la máquina del cliente
            // Primero comprobamos si se accede desde un proxy
            var ipAddress1 = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            // Acceso desde una máquina particular
            var ipAddress2 = request.ServerVariables["REMOTE_ADDR"];

            var ipAddress = string.IsNullOrEmpty(ipAddress1) ? ipAddress2 : ipAddress1;

            // Devolvemos la ip
            return ipAddress;
        }

        private void RegisterException(GlobalLogger logger)
        {
            
            tableManager.InsertOrUpdate(logger);
        }
    }
}