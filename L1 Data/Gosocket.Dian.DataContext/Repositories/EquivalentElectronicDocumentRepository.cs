using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class EquivalentElectronicDocumentRepository : IEquivalentElectronicDocumentRepository
    {
        public EquivalentElectronicDocument GetEquivalentElectronicDocument(int id)
        {
            using (var context = new SqlDBContext())
            {
                return context.EquivalentElectronicDocuments.Find(id);
            }
        }

        public List<EquivalentElectronicDocument> GetEquivalentElectronicDocuments()
        {
            using (var context = new SqlDBContext())
            {
                return context.EquivalentElectronicDocuments.ToList();
            }
        }
    }
}
