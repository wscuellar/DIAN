using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalBatchFileMapper : TableEntity
    {
        public GlobalBatchFileMapper(){ }

        public GlobalBatchFileMapper(string pk, string rk) : base(pk, rk)
        {
        }
    }
}
