using Gosocket.Dian.Domain.Sql;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces
{
    public interface IElectronicDocumentService
    {
        List<ElectronicDocument> GetElectronicDocuments();
        int InsertElectronicDocuments(ElectronicDocument electronicDocument);

        string GetNameById(int id);
    }
}
