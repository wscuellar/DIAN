using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Gosocket.Dian.Plugin.Functions.Cryptography.Common
{
    public class VerificationResult
    {
        public string Timestamp { get; set; }
        public XmlDocument OriginalDocument { get; set; }
        public X509Certificate2 SigningCertificate { get; set; }

        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
