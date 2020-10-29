using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<RadianContributorFileType> GetRadianContributorFileTypes(string name, int page, int length )
        {
            var query = _sqlDBContext.RadianContributorFileTypes.Where(ft => name == null || ft.Name.Contains(name)).OrderByDescending(c => c.Mandatory).Skip(page * length).Take(length);

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

        public RadianContributorFileType Get( int id )
        {
            return _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(x => x.Id == id);
        }

        public List<RadianContributorFileType> GetAllMandatory()
        {
            return _sqlDBContext.RadianContributorFileTypes.Where(x => x.Mandatory).ToList();

        }
    }
}
