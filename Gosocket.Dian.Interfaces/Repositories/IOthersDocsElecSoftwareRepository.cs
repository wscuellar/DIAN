using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IOthersDocsElecSoftwareRepository
    {
        OtherDocElecSoftware Get(Expression<Func<OtherDocElecSoftware, bool>> expression);
        PagedResult<OtherDocElecSoftware> List(Expression<Func<OtherDocElecSoftware, bool>> expression, int page, int pagesize);
        bool Delete(Guid id);
        Guid AddOrUpdate(OtherDocElecSoftware Software);

        string GetSoftwareStatusName(int id);
    }
}
