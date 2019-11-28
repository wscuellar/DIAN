using System;

namespace Gosocket.Dian.Domain.KeyVault
{
    public class ImportCertificateResult : BaseResult
    {
        public DateTime? NotBefore { get; set; }
        public DateTime? Expires { get; set; }
    }
}
