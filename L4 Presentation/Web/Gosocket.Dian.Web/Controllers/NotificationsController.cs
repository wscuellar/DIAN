using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    public class NotificationsController : Controller
    {
        private ApplicationUserManager _userManager;
        // GET: Notifications
        public ActionResult Index()
        {
            return View();
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ActionResult EventNotifications(String type, NotificationEntity data)
        {
            RequestNotification notification = new RequestNotification();

            var id = User.Identity.GetUserId();

            ApplicationUser accountCoManager = UserManager.FindById(id);

            string serie = " " + data.Description + "" + data.State + "";
            string url = data.Description;
            string docType = data.Subject;
            var date = data.CreationDateTime.ToString("dd/MM/yyyy");

            switch (type)
            {
                case "030":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa que se generó un “Acuse de Recibió” para la " + docType + " n° " + serie + " del " + date + " del cliente " + accountCoManager.Name + "";
                    notification.Description = "Se generó un “Acuse de Recibo”, para la " + docType + " n° " + serie + " del " + date + "  del cliente " + accountCoManager + "";
                    notification.Subject = "Acuse de Recibo";
                    notification.Matters = "Acuse de Recibo";
                    notification.NotificationId = 2;
                    notification.NoticationType = 6;
                    break;
                case "031":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa que se generó un “Reclamo” para la " + docType + " n° " + serie + " del " + date + " del cliente " + accountCoManager.Name + "Concepto de reclamo: ";
                    notification.Description = "Se generó un “Reclamo”, para la " + docType + " n° " + serie + " del " + date + "  del cliente " + accountCoManager.Name + " Concepto de Reclamo";
                    notification.Subject = "Reclamo";
                    notification.Matters = "Reclamo";
                    notification.NotificationId = 2;
                    notification.NoticationType = 6;
                    break;
                case "032":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa notificación de “Recibo del bien y prestación del servicio” para la " + docType + " n° " + serie + " del " + date + " del cliente " + accountCoManager.Name + "";
                    notification.Description = "Notificación de “Recibo del bien y/o prestación del servicio”, para la " + docType + " n° " + serie + " del " + date + "  del cliente " + accountCoManager.Name + "";
                    notification.Subject = "Recibo del bien y prestación del servicio";
                    notification.Matters = "Recibo del bien y prestación del servicio";
                    notification.NotificationId = 2;
                    notification.NoticationType = 6;
                    break;
                case "033":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa que se generó “Aceptacion Expresa” para la " + docType + " n° " + serie + " del " + date + " del cliente " + accountCoManager.Name + "<br>" +
                        " Una vez generado un evento de aceptación(Expresa o Tácita) NO se pueden generar notas crédito o notas débito a las facturas electrónicas. ";
                    notification.Description = "Se generó “Aceptación Expresa”, para la " + docType + " n° " + serie + " del " + date + "  del cliente " + accountCoManager.Name + "<br>" +
                        "Una vez generado un evento de aceptación (Expresa o Tácita) NO se pueden generar notas crédito o notas débito a las facturas electrónicas.";
                    notification.Subject = "Aceptación Expresa";
                    notification.Matters = "Aceptación Expresa";
                    notification.NotificationId = 2;
                    notification.NoticationType = 6;
                    break;
                case "034":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa que se generó una “Aceptación tácita” para la " + docType + " n° " + serie + " del " + date + " del cliente " + accountCoManager.Name +
                        " dado que ya genero un acuse de recibo y recibo del bien  <br> como no ha generado a la aceptación expresa y trascurridos los 3 días hábiles se generó la Aceptación tácita. <br>  <br>  " +
                        "Según el “Decreto 1074 del 2015 artículo 2.2.2.5.4. Aceptación de la factura electrónica de venta como título valor”  <br>  <br>  " +
                        "Una vez generado un evento de aceptación (Expresa o Tácita) NO se pueden generar notas crédito o notas débito a las facturas electrónicas.";
                    notification.Description = "Se informa que se generó una “Aceptación tácita” para la " + docType + " n° " + serie + " del " + date + "  del cliente " + accountCoManager.Name +
                        " dado que ya genero un acuse de recibo y recibo del bien <br> como no ha generado a la aceptación expresa y trascurridos los 3 días hábiles se generó la Aceptación tácita.  <br>  <br> " +
                        "Una vez generado un evento de aceptación (Expresa o Tácita) NO se pueden generar notas crédito o notas débito a las facturas electrónicas.";
                    notification.Subject = "Aceptación tácita";
                    notification.Matters = "Aceptación tácita";
                    notification.NotificationId = 2;
                    notification.NoticationType = 6;
                    break;
                case "035":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa que solicito la descarga del listado de …(Compradores / Vendedores/ Trabajadores / Documentos emitidos/Documentos Recibidos)" +
                        "esta solicitud tomara un <br> tiempo, cuando se realice la descarga se notificará";
                    notification.Description = "Se generó una solicitud de descarga esta solicitud tomará un tiempo, cuando se realice la descarga se notificará.";
                    notification.Subject = "Descarga de Listados";
                    notification.Matters = "Descarga de Listados";
                    notification.NotificationId = 1;
                    notification.NoticationType = 6;
                    break;
                case "036":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa que la descarga solicitada ya se encuentra lista, por favor consultar el adjunto Link adjunto. <br>" + url + "";
                    notification.Description = "Se informa que la descarga solicitada ya se encuentra lista, por favor consultar el adjunto Link adjunto. <br>" + url + "";
                    notification.Subject = "Descarga de Listados";
                    notification.Matters = "Descarga de Listados";
                    notification.NotificationId = 1;
                    notification.NoticationType = 6;
                    break;
                case "037":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa que se encuentra procesando la carga del listado de …(Compradores / Vendedores/ Trabajadores) esta solicitud tomara un tiempo, cuando se realice la carga se notificará.";
                    notification.Description = "Su carga se encuentra en proceso, esta solicitud tomará un tiempo, cuando se realice la descarga se notificará.";
                    notification.Subject = "Carga de Listados ";
                    notification.Matters = "Carga de Listados ";
                    notification.NotificationId = 6;
                    notification.NoticationType = 6;
                    break;
                case "038":
                    notification.Menssage = "Estimad@ usuari@ <br> Se informa que la carga del listado de …(Compradores / Vendedores/ Trabajadores), se cargó exitosamente.";
                    notification.Description = "Se informa que la carga del listado de …(Compradores / Vendedores/ Trabajadores), se cargó exitosamente.";
                    notification.Subject = "Carga de Listados ";
                    notification.Matters = "Carga de Listados ";
                    notification.NotificationId = 6;
                    notification.NoticationType = 6;
                    break;

            }

            notification.PetitionName = accountCoManager.Email;
            notification.UserName = accountCoManager.Code;
            notification.PartitionKey = accountCoManager.ContributorCode;
            notification.RecipientEmail = accountCoManager.Email;

            if (type == "036")
            {
                var insertNotification = ConfigurationManager.GetValue("InsertNotification");
                var clientNot = new RestClient(insertNotification);
                var requestNot = new RestRequest();
                requestNot.Method = Method.POST;
                requestNot.AddHeader("Content-Type", "application/json");
                requestNot.Parameters.Clear();
                requestNot.AddParameter("application/json", JsonConvert.SerializeObject(notification), ParameterType.RequestBody);
                var responseNot = clientNot.Execute(requestNot);
                Console.WriteLine(responseNot.Content);
            }
            else
            {
                var sendEmail = ConfigurationManager.GetValue("SendEmailFunction");
                var clienteEmail = new RestClient(sendEmail);
                var requestEmail = new RestRequest();
                requestEmail.Method = Method.POST;
                requestEmail.AddHeader("Content-Type", "application/json");
                requestEmail.Parameters.Clear();
                requestEmail.AddParameter("application/json", JsonConvert.SerializeObject(notification), ParameterType.RequestBody);
                var responsee = clienteEmail.Execute(requestEmail);
                Console.WriteLine(responsee.Content);

                var insertNotification = ConfigurationManager.GetValue("InsertNotification");
                var clientNot = new RestClient(insertNotification);
                var requestNot = new RestRequest();
                requestNot.Method = Method.POST;
                requestNot.AddHeader("Content-Type", "application/json");
                requestNot.Parameters.Clear();
                requestNot.AddParameter("application/json", JsonConvert.SerializeObject(notification), ParameterType.RequestBody);
                var responseNot = clientNot.Execute(requestNot);
                Console.WriteLine(responseNot.Content);

                var logNotification = ConfigurationManager.GetValue("LogNotification");
                var clientLog = new RestClient(logNotification);
                var requestLog = new RestRequest();
                requestLog.Method = Method.POST;
                requestLog.AddHeader("Content-Type", "application/json");
                requestLog.Parameters.Clear();
                requestLog.AddParameter("application/json", JsonConvert.SerializeObject(notification), ParameterType.RequestBody);
                var responseLog = clientLog.Execute(requestLog);
                Console.WriteLine(responseLog.Content);
            }
            return null;
        }

    }

    public class RequestNotification
    {
        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "Subject")]
        public string Subject { get; set; }
        [JsonProperty(PropertyName = "NoticationType")]
        public long NoticationType { get; set; }
        [JsonProperty(PropertyName = "NotificationId")]
        public long NotificationId { get; set; }
        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty(PropertyName = "RecipientEmail")]
        public string RecipientEmail { get; set; }

        [JsonProperty(PropertyName = "Menssage")]
        public string Menssage { get; set; }

        [JsonProperty(PropertyName = "Matters")]
        public string Matters { get; set; }

        [JsonProperty("UserName")]
        public string UserName { get; set; }

        [JsonProperty("PetitionName")]
        public string PetitionName { get; set; }
    }
}