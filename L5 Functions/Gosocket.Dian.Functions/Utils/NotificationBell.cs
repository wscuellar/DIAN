using Gosocket.Dian.Infrastructure;
using Newtonsoft.Json;
using System;
using RestSharp;
using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Services.Utils.Helpers;
using System.Collections.Generic;

namespace Gosocket.Dian.Functions.Utils
{
    class NotificationBell
    {
        private static readonly ContributorService contributorService = new ContributorService();

        public NotificationBell() { }

        public async void EventNotificationsAsync(string type, string id)
        {
            RequestNotification notification = new RequestNotification();
            RequestNotification notificationProd = new RequestNotification();
            var accountId = await ApiHelpers.ExecuteRequestAsync<string>(ConfigurationManager.GetValue("AccountByNit"), new { Nit = id });
            Contributor contributor = contributorService.GetByCode(id);
            var business = contributor.Name;
            notification.PetitionName = contributor.ExchangeEmail;
            notification.UserName = contributor.Code;
            notification.PartitionKey = accountId.ToString();
            notification.RecipientEmail = contributor.ExchangeEmail;

            switch (type)
            {
                case "02":
                    notification.Menssage = "Estimad@ usuari@ Nombre de la Empresa " + business + "<br>" +
                            " Se informa que se encuentra en el proceso de pruebas de validación y su set de pruebas se encuentra rechazado, si desea continuar con el proceso deberá dirigirse a la pantalla configurar modos de operación y Reiniciarlo.<br>" +
                            " Saludes cordiales.<br><br>";
                    notification.Description = "Su set de pruebas se encuentra rechazado, si desea continuar con el proceso deberá dirigirse a la pantalla configurar modos de operación y allí encontrará el botón para Reiniciarlo.";
                    notification.Subject = "Rechazo del Set de pruebas ";
                    notification.Matters = "Rechazo del Set de pruebas";
                    notification.NotificationId = 4;
                    notification.NoticationType = 1;                    
                    break;
                case "03":
                    notification.Menssage = "Estimad@ usuari@ Nombre de la Empresa " + business + "<br>" +
                        " Se informa que ha finalizado el proceso de pruebas y actualmente se encuentra en estado habilitado <br>" +
                        " Por favor tener en cuenta las siguientes indicaciones al ingresar a producción. <br>" +
                        " 1.         Ingresar desde la opción “Facturando Electrónicamente” <br>" +
                        " 2.         Verificar que su certificado digital se encuentre cargado. <br>" +
                        " 3.         Asociar y crear la numeración necesaria. <br>" +
                        " 4.         Cada vez que configure un modo de operación en producción deberá crear nuevamente sus insumos (Compradores/Vendedores/Trabajadores/Productos y Servicios). <br>" +
                        " Saludes cordiales.<br><br> ";
                    notification.Description = "Se encuentra Habilitado para generar… Factura. para elaborar sus documentos desde Producción deberá ingresar desde la opción “Facturando Electrónicamente”. ";
                    notification.Subject = "Aprobación del set de pruebas ";
                    notification.Matters = "Aprobación del set de pruebas ";
                    notification.NotificationId = 2;
                    notification.NoticationType = 2;

                    notificationProd.Menssage = "1. Su certificado digital se encuentra cargado correctamente en producción.<br><br>" +
                        "2.  Recuerde asociar y/o crear la numeración -Ruta Configuración/Rangos de numeración.<br><br>" +
                        "3. Por favor crear sus insumos (Compradores/Vendedores/Trabajadores/Productos y Servicios). - Ruta Configuración.<br><br>";
                    notificationProd.Description = "1. Su certificado digital se encuentra cargado correctamente en producción.<br><br>" +
                        "2.  Recuerde asociar y/o crear la numeración -Ruta Configuración/Rangos de numeración.<br><br>" +
                        "3. Por favor crear sus insumos (Compradores/Vendedores/Trabajadores/Productos y Servicios). - Ruta Configuración.<br><br>";
                    notificationProd.Subject = "Creación de Insumos";
                    notificationProd.Matters = "Creación de Insumos";
                    notificationProd.NotificationId = 2;
                    notificationProd.NoticationType = 2;
                    notificationProd.PetitionName = contributor.Email;
                    notificationProd.UserName = contributor.Code;
                    notificationProd.PartitionKey = accountId;
                    notificationProd.RecipientEmail = contributor.Email;               
                    
                    break;
            }
            notification.PetitionName = contributor.Email;
            notification.UserName = contributor.Code;
            notification.PartitionKey = accountId;
            notification.RecipientEmail = contributor.Email;

            SentEvent(notification);

            if ( type.Equals("03")) { 
                SentEventProd(notificationProd);
            }
        }


        public void SentEvent(RequestNotification notification)
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

        public void SentEventProd(RequestNotification notificacionprod)
        {
            var insertNotification = ConfigurationManager.GetValue("InsertNotificationProd");
            var clientNot = new RestClient(insertNotification);
            var requestNot = new RestRequest();
            requestNot.Method = Method.POST;
            requestNot.AddHeader("Content-Type", "application/json");
            requestNot.Parameters.Clear();
            requestNot.AddParameter("application/json", JsonConvert.SerializeObject(notificacionprod), ParameterType.RequestBody);
            var responseNot = clientNot.Execute(requestNot);
            Console.WriteLine(responseNot.Content);

            var logNotification = ConfigurationManager.GetValue("LogNotification");
            var clientLog = new RestClient(logNotification);
            var requestLog = new RestRequest();
            requestLog.Method = Method.POST;
            requestLog.AddHeader("Content-Type", "application/json");
            requestLog.Parameters.Clear();
            requestLog.AddParameter("application/json", JsonConvert.SerializeObject(notificacionprod), ParameterType.RequestBody);
            var responseLog = clientLog.Execute(requestLog);
            Console.WriteLine(responseLog.Content);
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
