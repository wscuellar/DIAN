using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalBatchFileStatus : TableEntity
    {
        public GlobalBatchFileStatus() { }

        public GlobalBatchFileStatus(string pk, string rk) : base(pk, rk)
        {
        }

        public string AuthCode { get; set; }
        public string FileName { get; set; }
        public string StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string ZipKey { get; set; }
    }
}
