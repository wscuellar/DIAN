using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianContributorOperationRepository
    {
        int Add(RadianContributorOperation contributorOperation);
        RadianContributorOperation Get(Expression<Func<RadianContributorOperation, bool>> expression);
        List<RadianContributorOperation> List(Expression<Func<RadianContributorOperation, bool>> expression);

        ResponseMessage Delete(int radianContributorOperationId);
        bool Update(RadianContributorOperation contributorOperation);
    }
}