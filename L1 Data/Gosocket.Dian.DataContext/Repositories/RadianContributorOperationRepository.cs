using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorOperationRepository : IRadianContributorOperationRepository
    {
        private readonly SqlDBContext sqlDBContext;
        private ResponseMessage responseMessage;

        public RadianContributorOperationRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public List<RadianContributorOperation> List(Expression<Func<RadianContributorOperation, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorOperations.Where(expression);
            return query.ToList();
        }

        public RadianContributorOperation Get(Expression<Func<RadianContributorOperation, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorOperations.Where(expression);
            return query.FirstOrDefault();
        }

        public ResponseMessage Update(int radianContributorOperationId)
        {
            using (var context = new SqlDBContext())
            {
                var radianContributorOperationInstance = context.RadianContributorOperations
                    .FirstOrDefault(c => c.Id == radianContributorOperationId);

                if (radianContributorOperationInstance != null)
                {
                    radianContributorOperationInstance.Deleted = true;
                    context.Entry(radianContributorOperationInstance).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    responseMessage = new ResponseMessage("Datos actuzalizados corresctamente", "Actualizado");
                }
                else
                {
                    responseMessage = new ResponseMessage("Registro no encontrado en la base de datos", "Nulo");
                }

                return responseMessage;
            }
        }
    }
}
