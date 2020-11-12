using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Managers;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Managers
{
    public class RadianTestSetResultManager : IRadianTestSetResultManager
    {
        private static readonly TableManager testSetManager = new TableManager("RadianTestSetResult");

        public bool InsertTestSet(RadianTestSetResult testSetResult)
        {
            return testSetManager.InsertOrUpdate(testSetResult);
        }

        public IEnumerable<RadianTestSetResult> GetAllTestSetResult()
        {
            try
            {
                TableContinuationToken token = null;
                var testSets = new List<RadianTestSetResult>();
                foreach (var operationModeId in new List<int> { (int)RadianContributorType.ElectronicInvoice, (int)RadianContributorType.TechnologyProvider, (int)RadianContributorType.TradingSystem, (int)RadianContributorType.Factor })
                {
                    var data = testSetManager.GetRangeRows<RadianTestSetResult>($"{operationModeId}", 1000, token);
                    testSets.AddRange(data.Item1);
                }

                return testSets;
            }
            catch (Exception)
            {
                return new List<RadianTestSetResult>();
            }
        }

        public RadianTestSetResult GetTestSetResult(string partitionKey, string rowKey)
        {
            return testSetManager.Find<RadianTestSetResult>(partitionKey, rowKey);
        }

        public IEnumerable<RadianTestSetResult> GetAllTestSetResultByContributor(int contributorId)
        {
            return testSetManager.FindByContributorIdWithPagination<RadianTestSetResult>(contributorId);
        }
    }
}
