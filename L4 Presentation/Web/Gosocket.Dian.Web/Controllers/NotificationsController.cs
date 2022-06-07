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
using Gosocket.Dian.Application;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Domain;
using Microsoft.WindowsAzure.Storage;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Services.Utils.Helpers;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Web.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ContributorService contributorService = new ContributorService();
        private int operationModeId = 0;
        // GET: Notifications
        public ActionResult Index() 
        {
            return View();
        }
        public NotificationsController()
        {
        }
        public async Task<ActionResult> EventNotificationsAsync(string type, string id)
        {
            RequestNotification notification = new RequestNotification();
            var accountId = await ApiHelpers.ExecuteRequestAsync<string>(ConfigurationManager.GetValue("AccountByNit"), new { Nit = id });
            Contributor contributor = contributorService.GetByCode(id);
            var business = contributor.Name;
            notification.PetitionName = contributor.ExchangeEmail;
            notification.UserName = contributor.Code;
            notification.PartitionKey = accountId.ToString();
            notification.RecipientEmail = contributor.ExchangeEmail;

            string []modosOperacion = new string[]{"", "Solución Gratuita", "Software Propio", "Proveedor tecnológico" };

            switch (type)
            {
                case "01":
                    notification.Description = "Su certificado digital se encuentra cargado correctamente en producción.";
                    notification.Subject = "Traslado de certificado digital al ambiente de Producción";
                    notification.NotificationId = 5;
                    notification.NoticationType = 6;
                    SentEventNotification(notification);
                    notification.Description = "Recuerde asociar y/o crear la numeración -Ruta Configuración/Rangos de numeración";
                    notification.Subject = "Solicitud de Numeración";
                    notification.NotificationId = 5;
                    notification.NoticationType = 3;
                    SentEventNotification(notification);
                    notification.Description = "Por favor crear sus insumos (Compradores/Vendedores/Trabajadores/Productos y Servicios). - Ruta Configuración.  N  trabajadores Nomina";
                    notification.Subject = "Creación de Insumos";
                    notification.NotificationId = 5;
                    notification.NoticationType = 4;
                    SentEventNotification(notification);

                    break;
                case "02":
                    notification.Menssage = "Estimad@ usuari@ Nombre de la Empresa "+ business + "<br>" +
                            " Se informa que se encuentra en el proceso de pruebas de validación y su set de pruebas se encuentra rechazado, si desea continuar con el proceso deberá dirigirse a la pantalla configurar modos de operación y Reiniciarlo." +
                            " Saludes cordiales.<br><br><br><br><br>";
                    notification.Description = "Su set de pruebas se encuentra rechazado, si desea continuar con el proceso deberá dirigirse a la pantalla configurar modos de operación y allí encontrará el botón para Reiniciarlo.";
                    notification.Subject = "Rechazo del Set de pruebas ";
                    notification.Matters = "Rechazo del Set de pruebas";
                    notification.NotificationId = 4;
                    notification.NoticationType = 1;
                    SentEvent(notification);
                    break;
                case "03":
                    notification.Menssage = "Estimad@ usuari@ Nombre de la Empresa " + business + "<br>" +
                        " Se informa que ha finalizado el proceso de pruebas y actualmente se encuentra en estado habilitado <br>" +
                        " Por favor tener en cuenta las siguientes indicaciones al ingresar a producción. <br>" +
                        " 1.         Ingresar desde la opción “Facturando Electrónicamente” <br>" +
                        " 2.         Verificar que su certificado digital se encuentre cargado. <br>" +
                        " 3.         Asociar y crear la numeración necesaria. <br>" +
                        " 4.         Cada vez que configure un modo de operación en producción deberá crear nuevamente sus insumos (Compradores / Vendedores / Trabajadores /Productos y Servicios). <br>" +
                        " Saludes cordiales <br><br><br><br><br>";
                    notification.Description = "Se encuentra Habilitado para generar… Nomina/Factura/D.S. para elaborar sus documentos desde Producción deberá ingresar desde la opción “Facturando Electrónicamente” ";
                    notification.Subject = "Aprobación del set de pruebas ";
                    notification.Matters = "Aprobación del set de pruebas ";
                    notification.NotificationId = 2;
                    notification.NoticationType = 2;
                    SentEvent(notification);
                    break;
                case "04":
                    notification.Menssage = "Estimad@ usuari@ Nombre de la Empresa " + business + " Se informa que se encuentra registrado y ha seleccionado el modo de operación software <strong>" + modosOperacion[operationModeId] + "</strong>, se recuerda que debe surtir el proceso de pruebas para finalizar la habilitación. <br><br><br><br><br>";
                    notification.Matters = "Inicio Modo de Operación";
                    SentEmail(notification);
                    break;        

            }
            return null;
        }

        public JsonResult SentEmail(RequestNotification notification)
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
            return null;
        }

        public JsonResult SentEvent(RequestNotification notification)
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

            return null;
        }
        public JsonResult SentEventNotification(RequestNotification notification)
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

            var logNotification = ConfigurationManager.GetValue("LogNotification");
            var clientLog = new RestClient(logNotification);
            var requestLog = new RestRequest();
            requestLog.Method = Method.POST;
            requestLog.AddHeader("Content-Type", "application/json");
            requestLog.Parameters.Clear();
            requestLog.AddParameter("application/json", JsonConvert.SerializeObject(notification), ParameterType.RequestBody);
            var responseLog = clientLog.Execute(requestLog);
            Console.WriteLine(responseLog.Content);

            return null;
        }

        public async Task<ActionResult> EventNotificationsAsyncOperationMode(string type, string id, int operationModeId)
        {
            this.operationModeId = operationModeId;

            this.EventNotificationsAsync(type, id);
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