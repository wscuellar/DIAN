using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class GlobalDocPayrollService : IGlobalDocPayrollService
    {
        private readonly TableManager payrollTableManager = new TableManager("GlobalDocPayroll");

        public GlobalDocPayroll Find(string partitionKey)
        {
            return this.payrollTableManager.Find<GlobalDocPayroll>(partitionKey, partitionKey);
        }
    }
}
