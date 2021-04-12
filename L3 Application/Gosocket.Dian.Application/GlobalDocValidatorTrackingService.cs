using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class GlobalDocValidatorTrackingService : IGlobalDocValidatorTrackingService
    {
        private readonly ITableManager globalDocValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");

        public GlobalDocValidatorTrackingService() { }

        public GlobalDocValidatorTrackingService(ITableManager _globalDocValidatorTrackingTableManager) {
            globalDocValidatorTrackingTableManager = _globalDocValidatorTrackingTableManager;
        }

        public List<Domain.Entity.GlobalDocValidatorTracking> ListTracking(string eventDocumentKey)
        {
            return globalDocValidatorTrackingTableManager.FindByPartition<Domain.Entity.GlobalDocValidatorTracking>(eventDocumentKey);
        }
    }
}
