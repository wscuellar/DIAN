using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gosocket.Dian.Application.Common
{
    public static class CertificateExtensions
    {
        private const string CertificatesCollection = "Certificate/Collection";
        private static readonly string[] witheListPkixCertPathBuilderException = { "Certificate has unsupported critical extension.", "Subject alternative name extension could not be decoded." };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="crls"></param>
        /// <returns></returns>
        public static bool IsRevoked(this X509Certificate certificate, IEnumerable<X509Crl> crls)
        {
            if (crls == null || !crls.Any()) return false;

            if (crls.Any(c => c.IsRevoked(certificate))) return true;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="chainCertificates"></param>
        /// <returns></returns>
        public static bool IsTrusted(this X509Certificate certificate, IEnumerable<X509Certificate> chainCertificates)
        {
            try
            {
                var tupple = LoadCertificates(chainCertificates);

                var trustedRoots = tupple.Item1;
                var intermediates = tupple.Item2;

                var selector = new X509CertStoreSelector { Certificate = certificate };

                var builderParams = new PkixBuilderParameters(trustedRoots, selector) { IsRevocationEnabled = false };
                builderParams.AddStore(X509StoreFactory.Create(CertificatesCollection, new X509CollectionStoreParameters(intermediates)));
                builderParams.AddStore(X509StoreFactory.Create(CertificatesCollection, new X509CollectionStoreParameters(new[] { certificate })));

                var builder = new PkixCertPathBuilder();
                var result = builder.Build(builderParams);
            }
            catch (PkixCertPathBuilderException e)
            {
                Debug.WriteLine(e.InnerException?.Message ?? e.Message);
                if (!witheListPkixCertPathBuilderException.Contains(e.InnerException?.Message))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        private static bool IsSelfSigned(X509Certificate certificate)
        {
            return certificate.IssuerDN.Equivalent(certificate.SubjectDN);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chainCertificates"></param>
        /// <returns></returns>
        private static Tuple<HashSet, List<X509Certificate>> LoadCertificates(IEnumerable<X509Certificate> chainCertificates)
        {
            var trustedRoots = new HashSet();
            var intermediates = new List<X509Certificate>();

            foreach (var root in chainCertificates)
            {
                if (IsSelfSigned(root))
                    trustedRoots.Add(new TrustAnchor(root, null));
                else
                    intermediates.Add(root);
            }

            return new Tuple<HashSet, List<X509Certificate>>(trustedRoots, intermediates);
        }
    }
}