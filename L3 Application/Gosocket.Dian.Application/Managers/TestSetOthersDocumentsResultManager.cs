using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Managers;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Managers
{
    public class TestSetOthersDocumentsResultManager : ITestSetOthersDocumentsResultManager
    {
        private static readonly TableManager testSetManager = new TableManager("GlobalTestSetOthersDocumentsResult"); 

        public bool InsertOrUpdateTestSetResult(GlobalTestSetOthersDocumentsResult testSetResult)
        {
            return testSetManager.InsertOrUpdate(testSetResult);
        }

        public IEnumerable<GlobalTestSetOthersDocumentsResult> GetAllTestSetResult()
        {
            try
            {
                var testSets = new List<GlobalTestSetOthersDocumentsResult>();
                /*foreach (var operationModeId in new List<int> { (int)RadianContributorType.ElectronicInvoice, (int)RadianContributorType.TechnologyProvider, (int)RadianContributorType.TradingSystem, (int)RadianContributorType.Factor })
                {
                    var data = testSetManager.GetRangeRows<RadianTestSetResult>($"{operationModeId}", 1000, token);
                    testSets.AddRange(data.Item1);
                }*/

                return testSets;
            }
            catch (Exception)
            {
                return new List<GlobalTestSetOthersDocumentsResult>();
            }
        }

        public GlobalTestSetOthersDocumentsResult GetTestSetResult(string partitionKey, string rowKey)
        {
            return testSetManager.Find<GlobalTestSetOthersDocumentsResult>(partitionKey, rowKey);
        }

        public IEnumerable<GlobalTestSetOthersDocumentsResult> GetAllTestSetResultByContributor(int contributorId)
        {
            return testSetManager.FindByContributorIdWithPagination<GlobalTestSetOthersDocumentsResult>(contributorId);
        }

        public List<GlobalTestSetOthersDocumentsResult> GetTestSetResultByNit(string nit)
        {
            return testSetManager.FindByPartition<GlobalTestSetOthersDocumentsResult>(nit);
        }

    }
}