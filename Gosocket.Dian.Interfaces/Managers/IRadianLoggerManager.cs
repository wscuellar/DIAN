using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Managers
{
    public interface IRadianLoggerManager
    {
        IEnumerable<RadianLogger> GetAllRadianLogger ();
        RadianLogger GetRadianLogger(string partitionKey, string rowKey);
        bool InsertOrUpdateRadianLogger(RadianLogger logger);
    }
}
