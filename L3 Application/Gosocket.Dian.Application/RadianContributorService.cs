using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.Application
{
    public class RadianContributorService : IRadianContributorService
    {

        SqlDBContext sqlDBContext;
        //private static StackExchange.Redis.IDatabase cache;

        public RadianContributorService()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
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
            var query = sqlDBContext.RadianContributors.Where(expression).Include("Contributor").Include("RadianContributorType").Include("RadianOperationMode").Include("RadianContributorFile");
            if (page > 0 && length > 0)
            {
                query = query.Skip(page * length).Take(length);
            }
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
                var radianContributorInstance = context.RadianContributors.FirstOrDefault(c => c.Id == radianContributor.Id);
                if (radianContributorInstance != null)
                {
                    radianContributorInstance.RadianContributorTypeId = radianContributor.RadianContributorTypeId;
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

        public List<RadianContributorType> GetRadianContributorTypes(Expression<Func<RadianContributorType, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorTypes.Where(expression);
            return query.ToList();
        }

    }
}
