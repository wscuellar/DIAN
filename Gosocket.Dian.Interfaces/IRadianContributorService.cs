using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces
{
    public interface IRadianContributorService
    {
        int AddOrUpdate(RadianContributor radianContributor);
        List<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);
        void RemoveRadianContributor(RadianContributor radianContributor);
        List<RadianContributorType> GetRadianContributorTypes(Expression<Func<RadianContributorType, bool>> expression);
        List<RadianContributorFileStatus> GetRadianContributorFileStatus(Expression<Func<RadianContributorFileStatus, bool>> expression);

    }
}