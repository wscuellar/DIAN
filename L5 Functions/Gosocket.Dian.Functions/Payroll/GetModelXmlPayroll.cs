using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Common;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Payroll
{


	public static class GetModelXmlPayroll
	{

		private static readonly TableManager tableManagerbatchFileResult = new TableManager("GlobalBatchFileResult");

		[FunctionName("GetModelXmlPayroll")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
		{
			log.Info("C# HTTP trigger function processed a request.");

			// Get request body
			var data = await req.Content.ReadAsAsync<RequestObject>();


			try
			{

				var xmlBytes = await Utils.Utils.GetXmlFromStorageAsync(data.TrackId);
				var xmlParser = new XmlParseNomina(xmlBytes);
				if (!xmlParser.Parser())
					throw new Exception(xmlParser.ParserError);

				var objNomina = xmlParser.globalDocPayrolls; 

				var resp = new HttpResponseMessage
				{
					Content = new StringContent(JsonConvert.SerializeObject(objNomina),
											System.Text.Encoding.UTF8, "application/json")

				};

				return resp;

			}
			catch (Exception ex)
			{
				log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
				return new HttpResponseMessage();
			}




		}

		public class RequestObject
		{
			[JsonProperty(PropertyName = "trackId")]
			public string TrackId { get; set; }
		}
	}
}
