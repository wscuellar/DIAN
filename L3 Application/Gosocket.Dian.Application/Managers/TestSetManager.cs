using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Managers
{
    public class TestSetManager
    {
        private static readonly TableManager testSetManager = new TableManager("GlobalTestSet");
        private static readonly TableManager testSetTrackingManager = new TableManager("GlobalTestSetTracking");
        public TestSetManager() { }

        public bool InsertTestSet(GlobalTestSet testSet)
        {
            return testSetManager.InsertOrUpdate(testSet);
        }

        public bool InsertTestSetTracking(GlobalTestSetTracking testSetTracking)
        {
            return testSetTrackingManager.Insert(testSetTracking);
        }

        public IEnumerable<GlobalTestSet> GetAllTestSet(/*string partitionKey*/)
        {
            try
            {
                TableContinuationToken token = null;
                var testSets = new List<GlobalTestSet>();
                foreach (var operationModeId in new List<int> { (int)OperationMode.Free, (int)OperationMode.Own, (int)OperationMode.Provider })
                {
                    var data = testSetManager.GetRangeRows<GlobalTestSet>($"{operationModeId}", 1000, token);
                    testSets.AddRange(data.Item1); 
                }

                return testSets;
            }
            catch (Exception ex)
            {
                return new List<GlobalTestSet>();
            }
        }

        public GlobalTestSet GetTestSet(string partitionKey, string rowKey)
        {
            return testSetManager.Find<GlobalTestSet>(partitionKey, rowKey);
        }

        public IEnumerable<GlobalTestSetTracking> GetAllTestSetTracking(string partitionKey)
        {
            try
            {
                TableContinuationToken token = null;
                var trackings = new List<GlobalTestSetTracking>();

                do
                {
                    var data = testSetTrackingManager.GetRangeRows<GlobalTestSetTracking>(partitionKey,1000, token);
                    trackings.AddRange(data.Item1);
                }
                while (token != null);

                return trackings;
            }
            catch (Exception ex)
            {
                return new List<GlobalTestSetTracking>();
            }
        }
    }
}
