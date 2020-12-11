using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{
    public class InvoiceNotesViewModel
    {
        public GlobalDocValidatorDocument GlobalDocValidatorDocument { get; }
        public List<GlobalDocValidatorDocumentMeta> ListGlobalDocValidatorDocumentMetas { get; }

        public InvoiceNotesViewModel(GlobalDocValidatorDocument globalDocValidatorDocument, List<GlobalDocValidatorDocumentMeta> listGlobalDocValidatorDocumentMetas)
        {
            GlobalDocValidatorDocument = globalDocValidatorDocument;
            ListGlobalDocValidatorDocumentMetas = listGlobalDocValidatorDocumentMetas;
        }
    }
}