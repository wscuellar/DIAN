using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IGlobalAuthorizationService
    {
        GlobalAuthorization Find(string partitionKey, string rowKey);

        bool InsertOrUpdate(GlobalAuthorization globalAuthorization);
    }
}
