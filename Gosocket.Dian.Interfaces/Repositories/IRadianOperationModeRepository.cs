using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianOperationModeRepository
    {
        RadianOperationMode Get(Expression<Func<RadianOperationMode, bool>> expression);
        List<RadianOperationMode> List(Expression<Func<RadianOperationMode, bool>> expression);
    }
}