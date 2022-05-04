using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;



namespace Gosocket.Dian.Application
{
    public class GlobalRadianContributorEnabledService : IGlobalRadianContributorEnabledService
    {
        private readonly ITableManager globalRadianContributorEnabled = new TableManager("GlobalRadianContributorEnabled");

        public GlobalRadianContributorEnabledService() { }

        public GlobalRadianContributorEnabledService(ITableManager _globalRadianContributorEnabled)
        {
            globalRadianContributorEnabled = _globalRadianContributorEnabled;
        }

        public bool Insert(GlobalRadianContributorEnabled item)
        {
            GlobalRadianContributorEnabled radianEnabled = new GlobalRadianContributorEnabled(item.PartitionKey, item.RowKey)
            {
                IsActive = item.IsActive,
                UpdateBy = item.UpdateBy
            };

            return globalRadianContributorEnabled.InsertOrUpdate(radianEnabled);
        }
    }
}
