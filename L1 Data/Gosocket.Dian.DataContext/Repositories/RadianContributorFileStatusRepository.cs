using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorFileStatusRepository : IRadianContributorFileStatusRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public RadianContributorFileStatusRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public List<RadianContributorFileStatus> GetRadianContributorFileStatus(Expression<Func<RadianContributorFileStatus, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorFileStatuses.Where(expression);
            return query.ToList();
        }
    }

}
