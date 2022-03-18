using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.KeyVault;
using Gosocket.Dian.Functions.Cache;
using Gosocket.Dian.Functions.Cryptography.XmlDsig;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Gosocket.Dian.Functions.Cryptography.Signer
{
    public class DocumentSigner
    {
        private readonly KeyManager _keyManager;

        public readonly X509Certificate2 _certificate;

        private readonly byte[] _certificateContent;

        private static readonly TableManager tableGlobalLogger = new TableManager("GlobalLogger");

        private object obj = new object();

        public DocumentSigner()
        {
            //var certificate = GetCertificatFromKeyVault();
            //_certificateContent = Convert.FromBase64String(certificate.Base64Data);

            var certificate = GetCertificate();
            _certificate = certificate;
        }

        public DocumentSigner(string certificateName, string certificatePass)
        {
            var fileManager = new FileManager();

            _certificateContent = fileManager.GetBytes("dian", $"certificates/signdian/{certificateName}.pfx");
            _keyManager = new KeyManager(_certificateContent, certificatePass);
        }

        public X509Certificate2 GetCertificate()
        {
            X509Certificate2 certificate = null;
            CacheItem cacheItem = null; // InstanceCache.CertificateInstanceCache.GetCacheItem("Certificate");

            if (cacheItem == null)
            {
                certificate = GetCertificateByThumbprint();
                if (certificate == null) return null;
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24)
                };
                InstanceCache.CertificateInstanceCache.Set(new CacheItem("Certificate", certificate), policy);
            }
            else
                certificate = (X509Certificate2)cacheItem.Value;

            return certificate;
        }

        //public ExportCertificatResult GetCertificatFromKeyVault()
        //{
        //    ExportCertificatResult certificate = null;
        //    var cacheItem = InstanceCache.CertificateInstanceCache.GetCacheItem("Certificate");

        //    if (cacheItem == null)
        //    {
        //        var requestObj = new { Name = ConfigurationManager.GetValue("KeyVaultCertificateName") };
        //        certificate = ApiHelpers.ExecuteRequest<ExportCertificatResult>(ConfigurationManager.GetValue("ExportCertificateUrl"), requestObj);
        //        if (certificate == null) return null;
        //        CacheItemPolicy policy = new CacheItemPolicy
        //        {
        //            AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24)
        //        };
        //        InstanceCache.CertificateInstanceCache.Set(new CacheItem("Certificate", certificate), policy);
        //    }
        //    else
        //        certificate = (ExportCertificatResult)cacheItem.Value;

        //    return certificate;
        //}

        private X509Certificate2 GetCertificateByThumbprint()
        {
            var thumbprint = ConfigurationManager.GetValue("CertificateThumbprint"); //"BF6B7AE700D03E317C8792D93C4C3DD488C1A002";
            lock (obj)
            {
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                store.Close();

                foreach (var certificate in certificateCollection)
                {
                    if (certificate.Thumbprint == thumbprint)
                    {
                        using (certificate.GetRSAPrivateKey()) { }
                        return certificate;
                    }
                }
                throw new CryptographicException($"No certificate found with thumbprint: {thumbprint}");
            }
        }
        public static SecureXmlDocument SignWithRsaSha1(SecureXmlDocument xDoc, XmlNode xmlElementToSign, KeyManager key)
        {
            try
            {
                xDoc.PreserveWhitespace = true;
                xDoc.IncludeKeyInfo = false;

                xDoc.SignEnveloped(key, false, xmlElementToSign);

                return xDoc;
            }
            catch (Exception ex)
            {
                throw new CryptographicException(
                    "The enveloping signature could not be applied to the element " +
                    xmlElementToSign + "\r\n" + ex.Message, ex);
            }
        }

        public string SignXml(string xml, bool isDocument = false)
        {
            try
            {
                var xmldoc = new SignXmlDocument();
                xmldoc.Cert1Issuername = "C=CO,L=BOGOTÁ\\, D.C.,STREET=Carrera 21 A No 124 - 55 Oficina 303. https://www.gse.com.co/direccion,OU=http://www.gse.com.co,2.5.4.12=#0c0b41432047534520532e412e,O=GESTION DE SEGURIDAD ELECTRONICA S.A.,1.2.840.113549.1.9.1=#160d6361406773652e636f6d2e636f,2.5.4.5=#130e4e49542039303032303432373238,CN=SUB001 AC GSE S.A.,2.5.4.13=#0c27436572746966696361646f205375626f7264696e616461203030312041432047534520532e412e";
                xmldoc.Cert2Issuername = "1.3.6.1.4.1.31136.1.1.10.2=#0c819a456e7469646164206465204365727469666963616369c3b36e204469676974616c2041626965727461204175746f72697a61646120706f72206c61205375706572696e74656e64656e63696120646520496e64757374726961207920436f6d657263696f20646520436f6c6f6d6269612e2068747470733a2f2f7777772e6773652e636f6d2e636f2f5265736f6c7563696f6e5349432e706466,C=CO,L=BOGOTÁ\\, D.C.,STREET=Carrera 21 A No 124 - 55 Oficina 303. https://www.gse.com.co/direccion,OU=http://www.gse.com.co,2.5.4.5=#130e4e49542039303032303432373238,O=GESTION DE SEGURIDAD ELECTRONICA S.A.,CN=ROOT AC GSE S.A.,1.2.840.113549.1.9.1=#160d6361406773652e636f6d2e636f";
                xmldoc.Cert3Issuername = "1.3.6.1.4.1.31136.1.1.10.2=#0c819a456e7469646164206465204365727469666963616369c3b36e204469676974616c2041626965727461204175746f72697a61646120706f72206c61205375706572696e74656e64656e63696120646520496e64757374726961207920436f6d657263696f20646520436f6c6f6d6269612e2068747470733a2f2f7777772e6773652e636f6d2e636f2f5265736f6c7563696f6e5349432e706466,C=CO,L=BOGOTÁ\\, D.C.,STREET=Carrera 21 A No 124 - 55 Oficina 303. https://www.gse.com.co/direccion,OU=http://www.gse.com.co,2.5.4.5=#130e4e49542039303032303432373238,O=GESTION DE SEGURIDAD ELECTRONICA S.A.,CN=ROOT AC GSE S.A.,1.2.840.113549.1.9.1=#160d6361406773652e636f6d2e636f";
                //xmldoc.PreserveWhitespace = true;

                xmldoc.LoadXml(xml);

                xmldoc.SignWithXades2SHA256(_certificateContent, "");


                return xmldoc.OuterXml;
            }
            catch (Exception ex)
            {
                var logger = new GlobalLogger("SIGNXML", Guid.NewGuid().ToString()) { Message = ex.Message, StackTrace = ex.StackTrace };
                tableGlobalLogger.InsertOrUpdate(logger);
            }

            return null;
        }

        #region PERUSIGNXML
        //public static string SignXml(X509Certificate2 x509, string xml, List<string> xmlElementsToSign)
        //{
        //    try
        //    {
        //        CspParameters csp = null;
        //        RSAParameters rsaparameters = new RSAParameters();

        //        if (x509.HasPrivateKey)
        //        {
        //            string privateKey = x509.PrivateKey.ToXmlString(true);
        //            RSACryptoServiceProvider key = new RSACryptoServiceProvider();
        //            key.FromXmlString(privateKey);
        //            rsaparameters = key.ExportParameters(true);
        //        }

        //        SecureXmlDocument secureXml = new SecureXmlDocument
        //        {
        //            PreserveWhitespace = true
        //        };

        //        secureXml.LoadXml(xml);

        //        foreach (var xmlElementToSign in xmlElementsToSign)
        //        {
        //            secureXml.SignWithRsaSha1(xmlElementToSign, 0, csp, rsaparameters, x509, x509.HasPrivateKey);
        //            bool b = secureXml.CheckSignature(xmlElementToSign);
        //        }
        //        return secureXml.OuterXml;
        //    }

        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error al firmar xml.", ex);
        //    }

        //} 
        #endregion

        public static XmlDocument GetNodeToSign(string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var xmlReader = new XmlTextReader(new MemoryStream(Encoding.GetEncoding("ISO-8859-1").GetBytes(xml))) { Namespaces = true };

            XPathDocument document = new XPathDocument(xmlReader);
            XPathNavigator navigator = document.CreateNavigator();
            XPathNavigator navNs = document.CreateNavigator();
            navNs.MoveToFollowing(XPathNodeType.Element);
            IDictionary<string, string> nameSpaceList = navNs.GetNamespacesInScope(XmlNamespaceScope.All);
            XmlNamespaceManager ns = new XmlNamespaceManager(xmlDocument.NameTable);
            foreach (var nsItem in nameSpaceList)
            {
                if (string.IsNullOrEmpty(nsItem.Key))
                    ns.AddNamespace("sig", nsItem.Value);
                else
                    ns.AddNamespace(nsItem.Key, nsItem.Value);
            }
            ns.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
            foreach (var nsItem in nameSpaceList)
            {
                if (string.IsNullOrEmpty(nsItem.Key))
                    ns.AddNamespace("sig", nsItem.Value);
                else
                    ns.AddNamespace(nsItem.Key, nsItem.Value);
            }
            return xmlDocument;
        }
    }
}
