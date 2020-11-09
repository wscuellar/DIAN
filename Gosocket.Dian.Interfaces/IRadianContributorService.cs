using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces
{
    public interface IRadianContributorService
    {

        NameValueCollection Summary(string userCode);

        int AddOrUpdate(RadianContributor radianContributor, string approveState);
        List<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);
        void RemoveRadianContributor(RadianContributor radianContributor);
        List<RadianContributorType> GetRadianContributorTypes(Expression<Func<RadianContributorType, bool>> expression);
        List<RadianContributorFileStatus> GetRadianContributorFileStatus(Expression<Func<RadianContributorFileStatus, bool>> expression);
        Guid UpdateRadianContributorFile(RadianContributorFile radianContributorFile);
        List<RadianContributorFile> GetRadianContributorFile(Expression<Func<RadianContributorFile, bool>> expression);
    }
}