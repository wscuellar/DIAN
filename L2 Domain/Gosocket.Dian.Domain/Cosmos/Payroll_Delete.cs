using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Gosocket.Dian.Domain.Cosmos
{
	public partial class Payroll_Delete
	{
		[JsonProperty("PartitionKey")]
		public string PartitionKey { get; set; }

		[JsonProperty("DocumentKey")]
		public string DocumentKey { get; set; }

		[JsonProperty("AccountId")]
		public Guid AccountId { get; set; }

		[JsonProperty("CUNE")]
		public string Cune { get; set; }

		[JsonProperty("NoteTypeDelete")]
		public NoteTypeDelete NoteTypeDelete { get; set; }

		[JsonProperty("DataDocumentDelete")]
		public DataDocumentDelete DataDocumentDelete { get; set; }

		[JsonProperty("DocumentDataDelete")]
		public DocumentDataDelete DocumentDataDelete { get; set; }

		[JsonProperty("NoteDelete")]
		public NoteDelete[] NoteDelete { get; set; }

		[JsonProperty("EmployerDataDelete")]
		public EmployerDataDelete EmployerDataDelete { get; set; }

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

	public partial class NoteTypeDelete
	{
		[JsonProperty("NoteTypeID")]
		public int NoteTypeID { get; set; }

		[JsonProperty("NameNoteType")]
		public string NameNoteType { get; set; }
	}

	public partial class DataDocumentDelete
	{
		[JsonProperty("ReplaceCUNE")]
		public string ReplaceCUNE { get; set; }

		[JsonProperty("NumberReplace")]
		public string NumberReplace { get; set; }

		[JsonProperty("GenerationDate")]
		public string GenerationDate { get; set; }
	}

	public partial class DocumentDataDelete
	{
		[JsonProperty("Language")]
		public string Language { get; set; }
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
	}

	public partial class EmployerDataDelete
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

	public partial class NoteDelete
	{
		[JsonProperty("Note")]
		public string NoteNote { get; set; }
	}
}
