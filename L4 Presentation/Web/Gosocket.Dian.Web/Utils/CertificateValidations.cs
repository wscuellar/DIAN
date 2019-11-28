
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Gosocket.Dian.Web.Utils
{
    public class CertificateValidations
    {
        public X509Certificate2 PACertificate;
        public bool isRevocated;
        public bool isExpired;
        public bool isChainTrusted;

        public CertificateValidations(X509Certificate2 paCertificate)
        {
            PACertificate = paCertificate;
            isRevocated = IsNotRevocated(PACertificate);
            isExpired = IsNotExpired(PACertificate);
            isChainTrusted = IsChainTrusted(PACertificate);
        }

        private static bool IsNotRevocated(X509Certificate2 PACertificate)
        {
            var thumbprint = PACertificate.Thumbprint;

            return true;
        }
        private static bool IsNotExpired(X509Certificate2 PACertificate)
        {
            bool notExpired = true;

            var now = DateTime.UtcNow;
            var caEffectiveDate = DateTime.Parse(PACertificate.GetEffectiveDateString());
            var caExpirationDate = DateTime.Parse(PACertificate.GetExpirationDateString());

            if (now <= caEffectiveDate || now > caExpirationDate)
                notExpired = false;

            return notExpired;
        }
        private static bool IsChainTrusted(X509Certificate2 PACertificate)
        {
            bool isTrusted = true;

            X509Chain chain2 = new X509Chain();

            //// Check all properties
            chain2.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

            //// This setup does not have revocation information
            chain2.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            //// Build the chain
            chain2.Build(PACertificate);

            // Check if the only error of the chain is the missing root CA,
            // otherwise reject the given certificate.
            if (chain2.ChainStatus.Any(statusFlags => statusFlags.Status != X509ChainStatusFlags.PartialChain))
                isTrusted = false;

            // Check if CA certificate is available in the chain.
                isTrusted = chain2.ChainElements.Cast<X509ChainElement>()
                                      .Select(element => element.Certificate)
                                      .Where(chainCertificate => chainCertificate.Subject == PACertificate.Subject)
                                      .Where(chainCertificate => chainCertificate.GetRawCertData().SequenceEqual(PACertificate.GetRawCertData()))
                                      .Any();

            return isTrusted;
        }
    }
}