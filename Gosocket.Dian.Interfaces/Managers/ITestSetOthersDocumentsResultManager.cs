using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Managers
{
    public interface ITestSetOthersDocumentsResultManager
    {
        IEnumerable<GlobalTestSetOthersDocumentsResult> GetAllTestSetResult();
        IEnumerable<GlobalTestSetOthersDocumentsResult> GetAllTestSetResultByContributor(int contributorId);
        GlobalTestSetOthersDocumentsResult GetTestSetResult(string partitionKey, string rowKey);
        bool InsertOrUpdateTestSetResult(GlobalTestSetOthersDocumentsResult testSetResult);
        List<GlobalTestSetOthersDocumentsResult> GetTestSetResultByNit(string nit); 
    }
}