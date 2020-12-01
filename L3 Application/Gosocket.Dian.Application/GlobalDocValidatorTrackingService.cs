using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class GlobalDocValidatorTrackingService : IGlobalDocValidatorTrackingService
    {
        private readonly TableManager globalDocValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");

        public List<Domain.Entity.GlobalDocValidatorTracking> ListTracking(string eventDocumentKey)
        {
             return globalDocValidatorTrackingTableManager.FindByPartition<Domain.Entity.GlobalDocValidatorTracking>(eventDocumentKey);
        }
    }
}
