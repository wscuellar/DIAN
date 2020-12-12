using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IGlobalDocValidatorDocumentService
    {
        GlobalDocValidatorDocument EventVerification(string eventItemIdentifier);
        GlobalDocValidatorDocument FindByGlobalDocumentId(string globalDocumentId);
    }
}
