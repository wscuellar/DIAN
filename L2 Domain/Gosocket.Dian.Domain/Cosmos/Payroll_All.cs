using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Domain.Cosmos
{


	public class Payroll_All
	{
		[JsonProperty("PartitionKey")]
		public string PartitionKey { get; set; }

		[JsonProperty("DocumentKey")]
		public string DocumentKey { get; set; }

		[JsonProperty("AccountId")]
		public string AccountId { get; set; }

		[JsonProperty("Cune")]
		public string Cune { get; set; }

		[JsonProperty("PredecesorCune")]
		public string PredecesorCune { get; set; }

		[RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Solo letras")]
		[JsonProperty("Prefix")]
		public string Prefix { get; set; }

		[JsonProperty("Consecutive")]
		public string Consecutive { get; set; }

		[RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Solo letras y números")]
		[JsonProperty("CompositeNumber")]
		public string CompositeNumber { get; set; }

		[JsonProperty("DocumentTypeId")]
		public string DocumentTypeId { get; set; }

		[JsonProperty("DocumentTypeName")]
		public string DocumentTypeName { get; set; }

		[JsonProperty("SubTypeDocumentId")]
		public string SubTypeDocumentId { get; set; }

		[JsonProperty("SubTypeDocumentName")]
		public string SubTypeDocumentName { get; set; }

		[JsonProperty("DocNumberSender")]
		public string DocNumberSender { get; set; }

		[JsonProperty("CompositeNameSender")]
		public string CompositeNameSender { get; set; }

		[JsonProperty("CodeEmployee")]
		public string CodeEmployee { get; set; }

		[JsonProperty("DocTypeWorker")]
		public long DocTypeWorker { get; set; }

		[JsonProperty("DocNumberWorker")]
		public string DocNumberWorker { get; set; }

		[JsonProperty("NameDocTypeWorker")]
		public string NameDocTypeWorker { get; set; }

		[JsonProperty("FirstNamerWorker")]
		public string FirstNamerWorker { get; set; }

		[JsonProperty("SecondNameWorker")]
		public string SecondNameWorker { get; set; }

		[JsonProperty("LastNameWorker")]
		public string LastNameWorker { get; set; }

		[JsonProperty("SecondLastNameWorker")]
		public string SecondLastNameWorker { get; set; }

		[JsonProperty("CompositeNameWorker")]
		public string CompositeNameWorker { get; set; }

		[JsonProperty("GenerationDate")]
		public string GenerationDate { get; set; }

		[JsonProperty("InitialDate")]
		public string InitialDate { get; set; }

		[JsonProperty("FinalDate")]
		public string FinalDate { get; set; }

		[JsonProperty("Salary")]
		public string Salary { get; set; }

		[JsonProperty("TotalAccrued")]
		public double TotalAccrued { get; set; }

		[JsonProperty("TotalDiscounts")]
		public double TotalDiscounts { get; set; }

		[JsonProperty("PaymentReceipt")]
		public double PaymentReceipt { get; set; }

		[JsonProperty("numeration")]
		public string Numeration { get; set; }

		[JsonProperty("State")]
		public string State { get; set; }

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

		public string From { get; set; }
		public string To { get; set; }
		public long count { get; set; }
		public string EmisionDate { get; set; }
		public string checkBox { get; set; }
		public string action { get; set; }

		[RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
		public string numerationStart { get; set; }

		[RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
		public string numerationEnd { get; set; }

		[JsonProperty("FinalDateStr")]
		public string FinalDateStr { get; set; }

		[JsonProperty("InitialDateStr")]
		public string InitialDateStr { get; set; }
	}
}

