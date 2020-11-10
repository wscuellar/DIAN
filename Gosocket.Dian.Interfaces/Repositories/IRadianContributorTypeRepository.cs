using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianContributorTypeRepository
    {
         List<RadianContributorType> List(Expression<Func<RadianContributorType, bool>> expression);

    }
}
