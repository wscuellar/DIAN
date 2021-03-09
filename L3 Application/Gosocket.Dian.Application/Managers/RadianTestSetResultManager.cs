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
        private static readonly TableManager tableManagerTestSetResult = new TableManager("GlobalTestSetResult");
        private static readonly TableManager tableManagerGlobalTestSetTracking = new TableManager("GlobalTestSetTracking");

        public bool InsertOrUpdateTestSetResult(RadianTestSetResult testSetResult)
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
            RadianTestSetResult result = testSetManager.Find<RadianTestSetResult>(partitionKey, rowKey);
            return result == null ? new RadianTestSetResult() : result;
        }

        public IEnumerable<RadianTestSetResult> GetAllTestSetResultByContributor(int contributorId)
        {
            return testSetManager.FindByContributorIdWithPagination<RadianTestSetResult>(contributorId);
        }

        public List<RadianTestSetResult> GetTestSetResultByNit(string nit)
        {
            return testSetManager.FindByPartition<RadianTestSetResult> (nit);
        }

        public List<GlobalTestSetResult> GetTestSetResulByCatalog(string code)
        {
            return tableManagerTestSetResult.FindByPartition<GlobalTestSetResult>(code);
        }

        public bool ResetPreviousCounts(string testSetId)
        {
            List<GlobalTestSetTracking> tracking = tableManagerGlobalTestSetTracking.FindByPartition<GlobalTestSetTracking>(testSetId);
            foreach(GlobalTestSetTracking item in tracking)
            {
                tableManagerGlobalTestSetTracking.Delete(item);
            }
            return true;
        } 

    }
}
