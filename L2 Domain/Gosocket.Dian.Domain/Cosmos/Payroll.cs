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
		public string WorkedDays { get; set; }

		[JsonProperty("SalaryPaid")]
		public string SalaryPaid { get; set; }
	}

	public partial class DocumentData
	{
		[JsonProperty("Novelty")]
		public string Novelty { get; set; }
		[JsonProperty("NoveltyCUNE")]
		public string NoveltyCune { get; set; }
		[JsonProperty("AdmissionDate")]
		public string AdmissionDate { get; set; }
		[JsonProperty("SettlementDateStartMonth")]
		public string SettlementDateStartMonth { get; set; }
		[JsonProperty("SettlementDateEndMonth")]
		public string SettlementDateEndMonth { get; set; }
		[JsonProperty("TimeWorkedCompany")]
		public string TimeWorkedCompany { get; set; }
		[JsonProperty("GenerationDate")]
		public string GenerationDate { get; set; }
		[JsonProperty("GenerationDateNumber")]
		public string GenerationDateNumber { get; set; }
		[JsonProperty("Language")]
		public string Language { get; set; }
		[JsonProperty("IdPeriodPayroll")]
		public string IdPeriodPayroll { get; set; }
		[JsonProperty("NamePeriodPayroll")]
		public string NamePeriodPayroll { get; set; }
		[JsonProperty("TypeCoin")]
		public string TypeCoin { get; set; }
		[JsonProperty("CompositeNameTypeCoin")]
		public string CompositeNameTypeCoin { get; set; }
		[JsonProperty("TRM")]
		public string Trm { get; set; }
		[JsonProperty("Rounding")]
		public string Rounding { get; set; }
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
		public string NumberDocEmployeer { get; set; }
		[JsonProperty("DVEmployeer")]
		public string DvEmployeer { get; set; }
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
		public bool HighRiskPensionWorker { get; set; }
		[JsonProperty("ContractTypeWorker")]
		public string ContractTypeWorker { get; set; }
		[JsonProperty("NameContractTypeWorker")]
		public string NameContractTypeWorker { get; set; }
		[JsonProperty("SalaryIntegralWorker")]
		public bool SalaryIntegralWorker { get; set; }
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
		public string DocTypeWorker { get; set; }
		[JsonProperty("NameDocTypeWorker")]
		public string NameDocTypeWorker { get; set; }
		[JsonProperty("NumberDocWorker")]
		public string NumberDocWorker { get; set; }
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
		public string PaymentDate { get; set; }
	}

	public partial class Countries
	{
		[JsonProperty("NameCountry")]
		public string NameCountry { get; set; }

		[JsonProperty("NameCountryISO")]
		public string NameCountryIso { get; set; }

		[JsonProperty("CodeAlfa2")]
		public string CodeAlfa2 { get; set; }

		[JsonProperty("CodeAlfa3")]
		public string CodeAlfa3 { get; set; }

		[JsonProperty("IDContry")]
		public string IdContry { get; set; }

		[JsonProperty("Observaciones")]
		public string Observaciones { get; set; }

		[JsonProperty("CompositeNameCountry")]
		public string CompositeNameCountry { get; set; }

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

	public partial class Departament
	{

		[JsonProperty("IDDepartament")]
		// [JsonConverter(typeof(ParseStringConverter))]
		public string IdDepartament { get; set; }

		[JsonProperty("NameDepartament")]
		public string NameDepartament { get; set; }

		[JsonProperty("CodeISO")]
		public string CodeIso { get; set; }

		[JsonProperty("CompositeNameDepartament")]
		public string CompositeNameDepartament { get; set; }

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

	public partial class Location
	{
		[JsonProperty("IDDepartament")]
		public long IdDepartament { get; set; }

		[JsonProperty("IDCity")]
		public string IdCity { get; set; }

		[JsonProperty("NameDepartament")]
		public string NameDepartament { get; set; }

		[JsonProperty("NameCity")]
		public string NameCity { get; set; }

		[JsonProperty("CompositeNameCity")]
		public string CompositeNameCity { get; set; }

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

	public partial class CoinType
	{
		[JsonProperty("IDCoinType")]
		public string IdCoinType { get; set; }

		[JsonProperty("NameCoinType")]
		public string NameCoinType { get; set; }

		[JsonProperty("CompositeNameCoinType")]
		public string CompositeNameCoinType { get; set; }

		[JsonProperty("State")]
		public long State { get; set; }

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

	public partial class ContractType
	{
		[JsonProperty("IDContractType")]
		public string IdContractType { get; set; }

		[JsonProperty("NameContractType")]
		public string NameContractType { get; set; }

		[JsonProperty("CompositeName")]
		public string CompositeName { get; set; }

		[JsonProperty("State")]
		public long State { get; set; }

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

	public partial class DocumentTypes
	{
		[JsonProperty("IDDocumentType")]
		public string IdDocumentType { get; set; }

		[JsonProperty("NameDocumentType")]
		public string NameDocumentType { get; set; }

		[JsonProperty("CompositeName")]
		public string CompositeName { get; set; }

		[JsonProperty("State")]
		public long State { get; set; }

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


	public partial class SubWorkerType
	{
		[JsonProperty("IDSubWorkerType")]
		public string IdSubWorkerType { get; set; }

		[JsonProperty("NameSubWorkerType")]
		public string NameSubWorkerType { get; set; }

		[JsonProperty("CompositeName")]
		public string CompositeName { get; set; }

		[JsonProperty("State")]
		public long State { get; set; }

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

	public partial class WorkerType
	{
		[JsonProperty("IDWorkerType")]
		public string IdWorkerType { get; set; }

		[JsonProperty("NameWorkerType")]
		public string NameWorkerType { get; set; }

		[JsonProperty("CompositeName")]
		public string CompositeName { get; set; }

		[JsonProperty("State")]
		public long State { get; set; }

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


	public partial class City
	{
		[JsonProperty("IDDepartament")]
		public long IdDepartament { get; set; }

		[JsonProperty("IDCity")]
		public string IdCity { get; set; }

		[JsonProperty("NameDepartament")]
		public string NameDepartament { get; set; }

		[JsonProperty("NameCity")]
		public string NameCity { get; set; }

		[JsonProperty("CompositeNameCity")]
		public string CompositeNameCity { get; set; }

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

	public partial class PeriodPayroll
	{
		[JsonProperty("IDPeriodPayroll")]
		public string IdPeriodPayroll { get; set; }

		[JsonProperty("NamePeriodPayroll")]
		public string NamePeriodPayroll { get; set; }

		[JsonProperty("CompositeNamePeriodPayroll")]
		public string CompositeNamePeriodPayroll { get; set; }

		[JsonProperty("State")]
		public long State { get; set; }
	}


	public partial class NumberingRange
	{
		[JsonProperty("PartitionKey")]
		public string PartitionKey { get; set; }

		[JsonProperty("DocumentKey")]
		public string DocumentKey { get; set; }

		[JsonProperty("AccountId")]
		public Guid AccountId { get; set; }

		[JsonProperty("IDNumberingRange")]
		public long IdNumberingRange { get; set; }

		[JsonProperty("IdDocumentTypePayroll")]
		public string IdDocumentTypePayroll { get; set; }

		[JsonProperty("DocumentTypePayroll")]
		public string DocumentTypePayroll { get; set; }

		[JsonProperty("Prefix")]
		public string Prefix { get; set; }

		[JsonProperty("NumberFrom")]
		public long NumberFrom { get; set; }

		[JsonProperty("NumberTo")]
		public long NumberTo { get; set; }

		[JsonProperty("CurrentNumber")]
		public long CurrentNumber { get; set; }

		[JsonProperty("TypeXML")]
		public object TypeXml { get; set; }

		[JsonProperty("Current")]
		public string Current { get; set; }

		[JsonProperty("N102")]
		public string N102 { get; set; }

		[JsonProperty("N103")]
		public string N103 { get; set; }

		[JsonProperty("State")]
		public long State { get; set; }

		[JsonProperty("CreationDate")]
		public DateTime CreationDate { get; set; }

		[JsonProperty("id")]
		public Guid id { get; set; }

        public string ResolutionNumber { get; set; }
        public DateTime ExpirationDate { get; set; }
        public long OtherDocElecContributorOperation { get; set; }
    }

	public partial class PaymentForm
	{
		[JsonProperty("IDPaymentForm")]
		public string IdPaymentForm { get; set; }

		[JsonProperty("NamePaymentForm")]
		public string NamePaymentForm { get; set; }

		[JsonProperty("CompositeName")]
		public string CompositeName { get; set; }
	}

	public partial class PaymentMethod
	{
		[JsonProperty("IDPaymentMethod")]
		public string IdPaymentMethod { get; set; }

		[JsonProperty("NamePaymentMethod")]
		public string NamePaymentMethod { get; set; }

		[JsonProperty("CompositeName")]
		public string CompositeName { get; set; }
	}
	public partial class List
	{
		[JsonProperty("PartitionKey")]
		public long PartitionKey { get; set; }
		[JsonProperty("IdList")]
		public long IdList { get; set; }

		[JsonProperty("IdSubList")]

		public string IdSubList { get; set; }

		[JsonProperty("ListName")]
		public string ListName { get; set; }

		[JsonProperty("CompositeName")]
		public string CompositeName { get; set; }

		[JsonProperty("State")]
		public long State { get; set; }

		[JsonProperty("CreationDate")]
		public string CreationDate { get; set; }

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

}
