using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public interface IRadianLoggerService
    {
        List<RadianLogger> GetAllLogger();
        RadianLogger GetRadianLogger(string partitionKey, string rowKey);
        bool InsertOrUpdateRadianLogger(RadianLogger logger);
    }
}