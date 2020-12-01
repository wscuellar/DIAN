using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianTestSetResultService : IRadianTestSetResultService
    {
        private readonly IRadianTestSetResultManager _radianTestSetResultManager;

        public RadianTestSetResultService(IRadianTestSetResultManager radianTestSetResultManager)
        {
            _radianTestSetResultManager = radianTestSetResultManager;
        }


        public List<RadianTestSetResult> GetAllTestSetResult()
        {
            return _radianTestSetResultManager.GetAllTestSetResult().ToList();
        }


        public RadianTestSetResult GetTestSetResult(string partitionKey, string rowKey)
        {
            return _radianTestSetResultManager.GetTestSetResult(partitionKey, rowKey);
        }

        public bool InsertTestSetResult(RadianTestSetResult testSet)
        {
            return _radianTestSetResultManager.InsertOrUpdateTestSetResult(testSet);
        }

        public List<RadianTestSetResult> GetTestSetResultByNit(string nit)
        {
            return _radianTestSetResultManager.GetTestSetResultByNit(nit);
        }

    }
}
