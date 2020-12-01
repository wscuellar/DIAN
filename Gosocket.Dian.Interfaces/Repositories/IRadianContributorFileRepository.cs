using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianContributorFileRepository
    {
        
        List<RadianContributorFile> List(Expression<Func<RadianContributorFile, bool>> expression);

        Guid Update(RadianContributorFile radianContributorFile);

        string AddOrUpdate(RadianContributorFile radianContributorFile);

    }
}
