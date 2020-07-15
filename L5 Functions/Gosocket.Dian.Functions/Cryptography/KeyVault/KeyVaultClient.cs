using Gosocket.Dian.Infrastructure;
using Newtonsoft.Json;

namespace Gosocket.Dian.Functions.Cryptography.KeyVault
{
    public class KeyVaultClient
    {
        private readonly KeyVaultManager _manager;

        public KeyVaultClient(string vaultAddress, string clientId, string clientSecret)
        {
            _manager = new KeyVaultManager(vaultAddress, clientId, clientSecret);
        }

        public string DeleteCertificate(string name)
        {
            var result = _manager.DeleteCertificate(name);

            return JsonConvert.SerializeObject(result);
        }

        public string ExportCertificate(string name)
        {
            var result = _manager.ExportCertificate(name);

            return JsonConvert.SerializeObject(result);
        }

        public string ImportCertificate(byte[] content, string name, string password)
        {
            var result = _manager.ImportCertificate(content, name, password);

            return JsonConvert.SerializeObject(result);
        }

    }
}
