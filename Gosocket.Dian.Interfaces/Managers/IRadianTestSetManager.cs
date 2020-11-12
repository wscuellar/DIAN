using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Managers
{
    public interface IRadianTestSetManager
    {
        IEnumerable<RadianTestSet> GetAllTestSet();
        RadianTestSet GetTestSet(string partitionKey, string rowKey);
        bool InsertTestSet(RadianTestSet testSet);
    }
}