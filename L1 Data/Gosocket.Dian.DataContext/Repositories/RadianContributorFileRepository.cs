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
        private readonly SqlDBContext _sqlDBContext;

        public RadianContributorFileRepository()
        {
            if (_sqlDBContext == null)
                _sqlDBContext = new SqlDBContext();
        }

        public List<RadianContributorFile> List(Expression<Func<RadianContributorFile, bool>> expression)
        {
            var query = _sqlDBContext.RadianContributorFiles.Where(expression);
            return query.ToList();
        }

        public Guid Update(RadianContributorFile radianContributorFile)
        {
            var radianContributorFileInstance = _sqlDBContext.RadianContributorFiles.FirstOrDefault(c => c.Id == radianContributorFile.Id);
            if (radianContributorFileInstance != null)
            {
                radianContributorFileInstance.Status = radianContributorFile.Status;
                _sqlDBContext.Entry(radianContributorFileInstance).State = System.Data.Entity.EntityState.Modified;
                _sqlDBContext.SaveChanges();
                return radianContributorFileInstance.Id;
            }
            else
            {
                return radianContributorFile.Id;
            }
        }

        public string AddOrUpdate(RadianContributorFile radianContributorFile)
        {
            RadianContributorFile radianContributorFileInstance = _sqlDBContext
                .RadianContributorFiles
                .FirstOrDefault(c => c.FileType == radianContributorFile.FileType && c.RadianContributorId == radianContributorFile.RadianContributorId);

            if (radianContributorFileInstance != null)
            {
                radianContributorFile.Id = radianContributorFileInstance.Id;
                radianContributorFileInstance.Status = radianContributorFile.Status;
                radianContributorFileInstance.FileName = radianContributorFile.FileName;
                radianContributorFileInstance.FileType = radianContributorFile.FileType;
                radianContributorFileInstance.Deleted = radianContributorFile.Deleted;
                radianContributorFileInstance.Updated = System.DateTime.Now;
                _sqlDBContext.Entry(radianContributorFileInstance).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                radianContributorFile.Id = Guid.NewGuid();
                radianContributorFile.Timestamp = System.DateTime.Now;
                radianContributorFile.Updated = System.DateTime.Now;
                _sqlDBContext.Entry(radianContributorFile).State = System.Data.Entity.EntityState.Added;
            }

            _sqlDBContext.SaveChanges();

            return radianContributorFile.Id.ToString();
        }
    }

}
