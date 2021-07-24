using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class GlobalDocValidationDocumentMetaService : IGlobalDocValidationDocumentMetaService
    {
        private readonly ITableManager documentMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
        private readonly ITableManager documentTableManager = new TableManager("GlobalDocValidatorDocument");
        private readonly ITableManager ReferenceAttorneyTableManager = new TableManager("GlobalDocReferenceAttorney");

        public GlobalDocValidationDocumentMetaService() { }

        public GlobalDocValidationDocumentMetaService(ITableManager _documentMetaTableManager, ITableManager _documentTableManager,  ITableManager _ReferenceAttorneyTableManager) {
            documentMetaTableManager = _documentMetaTableManager;
            documentTableManager = _documentTableManager;
            ReferenceAttorneyTableManager = _ReferenceAttorneyTableManager;
        }

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
                .FindpartitionKey<GlobalDocValidatorDocumentMeta>(documentKey).Where(x => x.EventCode == eventCode).ToList();
        }

        public GlobalDocValidatorDocument EventValidator(GlobalDocValidatorDocumentMeta eventItem)
        {
            return documentTableManager.FindByDocumentKey<GlobalDocValidatorDocument>(eventItem.Identifier, eventItem.Identifier, eventItem.PartitionKey);
        }
    }
}
