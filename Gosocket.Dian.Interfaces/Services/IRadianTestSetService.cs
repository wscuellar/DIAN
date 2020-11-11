using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianTestSetService
    {
        List<RadianTestSet> GetAllTestSet();
        OperationMode GetOperationMode(int id);
        RadianTestSet GetTestSet(string partitionKey, string rowKey);
        bool InsertTestSet(RadianTestSet testSet);
    }
}