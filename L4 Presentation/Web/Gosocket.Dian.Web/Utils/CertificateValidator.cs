using Gosocket.Dian.Application.Common;
using Gosocket.Dian.Infrastructure;
using Org.BouncyCastle.X509;
using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using Manager = Gosocket.Dian.Application.Managers;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Gosocket.Dian.Web.Utils
{
    public class CertificateValidator : X509CertificateValidator
    {
        private static readonly FileManager fileManager = new FileManager();

        public override void Validate(X509Certificate2 certificate)
        {
            ValidateCertificate(certificate);
        }

        public void ValidateCertificate(X509Certificate2 certificate)
        {
            ////Valida vigencia
            if (DateTime.Now < certificate.NotBefore)
                throw new Exception("Certificado aún no se encuentra vigente.");

            if (DateTime.Now > certificate.NotAfter)
                throw new Exception("Certificate se encuentra expirado.");

            // Get all crt certificates
            var crts = Manager.CertificateManager.Instance.GetRootCertificates();

            // Get all crls
            var crls = Manager.CertificateManager.Instance.GetCrls();

            var primary = GetPrimaryCertificate(certificate);

            if (!primary.IsTrusted(crts)) throw new Exception(ConfigurationManager.GetValue("UnTrustedCertificateMessage"));

            if (primary.IsRevoked(crls)) throw new Exception("Certificado se encuentra revocado.");
        }

        private X509Certificate GetPrimaryCertificate(X509Certificate2 certificate)
        {
            X509Certificate x509Certificate = new X509CertificateParser().ReadCertificate(certificate.RawData);
            return x509Certificate;
        }
    }
}