using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalBatchFileFailed : TableEntity
    {
        public GlobalBatchFileFailed() { }

        public GlobalBatchFileFailed(string pk, string rk) : base(pk, rk)
        {
        }

        public string DocumentKey { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }
        public string ZipKey { get; set; }

    }
}
