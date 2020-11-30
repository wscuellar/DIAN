using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianTestSetService : IRadianTestSetService
    {
        private readonly IRadianContributorService _contributorService;
        private readonly IRadianTestSetManager _testSetManager;

        public RadianTestSetService(IRadianTestSetManager radianTestSetManager, IRadianContributorService contributorService)
        {
            _testSetManager = radianTestSetManager;
            _contributorService = contributorService;
        }

        public List<RadianTestSet> GetAllTestSet()
        {
            return _testSetManager.GetAllTestSet().ToList();
        }

        public Domain.RadianOperationMode GetOperationMode(int id)
        {
            Domain.RadianOperationMode operationMode;
            switch (id)
            {
                case 1:
                    operationMode = new Domain.RadianOperationMode { Id = (int)RadianOperationModeTestSet.OwnSoftware, Name = RadianOperationModeTestSet.OwnSoftware.GetDescription() };
                    break;

                case 2:
                    operationMode = new Domain.RadianOperationMode { Id = (int)RadianOperationModeTestSet.SoftwareTechnologyProvider, Name = RadianOperationModeTestSet.SoftwareTechnologyProvider.GetDescription() };
                    break;

                case 3:
                    operationMode = new Domain.RadianOperationMode { Id = (int)RadianOperationModeTestSet.SoftwareTradingSystem, Name = RadianOperationModeTestSet.SoftwareTradingSystem.GetDescription() };
                    break;

                case 4:
                    operationMode = new Domain.RadianOperationMode { Id = (int)RadianOperationModeTestSet.SoftwareFactor, Name = RadianOperationModeTestSet.SoftwareFactor.GetDescription() };
                    break;

                default:
                    operationMode = new Domain.RadianOperationMode();
                    break;
            }

            return operationMode;
        }

        public List<Domain.RadianOperationMode> OperationModeList(RadianOperationMode radianOperation)
        {
            List<Domain.RadianOperationMode> list = new List<Domain.RadianOperationMode>();
            if(radianOperation == RadianOperationMode.Direct || radianOperation == RadianOperationMode.None)
                list.Add(new Domain.RadianOperationMode { Id = (int)RadianOperationModeTestSet.OwnSoftware, Name = RadianOperationModeTestSet.OwnSoftware.GetDescription() });

            if (radianOperation == RadianOperationMode.Indirect || radianOperation == RadianOperationMode.None)
            {
                list.Add(new Domain.RadianOperationMode { Id = (int)RadianOperationModeTestSet.SoftwareTechnologyProvider, Name = RadianOperationModeTestSet.SoftwareTechnologyProvider.GetDescription() });
                list.Add(new Domain.RadianOperationMode { Id = (int)RadianOperationModeTestSet.SoftwareTradingSystem, Name = RadianOperationModeTestSet.SoftwareTradingSystem.GetDescription() });
                list.Add(new Domain.RadianOperationMode { Id = (int)RadianOperationModeTestSet.SoftwareFactor, Name = RadianOperationModeTestSet.SoftwareFactor.GetDescription() });
            }

            return list;
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
