using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianContributorFileRepository
    {
        
        List<RadianContributorFile> GetRadianContributorFile(Expression<Func<RadianContributorFile, bool>> expression);

        Guid UpdateRadianContributorFile(RadianContributorFile radianContributorFile);

    }
}
