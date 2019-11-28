using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalBatchFileRuntime : TableEntity
    {
        public GlobalBatchFileRuntime() { }

        public GlobalBatchFileRuntime(string pk, string rk, string fileName) : base(pk, rk)
        {
            FileName = fileName;
        }

        public string FileName { get; set; }
    }
}
