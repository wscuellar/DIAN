using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces
{
    public interface IContributorOperationsService
    {
        int AddOrUpdate(ContributorOperations contributorOperations);
        List<ContributorOperations> Find(int contributorId, int operationModeId, int? providerId, Guid? softwareId);
        ContributorOperations Get(int id);
        List<ContributorOperations> Get(int contributorId, int page, int length);
        ContributorOperations Get(int contributorId, int operationModeId, int? providerId, Guid? softwareId);
        Contributor GetContributor(int contributorId);
        List<ContributorOperations> GetContributorOperations(int contributorId);
        List<ContributorOperations> GetContributorOperations(int contributorId, string clientCode);
        ContributorOperations GetForDelete(int id);
    }
}