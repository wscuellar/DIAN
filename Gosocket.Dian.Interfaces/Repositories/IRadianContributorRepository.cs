using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{

    public interface IRadianContributorRepository
    {

        RadianContributor Get(Expression<Func<RadianContributor, bool>> expression);
        List<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);

        int AddOrUpdate(RadianContributor radianContributor);
        
        void RemoveRadianContributor(RadianContributor radianContributor);
        
    }

}