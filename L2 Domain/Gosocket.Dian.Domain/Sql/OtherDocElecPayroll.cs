using System;
using System.ComponentModel.DataAnnotations;


namespace Gosocket.Dian.Domain.Sql
{
    [System.ComponentModel.DataAnnotations.Schema.Table("OtherDocElecPayroll")]
    public class OtherDocElecPayroll
    {
        public OtherDocElecPayroll()
        {

        }

        [Key]
		public Guid Id { get; set; }
		public string CUNE { get; set; }
		public long CompanyNIT { get; set; }
		public DateTime? CreateDate { get; set; }
		public bool HighPensionRisk { get; set; }
		public int Environment { get; set; }
		public string CUNEPred { get; set; }
		public string WorkerCode { get; set; }
		public string CompensationE { get; set; }
		public string CompensationO { get; set; }
		public string TotalVoucher { get; set; }
		public string Consecutive { get; set; }
		public short? DV { get; set; }
		public string DeductionsTotal { get; set; }
		public string StateDepartment { get; set; }
		public string AccruedTotal { get; set; }
		public string WorkedDays { get; set; }
		public short? CompanyDV { get; set; }
		public string CompanyStateDepartment { get; set; }
		public string CompanyAddress { get; set; }
		public string CompanyCityMunicipality { get; set; }
		public string CompanyCountry { get; set; }
		public string CompanyBusinessName { get; set; }
		public string EncripCUNE { get; set; }
		public string FP_Deduction { get; set; }
		public string FP_Percentage { get; set; }
		public string FSP_Deduction { get; set; }
		public string FSP_DeductionSub { get; set; }
		public string FSP_Percentage { get; set; }
		public string FSP_PercentageSub { get; set; }
		public DateTime? GenDate { get; set; }
		public DateTime? AdmissionDate { get; set; }
		public DateTime? EndPaymentDate { get; set; }
		public DateTime? StartPaymentDate { get; set; }
		public string PaymentDate { get; set; }
		public string Shape { get; set; }
		public string HED { get; set; }
		public string HEDDF { get; set; }
		public string HEN { get; set; }
		public string HENDF { get; set; }
		public string HRDDF { get; set; }
		public string HRN { get; set; }
		public string HRNDF { get; set; }
		public string GenTime { get; set; }
		public string Idiom { get; set; }
		public string Inc_Quantity { get; set; }
		public string Inc_Payment { get; set; }
		public DateTime? Info_DateGen { get; set; }
		public string WorkplaceStateDepartment { get; set; }
		public string WorkplaceAddress { get; set; }
		public string WorkplaceMunicipalityCity { get; set; }
		public string PlaceWorkCountry { get; set; }
		public string Method { get; set; }
		public string CityMunicipality { get; set; }
		public long? NIT { get; set; }
		public bool? Novelty { get; set; }
		public string SerialNumber { get; set; }
		public string AccountNumber { get; set; }
		public string DocumentNumber { get; set; }
		public string Country { get; set; }
		public string PayrollPeriod { get; set; }
		public string SerialPrefix { get; set; }
		public string Surname { get; set; }
		public string FirstName { get; set; }
		public string Prov_CompanyName { get; set; }
		public bool? ComprehensiveSalary { get; set; }
		public string SalaryWorked { get; set; }
		public string SecondSurname { get; set; }
		public string SoftwareID { get; set; }
		public string SoftwareSC { get; set; }
		public string WorkerSubType { get; set; }
		public string Salary { get; set; }
		public string TRM { get; set; }
		public string TimeWorked { get; set; }
		public string ContractType { get; set; }
		public string AccountType { get; set; }
		public string DocumentType { get; set; }
		public string CurrencyType { get; set; }
		public string WorkerType { get; set; }
		public string XMLType { get; set; }
		public string JobCodeWorker { get; set; }
		public string Version { get; set; }
		public string S_Deduction { get; set; }
		public string S_Percentage { get; set; }
		public string AuxTransport { get; set; }
		public string Bank { get; set; }
		public string CompanySurname { get; set; }
		public string CompanyFirstName { get; set; }
		public string CompanySecondSurname { get; set; }
		public string OtherNames { get; set; }
		public string Pri_Quantity { get; set; }
		public string Pri_Payment { get; set; }
		public string Pri_PaymentNS { get; set; }
		public string ViaticoManuAlojS { get; set; }
		public string WithdrawalDate { get; set; }
		public string BonusS { get; set; }
		public string BonusNS { get; set; }
		public string RetentionSource { get; set; }
		public string Ces_Paymet { get; set; }
		public string Ces_InterestPayment { get; set; }
		public string Ces_Percentage { get; set; }
		public string Commissions { get; set; }
		public string GenPredDate { get; set; }
		public string Notes { get; set; }
		public string NumberPred { get; set; }
		public short TypeNote { get; set; }
		public string Quantity { get; set; }
		public string EndDate { get; set; }
		public string StartDate { get; set; }
		public string Payment { get; set; }
		public string CompanyOtherNames { get; set; }
		public string ViaticoManuAlojNS { get; set; }
		public string CUNENov { get; set; }
		public string ProvOtherNames { get; set; }
		public string ProvSurname { get; set; }
		public string ProvFirstName { get; set; }
		public string ProvSecondSurname { get; set; }
		public string FP_BaseValue { get; set; }
		public string SettlementDate { get; set; }
		public string PayrollType { get; set; }
		public string S_BaseValue { get; set; }
	}
}
