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

                if (otherDocElecPayrollInstance != null)
                {
                    otherDocElecPayrollInstance.CreateDate = otherDocElecPayroll.CreateDate;
                    otherDocElecPayrollInstance.HighPensionRisk = otherDocElecPayroll.HighPensionRisk;
                    otherDocElecPayrollInstance.Environment = otherDocElecPayroll.Environment;
                    otherDocElecPayrollInstance.AuxTransport = otherDocElecPayroll.AuxTransport;
                    otherDocElecPayrollInstance.Bank = otherDocElecPayroll.Bank;
                    otherDocElecPayrollInstance.BonusNS = otherDocElecPayroll.BonusNS;
                    otherDocElecPayrollInstance.BonusS = otherDocElecPayroll.BonusS;
                    otherDocElecPayrollInstance.Quantity = otherDocElecPayroll.Quantity;
                    otherDocElecPayrollInstance.Ces_Paymet = otherDocElecPayroll.Ces_Paymet;
                    otherDocElecPayrollInstance.Ces_InterestPayment = otherDocElecPayroll.Ces_InterestPayment;
                    otherDocElecPayrollInstance.Ces_Percentage = otherDocElecPayroll.Ces_Percentage;
                    otherDocElecPayrollInstance.WorkerCode = otherDocElecPayroll.WorkerCode;
                    otherDocElecPayrollInstance.Commissions = otherDocElecPayroll.Commissions;
                    otherDocElecPayrollInstance.CompensationE = otherDocElecPayroll.CompensationE;
                    otherDocElecPayrollInstance.CompensationO = otherDocElecPayroll.CompensationO;
                    otherDocElecPayrollInstance.TotalVoucher = otherDocElecPayroll.TotalVoucher;
                    otherDocElecPayrollInstance.Consecutive = otherDocElecPayroll.Consecutive;
                    otherDocElecPayrollInstance.CUNE = otherDocElecPayroll.CUNE;
                    otherDocElecPayrollInstance.CUNENov = otherDocElecPayroll.CUNENov;
                    otherDocElecPayrollInstance.CUNEPred = otherDocElecPayroll.CUNEPred;
                    otherDocElecPayrollInstance.DeductionsTotal = otherDocElecPayroll.DeductionsTotal;
                    otherDocElecPayrollInstance.StateDepartment = otherDocElecPayroll.StateDepartment;
                    otherDocElecPayrollInstance.AccruedTotal = otherDocElecPayroll.AccruedTotal;
                    otherDocElecPayrollInstance.WorkedDays = otherDocElecPayroll.WorkedDays;
                    otherDocElecPayrollInstance.DV = otherDocElecPayroll.DV;
                    otherDocElecPayrollInstance.CompanyStateDepartment = otherDocElecPayroll.CompanyStateDepartment;
                    otherDocElecPayrollInstance.CompanyAddress = otherDocElecPayroll.CompanyAddress;
                    otherDocElecPayrollInstance.CompanyDV = otherDocElecPayroll.CompanyDV;
                    otherDocElecPayrollInstance.CompanyCityMunicipality = otherDocElecPayroll.CompanyCityMunicipality;
                    otherDocElecPayrollInstance.CompanyNIT = otherDocElecPayroll.CompanyNIT;
                    otherDocElecPayrollInstance.CompanyOtherNames = otherDocElecPayroll.CompanyOtherNames;
                    otherDocElecPayrollInstance.CompanyCountry = otherDocElecPayroll.CompanyCountry;
                    otherDocElecPayrollInstance.CompanySurname = otherDocElecPayroll.CompanySurname;
                    otherDocElecPayrollInstance.CompanyFirstName = otherDocElecPayroll.CompanyFirstName;
                    otherDocElecPayrollInstance.CompanyBusinessName = otherDocElecPayroll.CompanyBusinessName;
                    otherDocElecPayrollInstance.CompanySecondSurname = otherDocElecPayroll.CompanySecondSurname;
                    otherDocElecPayrollInstance.EncripCUNE = otherDocElecPayroll.EncripCUNE;
                    otherDocElecPayrollInstance.EndDate = otherDocElecPayroll.EndDate;
                    otherDocElecPayrollInstance.GenDate = otherDocElecPayroll.GenDate;
                    otherDocElecPayrollInstance.GenPredDate = otherDocElecPayroll.GenPredDate;
                    otherDocElecPayrollInstance.AdmissionDate = otherDocElecPayroll.AdmissionDate;
                    otherDocElecPayrollInstance.StartDate = otherDocElecPayroll.StartDate;
                    otherDocElecPayrollInstance.SettlementDate = otherDocElecPayroll.SettlementDate;
                    otherDocElecPayrollInstance.EndPaymentDate = otherDocElecPayroll.EndPaymentDate;
                    otherDocElecPayrollInstance.StartPaymentDate = otherDocElecPayroll.StartPaymentDate;
                    otherDocElecPayrollInstance.WithdrawalDate = otherDocElecPayroll.WithdrawalDate;
                    otherDocElecPayrollInstance.PaymentDate = otherDocElecPayroll.PaymentDate;
                    otherDocElecPayrollInstance.Shape = otherDocElecPayroll.Shape;
                    otherDocElecPayrollInstance.FP_Deduction = otherDocElecPayroll.FP_Deduction;
                    otherDocElecPayrollInstance.FP_Percentage = otherDocElecPayroll.FP_Percentage;
                    otherDocElecPayrollInstance.FSP_Deduction = otherDocElecPayroll.FSP_Deduction;
                    otherDocElecPayrollInstance.FSP_DeductionSub = otherDocElecPayroll.FSP_DeductionSub;
                    otherDocElecPayrollInstance.FSP_Percentage = otherDocElecPayroll.FSP_Percentage;
                    otherDocElecPayrollInstance.FSP_PercentageSub = otherDocElecPayroll.FSP_PercentageSub;
                    otherDocElecPayrollInstance.HED = otherDocElecPayroll.HED;
                    otherDocElecPayrollInstance.HEDDF = otherDocElecPayroll.HEDDF;
                    otherDocElecPayrollInstance.HEN = otherDocElecPayroll.HEN;
                    otherDocElecPayrollInstance.HENDF = otherDocElecPayroll.HENDF;
                    otherDocElecPayrollInstance.GenTime = otherDocElecPayroll.GenTime;
                    otherDocElecPayrollInstance.HRDDF = otherDocElecPayroll.HRDDF;
                    otherDocElecPayrollInstance.HRN = otherDocElecPayroll.HRN;
                    otherDocElecPayrollInstance.HRNDF = otherDocElecPayroll.HRNDF;
                    otherDocElecPayrollInstance.Idiom = otherDocElecPayroll.Idiom;
                    otherDocElecPayrollInstance.Inc_Quantity = otherDocElecPayroll.Inc_Quantity;
                    otherDocElecPayrollInstance.Inc_Payment = otherDocElecPayroll.Inc_Payment;
                    otherDocElecPayrollInstance.Info_DateGen = otherDocElecPayroll.Info_DateGen;
                    otherDocElecPayrollInstance.WorkplaceStateDepartment = otherDocElecPayroll.WorkplaceStateDepartment;
                    otherDocElecPayrollInstance.WorkplaceAddress = otherDocElecPayroll.WorkplaceAddress;
                    otherDocElecPayrollInstance.WorkplaceMunicipalityCity = otherDocElecPayroll.WorkplaceMunicipalityCity;
                    otherDocElecPayrollInstance.PlaceWorkCountry = otherDocElecPayroll.PlaceWorkCountry;
                    otherDocElecPayrollInstance.Method = otherDocElecPayroll.Method;
                    otherDocElecPayrollInstance.CityMunicipality = otherDocElecPayroll.CityMunicipality;
                    otherDocElecPayrollInstance.NIT = otherDocElecPayroll.NIT;
                    otherDocElecPayrollInstance.Notes = otherDocElecPayroll.Notes;
                    otherDocElecPayrollInstance.Novelty = otherDocElecPayroll.Novelty;
                    otherDocElecPayrollInstance.SerialNumber = otherDocElecPayroll.SerialNumber;
                    otherDocElecPayrollInstance.AccountNumber = otherDocElecPayroll.AccountNumber;
                    otherDocElecPayrollInstance.DocumentNumber = otherDocElecPayroll.DocumentNumber;
                    otherDocElecPayrollInstance.NumberPred = otherDocElecPayroll.NumberPred;
                    otherDocElecPayrollInstance.OtherNames = otherDocElecPayroll.OtherNames;
                    otherDocElecPayrollInstance.Payment = otherDocElecPayroll.Payment;
                    otherDocElecPayrollInstance.Country = otherDocElecPayroll.Country;
                    otherDocElecPayrollInstance.PayrollPeriod = otherDocElecPayroll.PayrollPeriod;
                    otherDocElecPayrollInstance.SerialPrefix = otherDocElecPayroll.SerialPrefix;
                    otherDocElecPayrollInstance.Surname = otherDocElecPayroll.Surname;
                    otherDocElecPayrollInstance.FirstName = otherDocElecPayroll.FirstName;
                    otherDocElecPayrollInstance.Pri_Quantity = otherDocElecPayroll.Pri_Quantity;
                    otherDocElecPayrollInstance.Pri_Payment = otherDocElecPayroll.Pri_Payment;
                    otherDocElecPayrollInstance.Pri_PaymentNS = otherDocElecPayroll.Pri_PaymentNS;
                    otherDocElecPayrollInstance.ProvOtherNames = otherDocElecPayroll.ProvOtherNames;
                    otherDocElecPayrollInstance.ProvSurname = otherDocElecPayroll.ProvSurname;
                    otherDocElecPayrollInstance.ProvFirstName = otherDocElecPayroll.ProvFirstName;
                    otherDocElecPayrollInstance.Prov_CompanyName = otherDocElecPayroll.Prov_CompanyName;
                    otherDocElecPayrollInstance.ProvSecondSurname = otherDocElecPayroll.ProvSecondSurname;
                    otherDocElecPayrollInstance.RetentionSource = otherDocElecPayroll.RetentionSource;
                    otherDocElecPayrollInstance.ComprehensiveSalary = otherDocElecPayroll.ComprehensiveSalary;
                    otherDocElecPayrollInstance.SalaryWorked = otherDocElecPayroll.SalaryWorked;
                    otherDocElecPayrollInstance.SecondSurname = otherDocElecPayroll.SecondSurname;
                    otherDocElecPayrollInstance.SoftwareID = otherDocElecPayroll.SoftwareID;
                    otherDocElecPayrollInstance.SoftwareSC = otherDocElecPayroll.SoftwareSC;
                    otherDocElecPayrollInstance.WorkerSubType = otherDocElecPayroll.WorkerSubType;
                    otherDocElecPayrollInstance.Salary = otherDocElecPayroll.Salary;
                    otherDocElecPayrollInstance.S_Deduction = otherDocElecPayroll.S_Deduction;
                    otherDocElecPayrollInstance.S_Percentage = otherDocElecPayroll.S_Percentage;
                    otherDocElecPayrollInstance.TimeWorked = otherDocElecPayroll.TimeWorked;
                    otherDocElecPayrollInstance.ContractType = otherDocElecPayroll.ContractType;
                    otherDocElecPayrollInstance.AccountType = otherDocElecPayroll.AccountType;
                    otherDocElecPayrollInstance.DocumentType = otherDocElecPayroll.DocumentType;
                    otherDocElecPayrollInstance.CurrencyType = otherDocElecPayroll.CurrencyType;
                    otherDocElecPayrollInstance.TypeNote = otherDocElecPayroll.TypeNote;
                    otherDocElecPayrollInstance.WorkerType = otherDocElecPayroll.WorkerType;
                    otherDocElecPayrollInstance.XMLType = otherDocElecPayroll.XMLType;
                    otherDocElecPayrollInstance.JobCodeWorker = otherDocElecPayroll.JobCodeWorker;
                    otherDocElecPayrollInstance.TRM = otherDocElecPayroll.TRM;
                    otherDocElecPayrollInstance.Version = otherDocElecPayroll.Version;
                    otherDocElecPayrollInstance.ViaticoManuAlojNS = otherDocElecPayroll.ViaticoManuAlojNS;
                    otherDocElecPayrollInstance.ViaticoManuAlojS = otherDocElecPayroll.ViaticoManuAlojS;

                    context.Entry(otherDocElecPayrollInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    otherDocElecPayroll.Id = Guid.NewGuid();
                    context.Entry(otherDocElecPayroll).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();

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

            if (enumerationStart.HasValue)
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
