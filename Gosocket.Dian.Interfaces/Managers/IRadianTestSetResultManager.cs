using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Managers
{
    public interface IRadianTestSetResultManager
    {
        IEnumerable<RadianTestSetResult> GetAllTestSetResult();
        IEnumerable<RadianTestSetResult> GetAllTestSetResultByContributor(int contributorId);
        RadianTestSetResult GetTestSetResult(string partitionKey, string rowKey);
        bool InsertOrUpdateTestSetResult(RadianTestSetResult testSetResult);
        List<RadianTestSetResult> GetTestSetResultByNit(string nit);
        List<GlobalTestSetResult> GetTestSetResulByCatalog(string code);
        bool ResetPreviousCounts(string testSetId);
    }
}