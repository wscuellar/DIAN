using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class TestSetOthersDocumentsResultService : ITestSetOthersDocumentsResultService
    {
        private readonly ITestSetOthersDocumentsResultManager _testSetOthersDocumentsResultManager;

        public TestSetOthersDocumentsResultService(ITestSetOthersDocumentsResultManager testSetOthersDocumentsResultManager)
        {
            _testSetOthersDocumentsResultManager = testSetOthersDocumentsResultManager;
        }


        public List<GlobalTestSetOthersDocumentsResult> GetAllTestSetResult()
        {
            return _testSetOthersDocumentsResultManager.GetAllTestSetResult().ToList();
        }


        public GlobalTestSetOthersDocumentsResult GetTestSetResult(string partitionKey, string rowKey)
        {
            return _testSetOthersDocumentsResultManager.GetTestSetResult(partitionKey, rowKey);
        }

        public bool InsertTestSetResult(GlobalTestSetOthersDocumentsResult testSet)
        {
            return _testSetOthersDocumentsResultManager.InsertOrUpdateTestSetResult(testSet);
        }

        public List<GlobalTestSetOthersDocumentsResult> GetTestSetResultByNit(string nit)
        {
            return _testSetOthersDocumentsResultManager.GetTestSetResultByNit(nit);
        }

        public List<GlobalTestSetOthersDocumentsResult> GetTestSetResultAcepted(string nit, int electronicDocumentId, int otherDocElecContributorId, string softwareId)
        {
            var testSetsResult = _testSetOthersDocumentsResultManager.GetTestSetResultByNit(nit);

            var testSetsResultAcepted = testSetsResult
                .Where(t =>
                    t.PartitionKey == nit &&
                    t.ElectronicDocumentId == electronicDocumentId &&
                    t.OtherDocElecContributorId == otherDocElecContributorId &&
                    t.SoftwareId == softwareId &&
                    t.Status == (int)TestSetStatus.Accepted)
                .ToList();

            return testSetsResultAcepted;
        }
    }
}
