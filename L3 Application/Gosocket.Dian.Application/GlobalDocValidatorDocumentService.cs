using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;

namespace Gosocket.Dian.Application
{
    public class GlobalDocValidatorDocumentService : IGlobalDocValidatorDocumentService
    {
        private readonly ITableManager globalDocValidatorDocumentTableManager = new TableManager("GlobalDocValidatorDocument");

        public GlobalDocValidatorDocumentService() { }

        public GlobalDocValidatorDocumentService(ITableManager _globalDocValidatorDocumentTableManager)
        {
            globalDocValidatorDocumentTableManager = _globalDocValidatorDocumentTableManager;
        }


        public GlobalDocValidatorDocument EventVerification(GlobalDocValidatorDocumentMeta eventItem)
        {
            return globalDocValidatorDocumentTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier, eventItem.PartitionKey);
        }

        public GlobalDocValidatorDocument FindByGlobalDocumentId(string globalDocumentId)
        {
            return globalDocValidatorDocumentTableManager.FindByGlobalDocumentId<GlobalDocValidatorDocument>(globalDocumentId);
        }

        public GlobalDocValidatorDocument DocumentValidation(string reference)
        {
            return globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(reference, reference);
        }
    }
}
