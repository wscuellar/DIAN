using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Rebex.Mail;
using Rebex.Net;

namespace Gosocket.Dian.Functions.EmailSender
{
    public static class EmailSenderVerifyDsn
    {
        [FunctionName("EmailSenderVerifyDsn")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                Rebex.Licensing.Key = "==FkHd2B4TbfMAYHmvRXhpBVqnoCkCqtfaZoRSsDXqkX2PyyraGJJLmx5B3OTr7JIM/GQW4==";
                EmailSenderVerifyDsnInput data = await req.Content.ReadAsAsync<EmailSenderVerifyDsnInput>();
                log.Info("Staring to get emails for application : " + data.Application);

                Pop3 client = new Pop3();
                client.ValidatingCertificate += pop_ValidatingCertificate;
                client.Connect(data.Server, SslMode.Implicit);
                client.Login(data.User, data.Password);
                Pop3MessageCollection messages = client.GetMessageList();
                MailMessage message = null;
                foreach (var item in messages)
                {
                    MailMessage message1 = client.GetMailMessage(item.SequenceNumber);
                    if (message1.InReplyTo.FirstOrDefault(z => z.Id == data.EmailID) != null)
                    {
                        message = message1;
                        break;
                    }
                }
                DeliveryStatus dsn;
                if (message != null)
                {
                    dsn = DeliveryStatus.ParseMessage(message);
                    if (dsn != null)
                    {
                        return req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(dsn._values));
                    }
                    else return req.CreateResponse(HttpStatusCode.OK, "No error");
                }
                else return req.CreateResponse(HttpStatusCode.OK, "Not found");
            }
            catch (System.Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }


        // A custom certificate verification handler.
        private static void pop_ValidatingCertificate(object sender, SslCertificateValidationEventArgs e)
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
    }
}
