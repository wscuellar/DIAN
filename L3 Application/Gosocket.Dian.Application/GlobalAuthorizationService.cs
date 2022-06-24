using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;

namespace Gosocket.Dian.Application
{
    public class GlobalAuthorizationService : IGlobalAuthorizationService
    {
        private readonly ITableManager globalAuthorizationTableManager = new TableManager("GlobalAuthorization");
        public GlobalAuthorizationService() { }

        public GlobalAuthorizationService(ITableManager _globalAuthorizationTableManager) { globalAuthorizationTableManager = _globalAuthorizationTableManager; }

        public GlobalAuthorization Find(string partitionKey, string rowKey)
        {
            return this.globalAuthorizationTableManager.Find<GlobalAuthorization>(partitionKey, rowKey);
        }

        public bool InsertOrUpdate(GlobalAuthorization entity)
        {
            return this.globalAuthorizationTableManager.InsertOrUpdate(entity);
        }
    }
}
