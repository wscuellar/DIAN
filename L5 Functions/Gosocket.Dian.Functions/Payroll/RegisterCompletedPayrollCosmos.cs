using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Payroll
{


	public static class RegisterCompletedPayrollCosmos
	{

		private static readonly TableManager tableManagerbatchFileResult = new TableManager("GlobalBatchFileResult");

		[FunctionName("RegisterCompletedPayrollCosmos")]
		public static async Task<EventResponse> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
		{
			log.Info("C# HTTP trigger function processed a request.");

			// Get request body
			var data = await req.Content.ReadAsAsync<RequestObject>();

			if (data == null)
				return new EventResponse { Code = "400", Message = "Request body is empty." };

			if (string.IsNullOrEmpty(data.TrackId))
				return new EventResponse { Code = "400", Message = "Please pass a trackId in the request body." };

			var response = new EventResponse
			{
				Code = ((int)EventValidationMessage.Success).ToString(),
				Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success),
			};

			try
			{
				var xmlBytes = await Utils.Utils.GetXmlFromStorageAsync(data.TrackId);
				var xmlParser = new XmlParseNomina(xmlBytes);
				if (!xmlParser.Parser())
					throw new Exception(xmlParser.ParserError);
				var obj = xmlParser.globalDocPayrolls;
				var Insert = new Domain.Cosmos.Payroll_All()
				{
					DocumentKey = obj.CUNE,
					AccountId = obj.CUNE,
					Cune = obj.CUNE,
					PredecesorCune = obj.CUNEPred,
					Prefix = obj.Prefijo,
					Consecutive = obj.Consecutivo.ToString(),
					CompositeNumber = obj.Numero,
					DocumentTypeId = obj.TipoDocumento =="102" ?"NI":"NA",
					//mapear demás campos


				};

				var Cosmos = new Gosocket.Dian.DataContext.CosmosDbManagerPayroll();
				await Cosmos.UpsertDocumentPayroll_All(Insert);





			}
			catch (Exception ex)
			{
				log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
				response.Code = ((int)EventValidationMessage.Error).ToString();
				response.Message = ex.Message;
			}

			return response;


		}

		public class RequestObject
		{
			[JsonProperty(PropertyName = "trackId")]
			public string TrackId { get; set; }
		}
	}
}
