using System;
using Newtonsoft.Json;

namespace Gosocket.Dian.Domain.Cosmos
{
	public partial class Payroll
	{
		[JsonProperty("PartitionKey")]
		public string PartitionKey { get; set; }
		[JsonProperty("DocumentKey")]
		public string DocumentKey { get; set; }
		[JsonProperty("AccountId")]
		public Guid AccountId { get; set; }
		[JsonProperty("CUNE")]
		public string Cune { get; set; }
		[JsonProperty("DocumentData")]
		public DocumentData DocumentData { get; set; }
		[JsonProperty("Notes")]
		public Note[] Notes { get; set; }
		[JsonProperty("EmployerData")]
		public EmployerData EmployerData { get; set; }
		[JsonProperty("WorkerData")]
		public WorkerData WorkerData { get; set; }
		[JsonProperty("PaymentData")]
		public PaymentData PaymentData { get; set; }
		[JsonProperty("BasicAccruals")]
		public BasicAccruals BasicAccruals { get; set; }
		[JsonProperty("id")]
		public Guid Id { get; set; }
		[JsonProperty("_rid")]
		public string Rid { get; set; }
		[JsonProperty("_self")]
		public string Self { get; set; }
		[JsonProperty("_etag")]
		public string Etag { get; set; }
		[JsonProperty("_attachments")]
		public string Attachments { get; set; }
		[JsonProperty("_ts")]
		public long Ts { get; set; }
	}
	public partial class BasicAccruals
	{
		[JsonProperty("WorkedDays")]
		public long WorkedDays { get; set; }

		[JsonProperty("SalaryPaid")]
		public long SalaryPaid { get; set; }
	}

	public partial class DocumentData
	{
		[JsonProperty("Novelty")]
		public string Novelty { get; set; }
		[JsonProperty("NoveltyCUNE")]
		public string NoveltyCune { get; set; }
		[JsonProperty("AdmissionDate")]
		public DateTimeOffset AdmissionDate { get; set; }
		[JsonProperty("SettlementDateStartMonth")]
		public DateTimeOffset SettlementDateStartMonth { get; set; }
		[JsonProperty("SettlementDateEndMonth")]
		public DateTimeOffset SettlementDateEndMonth { get; set; }
		[JsonProperty("TimeWorkedCompany")]
		public long TimeWorkedCompany { get; set; }
		[JsonProperty("GenerationDate")]
		public DateTimeOffset GenerationDate { get; set; }
		[JsonProperty("GenerationDateNumber")]
		public long GenerationDateNumber { get; set; }
		[JsonProperty("Language")]
		public string Language { get; set; }
		[JsonProperty("IdPeriodPayroll")]
		public long IdPeriodPayroll { get; set; }
		[JsonProperty("NamePeriodPayroll")]
		public string NamePeriodPayroll { get; set; }
		[JsonProperty("TypeCoin")]
		public string TypeCoin { get; set; }
		[JsonProperty("CompositeNameTypeCoin")]
		public string CompositeNameTypeCoin { get; set; }
		[JsonProperty("TRM")]
		public double Trm { get; set; }
		[JsonProperty("Rounding")]
		public object Rounding { get; set; }
		[JsonProperty("CodeEmployee")]
		public string CodeEmployee { get; set; }
		[JsonProperty("IdNumberRange")]
		public string IdNumberRange { get; set; }
		[JsonProperty("NameNumberRange")]
		public string NameNumberRange { get; set; }
		[JsonProperty("Prefix")]
		public string Prefix { get; set; }
		[JsonProperty("Consecutive")]
		public string Consecutive { get; set; }
		[JsonProperty("Number")]
		public string Number { get; set; }
		[JsonProperty("GenerationContry")]
		public string GenerationContry { get; set; }
		[JsonProperty("IdGenerationCountry")]
		public string IdGenerationCountry { get; set; }
		[JsonProperty("IdGenerationDepartament")]
		public string IdGenerationDepartament { get; set; }
		[JsonProperty("NameCompositeGenerationDepartament")]
		public string NameCompositeGenerationDepartament { get; set; }
		[JsonProperty("IdGenerationCity")]
		public string IdGenerationCity { get; set; }
		[JsonProperty("NameCompositeGenerationCity")]
		public string NameCompositeGenerationCity { get; set; }
		[JsonProperty("Settlementdocument")]
		public string Settlementdocument { get; set; }
		[JsonProperty("CompanyWithdrawalDate")]
		public DateTimeOffset CompanyWithdrawalDate { get; set; }
	}
	public partial class EmployerData
	{
		[JsonProperty("IsBusinessName")]
		public string IsBusinessName { get; set; }
		[JsonProperty("NumberDocEmployeer")]
		public long NumberDocEmployeer { get; set; }
		[JsonProperty("DVEmployeer")]
		public long DvEmployeer { get; set; }
		[JsonProperty("BusinessName")]
		public string BusinessName { get; set; }
		[JsonProperty("FirstName")]
		public string FirstName { get; set; }
		[JsonProperty("SecondName")]
		public string SecondName { get; set; }
		[JsonProperty("LastName")]
		public string LastName { get; set; }
		[JsonProperty("SecondLastName")]
		public string SecondLastName { get; set; }
		[JsonProperty("IdCountryEmployer")]
		public string IdCountryEmployer { get; set; }
		[JsonProperty("ContryEmployeer")]
		public string ContryEmployeer { get; set; }
		[JsonProperty("IdDepartamentEmployer")]
		public string IdDepartamentEmployer { get; set; }
		[JsonProperty("NameDepartamentEmployeer")]
		public string NameDepartamentEmployeer { get; set; }
		[JsonProperty("IdCityEmployer")]
		public string IdCityEmployer { get; set; }
		[JsonProperty("NameCompositeEmployer")]
		public string NameCompositeEmployer { get; set; }
		[JsonProperty("AddressEmployer")]
		public string AddressEmployer { get; set; }
	}

	public partial class Note
	{
		[JsonProperty("Note")]
		public string NoteNote { get; set; }
	}

	public partial class PaymentData
	{
		[JsonProperty("PaymentForm")]
		public string PaymentForm { get; set; }
		[JsonProperty("NamePaymentForm")]
		public string NamePaymentForm { get; set; }
		[JsonProperty("PaymentMethod")]
		public string PaymentMethod { get; set; }
		[JsonProperty("NamePaymentMethod")]
		public string NamePaymentMethod { get; set; }
		[JsonProperty("Bank")]
		public string Bank { get; set; }
		[JsonProperty("AccountType")]
		public string AccountType { get; set; }
		[JsonProperty("AccountNumber")]
		public string AccountNumber { get; set; }
		[JsonProperty("PaymentDateData")]
		public PaymentDateDatum[] PaymentDateData { get; set; }
	}



	public partial class WorkerData
	{
		[JsonProperty("TypeWorker")]
		public string TypeWorker { get; set; }
		[JsonProperty("NameTypeWorker")]
		public string NameTypeWorker { get; set; }
		[JsonProperty("SubTypeWorker")]
		public string SubTypeWorker { get; set; }
		[JsonProperty("NameSubTypeWorker")]
		public string NameSubTypeWorker { get; set; }
		[JsonProperty("HighRiskPensionWorker")]
		public string HighRiskPensionWorker { get; set; }
		[JsonProperty("ContractTypeWorker")]
		public long ContractTypeWorker { get; set; }
		[JsonProperty("NameContractTypeWorker")]
		public string NameContractTypeWorker { get; set; }
		[JsonProperty("SalaryIntegralWorker")]
		public string SalaryIntegralWorker { get; set; }
		[JsonProperty("SalaryWorker")]
		public double SalaryWorker { get; set; }
		[JsonProperty("IdCountryWorker")]
		public string IdCountryWorker { get; set; }
		[JsonProperty("ContryWorkeer")]
		public string ContryWorkeer { get; set; }
		[JsonProperty("IdDepartamentWorker")]
		public string IdDepartamentWorker { get; set; }
		[JsonProperty("NameDepartamentWorker")]
		public string NameDepartamentWorker { get; set; }
		[JsonProperty("IdCityWorker")]
		public string IdCityWorker { get; set; }
		[JsonProperty("NameCompositeWorker")]
		public string NameCompositeWorker { get; set; }
		[JsonProperty("AddressWorker")]
		public string AddressWorker { get; set; }
		[JsonProperty("DocTypeWorker")]
		public long DocTypeWorker { get; set; }
		[JsonProperty("NameDocTypeWorker")]
		public string NameDocTypeWorker { get; set; }
		[JsonProperty("NumberDocWorker")]
		public long NumberDocWorker { get; set; }
		[JsonProperty("FirstNameWorker")]
		public string FirstNameWorker { get; set; }
		[JsonProperty("SecondNameWorker")]
		public string SecondNameWorker { get; set; }
		[JsonProperty("LastNameWorker")]
		public string LastNameWorker { get; set; }
		[JsonProperty("SecondLastNameWorker")]
		public string SecondLastNameWorker { get; set; }
		[JsonProperty("CodeWorker")]
		public string CodeWorker { get; set; }
	}
	public partial class PaymentDateDatum
	{
		[JsonProperty("PaymentDate")]
		public DateTime PaymentDate { get; set; }
	}


}
