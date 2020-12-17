using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{

    public interface IOthersDocsElecContributorRepository
    {

        OtherDocElecContributor Get(Expression<Func<OtherDocElecContributor, bool>> expression);
        PagedResult<OtherDocElecContributor> List(Expression<Func<OtherDocElecContributor, bool>> expression, int page = 0, int length = 0);

        int AddOrUpdate(OtherDocElecContributor othersDocsElecContributor);
        
        void RemoveOthersDocsElecContributor(OtherDocElecContributor othersDocsElecContributor); 
    }

}