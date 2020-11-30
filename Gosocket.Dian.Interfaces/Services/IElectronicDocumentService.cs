using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces
{
    public interface IElectronicDocumentService
    {
        List<ElectronicDocument> GetElectronicDocuments();
        int InsertElectronicDocuments(ElectronicDocument electronicDocument);
    }
}
