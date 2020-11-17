using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianLoggerService : IRadianLoggerService
    {
        private readonly IRadianLoggerManager _radianLoggerManager;

        public RadianLoggerService(IRadianLoggerManager radianLoggerManager)
        {
            _radianLoggerManager = radianLoggerManager;
        }

        public bool InsertOrUpdateRadianLogger(RadianLogger logger)
        {
            return _radianLoggerManager.InsertOrUpdateRadianLogger(logger);
        }

        public RadianLogger GetRadianLogger(string partitionKey, string rowKey)
        {
            return _radianLoggerManager.GetRadianLogger(partitionKey, rowKey);
        }

        public List<RadianLogger> GetAllLogger()
        {
            return _radianLoggerManager.GetAllRadianLogger().ToList();
        }
    }
}
