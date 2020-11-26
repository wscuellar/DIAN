using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianContributorFileTypeRepository
    {

        List<KeyValue> FileTypeCounter();
        int AddOrUpdate(RadianContributorFileType radianContributorFileType);
        int Delete(RadianContributorFileType radianContributorFileType);
        RadianContributorFileType Get(int id);
        List<RadianContributorFileType> List(Expression<Func<RadianContributorFileType, bool>> expression, int page = 0, int length = 0);
        bool IsAbleForDelete(RadianContributorFileType radianContributorFileType);        
    }
}
