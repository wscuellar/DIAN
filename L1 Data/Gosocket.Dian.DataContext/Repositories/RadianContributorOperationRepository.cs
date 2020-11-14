using Gosocket.Dian.Domain;
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
    }
}
