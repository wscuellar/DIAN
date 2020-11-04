using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Managers
{
    public class RadianTestSetManager
    {
        private static readonly TableManager testSetManager = new TableManager("RadianTestSet");

        public bool InsertTestSet(RadianTestSet testSet)
        {
            return testSetManager.InsertOrUpdate(testSet);
        }

        //public bool InsertTestSetTracking(GlobalTestSetTracking testSetTracking)
        //{
        //    return testSetTrackingManager.Insert(testSetTracking);
        //}

        public IEnumerable<RadianTestSet> GetAllTestSet(/*string partitionKey*/)
        {
            try
            {
                TableContinuationToken token = null;
                var testSets = new List<RadianTestSet>();
                foreach (var operationModeId in new List<int> { (int)OperationMode.Free, (int)OperationMode.Own, (int)OperationMode.Provider })
                {
                    var data = testSetManager.GetRangeRows<RadianTestSet>($"{operationModeId}", 1000, token);
                    testSets.AddRange(data.Item1);
                }

                return testSets;
            }
            catch (Exception )
            {
                return new List<RadianTestSet>();
            }
        }

        public RadianTestSet GetTestSet(string partitionKey, string rowKey)
        {
            return testSetManager.Find<RadianTestSet>(partitionKey, rowKey);
        }

        //public IEnumerable<GlobalTestSetTracking> GetAllTestSetTracking(string partitionKey)
        //{
        //    try
        //    {
        //        TableContinuationToken token = null;
        //        var trackings = new List<GlobalTestSetTracking>();

        //        do
        //        {
        //            var data = testSetTrackingManager.GetRangeRows<GlobalTestSetTracking>(partitionKey, 1000, token);
        //            trackings.AddRange(data.Item1);
        //        }
        //        while (token != null);

        //        return trackings;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<GlobalTestSetTracking>();
        //    }
        //}

    }
}
