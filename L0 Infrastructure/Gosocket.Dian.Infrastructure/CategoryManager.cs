using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public class CategoryManager
    {
        public static string PartitionKey = "Category";
        private static readonly TableManager TableManager = new TableManager("GlobalDocValidatorCategory");

        public bool Insert(string code, string name, string description, string xpathCondition)
        {
            return Insert(new Domain.Entity.GlobalDocValidatorCategory
            {
                PartitionKey = PartitionKey,
                RowKey = code,
                Code = code,
                Name = name,
                Description = description,
                XpathCondition = xpathCondition,
                Active = true,
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
