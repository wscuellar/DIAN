using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorOperationRepository : IRadianContributorOperationRepository
    {
        private readonly SqlDBContext _sqlDBContext;
        private ResponseMessage responseMessage;

        public RadianContributorOperationRepository()
        {
            if (_sqlDBContext == null)
                _sqlDBContext = new SqlDBContext();
        }

        public List<RadianContributorOperation> List(Expression<Func<RadianContributorOperation, bool>> expression)
        {
            var query = _sqlDBContext.RadianContributorOperations.Include("RadianContributor").Include("Software")
                .Where(expression);
            return query.ToList();
        }

        public RadianContributorOperation Get(Expression<Func<RadianContributorOperation, bool>> expression)
        {
            var query = _sqlDBContext.RadianContributorOperations.Include("RadianContributor").Include("Software")
                .Where(expression);
            return query.FirstOrDefault();
        }

        public ResponseMessage Delete(int radianContributorOperationId)
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

                    responseMessage = new ResponseMessage("Datos actualizados correctamente", "Actualizado");
                }
                else
                {
                    responseMessage = new ResponseMessage("Registro no encontrado en la base de datos", "Nulo");
                }

                return responseMessage;
            }
        }

        public bool Update(RadianContributorOperation contributorOperation)
        {
            using (SqlDBContext context = new SqlDBContext())
            {
                RadianContributorOperation radianContributorOperationInstance = context.RadianContributorOperations.FirstOrDefault(c => c.Id == contributorOperation.Id);

                radianContributorOperationInstance.OperationStatusId = contributorOperation.OperationStatusId;
                radianContributorOperationInstance.RadianContributorId = contributorOperation.RadianContributorId;
                radianContributorOperationInstance.SoftwareId = contributorOperation.SoftwareId;
                radianContributorOperationInstance.SoftwareType = contributorOperation.SoftwareType;
                radianContributorOperationInstance.Deleted = contributorOperation.Deleted;
                radianContributorOperationInstance.Timestamp = System.DateTime.Now;
                context.Entry(radianContributorOperationInstance).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                return true;
            }
        }


        public int Add(RadianContributorOperation contributorOperation)
        {
            int affectedRecords = 0;
            using (var context = new SqlDBContext())
            {
                context.RadianContributorOperations.Add(contributorOperation);
                affectedRecords = context.SaveChanges();
            }
            return affectedRecords > 0 ? contributorOperation.Id : 0;
        }
    }
}
