using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorFileHistoryRepository : IRadianContributorFileHistoryRepository
    {
        private readonly SqlDBContext _sqlDBContext;

        public RadianContributorFileHistoryRepository()
        {
            if (_sqlDBContext == null)
                _sqlDBContext = new SqlDBContext();
        }

        public Guid AddRegisterHistory(RadianContributorFileHistory radianContributorFileHistory)
        {            
            _sqlDBContext.Entry(radianContributorFileHistory).State = System.Data.Entity.EntityState.Added;
            _sqlDBContext.SaveChanges();
            return radianContributorFileHistory.Id;
        }

        public PagedResult<RadianContributorFileHistory> List(Expression<Func<RadianContributorFileHistory, bool>> expression, int page, int pagesize)
        {
           var query = _sqlDBContext.RadianContributorFileHistories.Where(expression);
            return query.Paginate(page, pagesize, t => t.Id.ToString());
        }
    }
}
