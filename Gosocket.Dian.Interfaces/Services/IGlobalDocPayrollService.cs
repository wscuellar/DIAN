using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IGlobalDocPayrollService
    {
        GlobalDocPayroll Find(string partitionKey);
    }
}
