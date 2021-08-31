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
    public static class RegistrateCompletedPayroll
    {
        private static readonly TableManager TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager TableManagerGlobalDocPayroll = new TableManager("GlobalDocPayroll");
        private static readonly TableManager TableManagerGlobalDocPayrollHistoric = new TableManager("GlobalDocPayrollHistoric");
        private static readonly TableManager TableManagerGlobalDocPayrollEmployees = new TableManager("GlobalDocPayrollEmployees");
        private static readonly TableManager TableManagerGlobalDocPayrollRegister = new TableManager("GlobalDocPayrollRegister");       


        [FunctionName("RegistrateCompletedPayroll")]
        public static async Task<EventResponse> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
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

                var documentParsed = xmlParser.Fields.ToObject<DocumentParsedNomina>();
                DocumentParsedNomina.SetValues(ref documentParsed);

                GlobalDocPayroll docGlobalPayroll = xmlParser.globalDocPayrolls;
                docGlobalPayroll.Timestamp = DateTime.Now;

                var arrayTasks = new List<Task>();
                arrayTasks.Add(TableManagerGlobalDocPayroll.InsertOrUpdateAsync(docGlobalPayroll));

                var documentTypeId = int.Parse(documentParsed.DocumentTypeId);
                var numeroDocumento = string.IsNullOrEmpty(docGlobalPayroll.NumeroDocumento) ? "0" : docGlobalPayroll.NumeroDocumento;

                //Registra empleado solo para Nomina Individual
                if (documentTypeId == (int)DocumentType.IndividualPayroll)
                {
                    GlobalDocPayrollEmployees globalDocPayrollEmployees = new GlobalDocPayrollEmployees
                    {
                        PartitionKey = "Employee",
                        RowKey = $"{docGlobalPayroll.NIT}|{docGlobalPayroll.TipoDocumento}|{numeroDocumento}",
                        NumeroDocumento = numeroDocumento,
                        TipoDocumento = docGlobalPayroll.TipoDocumento,
                        NitEmpresa = docGlobalPayroll.NIT,
                        PrimerApellido = docGlobalPayroll.PrimerApellido.ToUpper(),
                        PrimerNombre = docGlobalPayroll.PrimerNombre.ToUpper(),
                        Timestamp = DateTime.Now,
                    };
                    arrayTasks.Add(TableManagerGlobalDocPayrollEmployees.InsertOrUpdateAsync(globalDocPayrollEmployees));
                }               

                // N�mina Individual de Ajuste...
                if ((documentTypeId == (int)DocumentType.IndividualPayroll && xmlParser.Novelty)
                    || documentTypeId == (int)DocumentType.IndividualPayrollAdjustments)
                {
                    var trackIdCuneNovOrCunePred = (documentTypeId == (int)DocumentType.IndividualPayroll) ? xmlParser.globalDocPayrolls.CUNENov : xmlParser.globalDocPayrolls.CUNEPred;
                    var trackIdCune = xmlParser.globalDocPayrolls.CUNE;

                    var docGlobalPayrollHistoric = new GlobalDocPayrollHistoric(trackIdCuneNovOrCunePred, trackIdCune);
                    docGlobalPayrollHistoric.DocumentTypeId = documentParsed.DocumentTypeId;
                    if (documentTypeId == (int)DocumentType.IndividualPayrollAdjustments && xmlParser.HasRemoveNode) docGlobalPayrollHistoric.Deleted = true;

                    arrayTasks.Add(TableManagerGlobalDocPayrollHistoric.InsertOrUpdateAsync(docGlobalPayrollHistoric));
                    // se actualiza en la Meta el DocumentReferenceKey con el ID del �ltimo ajuste...
                    var documentMetaAdjustment = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackIdCuneNovOrCunePred, trackIdCuneNovOrCunePred);
                    documentMetaAdjustment.DocumentReferencedKey = trackIdCune;
                    arrayTasks.Add(TableManagerGlobalDocValidatorDocumentMeta.InsertOrUpdateAsync(documentMetaAdjustment));
                }

                //Guarda el registro para validar futuras duplicidades y duplicidad en el mismo mes
                GlobalDocPayrollRegister globalDocPayrollRegister = new GlobalDocPayrollRegister
                {
                    PartitionKey = docGlobalPayroll.RowKey,
                    RowKey = docGlobalPayroll.Numero,
                    FechaPagoFin = docGlobalPayroll.FechaPagoFin,
                    FechaPagoInicio = docGlobalPayroll.FechaPagoInicio,
                    NumeroDocumento = numeroDocumento,
                    Timestamp = DateTime.Now,
                };
                arrayTasks.Add(TableManagerGlobalDocPayrollRegister.InsertOrUpdateAsync(globalDocPayrollRegister));

                // ...
                Task.WhenAll(arrayTasks).Wait();
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
