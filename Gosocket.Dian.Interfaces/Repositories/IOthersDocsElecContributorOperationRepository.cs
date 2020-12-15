using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IOthersDocsElecContributorOperationRepository
    {
        int Add(OtherDocElecContributorOperations contributorOperation);
        OtherDocElecContributorOperations Get(Expression<Func<OtherDocElecContributorOperations, bool>> expression);
        List<OtherDocElecContributorOperations> List(Expression<Func<OtherDocElecContributorOperations, bool>> expression);

        ResponseMessage Delete(int ContributorOperationId);
        bool Update(OtherDocElecContributorOperations contributorOperation);
    }
}