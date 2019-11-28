using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocValidatorRuntime : TableEntity
    {
        public GlobalDocValidatorRuntime(string pk, string rk, string category, string documentTypeId, string fileName) : base(pk, rk)
        {
            Category = category;
            FileName = fileName;
            DocunentTypeId = documentTypeId;
        }
        public GlobalDocValidatorRuntime(string pk, string rk, string category) : base(pk, rk)
        {
            Category = category;
        }
        public GlobalDocValidatorRuntime()
        {

        }

        public string Category { get; set; }
        public string FileName { get; set; }
        public string DocunentTypeId { get; set; }
    }
}
