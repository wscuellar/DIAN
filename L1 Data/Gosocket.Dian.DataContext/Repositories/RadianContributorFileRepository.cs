using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorFileRepository : IRadianContributorFileRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public RadianContributorFileRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public List<RadianContributorFile> List(Expression<Func<RadianContributorFile, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorFiles.Where(expression);
            return query.ToList();
        }

        public Guid Update(RadianContributorFile radianContributorFile)
        {
            using (var context = new SqlDBContext())
            {
                var radianContributorFileInstance = context.RadianContributorFiles.FirstOrDefault(c => c.Id == radianContributorFile.Id);
                if (radianContributorFileInstance != null)
                {
                    radianContributorFileInstance.Status = radianContributorFile.Status;
                    context.Entry(radianContributorFileInstance).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                    return radianContributorFileInstance.Id;
                }
                else
                {
                    return radianContributorFile.Id;
                }

            }
        }


    }

}
