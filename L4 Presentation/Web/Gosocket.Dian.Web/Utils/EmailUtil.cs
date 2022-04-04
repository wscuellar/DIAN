using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Utils;
using Gosocket.Dian.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Web.Utils
{
    public class EmailUtil
    {
        private static HttpClient client = new HttpClient();

        public async static Task<EmailSenderResponse> SendEmailAsync(AuthToken auth, string accessUrl)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var replacement = new Dictionary<string, string>
            {
                { "##LOGIN_LINK##", accessUrl },
                { "##USERNAME##", auth.Email }
            };

            
            
            var users = ConfigurationManager.GetValue("EmailUser").Split('|');
            var senders = ConfigurationManager.GetValue("EmailSender").Split('|');
            var index = GetRandomIndex(users.Length);
            var emailUser = users[index];
            var emailSender = senders[index];
            EmailSenderInput emaiSenderlData = new EmailSenderInput
            {
                Application = ConfigurationManager.GetValue("EmailApplicationName"),
                Server = ConfigurationManager.GetValue("EmailServer"),
                Username = emailUser, //ConfigurationManager.GetValue("EmailUser"),
                Password = ConfigurationManager.GetValue("EmailPassword"),
                Sender = emailSender, //ConfigurationManager.GetValue("EmailSender"),
                Port = ConfigurationManager.GetValue("EmailPort"),
                Receiver = auth.Email,
                Subject = "Token Acceso Dian",
                Body = EmailTemplateManager.GenerateHtmlBody("access_login_template", replacement),
                BodyIsHTML = true
            };

            var requestUrl = ConfigurationManager.GetValue("SendEmailFunctionUrl");
            HttpRequestMessage req = EmailSenderInput.CreatePostRequestSendEmailDataWithFile(emaiSenderlData, requestUrl);

            var res = await client.SendAsync(req);

            var content = await res.Content.ReadAsStringAsync();
            string emailSenderResponseString = JsonConvert.DeserializeObject<string>(content);
            EmailSenderResponse emailSenderResponse = JsonConvert.DeserializeObject<EmailSenderResponse>(emailSenderResponseString);
            return emailSenderResponse;
            
        }

        public static int GetRandomIndex(int length)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, length);
            return randomNumber;
        }
    }
}