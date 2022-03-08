using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IEquivalentElectronicDocumentRepository
    {
        List<EquivalentElectronicDocument> GetEquivalentElectronicDocuments();
        EquivalentElectronicDocument GetEquivalentElectronicDocument(int id);
    }
}
