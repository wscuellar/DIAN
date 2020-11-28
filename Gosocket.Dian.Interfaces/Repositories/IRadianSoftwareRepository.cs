using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianSoftwareRepository
    {
        RadianSoftware Get(Expression<Func<RadianSoftware, bool>> expression);
        PagedResult<RadianSoftware> List(Expression<Func<RadianSoftware, bool>> expression, int page, int pagesize);
        bool Delete(Guid id);
        Guid AddOrUpdate(RadianSoftware radianSoftware);
    }
}
