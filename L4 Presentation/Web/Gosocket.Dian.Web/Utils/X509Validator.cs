using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;

namespace Gosocket.Dian.Web.Utils
{
    public class X509Validator
    {
        private const string CertificatesCollection = "Certificate/Collection";
        private readonly HashSet _trustedRoots = new HashSet();
        private readonly List<X509Certificate> _intermediates = new List<X509Certificate>();

        public IEnumerable<X509Crl> Crls { get; private set; }

        public X509Validator(IEnumerable<X509Certificate> chainCertificates, IEnumerable<X509Crl> crls)
        {
            if (chainCertificates == null)
                throw new ArgumentNullException(nameof(chainCertificates));

            if (crls == null)
                throw new ArgumentNullException(nameof(crls));

            Crls = crls;
            LoadCertificates(chainCertificates);
        }

        private void LoadCertificates(IEnumerable<X509Certificate> chainCertificates)
        {
            foreach (var root in chainCertificates)
            {
                if (IsSelfSigned(root))
                    _trustedRoots.Add(new TrustAnchor(root, null));
                else
                    _intermediates.Add(root);
            }
        }

        public void Validate(X509Certificate certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            var errors = new List<string>();

            //ValidateChainTrust(certificate, errors);
            //if(errors.Count > 0)
            //    throw new SecurityTokenValidationException();

            ValidateCrl(certificate, errors);
            if (errors.Count > 0)
                throw new SecurityTokenValidationException();
        }

        private void ValidateChainTrust(X509Certificate certificate, ICollection<string> errors)
        {
            try
            {
                var selector = new X509CertStoreSelector { Certificate = certificate };

                var builderParams = new PkixBuilderParameters(_trustedRoots, selector) { IsRevocationEnabled = false };
                builderParams.AddStore(X509StoreFactory.Create(CertificatesCollection, new X509CollectionStoreParameters(_intermediates)));
                builderParams.AddStore(X509StoreFactory.Create(CertificatesCollection, new X509CollectionStoreParameters(new[] { certificate })));

                var builder = new PkixCertPathBuilder();
                var result = builder.Build(builderParams);
            }
            catch (PkixCertPathBuilderException e)
            {
                errors.Add(e.InnerException?.Message ?? e.Message);
            }
        }

        private void ValidateCrl(X509Certificate certificate, ICollection<string> errors)
        {
            if (Crls.Any(c => c.IsRevoked(certificate)))
                errors.Add("Certificate is revoked.");
        }

        private static bool IsSelfSigned(X509Certificate certificate)
        {
            return certificate.IssuerDN.Equivalent(certificate.SubjectDN);
        }
    }
}