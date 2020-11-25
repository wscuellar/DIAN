using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorFileTypeRepository : IRadianContributorFileTypeRepository
    {
        private readonly SqlDBContext _sqlDBContext;

        public RadianContributorFileTypeRepository()
        {
            if (_sqlDBContext == null)
                _sqlDBContext = new SqlDBContext();
        }

        public RadianContributorFileType Get(int id)
        {
            return _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(x => x.Id == id);
        }

        public List<RadianContributorFileType> List(Expression<Func<RadianContributorFileType, bool>> expression, int page = 0, int length = 0)
        {
            var query = _sqlDBContext.RadianContributorFileTypes.Where(expression).OrderBy(c => c.RadianContributorType.Id).Include("RadianContributorType");

            return query.ToList();
        }

        public List<KeyValue> FileTypeCounter()
        {
            return (from t in _sqlDBContext.RadianContributorFileTypes
                    join f in _sqlDBContext.RadianContributorFiles on t.Id equals f.FileType
                    select t.Id).GroupBy(t => t).Select(t => new KeyValue() { Key = t.Key, value = t.Count() }).ToList();
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

            return amountOfFiles.Count() <= 0;
        }
    }
}
