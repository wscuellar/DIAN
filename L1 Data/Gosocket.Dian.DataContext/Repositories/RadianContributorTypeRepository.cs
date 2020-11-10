using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorTypeRepository: IRadianContributorTypeRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public RadianContributorTypeRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public List<RadianContributorType> List(Expression<Func<RadianContributorType, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorTypes.Where(expression);
            return query.ToList();
        }


    }
}
