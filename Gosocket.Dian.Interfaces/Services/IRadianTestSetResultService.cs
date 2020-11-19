using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianTestSetResultService
    {
        List<RadianTestSetResult> GetAllTestSetResult();
        RadianTestSetResult GetTestSetResult(string partitionKey, string rowKey);
        bool InsertTestSetResult(RadianTestSetResult testSet);
        List<RadianTestSetResult> GetTestSetResultByNit(string nit);
    }
}