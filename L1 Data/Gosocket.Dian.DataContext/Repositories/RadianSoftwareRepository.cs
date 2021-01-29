using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianSoftwareRepository : IRadianSoftwareRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public RadianSoftwareRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public RadianSoftware Get(Expression<Func<RadianSoftware, bool>> expression)
        {
            IQueryable<RadianSoftware> query = sqlDBContext.RadianSoftwares.Where(expression)
                .Include("RadianContributorOperations");
            return query.FirstOrDefault();
        }

        public PagedResult<RadianSoftware> List(Expression<Func<RadianSoftware, bool>> expression, int page, int pagesize)
        {
            IQueryable<RadianSoftware> query = sqlDBContext.RadianSoftwares.Where(expression);
            return query.Paginate(page, pagesize, t => t.Id.ToString());
        }

        public Guid AddOrUpdate(RadianSoftware radianSoftware)
        {
            using (var context = new SqlDBContext())
            {
                RadianSoftware radianSoftwareInstance = context.RadianSoftwares.FirstOrDefault(c => c.Id == radianSoftware.Id);

                if (radianSoftwareInstance != null)
                {
                    radianSoftwareInstance.ContributorId = radianSoftware.ContributorId;
                    radianSoftwareInstance.Name = radianSoftware.Name;
                    radianSoftwareInstance.Pin = radianSoftware.Pin;
                    radianSoftwareInstance.Url = radianSoftware.Url;
                    radianSoftwareInstance.Status = radianSoftware.Status;
                    radianSoftwareInstance.Deleted = radianSoftware.Deleted;
                    radianSoftwareInstance.Updated = radianSoftware.Updated;
                    radianSoftwareInstance.RadianSoftwareStatusId = radianSoftware.RadianSoftwareStatusId;
                    context.Entry(radianSoftwareInstance).State = EntityState.Modified;
                }
                else
                {
                    radianSoftware.Id = radianSoftware.Id == Guid.Empty ? Guid.NewGuid() : radianSoftware.Id;
                    radianSoftware.Updated = DateTime.Now;
                    radianSoftware.RadianSoftwareStatusId = 1;
                    context.Entry(radianSoftware).State = EntityState.Added;
                }

                context.SaveChanges();

                return radianSoftwareInstance != null ? radianSoftwareInstance.Id : radianSoftware.Id;
            }
        }

        public bool Delete(Guid id)
        {
            RadianSoftware rc = sqlDBContext.RadianSoftwares.FirstOrDefault(x => x.Id == id);
            if (rc == null)
                return true;
            sqlDBContext.RadianSoftwares.Remove(rc);
            sqlDBContext.SaveChanges();
            return true;
        }


    }
}
