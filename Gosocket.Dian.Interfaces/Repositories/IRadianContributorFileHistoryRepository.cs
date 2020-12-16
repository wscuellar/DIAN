using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianContributorFileHistoryRepository
    {
        Guid AddRegisterHistory(RadianContributorFileHistory radianContributorFileHistory);

        PagedResult<RadianContributorFileHistory> List(Expression<Func<RadianContributorFileHistory, bool>> expression, int page, int pagesize);

        PagedResult<RadianContributorFileHistory> HistoryByParticipantList(int radiancontributorId, Expression<Func<RadianContributorFileHistory, bool>> expression, int page, int pagesize);


    }
}
