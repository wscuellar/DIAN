using Rebex.Mime.Headers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Gosocket.Dian.Domain.Utils
{
    public class EmailData
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

        private static readonly Regex validHostnameRegex = new Regex(@"^(([a-z]|[a-z][a-z0-9-]*[a-z0-9]).)*([a-z]|[a-z][a-z0-9-]*[a-z0-9])$", RegexOptions.IgnoreCase);

        public EmailData()
        {
            FilesList = new Dictionary<string, byte[]>();
        }
        
        public bool ValidateAllData()
        {

            if (!string.IsNullOrWhiteSpace(Server) && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) &&
                !string.IsNullOrWhiteSpace(Sender) && Receiver.Length > 0 && !string.IsNullOrWhiteSpace(Subject) && !string.IsNullOrWhiteSpace(Application) &&
                !string.IsNullOrWhiteSpace(Body))
            {
                IPAddress address;
                if (IPAddress.TryParse(Server, out address) || IsHostnameValid(Server))
                {
                    MailAddressCollection listSender = new MailAddressCollection(Sender);

                    if (listSender[0].Host != "")
                    {
                        MailAddressCollection listReceiver = new MailAddressCollection(Receiver);
                        bool isValid = true;
                        foreach (MailAddress item in listReceiver)
                        {
                            if (item.Host == "") isValid = false;
                        }
                        if (isValid)
                        {
                            bool isValidCC = true;
                            if (CC != null)
                            {
                                MailAddressCollection listCC = new MailAddressCollection(CC);
                                foreach (MailAddress item in listCC)
                                {
                                    if (item.Host == "") isValid = false;
                                }
                            }
                            if (isValidCC)
                            {
                                bool isValidCCO = true;
                                if (CCO != null)
                                {
                                    MailAddressCollection listCCO = new MailAddressCollection(CCO);
                                    foreach (MailAddress item in listCCO)
                                    {
                                        if (item.Host == "") isValid = false;
                                    }
                                }
                                if (isValidCCO)
                                {
                                    return true;
                                }
                                else
                                {
                                    ErrorMessage = "Format Error in : CCO --- Message : Invalid address.";
                                    return false;
                                }
                            }
                            else
                            {
                                ErrorMessage = "Format Error in : CC --- Message : Invalid address.";
                                return false;
                            }
                        }
                        else
                        {
                            ErrorMessage = "Format Error in : Receiver --- Message : Invalid address.";
                            return false;
                        }
                    }
                    else
                    {
                        ErrorMessage = "Format Error in : Sender --- Message : Invalid address.";
                        return false;
                    }
                }
                else
                {
                    ErrorMessage = "Format Error in : Server --- Message : Invalid server name.";
                    return false;
                }
            }
            else
            {
                ErrorMessage = "Format Error in : Fields --- Message : Some fields are null or empty.";
                return false;
            }
        }


        private bool IsHostnameValid(string hostname)
        {
            if (!string.IsNullOrWhiteSpace(hostname))
            {
                return validHostnameRegex.IsMatch(hostname.Trim());
            }

            return false;
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

            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            return request;
        }
    }
}
