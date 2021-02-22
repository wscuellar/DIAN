using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class ElectronicDocumentService : IElectronicDocumentService
    {
        /// <summary>
        /// Listar los documentos electronicos registrados
        /// </summary>
        /// <returns></returns>
        public List<ElectronicDocument> GetElectronicDocuments()
        {
            return new SqlDBContext().ElectronicDocuments.ToList();
        }

        /// <summary>
        /// Insertar un nuevo documento electronico
        /// </summary>
        /// <param name="electronicDocument"></param>
        /// <returns></returns>
        public int InsertElectronicDocuments(ElectronicDocument electronicDocument)
        {
            int result = 0;
            using (var context = new SqlDBContext())
            {
                context.Entry(electronicDocument).State = System.Data.Entity.EntityState.Added;
                context.SaveChanges();

                result = electronicDocument.Id;
            }

            return result;
        }

        public string GetNameById(int id)
        {
            return new SqlDBContext().ElectronicDocuments.FirstOrDefault(x => x.Id == id).Name;
        }
    }
}