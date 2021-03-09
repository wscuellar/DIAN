using Gosocket.Dian.Domain.Utils;
using Gosocket.Dian.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Gosocket.Dian.Application
{
    public class EmailService
    {
        public EmailSenderResponse SendEmail(string receiver, string subject, Dictionary<string, string> replacement)
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                using (var httpClient = new HttpClient())
                {
                    var users = ConfigurationManager.GetValue("EmailUser").Split('|');
                    var senders = ConfigurationManager.GetValue("EmailSender").Split('|');
                    var index = 0;
                    var emailUser = users[index];
                    var emailSender = senders[index];
                    EmailSenderInput emaiSenderlData = new EmailSenderInput
                    {
                        Application = ConfigurationManager.GetValue("EmailApplicationName"),
                        Server = ConfigurationManager.GetValue("EmailServer"),
                        Username = emailUser,
                        Password = ConfigurationManager.GetValue("EmailPassword"),
                        Sender = emailSender,
                        Port = ConfigurationManager.GetValue("EmailPort"),
                        Receiver = receiver,
                        Subject = subject,
                        Body = EmailTemplateManager.GenerateHtmlBody("template-content", replacement),
                        BodyIsHTML = true
                    };

                    var requestUrl = ConfigurationManager.GetValue("SendEmailFunctionUrl");
                    HttpRequestMessage req = EmailSenderInput.CreatePostRequestSendEmailDataWithFile(emaiSenderlData, requestUrl);

                    var res = httpClient.SendAsync(req).Result;

                    var content = res.Content.ReadAsStringAsync().Result;
                    string emailSenderResponseString = JsonConvert.DeserializeObject<string>(content);
                    EmailSenderResponse emailSenderResponse = JsonConvert.DeserializeObject<EmailSenderResponse>(emailSenderResponseString);
                    return emailSenderResponse;
                }
            }
            catch (Exception ex)
            {
                EmailSenderResponse emailSenderResponse = new EmailSenderResponse(null, ErrorTypes.InternalError, ex.Message, new List<EmailSenderResponseDetails>());
                return emailSenderResponse;
            }
        }
    }
}
