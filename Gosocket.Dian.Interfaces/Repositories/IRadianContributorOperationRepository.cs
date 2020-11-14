using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianContributorOperationRepository
    {
        RadianContributorOperation Get(Expression<Func<RadianContributorOperation, bool>> expression);
        List<RadianContributorOperation> List(Expression<Func<RadianContributorOperation, bool>> expression);
    }
}