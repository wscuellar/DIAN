using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class OthersDocsElecContributorOperationRepository : IOthersDocsElecContributorOperationRepository
    {
        private readonly SqlDBContext _sqlDBContext;
        private ResponseMessage responseMessage;

        public OthersDocsElecContributorOperationRepository()
        {
            if (_sqlDBContext == null)
                _sqlDBContext = new SqlDBContext();
        }

        public List<OtherDocElecContributorOperations> List(Expression<Func<OtherDocElecContributorOperations, bool>> expression)
        {
            var query = _sqlDBContext.OtherDocElecContributorOperations.Include("OtherDocElecContributor").Include("Software")
                .Where(expression);
            return query.ToList();
        }

        public OtherDocElecContributorOperations Get(Expression<Func<OtherDocElecContributorOperations, bool>> expression)
        {
            var query = _sqlDBContext.OtherDocElecContributorOperations.Include("OtherDocElecContributor").Include("Software")
                .Include(t => t.OtherDocElecContributor.Contributor.OtherDocElecContributors)
                .Where(expression);
            return query.FirstOrDefault();
        }

        public ResponseMessage Delete(int ContributorOperationId)
        {
            using (var context = new SqlDBContext())
            {
                var ContributorOperationInstance = context. OtherDocElecContributorOperations
                    .FirstOrDefault(c => c.Id == ContributorOperationId);

                if (ContributorOperationInstance != null)
                {
                    ContributorOperationInstance.Deleted = true;
                    context.Entry(ContributorOperationInstance).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    responseMessage = new ResponseMessage("Datos actualizados correctamente", "Actualizado");
                }
                else
                {
                    responseMessage = new ResponseMessage("Registro no encontrado en la base de datos", "Nulo", 500);
                }

                return responseMessage;
            }
        }

        public bool Update(OtherDocElecContributorOperations contributorOperation)
        {
            using (var context = new SqlDBContext())
            {
                var ContributorOperationInstance = context.OtherDocElecContributorOperations.FirstOrDefault(c => c.Id == contributorOperation.Id);

                ContributorOperationInstance.OperationStatusId = contributorOperation.OperationStatusId;
                ContributorOperationInstance.OtherDocElecContributorId = contributorOperation.OtherDocElecContributorId;
                ContributorOperationInstance.SoftwareId = contributorOperation.SoftwareId;
                ContributorOperationInstance.SoftwareType = contributorOperation.SoftwareType;
                ContributorOperationInstance.Deleted = contributorOperation.Deleted;
                ContributorOperationInstance.Timestamp = System.DateTime.Now;
                context.Entry(ContributorOperationInstance).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                return true;
            }
        }


        public int Add(OtherDocElecContributorOperations contributorOperation)
        {
			try
			{

			
            int affectedRecords = 0;
            using (var context = new SqlDBContext())
            {
                context.OtherDocElecContributorOperations.Add(contributorOperation);
                affectedRecords = context.SaveChanges();
            }
            return affectedRecords > 0 ? contributorOperation.Id : 0;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
