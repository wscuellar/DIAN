using Rebex.Mime.Headers;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gosocket.Dian.Domain.Utils
{
    public static class EmailSenderHelpers
    {
        private static readonly Regex validHostnameRegex = new Regex(@"^(([a-z]|[a-z][a-z0-9-]*[a-z0-9]).)*([a-z]|[a-z][a-z0-9-]*[a-z0-9])$", RegexOptions.IgnoreCase);

        public static async Task<EmailSenderInput> ReadStringDataAsync(MultipartMemoryStreamProvider provider)
        {
            EmailSenderInput emailData = new EmailSenderInput();
            foreach (var item in provider.Contents)
            {
                string fieldName = item.Headers.ContentDisposition.Name.Trim(new char[] { '"', '\\' }).ToLower();
                switch (fieldName)
                {
                    case "application": emailData.Application = await item.ReadAsStringAsync(); break;
                    case "server": emailData.Server = await item.ReadAsStringAsync(); break;
                    case "port": emailData.Port = await item.ReadAsStringAsync(); break;
                    case "username": emailData.Username = await item.ReadAsStringAsync(); break;
                    case "password": emailData.Password = await item.ReadAsStringAsync(); break;
                    case "sender": emailData.Sender = await item.ReadAsStringAsync(); break;
                    case "receiver": emailData.Receiver = await item.ReadAsStringAsync(); break;
                    case "cc": emailData.CC = await item.ReadAsStringAsync(); break;
                    case "cco": emailData.CCO = await item.ReadAsStringAsync(); break;
                    case "subject": emailData.Subject = await item.ReadAsStringAsync(); break;
                    case "body": emailData.Body = await item.ReadAsStringAsync(); break;
                    case "bodyishtml": emailData.BodyIsHTML = bool.Parse(await item.ReadAsStringAsync()); break;
                    default:
                        try
                        {
                            emailData.FilesList.Add(item.Headers.ContentDisposition.FileName.Trim(new char[] { '"', '\\' }), await item.ReadAsByteArrayAsync());
                        }
                        catch (Exception)
                        {
                            throw new Exception("There are some fields not allowed");
                        }
                        break;
                }
            }
            return emailData;
        }

        public static bool ValidateAllData(EmailSenderInput emailSenderInput)
        {

            if (!string.IsNullOrWhiteSpace(emailSenderInput.Server) && !string.IsNullOrWhiteSpace(emailSenderInput.Username) && !string.IsNullOrWhiteSpace(emailSenderInput.Password) &&
                !string.IsNullOrWhiteSpace(emailSenderInput.Sender) && emailSenderInput.Receiver.Length > 0 && !string.IsNullOrWhiteSpace(emailSenderInput.Subject) && !string.IsNullOrWhiteSpace(emailSenderInput.Application) &&
                !string.IsNullOrWhiteSpace(emailSenderInput.Body))
            {
                IPAddress address;
                if (IPAddress.TryParse(emailSenderInput.Server, out address) || IsHostnameValid(emailSenderInput.Server))
                {
                    MailAddressCollection listSender = new MailAddressCollection(emailSenderInput.Sender);

                    if (listSender[0].Host != "")
                    {
                        return true;
                               
                    }
                    else
                    {
                        emailSenderInput.ErrorMessage = "Format Error in : Sender --- Message : Invalid address.";
                        return false;
                    }
                }
                else
                {
                    emailSenderInput.ErrorMessage = "Format Error in : Server --- Message : Invalid server name.";
                    return false;
                }
            }
            else
            {
                emailSenderInput.ErrorMessage = "Format Error in : Fields --- Message : Some fields are null or empty.";
                return false;
            }
        }


        private static bool IsHostnameValid(string hostname)
        {
            if (!string.IsNullOrWhiteSpace(hostname))
            {
                return validHostnameRegex.IsMatch(hostname.Trim());
            }

            return false;
        }


    }
}

