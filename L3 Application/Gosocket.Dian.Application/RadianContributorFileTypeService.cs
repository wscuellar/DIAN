using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class RadianContributorFileTypeService : IRadianContributorFileTypeService
    {
        SqlDBContext _sqlDBContext;
        public RadianContributorFileTypeService()
        {
            if (_sqlDBContext == null)
            {
                _sqlDBContext = new SqlDBContext();
            }
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

        public int AddOrUpdate(RadianContributorFileType radianContributorFileType)
        {
            var fileTypeInstance = _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(c => c.Id == radianContributorFileType.Id);

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
                _sqlDBContext.Entry(fileTypeInstance).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                _sqlDBContext.Entry(radianContributorFileType).State = System.Data.Entity.EntityState.Added;
            }

            _sqlDBContext.SaveChanges();

            return fileTypeInstance != null ? fileTypeInstance.Id : radianContributorFileType.Id;
        }

        public int Delete(RadianContributorFileType radianContributorFileType)
        {
            var fileTypeInstance = _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(c => c.Id == radianContributorFileType.Id);

            if (fileTypeInstance != null)
            {
                fileTypeInstance.Updated = radianContributorFileType.Updated;
                fileTypeInstance.Timestamp = radianContributorFileType.Timestamp;
                fileTypeInstance.Deleted = radianContributorFileType.Deleted;
                _sqlDBContext.Entry(fileTypeInstance).State = System.Data.Entity.EntityState.Modified;
            }

            _sqlDBContext.SaveChanges();

            return fileTypeInstance != null ? fileTypeInstance.Id : radianContributorFileType.Id;
        }
        public bool IsAbleForDelete(RadianContributorFileType radianContributorFileType)
        {
            var fileTypeInstance = _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(c => c.Id == radianContributorFileType.Id);

            var amountOfFiles = _sqlDBContext.RadianContributorFiles.Where(rcf => rcf.FileType == fileTypeInstance.Id);

            return (amountOfFiles.Count() > 0) ? false : true;
        }
    }
}
