using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public interface IRadianTestSetAppliedService
    {
        List<RadianTestSetResult> GetAllTestSetResult();
        RadianTestSetResult GetTestSetResult(string partitionKey, string rowKey);
        bool InsertOrUpdateTestSet(RadianTestSetResult radianTestSet);
        bool ResetPreviousCounts(string testSetId);
    }
}