using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class RadianContributorFileTypeService
    {
        SqlDBContext _sqlDBContext;
        public RadianContributorFileTypeService()
        {
            if (_sqlDBContext == null)
            {
                _sqlDBContext = new SqlDBContext();
            }
        }

        public List<RadianContributorType> GetRadianContributorTypes() 
        {
            var query = _sqlDBContext.RadianContributorTypes;

            return query.ToList();
        }

        public RadianContributorFileType Get(int id)
        {
            return _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(x => x.Id == id);
        }

        public List<RadianContributorFileType> GetRadianContributorFileTypes(int page, int length, Expression<Func<RadianContributorFileType, bool>> expression)
        {
            var query = _sqlDBContext.RadianContributorFileTypes.Where(expression).OrderBy(c => c.RadianContributorType.Id).Skip(page * length).Take(length).Include("RadianContributorType");

            return query.ToList();
        }

        public int AddOrUpdate(RadianContributorFileType radianContributorFileType )
        {
            using (var context = new SqlDBContext())

            {
                var fileTypeInstance = context.RadianContributorFileTypes.FirstOrDefault(c => c.Id == radianContributorFileType.Id);

                if (fileTypeInstance != null)
                {
                    fileTypeInstance.Name = radianContributorFileType.Name;
                    fileTypeInstance.Mandatory = radianContributorFileType.Mandatory;
                    fileTypeInstance.Updated = radianContributorFileType.Updated;
                    fileTypeInstance.CreatedBy = radianContributorFileType.CreatedBy;
                    fileTypeInstance.Deleted = radianContributorFileType.Deleted;
                    fileTypeInstance.Timestamp = radianContributorFileType.Timestamp;
                    fileTypeInstance.RadianContributorTypeId = radianContributorFileType.RadianContributorTypeId;
                    fileTypeInstance.RadianContributorType = radianContributorFileType.RadianContributorType;
                    context.Entry(fileTypeInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(radianContributorFileType).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();

                return fileTypeInstance != null ? fileTypeInstance.Id : radianContributorFileType.Id;
            }
        }

        public int Delete(RadianContributorFileType radianContributorFileType)
        {
            using (var context = new SqlDBContext())

            {
                var fileTypeInstance = context.RadianContributorFileTypes.FirstOrDefault(c => c.Id == radianContributorFileType.Id);

                if (fileTypeInstance != null)
                {
                    fileTypeInstance.Updated = radianContributorFileType.Updated;
                    fileTypeInstance.Timestamp = radianContributorFileType.Timestamp;
                    fileTypeInstance.Deleted = radianContributorFileType.Deleted;
                    context.Entry(fileTypeInstance).State = System.Data.Entity.EntityState.Modified;
                }

                context.SaveChanges();

                return fileTypeInstance != null ? fileTypeInstance.Id : radianContributorFileType.Id;
            }
        }
    }
}
