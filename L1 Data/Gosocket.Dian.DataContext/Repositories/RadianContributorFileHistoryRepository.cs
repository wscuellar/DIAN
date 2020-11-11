using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using System;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorFileHistoryRepository : IRadianContributorFileHistoryRepository
    {
        private readonly SqlDBContext sqlDBContext;

        public RadianContributorFileHistoryRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public Guid AddRegisterHistory(RadianContributorFileHistory radianContributorFileHistory)
        {
            using (var context = new SqlDBContext())
            {
                context.Entry(radianContributorFileHistory).State = System.Data.Entity.EntityState.Added;

                context.SaveChanges();
                return radianContributorFileHistory.Id;
            }
        }
    }
}
