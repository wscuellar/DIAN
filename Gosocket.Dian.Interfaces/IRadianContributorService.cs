using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces
{
    public interface IRadianContributorService
    {
        int AddOrUpdate(RadianContributor radianContributor);
        List<RadianContributor> Get(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);
        void RemoveRadianContributor(RadianContributor radianContributor);
    }
}