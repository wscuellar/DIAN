using Gosocket.Dian.Domain;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianContributorService
    {
        int AddOrUpdate(RadianContributor radianContributor, string approveState = "");
        List<RadianContributor> List();
        void Remove(RadianContributor radianContributor);
    }
}