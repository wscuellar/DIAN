using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Managers;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Managers
{
    public class RadianLoggerManager : IRadianLoggerManager
    {
        private static readonly TableManager radianLogger = new TableManager("RadianLogger");

        public IEnumerable<RadianLogger> GetAllRadianLogger()
        {
            try
            {
                TableContinuationToken token = null;
                var testSets = new List<RadianLogger>();
                var data = radianLogger.GetRangeRows<RadianLogger>(1000, token);
                testSets.AddRange(data.Item1);

                return testSets;
            }
            catch (Exception)
            {
                return new List<RadianLogger>();
            }
        }

        public RadianLogger GetRadianLogger(string partitionKey, string rowKey)
        {
            return radianLogger.Find<RadianLogger>(partitionKey, rowKey);
        }

        public bool InsertOrUpdateRadianLogger(RadianLogger logger)
        {
            return radianLogger.InsertOrUpdate(logger);
        }
    }
}
