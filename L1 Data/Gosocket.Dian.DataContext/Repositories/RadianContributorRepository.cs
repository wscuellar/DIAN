using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
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
            IQueryable<RadianContributor> query = sqlDBContext.RadianContributors.Where(expression).Include("Contributor").Include("RadianContributorType").Include("RadianOperationMode").Include("RadianContributorFile");
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Consulta los contribuyentes de radian.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="length"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public List<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0)
        {
            IQueryable<RadianContributor> query = sqlDBContext.RadianContributors.Where(expression).Include("Contributor").Include("RadianContributorType").Include("RadianOperationMode").Include("RadianContributorFile");
            //if (page > 0 && length > 0)
            //{
            //    query = query.Skip(page * length).Take(length);
            //}
            return query.ToList();
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

                    context.Entry(radianContributorInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
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
