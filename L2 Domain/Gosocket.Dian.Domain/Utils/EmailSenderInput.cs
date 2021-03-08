using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Gosocket.Dian.Domain.Utils
{
    public class EmailSenderInput
    {
        public string Application { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string CC { get; set; }
        public string CCO { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool BodyIsHTML { get; set; }
        public Dictionary<string, byte[]> FilesList { get; set; }
        public string ErrorMessage { get; set; }


        public EmailSenderInput()
        {
            FilesList = new Dictionary<string, byte[]>();
        }

        public static HttpRequestMessage CreatePostRequestSendEmailDataWithFile(EmailSenderInput emailData, string hostName = null)
        {
            var requestPath = string.IsNullOrWhiteSpace(hostName) ? "https://localhost" : hostName;
            MultipartFormDataContent multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new StringContent(emailData.Application, Encoding.UTF8), "Application");
            multipartContent.Add(new StringContent(emailData.Server, Encoding.UTF8), "Server");
            multipartContent.Add(new StringContent(emailData.Username, Encoding.UTF8), "Username");
            multipartContent.Add(new StringContent(emailData.Password, Encoding.UTF8), "Password");
            multipartContent.Add(new StringContent(emailData.Sender, Encoding.UTF8), "Sender");
            multipartContent.Add(new StringContent(emailData.Receiver, Encoding.UTF8), "Receiver");
            multipartContent.Add(new StringContent(emailData.Subject, Encoding.UTF8), "Subject");
            multipartContent.Add(new StringContent(emailData.Body, Encoding.UTF8), "Body");
            multipartContent.Add(new StringContent(emailData.BodyIsHTML.ToString(), Encoding.UTF8), "BodyIsHTML");
            if (!string.IsNullOrWhiteSpace(emailData.CC)) multipartContent.Add(new StringContent(emailData.CC, Encoding.UTF8), "CC");
            if (!string.IsNullOrWhiteSpace(emailData.CCO)) multipartContent.Add(new StringContent(emailData.CCO, Encoding.UTF8), "CCO");
            if (!string.IsNullOrWhiteSpace(emailData.Port)) multipartContent.Add(new StringContent(emailData.Port, Encoding.UTF8), "Port");
            foreach (var item in emailData.FilesList)
            {
                ByteArrayContent byteContent = new ByteArrayContent(item.Value);
                byteContent.Headers.Add("Content-Disposition", "file; filename=" + item.Key + ";name=" + item.Key);
                multipartContent.Add(byteContent);
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(requestPath),
                Content = multipartContent
            };

            //var configuration = new HttpConfiguration();
            //request.SetConfiguration(configuration);

            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            return request;
        }

    }
}
