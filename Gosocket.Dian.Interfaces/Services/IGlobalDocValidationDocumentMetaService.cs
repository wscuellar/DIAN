using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IGlobalDocValidationDocumentMetaService
    {
        List<GlobalDocValidatorDocumentMeta> OtherEvents(string documentKey, string eventCode);
        List<GlobalDocReferenceAttorney> ReferenceAttorneys(string documentKey, string documentReferencedKey, string receiverCode, string senderCode);
        GlobalDocValidatorDocumentMeta DocumentValidation(string reference);
    }
}
