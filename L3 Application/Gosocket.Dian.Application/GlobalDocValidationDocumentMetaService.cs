using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class GlobalDocValidationDocumentMetaService : IGlobalDocValidationDocumentMetaService
    {
        private readonly TableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        private readonly TableManager documentTableManager = new TableManager("GlobalDocValidatorDocument");
        private readonly TableManager ReferenceAttorneyTableManager = new TableManager("GlobalDocReferenceAttorney");

        //Se utiliza en invoice, eventitem, referenceMeta
        public GlobalDocValidatorDocumentMeta DocumentValidation(string reference)
        {
            return documentMetaTableManager.Find<GlobalDocValidatorDocumentMeta>(reference, reference);
        }

        public List<GlobalDocReferenceAttorney> ReferenceAttorneys(string documentKey, string documentReferencedKey, string receiverCode, string senderCode)
        {
            return new List<GlobalDocReferenceAttorney>() { ReferenceAttorneyTableManager.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(documentKey) };
        }

        public List<GlobalDocValidatorDocumentMeta> GetAssociatedDocuments(string documentKey, string eventCode)
        {
            return documentMetaTableManager
                .FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(documentKey, eventCode);
        }

        //FindDocumentReferenced_TypeId
        public List<GlobalDocValidatorDocumentMeta> FindReferencedDocuments(string documentReferencedKey, string documentType)
        {
            return documentMetaTableManager
                .FindDocumentReferenced_TypeId<GlobalDocValidatorDocumentMeta>(documentReferencedKey, documentType);
        }

        //Find All referenced documents
        public List<GlobalDocValidatorDocumentMeta> FindDocumentByReference(string documentReferencedKey)
        {
            return documentMetaTableManager
                .FindDocumentByReference<GlobalDocValidatorDocumentMeta>(documentReferencedKey);
        } 

        public GlobalDocValidatorDocument EventValidator(GlobalDocValidatorDocumentMeta eventItem)
        {
            return documentTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier, eventItem.PartitionKey);
        }
    }
}
