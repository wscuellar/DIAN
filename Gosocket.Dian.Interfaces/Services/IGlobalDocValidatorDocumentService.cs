using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IGlobalDocValidatorDocumentService
    {
        GlobalDocValidatorDocument EventVerification(GlobalDocValidatorDocumentMeta eventItem);
        GlobalDocValidatorDocument FindByGlobalDocumentId(string globalDocumentId);
        GlobalDocValidatorDocument DocumentValidation(string reference);
    }
}
