using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Gosocket.Dian.Functions.Cryptography.Signer
{
    public class KeyManager
    {
        public RSAParameters RsaParams { get { return _rsaprms; } }
        private readonly RSAParameters _rsaprms;

        public X509Certificate X509Certificate { get { return _x509Cert; } }
        private readonly X509Certificate _x509Cert;

        public CspParameters CspParams { get; }

        public bool KeyInFile { get; }

        public string X509CertFile { get; }

        public string X509CertPassword { get; }

        public RSACryptoServiceProvider RSAKey { get; }

        public KeyManager()
        {
            CspParams = GetCspParametersFromUser(ref _x509Cert, ref _rsaprms);

            RSAKey = new RSACryptoServiceProvider(CspParams);
            RSAKey.ImportParameters(_rsaprms);
        }

        public KeyManager(string certFile, string password)
        {
            GetRSAParametersFromX509File(ref _x509Cert, ref _rsaprms, certFile, password);

            var csp = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore };
            RSAKey = new RSACryptoServiceProvider(csp);
            RSAKey.ImportParameters(_rsaprms);

            X509CertFile = certFile;
            X509CertPassword = password;

            KeyInFile = true;
        }

        public KeyManager(byte[] certContent, string password)
        {
            GetRSAParametersFromX509File(ref _x509Cert, ref _rsaprms, certContent, password);

            var csp = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore };
            RSAKey = new RSACryptoServiceProvider(csp);
            RSAKey.ImportParameters(_rsaprms);

            X509CertPassword = password;

            KeyInFile = true;
        }

        public KeyManager(string x509CertInfo)
        {
            GetRSAPublicParametersFromX509String(ref _x509Cert, ref _rsaprms, x509CertInfo);

            var csp = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore };
            RSAKey = new RSACryptoServiceProvider(csp);
            RSAKey.ImportParameters(_rsaprms);
        }

        internal CspParameters GetCspParametersFromUser(ref X509Certificate x509Certificate, ref RSAParameters rsaParameters)
        {
            //getting certificate information
            var csp = new CspParameters();
            X509Certificate2 selected;

            var st = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                st.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection selectedCerts;
                switch (st.Certificates.Count)
                {
                    case 0:
                        selected = null;
                        break;

                    case 1:
                        selectedCerts = st.Certificates;
                        selected = selectedCerts[0];
                        break;

                    default:
                        try
                        {
                            selectedCerts = X509Certificate2UI.SelectFromCollection(st.Certificates,
                                "Signature Server Web Signer",
                                "Seleccione el Certificado para firmar digitalmente el documento.",
                                X509SelectionFlag.SingleSelection);

                            selected = selectedCerts[0];
                        }
                        catch
                        {
                            return csp;
                        }
                        break;
                }
            }
            finally
            {
                st.Close();
            }

            if (selected != null)
            {
                x509Certificate = new X509Certificate(selected);
                var aa = selected.PrivateKey;
                var caa = (ICspAsymmetricAlgorithm)aa;
                csp.ProviderName = caa.CspKeyContainerInfo.ProviderName;
                csp.ProviderType = caa.CspKeyContainerInfo.ProviderType;
                csp.KeyContainerName = caa.CspKeyContainerInfo.KeyContainerName;

                if (selected.HasPrivateKey)
                {
                    var privatekey = selected.PrivateKey.ToXmlString(true);
                    var key = new RSACryptoServiceProvider();
                    key.FromXmlString(privatekey);
                    rsaParameters = key.ExportParameters(true);
                }
                else
                    rsaParameters = new RSAParameters();
            }

            //check expiration date
            if (Convert.ToDateTime(x509Certificate.GetExpirationDateString()) < DateTime.Now)
                throw new Exception("Certificado invalido, expiracion: "
                                    + Convert.ToDateTime(x509Certificate.GetExpirationDateString()).ToString("dd-MM-yyyy"));

            return csp;
        }

        internal void GetRSAParametersFromX509File(ref X509Certificate x509Certificate, ref RSAParameters rsaprms, string filename, string password)
        {
            var cert = new X509Certificate2(filename, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            var privatekey = cert.PrivateKey.ToXmlString(true);
            var key = new RSACryptoServiceProvider();
            key.FromXmlString(privatekey);
            rsaprms = key.ExportParameters(true);
            x509Certificate = new X509Certificate(cert);
        }

        internal void GetRSAParametersFromX509File(ref X509Certificate x509Certificate, ref RSAParameters rsaprms, byte[] certContent, string password)
        {
            var cert = new X509Certificate2(certContent, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            var privatekey = cert.PrivateKey.ToXmlString(true);
            var key = new RSACryptoServiceProvider();
            key.FromXmlString(privatekey);
            rsaprms = key.ExportParameters(true);
            x509Certificate = new X509Certificate(cert);
        }

        internal void GetRSAPublicParametersFromX509File(ref X509Certificate x509Certificate, ref RSAParameters rsaprms, string filename)
        {
            var cert = new X509Certificate2(filename);
            var privatekey = cert.PrivateKey.ToXmlString(false);
            var key = new RSACryptoServiceProvider();
            key.FromXmlString(privatekey);
            rsaprms = key.ExportParameters(true);
            x509Certificate = new X509Certificate(cert);
        }

        internal void GetRSAPublicParametersFromX509String(ref X509Certificate x509Certificate, ref RSAParameters rsaprms, string base64)
        {
            var cert = new X509Certificate2(Convert.FromBase64String(base64));
            var privatekey = cert.PrivateKey.ToXmlString(false);
            var key = new RSACryptoServiceProvider();
            key.FromXmlString(privatekey);
            rsaprms = key.ExportParameters(true);
            x509Certificate = new X509Certificate(cert);
        }

        internal bool CompareCertSubject(string subj1, string subj2)
        {
            var parts = subj1.Split(char.Parse(","));
            return parts.All(obj => subj2.Trim().IndexOf(obj.Trim(), StringComparison.Ordinal) != -1);
        }

        public static DateTime GetCertificateExpirtationDate(string certFile, string password)
        {
            var cert = new X509Certificate2(certFile, password);
            return Convert.ToDateTime(cert.GetExpirationDateString());
        }

        public static DateTime GetCertificateExpirtationDate(byte[] certContent, string password)
        {
            var cert = new X509Certificate2(certContent, password);
            return (Convert.ToDateTime(cert.GetExpirationDateString()));
        }

        public static X509Certificate GetCertificate(string certFile)
        {
            var cert = new X509Certificate2(certFile);
            return cert;
        }

        public static X509Certificate GetCertificateFromPfx(string certFile, string password)
        {
            var cert = new X509Certificate2(certFile, password);
            return cert;
        }
    }
}
