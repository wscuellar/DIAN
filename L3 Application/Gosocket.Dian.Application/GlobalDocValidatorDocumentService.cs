using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;

namespace Gosocket.Dian.Application
{
    public class GlobalDocValidatorDocumentService : IGlobalDocValidatorDocumentService
    {
        private readonly TableManager globalDocValidatorDocumentTableManager = new TableManager("GlobalDocValidatorDocument");

        public GlobalDocValidatorDocument EventVerification(GlobalDocValidatorDocumentMeta eventItem)
        {
            return globalDocValidatorDocumentTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier, eventItem.PartitionKey);
        }

        public GlobalDocValidatorDocument FindByGlobalDocumentId(string globalDocumentId)
        {
            return globalDocValidatorDocumentTableManager.FindByGlobalDocumentId<GlobalDocValidatorDocument>(globalDocumentId);
        }
    }
}
