using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class GlobalDocPayrollService : IGlobalDocPayrollService
    {
        private readonly ITableManager payrollTableManager = new TableManager("GlobalDocPayroll");

        public GlobalDocPayrollService() { }
        public GlobalDocPayrollService(ITableManager _payrollTableManager) { payrollTableManager = _payrollTableManager; }

        public GlobalDocPayroll Find(string partitionKey)
        {
            return this.payrollTableManager.FindByPartition<GlobalDocPayroll>(partitionKey).FirstOrDefault();
        }
    }
}
