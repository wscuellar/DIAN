using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Domain.Cosmos
{
	public partial class Payroll_Replace
	{
		[JsonProperty("PartitionKey")]
		public string PartitionKey { get; set; }

		[JsonProperty("DocumentKey")]
		public string DocumentKey { get; set; }

		[JsonProperty("AccountId")]
		public Guid AccountId { get; set; }

		[JsonProperty("CUNE")]
		public string Cune { get; set; }

		[JsonProperty("NoteType")]
		public NoteTypeReplace NoteType { get; set; }

		[JsonProperty("DataDocumentReplace")]
		public DataDocumentReplace DataDocumentReplace { get; set; }

		[JsonProperty("DocumentData")]
		public DocumentDataR DocumentData { get; set; }

		[JsonProperty("Notes")]
		public NoteR[] Notes { get; set; }

		[JsonProperty("EmployerData")]
		public EmployerDataR EmployerData { get; set; }

		[JsonProperty("WorkerData")]
		public WorkerDataR WorkerData { get; set; }

		[JsonProperty("PaymentData")]
		public PaymentData PaymentData { get; set; }

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

	public partial class NoteTypeReplace
	{
		[JsonProperty("NoteTypeID")]
		public int NoteTypeID { get; set; }

		[JsonProperty("NameNoteType")]
		public string NameNoteType { get; set; }
	}

	public partial class DataDocumentReplace
	{
		[JsonProperty("ReplaceCUNE")]
		public string ReplaceCUNE { get; set; }

		[JsonProperty("NumberReplace")]
		public string NumberReplace { get; set; }

		[JsonProperty("GenerationDate")]
		public string GenerationDate { get; set; }
	}

	public partial class DocumentDataR
	{
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
	public partial class EmployerDataR
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

	public partial class NoteR
	{
		[JsonProperty("Note")]
		public string NoteNote { get; set; }
	}

	public partial class WorkerDataR
	{
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






	}

}
