using Gosocket.Dian.Infrastructure;
//using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.X509;
//using System.Runtime.Caching;
using Manager = Gosocket.Dian.Application.Managers;
using Org.BouncyCastle.Utilities.Collections;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using Org.BouncyCastle.Pkix;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;

namespace Gosocket.Dian.Web.Utils
{
    public class CertificateValidator : X509CertificateValidator
    {
        private static readonly string container = $"dian";
        private static readonly string crtFilesFolder = $"certificates/crts/";
        private static readonly string crlFilesFolder = $"certificates/crls/";
        private const string CertificatesCollection = "Certificate/Collection";

        private readonly HashSet _trustedRoots = new HashSet();
        private readonly List<X509Certificate> _intermediates = new List<X509Certificate>();
        public IEnumerable<X509Crl> Crls { get; private set; }

        private static readonly FileManager fileManager = new FileManager();

        public override void Validate(X509Certificate2 certificate)
        {

            ////Valida vigencia
            if (DateTime.Now < certificate.NotBefore)
                throw new SecurityTokenNotYetValidException("Client certificate is not yet valid. ");
            if (DateTime.Now > certificate.NotAfter)
                throw new SecurityTokenExpiredException("Client certificate is expired.");

            ValidateCertificate(certificate);
        }

        public void ValidateCertificate(X509Certificate2 certificate)
        {
            // Get all crt certificates
            var crts = Manager.CertificateManager.Instance.GetRootCertificates(container, crtFilesFolder);

            // Get all crls
            var crls = Manager.CertificateManager.Instance.GetCrls(container, crlFilesFolder);

            var primary = GetPrimaryCertificate(certificate);

            var x509Validator = new X509Validator(crts, crls);

            x509Validator.Validate(primary);
        }

        private X509Certificate GetPrimaryCertificate(X509Certificate2 certificate)
        {
            X509Certificate x509Certificate = new X509CertificateParser().ReadCertificate(certificate.RawData);
            return x509Certificate;
        }

    }
}