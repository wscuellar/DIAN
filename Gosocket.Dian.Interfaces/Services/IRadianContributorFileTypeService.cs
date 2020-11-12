using Gosocket.Dian.Domain;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianContributorFileTypeService
    {
        List<RadianContributorFileType> FileTypeList();

        List<RadianContributorType> ContributorTypeList();

        List<RadianContributorFileType> Filter(string name, string selectedRadianContributorTypeId);

        int Update(RadianContributorFileType radianContributorFileType);

        RadianContributorFileType Get(int id);

        int Delete(RadianContributorFileType radianContributorFileType);

        bool IsAbleForDelete(RadianContributorFileType radianContributorFileType);
    }
}
