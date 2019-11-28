using Gosocket.Dian.Infrastructure;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Managers
{
    public class CategoryManager
    {
        public static string PartitionKey = "Category";
        private static readonly TableManager TableManager = new TableManager("GlobalDocValidatorCategory");

        public bool Insert(string code, string name, string description)
        {
            return Insert(new Domain.Entity.GlobalDocValidatorCategory
            {
                PartitionKey = PartitionKey,
                RowKey = code,
                Code = code,
                Name = name,
                Description = description
            });
        }

        public bool Insert(Domain.Entity.GlobalDocValidatorCategory category)
        {
            return TableManager.Insert(category);
        }

        public IEnumerable<Domain.Entity.GlobalDocValidatorCategory> GetAll()
        {
            return TableManager.FindByPartition<Domain.Entity.GlobalDocValidatorCategory>(PartitionKey);
        }
    }
}
