using Rebex.Mail;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gosocket.Dian.Domain.Utils
{
    /// <summary>
    /// Simple delivery status parser.
    /// See RFC1894.
    /// </summary>
    public class DeliveryStatus
    {
        public Dictionary<string, string> _values;

    public string FinalRecipient
        {
            get { return GetValue("final-recipient"); }
        }

        public string Disposition
        {
            get { return GetValue("disposition"); }
        }

        public string Action
        {
            get { return GetValue("actions"); }
        }

        public string Status
        {
            get { return GetValue("status"); }
        }

        public string OriginalMessageId
        {
            get { return GetValue("original-message-id"); }
        }

        public string OriginalEnvelopeId
        {
            get { return GetValue("original-envelope-id"); }
        }

        public string ReportingMailTransferAgent
        {
            get { return GetValue("reporting-mta"); }
        }

        private string GetValue(string name)
        {
            string value;
            if (!_values.TryGetValue(name, out value))
                return null;
            return value;
        }

        public static DeliveryStatus ParseMessage(MailMessage message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            foreach (Attachment att in message.Attachments)
            {
                switch (att.MediaType)
                {
                    case "message/delivery-status":
                    case "message/disposition-notification":
                        using (StreamReader reader = new StreamReader(att.GetContentStream()))
                        {
                            return new DeliveryStatus(reader.ReadToEnd());
                        }
                }
            }

            return null;
        }

        public DeliveryStatus(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            _values = new Dictionary<string, string>();

            message = message.Replace("\r", "");

            string[] lines = message.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int p = line.IndexOf(':');
                if (p < 0)
                continue;

                string name = line.Substring(0, p).ToLowerInvariant();
                string value = line.Substring(p + 1).Trim();

                _values[name] = value;
            }
        }
    }
}
