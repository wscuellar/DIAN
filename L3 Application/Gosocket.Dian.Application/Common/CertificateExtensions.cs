using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application.Common
{
    public static class CertificateExtensions
    {
        private const string CertificatesCollection = "Certificate/Collection";
        private static readonly string[] witheListPkixCertPathBuilderException = { "Certificate has unsupported critical extension.", "Subject alternative name extension could not be decoded." };
        private static readonly MemoryCache cache=MemoryCache.Default;
        private static readonly string CACHE_KEY = "CERT-";
        private static readonly string CACHE_KEY_ISTRUST = "CERT-TRUST-";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="crls"></param>
        /// <returns></returns>
        public static bool IsRevoked(this X509Certificate certificate, IEnumerable<X509Crl> crls)
        {
            /**
             * 1. Verificar si el serialnumber del certificado está en caché
             *      a. Si está en caché, retornar el resultado de la validación guardada
             * 2. de lo contrario
             *      a. Verificar si el certificado ha sido revicado en alguna de las CRLs
             *      b. Crear tupla con el resultado de la validación
             *      c. Enviar tupla a caché con un tiempo de vigencia
             *      d. Retornar resultado de validación
             * 3.  La próxima vez retornará lo guardado en caché si aún está en vigencia
             *      
             * 
             * 
             */
            
            if (crls == null || !crls.Any()) return false;

            string key = CACHE_KEY + certificate.SerialNumber.ToString();
            Tuple<string, bool> value=cache.Get(key) as Tuple<string, bool>;

            if (value != null)
            {
                return value.Item2;
            }
            else
            {
                if (crls.Any(c => c.IsRevoked(certificate)))
                {
                    value = new Tuple<string, bool>(key,true);
                }
                else
                {
                    value = new Tuple<string, bool>(key,false);
                }
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddHours(2)
                };
                cache.Set(key, value, policy);
                return value.Item2;

            }           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="chainCertificates"></param>
        /// <returns></returns>
        public static bool IsTrusted(this X509Certificate certificate, IEnumerable<X509Certificate> chainCertificates)
        {
          
            string key = CACHE_KEY_ISTRUST + certificate.SerialNumber.ToString();
            Tuple<string, bool> value = cache.Get(key) as Tuple<string, bool>;

            if (value != null)
            {
                return value.Item2;
            }
            else
            {
                value = new Tuple<string, bool>(key, true);
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
                    Trace.TraceError("IsTrusted: "+e.InnerException?.Message ?? e.Message);
                    if (!witheListPkixCertPathBuilderException.Contains(e.InnerException?.Message))
                    {
                        value = new Tuple<string, bool>(key, false);
                    }
                        
                }
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddHours(2)
                };
                cache.Set(key, value, policy);
                
                return value.Item2;
            }


            
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