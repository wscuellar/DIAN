using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Global.Common;
using Gosocket.Dian.Functions.Models;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Documents
{
    public static class RegisterDocumentData
    {
        private static readonly TableManager batchFileResultTableManager = new TableManager("GlobalBatchFileResult");

        //[FunctionName("RegisterDocumentData")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestRegisterDocumentData>();

            if (data == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, new ResponseRegisterDocumentData { Success = false, Message = "Request body is empty" });

            if (string.IsNullOrEmpty(data.TrackId))
                return req.CreateResponse(HttpStatusCode.BadRequest, new ResponseRegisterDocumentData { Success = false, Message = "Please pass a trackId in the request body" });

            if (string.IsNullOrEmpty(data.XmlBase64))
                return req.CreateResponse(HttpStatusCode.BadRequest, new ResponseRegisterDocumentData { Success = false, Message = "Please pass a xmlBase64 in the request body" });

            var response = new ResponseRegisterDocumentData { Success = true, Message = "OK" };

            try
            {
                byte[] xmlBytes = Convert.FromBase64String(data.XmlBase64);

                var documentMetaManager = new DocumentMetaData(xmlBytes);
                await documentMetaManager.InsertDocumentDataAsync(data);

                if (!string.IsNullOrEmpty(data.ZipKey))
                {
                    var batchFileResult = new GlobalBatchFileResult(data.ZipKey, data.TrackId) { FileName = data.FileName, StatusCode = (int)BatchFileStatus.InProcess, StatusDescription = EnumHelper.GetEnumDescription(BatchFileStatus.InProcess), TrackId = data.TrackId };
                    await batchFileResultTableManager.InsertOrUpdateAsync(batchFileResult);
                }
            }
            catch (Exception ex)
            {
                log.Error($"{ex.Message}");
                response.Success = false;
                response.Message = ex.Message;
            }

            return req.CreateResponse(HttpStatusCode.OK, response);

        }
    }
}
