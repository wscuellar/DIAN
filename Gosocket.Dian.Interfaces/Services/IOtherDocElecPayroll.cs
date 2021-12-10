using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IOtherDocElecPayroll
    {
        OtherDocElecPayroll CreateOtherDocElecPayroll(OtherDocElecPayroll otherDocElecPayroll);
        List<OtherDocElecPayroll> Find_ByMonth_EnumerationRange_EmployeeDocType_EmployeeDocNumber_FirstSurname_EmployeeSalaryRange_EmployerCity(int take, DateTime? monthStart, DateTime? monthEnd, double? enumerationStart, double? enumerationEnd, string employeeDocType,
            string employeeDocNumber, string firstSurname, double? employeeSalaryStart, double? employeeSalaryEnd, string employeeCity);
    }
}
