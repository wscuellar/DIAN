using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianOperationModeRepository : IRadianOperationModeRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public RadianOperationModeRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public List<RadianOperationMode> List(Expression<Func<RadianOperationMode, bool>> expression)
        {
            var query = sqlDBContext.RadianOperationModes.Where(expression);
            return query.ToList();
        }

        public RadianOperationMode Get(Expression<Func<RadianOperationMode, bool>> expression)
        {
            var query = sqlDBContext.RadianOperationModes.Where(expression);
            return query.FirstOrDefault();
        }

    }
}
