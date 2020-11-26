using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianContributorFileTypeService : IRadianContributorFileTypeService
    {
        private readonly IRadianContributorFileTypeRepository _radianContributorFileTypeRepository;
        private readonly IRadianContributorTypeRepository _radianContributorTypeRepository;

        public RadianContributorFileTypeService(IRadianContributorFileTypeRepository radianContributorFileTypeRepository, IRadianContributorTypeRepository radianContributorTypeRepository)
        {
            _radianContributorFileTypeRepository = radianContributorFileTypeRepository;
            _radianContributorTypeRepository = radianContributorTypeRepository;
        }


        private RadianContributorFileType map(RadianContributorFileType input, KeyValue Counter)
        {
            input.HideDelete = Counter != null && Counter.value > 0;
            return input;
        }

        public List<RadianContributorFileType> FileTypeList()
        {
            List<KeyValue> counter = _radianContributorFileTypeRepository.FileTypeCounter();
            List<RadianContributorFileType> fileTypes = _radianContributorFileTypeRepository.List(ft => !ft.Deleted);

            return (from f in fileTypes
                    join c in counter on f.Id equals c.Key into g
                    from x in g.DefaultIfEmpty()
                    select map(f, x)).ToList();
        }

        public List<RadianContributorType> ContributorTypeList()
        {
            return _radianContributorTypeRepository.List(t => true);
        }

        public List<RadianContributorFileType> Filter(string name, string selectedRadianContributorTypeId)
        {
            int selectedType = (selectedRadianContributorTypeId == null) ? 0 : int.Parse(selectedRadianContributorTypeId);

            return _radianContributorFileTypeRepository.List(ft => ((name == null) || ft.Name.Contains(name)) && ((selectedRadianContributorTypeId == null) || ft.RadianContributorTypeId == selectedType) && !ft.Deleted);
        }

        public int Update(RadianContributorFileType radianContributorFileType)
        {
            return _radianContributorFileTypeRepository.AddOrUpdate(radianContributorFileType);
        }

        public RadianContributorFileType Get(int id)
        {
            return _radianContributorFileTypeRepository.Get(id);
        }

        public bool IsAbleForDelete(RadianContributorFileType radianContributorFileType)
        {
            return _radianContributorFileTypeRepository.IsAbleForDelete(radianContributorFileType);
        }

        public int Delete(RadianContributorFileType radianContributorFileType)
        {
            return _radianContributorFileTypeRepository.Delete(radianContributorFileType);
        }

        //SqlDBContext _sqlDBContext;
        //public RadianContributorFileTypeService()
        //{
        //    if (_sqlDBContext == null)
        //    {
        //        _sqlDBContext = new SqlDBContext();
        //    }
        //}

        //public RadianContributorFileType Get(int id)
        //{
        //    return _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(x => x.Id == id);
        //}

        //public List<RadianContributorFileType> GetRadianContributorFileTypes(int page, int length, Expression<Func<RadianContributorFileType, bool>> expression)
        //{
        //    var query = _sqlDBContext.RadianContributorFileTypes.Where(expression).OrderBy(c => c.RadianContributorType.Id).Skip(page * length).Take(length).Include("RadianContributorType");

        //    return query.ToList();
        //}

        //public int AddOrUpdate(RadianContributorFileType radianContributorFileType)
        //{
        //    var fileTypeInstance = _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(c => c.Id == radianContributorFileType.Id);

        //    if (fileTypeInstance != null)
        //    {
        //        fileTypeInstance.Name = radianContributorFileType.Name;
        //        fileTypeInstance.Mandatory = radianContributorFileType.Mandatory;
        //        fileTypeInstance.Updated = radianContributorFileType.Updated;
        //        fileTypeInstance.CreatedBy = radianContributorFileType.CreatedBy;
        //        fileTypeInstance.Deleted = radianContributorFileType.Deleted;
        //        fileTypeInstance.Timestamp = radianContributorFileType.Timestamp;
        //        fileTypeInstance.RadianContributorTypeId = radianContributorFileType.RadianContributorTypeId;
        //        fileTypeInstance.RadianContributorType = radianContributorFileType.RadianContributorType;
        //        _sqlDBContext.Entry(fileTypeInstance).State = System.Data.Entity.EntityState.Modified;
        //    }
        //    else
        //    {
        //        _sqlDBContext.Entry(radianContributorFileType).State = System.Data.Entity.EntityState.Added;
        //    }

        //    _sqlDBContext.SaveChanges();

        //    return fileTypeInstance != null ? fileTypeInstance.Id : radianContributorFileType.Id;
        //}

        //public int Delete(RadianContributorFileType radianContributorFileType)
        //{
        //    var fileTypeInstance = _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(c => c.Id == radianContributorFileType.Id);

        //    if (fileTypeInstance != null)
        //    {
        //        fileTypeInstance.Updated = radianContributorFileType.Updated;
        //        fileTypeInstance.Timestamp = radianContributorFileType.Timestamp;
        //        fileTypeInstance.Deleted = radianContributorFileType.Deleted;
        //        _sqlDBContext.Entry(fileTypeInstance).State = System.Data.Entity.EntityState.Modified;
        //    }

        //    _sqlDBContext.SaveChanges();

        //    return fileTypeInstance != null ? fileTypeInstance.Id : radianContributorFileType.Id;
        //}
        //public bool IsAbleForDelete(RadianContributorFileType radianContributorFileType)
        //{
        //    var fileTypeInstance = _sqlDBContext.RadianContributorFileTypes.FirstOrDefault(c => c.Id == radianContributorFileType.Id);

        //    var amountOfFiles = _sqlDBContext.RadianContributorFiles.Where(rcf => rcf.FileType == fileTypeInstance.Id);

        //    return (amountOfFiles.Count() > 0) ? false : true;
        //}
    }
}
