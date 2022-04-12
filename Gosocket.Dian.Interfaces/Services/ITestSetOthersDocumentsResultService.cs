using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface ITestSetOthersDocumentsResultService
    {
        List<GlobalTestSetOthersDocumentsResult> GetAllTestSetResult();
        GlobalTestSetOthersDocumentsResult GetTestSetResult(string partitionKey, string rowKey);
        bool InsertTestSetResult(GlobalTestSetOthersDocumentsResult testSet);
        List<GlobalTestSetOthersDocumentsResult> GetTestSetResultByNit(string nit);
        List<GlobalTestSetOthersDocumentsResult> GetTestSetResultAcepted(string nit, int electronicDocumentId, int otherDocElecContributorId, string softwareId);
    }
}