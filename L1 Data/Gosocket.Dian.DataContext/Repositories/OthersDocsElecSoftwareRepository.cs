using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class OthersDocsElecSoftwareRepository : IOthersDocsElecSoftwareRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public OthersDocsElecSoftwareRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public OtherDocElecSoftware Get(Expression<Func<OtherDocElecSoftware, bool>> expression)
        {
            IQueryable<OtherDocElecSoftware> query = sqlDBContext.OtherDocElecSoftwares.Where(expression)
                .Include("OtherDocElecContributorOperations");
            return query.FirstOrDefault();
        }

        public PagedResult<OtherDocElecSoftware> List(Expression<Func<OtherDocElecSoftware, bool>> expression, int page, int pagesize)
        {
            IQueryable<OtherDocElecSoftware> query = sqlDBContext.OtherDocElecSoftwares.Where(expression);
            return query.Paginate(page, pagesize, t => t.Id.ToString());
        }

        public Guid AddOrUpdate(OtherDocElecSoftware Software)
        {
			try
			{

			
            using (var context = new SqlDBContext())
            {
                OtherDocElecSoftware  SoftwareInstance = context.OtherDocElecSoftwares.FirstOrDefault(c => c.Id == Software.Id);

                if (SoftwareInstance != null)
                {
                    SoftwareInstance. OtherDocElecContributorId = Software.OtherDocElecContributorId;
                    SoftwareInstance.Name = Software.Name;
                    SoftwareInstance.Pin = Software.Pin;
                    SoftwareInstance.Url = Software.Url;
                    SoftwareInstance.Status = Software.Status;
                    SoftwareInstance.Deleted = Software.Deleted;
                    SoftwareInstance.Updated = Software.Updated;
                    SoftwareInstance. OtherDocElecSoftwareStatusId = Software. OtherDocElecSoftwareStatusId;
                    context.Entry(SoftwareInstance).State = EntityState.Modified;
                }
                else
                {
                    Software.Id = Software.Id == Guid.Empty ? Guid.NewGuid() : Software.Id;
                    Software.Updated = DateTime.Now;
                    Software. OtherDocElecSoftwareStatusId = 1;
                    context.Entry(Software).State = EntityState.Added;
                }

                context.SaveChanges();

                return SoftwareInstance != null ? SoftwareInstance.Id : Software.Id;
            }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public bool Delete(Guid id)
        {
            OtherDocElecSoftware rc = sqlDBContext.OtherDocElecSoftwares.FirstOrDefault(x => x.Id == id);
            if (rc == null)
                return true;
            sqlDBContext.OtherDocElecSoftwares.Remove(rc);
            sqlDBContext.SaveChanges();
            return true;
        }

        public string GetSoftwareStatusName(int id)
        {
            var name = string.Empty;
            var statusModel = this.sqlDBContext.OtherDocElecSoftwareStatus.FirstOrDefault(x => x.Id == id);
            if (statusModel != null) name = statusModel.Name;
            return name;
        }
    }
}
