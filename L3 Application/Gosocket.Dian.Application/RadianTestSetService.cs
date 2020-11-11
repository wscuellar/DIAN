using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianTestSetService : IRadianTestSetService
    {
        private readonly IContributorService _contributorService;
        private readonly IRadianTestSetManager _testSetManager;

        public RadianTestSetService(IRadianTestSetManager radianTestSetManager, IContributorService contributorService)
        {
            _testSetManager = radianTestSetManager;
            _contributorService = contributorService;
        }

        public List<RadianTestSet> GetAllTestSet()
        {
            return _testSetManager.GetAllTestSet().ToList();
        }

        public OperationMode GetOperationMode(int id)
        {
            return _contributorService.GetOperationMode(id);
        }

        public RadianTestSet GetTestSet(string partitionKey, string rowKey)
        {
            return _testSetManager.GetTestSet(partitionKey, rowKey);
        }

        public bool InsertTestSet(RadianTestSet testSet)
        {
            return _testSetManager.InsertTestSet(testSet);
        }
    }
}
