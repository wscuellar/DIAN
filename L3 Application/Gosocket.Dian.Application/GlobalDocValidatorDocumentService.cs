using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;

namespace Gosocket.Dian.Application
{
    public class GlobalDocValidatorDocumentService : IGlobalDocValidatorDocumentService
    {
        private readonly TableManager globalDocValidatorDocumentTableManager = new TableManager("GlobalDocValidatorDocument");

        public GlobalDocValidatorDocument EventVerification(string eventItemIdentifier)
        {
            return globalDocValidatorDocumentTableManager.Find<GlobalDocValidatorDocument>(eventItemIdentifier, eventItemIdentifier);
        }
    }
}
