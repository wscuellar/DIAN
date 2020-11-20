using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using System;

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
    }
}
