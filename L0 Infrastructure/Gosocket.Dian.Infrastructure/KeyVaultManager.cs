using Gosocket.Dian.Domain.KeyVault;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public class KeyVaultManager
    {
        private KeyVaultClient _keyVaultClient;
        private readonly string _vaultAddress;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public KeyVaultManager(string vaultAddress, string clientId, string clientSecret)
        {
            _vaultAddress = vaultAddress;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _keyVaultClient = GetClient();
        }

        private KeyVaultClient GetClient()
        {
            if (_keyVaultClient != null) return _keyVaultClient;

            var clientCredential = new ClientCredential(_clientId, _clientSecret);

            _keyVaultClient = new KeyVaultClient(
                (authority, resource, scope) => GetAccessToken(authority, resource, clientCredential),
                new HttpMessageHandler());

            return _keyVaultClient;
        }

        private static async Task<string> GetAccessToken(string authority, string resource,
            ClientCredential clientCredential)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, clientCredential).ConfigureAwait(false);

            return result.AccessToken;
        }

        public DeleteCertificateResult DeleteCertificate(string name)
        {
            var result = new DeleteCertificateResult
            {
                Success = true,
                Name = name
            };

            try
            {
                if (string.IsNullOrEmpty(name)) throw new Exception("Parameter name can't be null or empty");

                var client = GetClient();
                Task
                    .Run(() => client.DeleteCertificateAsync(_vaultAddress, name)).ConfigureAwait(false)
                    .GetAwaiter().GetResult();

                #region Delete secret for password

                DeleteSecret(name);

                #endregion
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Error = e.Message;
            }

            return result;
        }

        public ImportCertificateResult ImportCertificate(byte[] content, string name, string password)
        {
            var result = new ImportCertificateResult
            {
                Success = true,
                Name = name
            };

            try
            {
                if (string.IsNullOrEmpty(name)) throw new System.Exception("Parameter name can't be null or empty");

                var x509Collection = new X509Certificate2Collection();
                x509Collection.Import(content, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

                // A pfx can contain a chain            
                var cert = x509Collection.Cast<X509Certificate2>().Single(s => s.HasPrivateKey);
                using (cert.GetRSAPrivateKey()) { }

                if (DateTime.UtcNow < cert.NotBefore)
                {
                    result.Success = false;
                    result.Error = "Certificado aún no se encuentra vigente.";
                    return result;
                }

                if (DateTime.UtcNow > cert.NotAfter)
                {
                    result.Success = false;
                    result.Error = "Certificado se encuentra expirado.";
                    return result;
                }

                var x509Bytes = cert.Export(X509ContentType.Pfx, password);
                var base64X509 = Convert.ToBase64String(x509Bytes);

                var policy = new CertificatePolicy
                {
                    KeyProperties = new KeyProperties
                    {
                        Exportable = true,
                        KeyType = "RSA",
                        KeySize = 2048
                    },
                    SecretProperties = new SecretProperties
                    {
                        ContentType = CertificateContentType.Pfx
                    }
                };

                var client = GetClient();
                var certificateName = $"{name}";
                var certificateBundle = Task.Run(() => client.ImportCertificateAsync(_vaultAddress, certificateName, base64X509, password, policy)).ConfigureAwait(false).GetAwaiter().GetResult();

                #region Create secret for password

                //CreateSecret(name, password, "plaintext", new Dictionary<string, string>
                //{
                //    {"purpose", $"Password of certificate {certificateName}"}
                //}, certificateBundle.Attributes.NotBefore, certificateBundle.Attributes.Expires);

                #endregion

                result.Expires = certificateBundle.Attributes.Expires;
                result.NotBefore = certificateBundle.Attributes.NotBefore;
            }
            catch (KeyVaultErrorException e)
            {
                result.Success = false;
                result.Error = e.Body.Error.InnerError?.Code == "KeySizeNotSupported"
                    ? "KeySizeNotSupported" + " " + e.Body.Error.InnerError?.Message
                    : e.Message;
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Error = e.Message;
            }

            return result;
        }

        public ExportCertificatResult ExportCertificate(string name)
        {
            var result = new ExportCertificatResult
            {
                Success = true,
                Name = name
            };

            try
            {
                if (string.IsNullOrEmpty(name)) throw new System.Exception("Parameter name can't be null or empty");

                var client = GetClient();
                var certContentSecret =
                //Task.Run(() => client.GetSecretAsync(_vaultAddress, name.StartsWith("KV-CERTIFICATE-") ? name : $"KV-CERTIFICATE-{name}"))
                //    .ConfigureAwait(false).GetAwaiter().GetResult();
                Task.Run(() => client.GetSecretAsync(_vaultAddress, name))
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                //var certContentSecret =
                //    Task.Run(() => client.GetCertificateAsync(_vaultAddress, name.StartsWith("KV-CERTIFICATE-") ? name : $"KV-CERTIFICATE-{name}"))
                //        .ConfigureAwait(false).GetAwaiter().GetResult();

                // Certificates can be exported in a mutiple formats (PFX, PEM).
                // Use the content type to determine how to strongly-type the certificate for the platform
                // The exported certificate doesn't have a password
                //if (0 != string.CompareOrdinal(certContentSecret.ContentType, CertificateContentType.Pfx)) return null;
                //var exportedCertCollection = new X509Certificate2Collection();
                //exportedCertCollection.Import(Convert.FromBase64String(certContentSecret.Value));
                //var certificate = exportedCertCollection.Cast<X509Certificate2>().Single(s => s.HasPrivateKey);

                #region Recovery password

                var password = "";//GetSecret(name).Value;

                #endregion

                result.Base64Data = certContentSecret.Value;
                result.Password = password;
            }
            catch (System.Exception e)
            {
                result.Success = false;
                result.Error = e.Message;
            }

            return result;
        }

        public DeleteSecretResult DeleteSecret(string name)
        {
            var result = new DeleteSecretResult
            {
                Success = true,
                Name = name
            };

            try
            {
                if (string.IsNullOrEmpty(name)) throw new System.Exception("Parameter name can't be null or empty");

                var client = GetClient();
                Task
                    .Run(() => client.DeleteSecretAsync(_vaultAddress, name).ConfigureAwait(false).GetAwaiter()
                    .GetResult());
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Error = e.Message;
            }

            return result;
        }
    }
}
