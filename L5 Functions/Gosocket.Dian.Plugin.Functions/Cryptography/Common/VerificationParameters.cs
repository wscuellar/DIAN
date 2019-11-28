using System.Security.Cryptography.X509Certificates;

namespace Gosocket.Dian.Plugin.Functions.Cryptography.Common
{
    public class VerificationParameters
    {
        public string InputPath { get; set; }
        public X509Certificate2 VerificationCertificate { get; set; }
        public bool VerifyCertificate { get; set; }
    }
}
