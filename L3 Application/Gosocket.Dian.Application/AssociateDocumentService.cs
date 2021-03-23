using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;
using Gosocket.Dian.Infrastructure;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class AssociateDocumentService : IAssociateDocuments
    {
        private static readonly TableManager TableManagerGlobalDocAssociate = new TableManager("GlobalDocAssociate");
        
        public List<GlobalDocValidatorDocumentMeta> GetEventsByTrackId(string trackId)
        {
            List<GlobalDocValidatorDocumentMeta> responses = new List<GlobalDocValidatorDocumentMeta>();

            List<GlobalDocAssociate> associateDocumentList = TableManagerGlobalDocAssociate.FindpartitionKey<GlobalDocAssociate>(trackId.ToLower()).ToList();
            if (!associateDocumentList.Any())
            {
                return null;
            }

            foreach (var item in associateDocumentList)
            {

            }

            return responses;
        }
    }
}
