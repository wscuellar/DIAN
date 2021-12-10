using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IOtherDocElecPayrollRepository
    {
        Guid AddOrUpdate(OtherDocElecPayroll otherDocElecPayroll);
        List<OtherDocElecPayroll> GetByFilters(int take, DateTime? monthStart, DateTime? monthEnd, double? enumerationStart, double? enumerationEnd, string employeeDocType, 
            string employeeDocNumber, string firstSurname, double? employeeSalaryStart, double? employeeSalaryEnd, string employeeCity);
    }
}
