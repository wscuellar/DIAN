using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Gosocket.Dian.Interfaces
{
    public interface IRadianContributorFileTypeService
    {
        RadianContributorFileType Get(int id);

        List<RadianContributorFileType> GetRadianContributorFileTypes(int page, int length, Expression<Func<RadianContributorFileType, bool>> expression);

        int AddOrUpdate(RadianContributorFileType radianContributorFileType);

        int Delete(RadianContributorFileType radianContributorFileType);
        bool IsAbleForDelete(RadianContributorFileType radianContributorFileType);
    }
}
