using Gosocket.Dian.Domain;
using System;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IRadianContributorFileHistoryRepository
    {
        Guid AddRegisterHistory(RadianContributorFileHistory radianContributorFileHistory);

    }
}
