using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class ContributorFileTypeService
    {
        private readonly SqlDBContext _sqlDBContext;
        public ContributorFileTypeService()
        {
            if (_sqlDBContext == null)
            {
                _sqlDBContext = new SqlDBContext();
            }
        }
        public List<ContributorFileType> GetContributorFileTypes(string name, int page, int length )
        {
            var query = _sqlDBContext.ContributorFileTypes.Where(ft => name == null || ft.Name.Contains(name)).OrderByDescending(c => c.Mandatory).Skip(page * length).Take(length);

            return query.ToList();
        }

        public int AddOrUpdate( ContributorFileType contributorFileType )
        {
            using (var context = new SqlDBContext())

            {
                var fileTypeInstance = context.ContributorFileTypes.FirstOrDefault(c => c.Id == contributorFileType.Id);

                if (fileTypeInstance != null)
                {
                    fileTypeInstance.Name = contributorFileType.Name;
                    fileTypeInstance.Mandatory = contributorFileType.Mandatory;
                    fileTypeInstance.Updated = contributorFileType.Updated;
                    fileTypeInstance.CreatedBy = contributorFileType.CreatedBy;
                    fileTypeInstance.Deleted = contributorFileType.Deleted;
                    fileTypeInstance.Timestamp = contributorFileType.Timestamp;
                    context.Entry(fileTypeInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(contributorFileType).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();

                return fileTypeInstance != null ? fileTypeInstance.Id : contributorFileType.Id;
            }
        }

        public ContributorFileType Get( int id )
        {
            return _sqlDBContext.ContributorFileTypes.FirstOrDefault(x => x.Id == id);
        }

        public List<ContributorFileType> GetAllMandatory()
        {
            return _sqlDBContext.ContributorFileTypes.Where(x => x.Mandatory).ToList();

        }
    }
}
