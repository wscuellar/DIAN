using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Utils;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Rebex.Mail;
using Rebex.Mime.Headers;
using Rebex.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.EmailSender
{
    public static class EmailSender
    {
        static string mailServerQueueId = string.Empty;
        static StringBuilder mailServerProtocol = new StringBuilder();
        private static readonly TableManager manager = new TableManager("DianEmailTracker");

        [FunctionName("EmailSender")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                Rebex.Licensing.Key = "==FkHd2B4TbfMAYHmvRXhpBVqnoCkCqtfaZoRSsDXqkX2PyyraGJJLmx5B3OTr7JIM/GQW4==";
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                EmailSenderInput emailData;

                DateTime dateTime = DateTime.Now;

                //Comentar para debuguear
                //Leyendo y validando los datos de entrada
                var requestData = await req.Content.ReadAsMultipartAsync();
                emailData = await EmailSenderHelpers.ReadStringDataAsync(requestData);

                // Descomentar para debueguear
                //emailData = await req.Content.ReadAsAsync<EmailSenderInput>();

                EmailSenderResponse response;
                if (!EmailSenderHelpers.ValidateAllData(emailData))
                {
                    response = new EmailSenderResponse(null, ErrorTypes.InputError, emailData.ErrorMessage, null);
                    log.Error("Call from : " + emailData.Application + " -- with error type : " + response.ErrorType + " -- message : " + response.ErrorMessage);
                    return req.CreateResponse(HttpStatusCode.BadRequest, JsonConvert.SerializeObject(response));
                }
                log.Info("Call from : " + emailData.Application + " -- Starting to send email to : " + emailData.Receiver + " --- from : " + emailData.Sender);

                MailMessage message = new MailMessage();

                // and set its properties to desired values
                message.From = emailData.Sender;

                //Inicaindo lista para almacenar los detalles y validaciones de cada address a inviar el email
                bool warningOnSendedMails = false;
                List<EmailSenderResponseDetails> emailSenderResponseDetails = new List<EmailSenderResponseDetails>();
                message.To = GetListAddresses(emailData.Receiver, ref emailSenderResponseDetails, ref warningOnSendedMails);
                if (emailData.CC != null) message.CC = GetListAddresses(emailData.CC, ref emailSenderResponseDetails, ref warningOnSendedMails);
                if (emailData.CCO != null) message.Bcc = GetListAddresses(emailData.CCO, ref emailSenderResponseDetails, ref warningOnSendedMails);
                message.Subject = emailData.Subject;
                if (emailData.BodyIsHTML) message.BodyHtml = emailData.Body;
                else message.BodyText = emailData.Body;
                message.Priority = MailPriority.Normal;
                message.Date = new MailDateTime(DateTime.Now);
                foreach (var item in emailData.FilesList)
                    message.Attachments.Add(new Attachment(item.Value, item.Key));

                // generate a unique message ID to help identify replies
                message.MessageId = new MessageId();
                message.EnvelopeId = message.MessageId.Id;
                message.Headers.Add("Disposition-Notification-To", emailData.Sender);
                // and display it
                // create SMTP client instance
                using (var smtp = new Rebex.Net.Smtp())
                {
                    smtp.ValidatingCertificate += SmtpValidatingCertificate;

                    //smtp.DeliveryStatusNotificationConditions = DeliveryStatusNotificationConditions.None;
                    if (string.IsNullOrWhiteSpace(emailData.Port)) smtp.Connect(emailData.Server, SslMode.Explicit);
                    else
                        if (emailData.Port == "25")
                        smtp.Connect(emailData.Server, int.Parse(emailData.Port), SslMode.None);
                    else smtp.Connect(emailData.Server, int.Parse(emailData.Port), SslMode.Explicit);

                    

                    // authenticate with your email address and password
                    smtp.Login(emailData.Username, emailData.Password);
                    log.Info("SMTPClient connected and authenticated to server : " + emailData.Server);

                    // send mail
                    smtp.Send(message);
                    // disconnect (not required, but polite)
                    smtp.Disconnect();
                }
                response = new EmailSenderResponse(message.MessageId.Id, warningOnSendedMails ? ErrorTypes.Warning : ErrorTypes.NoError, null, emailSenderResponseDetails);
                log.Info("Call from : " + emailData.Application + " -- Email sended to : " + emailData.Receiver + " ---  in " +
                    DateTime.Now.Subtract(dateTime).TotalMilliseconds + " ms");

                //log.Info($"TrackId de recepción de mensaje enviado a {emailData.Receiver}: {mailServerQueueId ?? "No recepcionado."}");
                //var tracker = new DianEmailTracker(emailData.Receiver, Guid.NewGuid().ToString())
                //{
                //    Description = !string.IsNullOrEmpty(mailServerQueueId) ? $"Correo electrónico entregado en {DateTime.Now.Subtract(dateTime).TotalMilliseconds} milisegundos." : "No se obtuvo respuesta de la entrega del mensaje.",
                //    Sender = emailData.Sender,
                //    Status = string.IsNullOrEmpty(mailServerQueueId) ? 1 : 0,
                //    TrackId = mailServerQueueId ?? ""
                //};
                //await TrackEmailResponse(tracker);

                return req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                EmailSenderResponse response = new EmailSenderResponse(null, ErrorTypes.InternalError, ex.Message, null);
                log.Error("Internal Error", ex);
                log.Error($"Message: {ex.Message}, Trace: {ex.StackTrace}");
                return req.CreateResponse(HttpStatusCode.BadRequest, JsonConvert.SerializeObject(response));
            }
        }

        // A custom certificate verification handler.
        private static void SmtpValidatingCertificate(object sender, SslCertificateValidationEventArgs e)
        {
            // get a string representation of the certificate's fingerprint
            string fingerprint = e.Certificate.Thumbprint;

            // check whether the fingerprint matches the desired fingerprin
            bool ok = true;

            if (ok)
                e.Accept();
            else
                e.Reject();
        }

        private static MailAddressCollection GetListAddresses(string addresses, ref List<EmailSenderResponseDetails> emailSenderResponseDetails, ref bool warning)
        {
            //Validando las direcciones a enviar
            MailAddressCollection listReceiver = new MailAddressCollection(addresses);
            MailAddressCollection listReceiverReturn = new MailAddressCollection();
            foreach (MailAddress item in listReceiver)
            {
                if (item.Host != "")
                {
                    listReceiverReturn.Add(item);
                    emailSenderResponseDetails.Add(new EmailSenderResponseDetails(item.Address, true, 200, null));

                }
                else
                {
                    emailSenderResponseDetails.Add(new EmailSenderResponseDetails(item.Address, false, 101, "Invalid Email"));
                    warning = true;
                }
            }
            return listReceiverReturn;
        }

        private static async Task TrackEmailResponse(DianEmailTracker tracker)
        {
            
            await manager.InsertOrUpdateAsync(tracker);
        }
    }
}
