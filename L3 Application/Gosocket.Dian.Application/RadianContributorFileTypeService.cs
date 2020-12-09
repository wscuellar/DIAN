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

    }
}
