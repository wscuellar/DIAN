namespace Gosocket.Dian.Domain.KeyVault
{
    public class ExportCertificatResult : BaseResult
    {
        public string Base64Data { get; set; }
        public string Password { get; set; }
    }
}
