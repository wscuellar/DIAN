using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Web.Models
{
    public class InvoiceNotesViewModel
    {
        public GlobalDocValidatorDocument GlobalDocValidatorDocument { get; }
        public List<GlobalDocValidatorDocumentMeta> ListGlobalDocValidatorDocumentMetas { get; }
        public List<DocValidatorModel> ListDocValidatorModels { get; }
        public Dictionary<int, string> IconsData { get; }

        public InvoiceNotesViewModel(GlobalDocValidatorDocument globalDocValidatorDocument, List<GlobalDocValidatorDocumentMeta> listGlobalDocValidatorDocumentMetas, List<DocValidatorModel> docValidatorModels, Dictionary<int, string> iconsData)
        {
            GlobalDocValidatorDocument = globalDocValidatorDocument;
            ListGlobalDocValidatorDocumentMetas = listGlobalDocValidatorDocumentMetas;
            ListDocValidatorModels = docValidatorModels;
            IconsData = iconsData;
        }
    }
}