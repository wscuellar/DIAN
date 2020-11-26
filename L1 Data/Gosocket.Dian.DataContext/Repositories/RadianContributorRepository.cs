using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorRepository : IRadianContributorRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public RadianContributorRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public RadianContributor Get(Expression<Func<RadianContributor, bool>> expression)
        {
            IQueryable<RadianContributor> query = sqlDBContext.RadianContributors.Where(expression)
                .Include("Contributor")
                .Include("RadianContributorType")
                .Include("RadianOperationMode")
                .Include("RadianContributorFile")
                .Include("RadianContributorOperations");
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Consulta los contribuyentes de radian.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="length"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public PagedResult<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0)
        {
            IQueryable<RadianContributor> query = sqlDBContext.RadianContributors.Where(expression)
                .Include("Contributor")
                .Include("RadianContributorType")
                .Include("RadianOperationMode")
                .Include("RadianContributorFile")
                .Include("RadianContributorOperations");
            return query.Paginate(page, length, t => t.Id.ToString());
        }

        /// <summary>
        /// Inserta y actualiza
        /// </summary>
        /// <param name="radianContributor"></param>
        /// <returns></returns>
        public int AddOrUpdate(RadianContributor radianContributor)
        {
            using (var context = new SqlDBContext())
            {
                RadianContributor radianContributorInstance = context.RadianContributors.FirstOrDefault(c => c.Id == radianContributor.Id);

                if (radianContributorInstance != null)
                {
                    radianContributorInstance.RadianContributorTypeId = radianContributor.RadianContributorTypeId;
                    radianContributorInstance.Update = DateTime.Now;
                    radianContributorInstance.RadianState = radianContributor.RadianState;
                    radianContributorInstance.RadianOperationModeId = radianContributor.RadianOperationModeId;
                    radianContributorInstance.CreatedBy = radianContributor.CreatedBy;
                    radianContributorInstance.Description = radianContributor.Description;
                    radianContributorInstance.Step = radianContributor.Step == 0 ? 1 : radianContributor.Step;

                    context.Entry(radianContributorInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    radianContributor.Step = 1;
                    radianContributor.Update = DateTime.Now;
                    context.Entry(radianContributor).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();

                return radianContributorInstance != null ? radianContributorInstance.Id : radianContributor.Id;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="radianContributor"></param>
        public void RemoveRadianContributor(RadianContributor radianContributor)
        {
            RadianContributor rc = sqlDBContext.RadianContributors.FirstOrDefault(x => x.Id == radianContributor.Id);
            if (rc != null)
            {
                sqlDBContext.RadianContributors.Remove(rc);
                sqlDBContext.SaveChanges();
            }
        }
    }
}
