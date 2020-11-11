using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Managers;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Managers
{
    public class RadianTestSetManager : IRadianTestSetManager
    {
        private static readonly TableManager testSetManager = new TableManager("RadianTestSet");

        public bool InsertTestSet(RadianTestSet testSet)
        {
            return testSetManager.InsertOrUpdate(testSet);
        }

        public IEnumerable<RadianTestSet> GetAllTestSet()
        {
            try
            {
                TableContinuationToken token = null;
                var testSets = new List<RadianTestSet>();
                foreach (var operationModeId in new List<int> { (int)RadianContributorType.ElectronicInvoice, (int)RadianContributorType.TechnologyProvider, (int)RadianContributorType.TradingSystem, (int)RadianContributorType.Factor })
                {
                    var data = testSetManager.GetRangeRows<RadianTestSet>($"{operationModeId}", 1000, token);
                    testSets.AddRange(data.Item1);
                }

                return testSets;
            }
            catch (Exception)
            {
                return new List<RadianTestSet>();
            }
        }

        public RadianTestSet GetTestSet(string partitionKey, string rowKey)
        {
            return testSetManager.Find<RadianTestSet>(partitionKey, rowKey);
        }

    }
}
