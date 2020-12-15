using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class OthersDocsElecContributorRepository : IOthersDocsElecContributorRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public OthersDocsElecContributorRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public OtherDocElecContributor Get(Expression<Func<OtherDocElecContributor, bool>> expression)
        {
            IQueryable<OtherDocElecContributor> query = sqlDBContext.OtherDocElecContributors.Where(expression)
                .Include("OtherDocElecContributor")
                .Include("OtherDocElecSoftware")
                .Include("OtherDocElecSoftware.OtherDocElecContributorOperations")
                .Include("OtherDocElecContributorType")
                .Include("OtherDocElecOperationMode")
                .Include("OtherDocElecContributorOperations");

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Inserta y actualiza
        /// </summary>
        /// <param name="radianContributor"></param>
        /// <returns></returns>
        public int AddOrUpdate(OtherDocElecContributor othersDocsElecContributor)
        {
            using (var context = new SqlDBContext())
            {
                OtherDocElecContributor ContributorInstance = context.OtherDocElecContributors.FirstOrDefault(c => c.Id == othersDocsElecContributor.Id);

                if (ContributorInstance != null)
                {
                    ContributorInstance.OtherDocElecContributorTypeId = othersDocsElecContributor.OtherDocElecContributorTypeId;
                    ContributorInstance.Update = DateTime.Now;
                    ContributorInstance.State = othersDocsElecContributor.State;
                    ContributorInstance.OtherDocElecOperationModeId = othersDocsElecContributor.OtherDocElecOperationModeId;
                    ContributorInstance.CreatedBy = othersDocsElecContributor.CreatedBy;
                    ContributorInstance.Description = othersDocsElecContributor.Description;
                    ContributorInstance.Step = othersDocsElecContributor.Step == 0 ? 1 : othersDocsElecContributor.Step;

                    context.Entry(ContributorInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    othersDocsElecContributor.Step = 1;
                    othersDocsElecContributor.Update = DateTime.Now;
                    context.Entry(othersDocsElecContributor).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();

                return ContributorInstance != null ? othersDocsElecContributor.Id : othersDocsElecContributor.Id;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="radianContributor"></param>
        public void RemoveOthersDocsElecContributor(OtherDocElecContributor othersDocsElecContributor)
        {
            OtherDocElecContributor rc = sqlDBContext.OtherDocElecContributors.FirstOrDefault(x => x.Id == othersDocsElecContributor.Id);
            if (rc != null)
            {
                sqlDBContext.OtherDocElecContributors.Remove(rc);
                sqlDBContext.SaveChanges();
            }
        }


        public PagedResult<OtherDocElecContributor> List(Expression<Func<OtherDocElecContributor, bool>> expression, int page = 0, int length = 0)
        {
            IQueryable<OtherDocElecContributor> query = sqlDBContext.OtherDocElecContributors.Where(expression)
                    .Include("Contributor")
                    .Include("OtherDocElecContributorTypes")
                    .Include("OtherDocElecOperationModes");
 
            try
            {
                IQueryable<OtherDocElecContributor> query4 = sqlDBContext.OtherDocElecContributors.Where(expression)
                 .Include("Contributor")
                 .Include("OtherDocElecContributorTypes")
                 .Include("OtherDocElecOperationModes")
                 .Include("OtherDocElecContributorOperations");
                var d = query4.Paginate(page, length, t => t.Id.ToString());
            }
            catch (Exception ex)
            {
                var exs = ex.Message;
            }
            return query.Paginate(page, length, t => t.Id.ToString());
        }


    }
}
