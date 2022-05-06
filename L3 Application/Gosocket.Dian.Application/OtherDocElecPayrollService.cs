using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class OtherDocElecPayrollService : IOtherDocElecPayroll
    {
        private IOtherDocElecPayrollRepository _otherDocElecPayrollRepository;
        private readonly SqlDBContext sqlDBContext;

        public OtherDocElecPayrollService(IOtherDocElecPayrollRepository otherDocElecPayrollRepository)
        {
            _otherDocElecPayrollRepository = otherDocElecPayrollRepository;
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public OtherDocElecPayrollService(){
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public OtherDocElecPayroll CreateOtherDocElecPayroll(OtherDocElecPayroll otherDocElecPayroll)
        {
            using (var context = new SqlDBContext())
            {
                OtherDocElecPayroll otherDocElecPayrollInstance =
                    context.OtherDocElecPayroll.FirstOrDefault(c => c.CUNE == otherDocElecPayroll.CUNE);

                if(otherDocElecPayrollInstance == null)
                {
                    otherDocElecPayroll.Id = Guid.NewGuid();
                    context.Entry(otherDocElecPayroll).State = System.Data.Entity.EntityState.Added;

                    context.SaveChanges();
                }

                return otherDocElecPayroll;

            }
        }

        public List<OtherDocElecPayroll> Find_ByMonth_EnumerationRange_EmployeeDocType_EmployeeDocNumber_FirstSurname_EmployeeSalaryRange_EmployerCity(int take, DateTime? monthStart,
            DateTime? monthEnd, double? enumerationStart, double? enumerationEnd, string employeeDocType, string employeeDocNumber, string firstSurname, double? employeeSalaryStart,
            double? employeeSalaryEnd, string employeeCity)
        {
            string sqlQuery = string.Empty;
            string sqlQueryFROM = $" SELECT TOP {take} [Id],[CUNE],[CompanyNIT],[CreateDate],[HighPensionRisk],[Environment],[CUNEPred],[WorkerCode],[CompensationE]," +
                $"[CompensationO],[TotalVoucher],[Consecutive],[DV],[DeductionsTotal],[StateDepartment],[AccruedTotal],[WorkedDays],[CompanyDV],[CompanyStateDepartment]," +
                $"[CompanyAddress],[CompanyCityMunicipality],[CompanyCountry],[CompanyBusinessName],[EncripCUNE],[FP_Deduction],[FP_Percentage],[FSP_Deduction]," +
                $"[FSP_DeductionSub],[FSP_Percentage],[FSP_PercentageSub],[GenDate],[AdmissionDate],[EndPaymentDate],[StartPaymentDate],[PaymentDate],[Shape]," +
                $"[HED],[HEDDF],[HEN],[HENDF],[HRDDF],[HRN],[HRNDF],[GenTime],[Idiom],[Inc_Quantity],[Inc_Payment],[Info_DateGen],[WorkplaceStateDepartment]," +
                $"[WorkplaceAddress],[WorkplaceMunicipalityCity],[PlaceWorkCountry],[Method],[CityMunicipality],[NIT],[Novelty],[SerialNumber],[AccountNumber]," +
                $"[DocumentNumber],[Country],[PayrollPeriod],[SerialPrefix],[Surname],[FirstName],[Prov_CompanyName],[ComprehensiveSalary],[SalaryWorked],[SecondSurname]," +
                $"[SoftwareID],[SoftwareSC],[WorkerSubType],[Salary],[TRM],[TimeWorked],[ContractType],[AccountType],[DocumentType],[CurrencyType],[WorkerType],[XMLType]," +
                $"[JobCodeWorker],[Version],[S_Deduction],[S_Percentage],[AuxTransport],[Bank],[CompanySurname],[CompanyFirstName],[CompanySecondSurname],[OtherNames]," +
                $"[Pri_Quantity],[Pri_Payment],[Pri_PaymentNS],[ViaticoManuAlojS],[WithdrawalDate],[BonusS],[BonusNS],[RetentionSource],[Ces_Paymet],[Ces_InterestPayment]," +
                $"[Ces_Percentage],[Commissions],[GenPredDate],[Notes],[NumberPred],[TypeNote],[Quantity],[EndDate],[StartDate],[Payment],[CompanyOtherNames]," +
                $"[ViaticoManuAlojNS],[CUNENov],[ProvOtherNames],[ProvSurname],[ProvFirstName],[ProvSecondSurname],[FP_BaseValue],[SettlementDate],[PayrollType],[S_BaseValue] " +
                $"FROM[dbo].[OtherDocElecPayroll] ";
            string sqlQueryConditions = string.Empty;
            string sqlQueryOrderBy = "ORDER BY CreateDate DESC";

            if (enumerationStart.HasValue && enumerationStart.Value > 0)
            {
                sqlQueryConditions += $" Convert(bigint,Consecutive) BETWEEN {enumerationStart.Value} AND {enumerationEnd.Value} AND ";
            }

            if (monthStart.HasValue)
            {
                sqlQueryConditions += $" StartPaymentDate BETWEEN '{monthStart.Value.ToString("yyyy-MM-ddT00:00:00.000Z")}' AND '{monthEnd.Value.ToString("yyyy-MM-ddT00:00:00.000Z")}' AND ";
            }

            if (!string.IsNullOrWhiteSpace(employeeDocType))
            {
                sqlQueryConditions += $" DocumentType = '{employeeDocType}' AND ";
            }

            if (!string.IsNullOrWhiteSpace(employeeDocNumber))
            {
                sqlQueryConditions += $" DocumentNumber = '{employeeDocNumber}' AND ";
            }

            if (!string.IsNullOrWhiteSpace(firstSurname))
            {
                sqlQueryConditions += $" Surname = '{firstSurname}' AND ";
            }

            if (employeeSalaryStart.HasValue)
            {
                sqlQueryConditions += $" Convert(bigint,Salary) BETWEEN {employeeSalaryStart.Value} AND {employeeSalaryEnd.Value} AND ";
            }

            if (!string.IsNullOrWhiteSpace(employeeCity))
            {
                sqlQueryConditions += $" WorkplaceMunicipalityCity = '{employeeCity}' AND ";
            }

            if (string.IsNullOrEmpty(sqlQueryConditions))
            {
                sqlQuery = $"{sqlQueryFROM} {sqlQueryOrderBy}";
            }
            else
            {
                sqlQueryConditions = sqlQueryConditions.Substring(0, sqlQueryConditions.Length - 4);

                sqlQuery = $"{sqlQueryFROM} WHERE {sqlQueryConditions} {sqlQueryOrderBy}";
            }

            List<OtherDocElecPayroll> otherDocElecPayrolls = new List<OtherDocElecPayroll>();

            otherDocElecPayrolls = sqlDBContext.OtherDocElecPayroll.SqlQuery(sqlQuery).ToList<OtherDocElecPayroll>();

            return otherDocElecPayrolls;
        }
    }
}
