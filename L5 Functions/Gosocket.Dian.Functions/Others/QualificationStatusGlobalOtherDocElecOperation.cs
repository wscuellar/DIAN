using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Others
{
    public static class QualificationStatusGlobalOtherDocElecOperation
    {
        private static readonly TableManager TableManagerGlobalOtherDocElecOperation = new TableManager("GlobalOtherDocElecOperation");
        private static readonly TableManager TableManagerGlobalTestSetOthersDocumentsResult = new TableManager("GlobalTestSetOthersDocumentsResult");//10169");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager TableManagerGlobalOtherDocElecOperationProd = new TableManager("GlobalOtherDocElecOperation", ConfigurationManager.GetValue("GlobalStorageProd"));
        private static readonly OthersDocsElecSoftwareService othersDocsElecSoftwareService = new OthersDocsElecSoftwareService();
        private static readonly OthersElectronicDocumentsService othersElectronicDocumentsService = new OthersElectronicDocumentsService();
        private static readonly SoftwareService softwareService = new SoftwareService();
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly string sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");

        [FunctionName("CorrectQualificationStatus")]
        public static async Task<EventResponse> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return new EventResponse { Code = "400", Message = "Request body is empty." };
            //NIT/S de los participantes separados por "|"
            if (string.IsNullOrEmpty(data.Nits))
                return new EventResponse { Code = "400", Message = "Please pass a nits in the request body." };
            //ModeTest: "1" ejecuta la funcion en modo de consulta, no genera update; "0" ejecuta la funcion actualizando las tablas.
            if (string.IsNullOrEmpty(data.ModeTest))
                data.ModeTest = "0";

            var response = new EventResponse
            {
                Code = ((int)EventValidationMessage.Success).ToString(),
                Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success),
            };
            var arrayTasks = new List<Task>();
            var nitSwIdOperMod = new List<Tuple<string, string, int, string>>();
            Tuple<string, string, int, string> itemTuple = null;
            GlobalOtherDocElecOperation itemGODEO;
            List<string> messages = new List<string>();
            string _StateOld = string.Empty;
            bool QualificationProd = false;
            Domain.Contributor contributor = null;
            List<string> ListNitsProcess = new List<string>();
            try
            {
                IEnumerable<String> nits = data.Nits.Split('|').AsEnumerable();
                IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperation = TableManagerGlobalOtherDocElecOperation.GetRowsContainsInPartitionKeys<GlobalOtherDocElecOperation>(nits);
                var listGlobalOtherDocElecOperationByType = listGlobalOtherDocElecOperation.Where(godeo => (!godeo.Deleted));
                nitSwIdOperMod = listGlobalOtherDocElecOperationByType.Select(godeo => new Tuple<string, string, int, string>(godeo.PartitionKey, godeo.RowKey, godeo.OperationModeId, godeo.State)).ToList();
                if (nitSwIdOperMod.Count() > 0)
                {

                    List<GlobalTestSetOthersDocumentsResult> listGlobalTestSetOthersDocumentsResult = TableManagerGlobalTestSetOthersDocumentsResult.GetRowsContainsInPartitionRowKey<GlobalTestSetOthersDocumentsResult>(nitSwIdOperMod).ToList();
                    //aceptados mayores o iguales al requerido y total aceptados mayores o iguales al total requerido
                    foreach (var item in listGlobalTestSetOthersDocumentsResult.Where(gtsodr => (!gtsodr.Deleted) &&
                        (gtsodr.ElectronicPayrollAjustmentAccepted >= gtsodr.ElectronicPayrollAjustmentAcceptedRequired) &&
                        (gtsodr.OthersDocumentsAccepted >= gtsodr.OthersDocumentsAcceptedRequired) &&
                        (gtsodr.TotalDocumentAccepted >= gtsodr.TotalDocumentAcceptedRequired)))
                    {
                        ListNitsProcess.Add(item.PartitionKey);
                        _StateOld = item.State;
                        if (item.Status != 1)
                        {
                            item.Status = 1;
                            item.StatusDescription = "Aceptado";
                            item.State = "Aceptado";
                            item.Timestamp = DateTime.Now;
                            //Actualiza GlobalTestSetOthersDocumentsResult
                            if (data.ModeTest == "0")
                                arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item));
                            messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item.PartitionKey, item.RowKey, item.RowKey.Split('|')[0], _StateOld, "Aceptado"));
                        }
                        itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item.PartitionKey && nso.Item2 == item.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item.RowKey.Split('|')[0]).FirstOrDefault();
                        if (itemTuple != null)// && itemTuple.Item4 != "Habilitado")
                        {
                            itemGODEO = listGlobalOtherDocElecOperationByType.Where(godeo => godeo.PartitionKey == item.PartitionKey &&
                                godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3).FirstOrDefault();
                            if (itemGODEO != null)
                            {
                                if (itemTuple.Item4 != "Habilitado")
                                {
                                    itemGODEO.State = "Habilitado";
                                    itemGODEO.Timestamp = DateTime.Now;
                                    //Actualiza GlobalOtherDocElecOperation
                                    if (data.ModeTest == "0")
                                        arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                    messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "Habilitado"));
                                    if (data.ModeTest == "0")
                                    {
                                        arrayTasks.Add(Task.Run(() =>
                                        {
                                            //Actualiza othersDocsElecSoftware
                                            var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                            if (_guid.Result == Guid.Empty)
                                                messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Aceptado", item.SoftwareId));
                                            else
                                                messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item.SoftwareId));
                                        }));
                                        arrayTasks.Add(Task.Run(() =>
                                        {
                                            //Actualiza OtherDocElecContributorOperations
                                            var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                            if (_ContributorOperationsId.Result == 0)
                                                messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Habilitado", item.SoftwareId));
                                            else
                                                messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item.SoftwareId));
                                        }));
                                    }
                                    else
                                    {
                                        messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado Aceptado", item.SoftwareId));
                                        messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado Habilitado", item.SoftwareId));
                                    }
                                }
                                contributor = contributorService.GetByCode(itemGODEO.PartitionKey);
                                if (contributor == null)
                                    messages.Add(String.Format("contributor - No se encontro con Code : {0}", itemGODEO.PartitionKey));
                                else
                                {
                                    //si no esta habilitado en produccion, se envia para su habilitacion
                                    if (!othersElectronicDocumentsService.QualifiedContributor(
                                        new Domain.Sql.OtherDocElecContributorOperations()
                                        {
                                            OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                            SoftwareId = new Guid(item.SoftwareId),
                                            OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                            Deleted = false
                                        },
                                        new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },
                                        sqlConnectionStringProd))
                                    {
                                        if (data.ModeTest == "0")
                                        {
                                            arrayTasks.Add(Task.Run(async () =>
                                            {
                                                Domain.Sql.OtherDocElecSoftware software = softwareService.GetOtherDocSoftware(new Guid(item.SoftwareId));
                                                if (software == null)
                                                    messages.Add(String.Format("OtherDocElecSoftware - No se encontro con SoftwareId : {0}", item.SoftwareId));
                                                else
                                                {
                                                    #region migracion SQL
                                                    try
                                                    {
                                                        var requestObject = new
                                                        {
                                                            code = itemGODEO.PartitionKey,
                                                            contributorId = contributor.Id,
                                                            contributorTypeId = itemGODEO.ContributorTypeId,
                                                            softwareId = itemGODEO.RowKey,
                                                            softwareType = itemGODEO.OperationModeId,
                                                            softwareUser = software.SoftwareUser,
                                                            softwarePassword = software.SoftwarePassword,
                                                            pin = software.Pin,
                                                            url = software.Url,
                                                            softwareName = software.Name,
                                                            enabled = othersElectronicDocumentsService.QualifiedContributor(
                                                                new Domain.Sql.OtherDocElecContributorOperations()
                                                                {
                                                                    OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                                                    SoftwareId = new Guid(item.SoftwareId),
                                                                    OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                                                    Deleted = false
                                                                }, new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },

                                                                string.Empty),
                                                            contributorOpertaionModeId = itemGODEO.OperationModeId
                                                        };


                                                        string functionPath = ConfigurationManager.GetValue("SendToActivateOtherDocumentContributorUrl");

                                                        var activation = await ApiHelpers.ExecuteRequestAsync<SendToActivateContributorResponse>(functionPath, requestObject);

                                                        if (activation.Success)
                                                        {
                                                            messages.Add(String.Format("OtherDocElecContributor - ContributorId : {0}, ContributorTypeId : {1}, SoftwareId : {2},  SoftwareType : {3}, Se Activo",
                                                            contributor.Id, itemGODEO.ContributorTypeId, itemGODEO.RowKey, itemGODEO.OperationModeId));

                                                            var guid = Guid.NewGuid().ToString();
                                                            var contributorActivation = new GlobalContributorActivation(contributor.Code, guid)
                                                            {
                                                                Success = true,
                                                                ContributorCode = Convert.ToString(itemGODEO.OtherDocElecContributorId),
                                                                ContributorTypeId = Convert.ToInt32(itemGODEO.ContributorTypeId),
                                                                OperationModeId = Convert.ToInt32(itemGODEO.OperationModeId),
                                                                OperationModeName = "OTHERDOCUMENTS",
                                                                SentToActivateBy = "Function",
                                                                SoftwareId = itemGODEO.RowKey,
                                                                SendDate = DateTime.UtcNow,
                                                                TestSetId = item.Id,
                                                                Request = JsonConvert.SerializeObject(requestObject)
                                                            };
                                                            var contAct = await contributorActivationTableManager.InsertOrUpdateAsync(contributorActivation);
                                                            if (contAct)
                                                                messages.Add(String.Format("GlobalContributorActivation - ContributorCode : {0}, ContributorTypeId : {1}, OperationModeId : {2},  SoftwareId : {3}, Se Activo",
                                                                itemGODEO.OtherDocElecContributorId, itemGODEO.ContributorTypeId, itemGODEO.OperationModeId, itemGODEO.RowKey));
                                                            else
                                                                messages.Add(String.Format("GlobalContributorActivation - ContributorCode : {0}, ContributorTypeId : {1}, OperationModeId : {2},  SoftwareId : {3}, NO Se Activo",
                                                                itemGODEO.OtherDocElecContributorId, itemGODEO.ContributorTypeId, itemGODEO.OperationModeId, itemGODEO.RowKey));
                                                        }
                                                        else
                                                            messages.Add(String.Format("SendToActivateOtherDocumentContributorUrl - ContributorId : {0}, ContributorTypeId : {1}, SoftwareId : {2},  SoftwareType : {3}, Se presento el siguiente error : {4}",
                                                            contributor.Id, itemGODEO.ContributorTypeId, itemGODEO.RowKey, itemGODEO.OperationModeId, activation.Message));
                                                    }

                                                    catch (Exception ex)
                                                    {
                                                        messages.Add(String.Format("OtherDocElecContributor - Error al enviar a activar contributor con id : {0}", contributor.Id));
                                                        log.Error($"Error al enviar a activar OtherDocument contribuyente con Code {itemGODEO.PartitionKey} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);

                                                    }
                                                    #endregion
                                                    //}
                                                }
                                            }));
                                        }
                                        else
                                            messages.Add(String.Format("OtherDocElecContributor - OtherDocElecContributorId : {0} , SoftwareId : {1}, SE ENVIARIA HABILITAR EN PRODUCCION", itemGODEO.OtherDocElecContributorId, item.SoftwareId));
                                    }
                                    else
                                        messages.Add(String.Format("OtherDocElecContributor - PartitionKey : {0}, OtherDocElecContributorId : {1} , SoftwareId : {2}, YA ESTA HABILITADO EN PRODUCCION", item.PartitionKey, itemGODEO.OtherDocElecContributorId, item.SoftwareId));
                                }
                            }
                        }

                    }


                    //aceptados menores al requerido y aceptados faltantes menores o iguales al faltante de total requerido
                    foreach (var item1 in listGlobalTestSetOthersDocumentsResult.Where(gtsodr => (!gtsodr.Deleted) &&
                        ((gtsodr.TotalDocumentAccepted < gtsodr.TotalDocumentAcceptedRequired) || 
                            ((gtsodr.OthersDocumentsAccepted < gtsodr.OthersDocumentsAcceptedRequired) || (gtsodr.ElectronicPayrollAjustmentAccepted < gtsodr.ElectronicPayrollAjustmentAcceptedRequired))) &&
                        (gtsodr.TotalDocumentsRejected <= (gtsodr.TotalDocumentRequired - gtsodr.TotalDocumentAcceptedRequired))
                        ))
                    {
                        ListNitsProcess.Add(item1.PartitionKey);
                        QualificationProd = false;
                        if (item1.Status != 0)
                        {
                            _StateOld = item1.State;
                            item1.Status = 0;
                            item1.StatusDescription = "En proceso";
                            item1.State = "En proceso";
                            item1.Timestamp = DateTime.Now;
                            //Actualiza GlobalTestSetOthersDocumentsResult
                            if (data.ModeTest == "0")
                                arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item1));
                            messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item1.PartitionKey, item1.RowKey, item1.RowKey.Split('|')[0], _StateOld, "En proceso"));
                        }
                        itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item1.PartitionKey && nso.Item2 == item1.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item1.RowKey.Split('|')[0]).FirstOrDefault();
                        if (itemTuple != null && itemTuple.Item4 != "En pruebas")
                        {
                            itemGODEO = listGlobalOtherDocElecOperationByType.Where(godeo => godeo.PartitionKey == item1.PartitionKey &&
                                godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3).FirstOrDefault();
                            if (itemGODEO != null)
                            {
                                if (itemGODEO.State == "Habilitado")
                                {
                                    contributor = contributorService.GetByCode(itemGODEO.PartitionKey);
                                    if (contributor == null)
                                    {
                                        //para que no cambie estado
                                        QualificationProd = true;
                                        messages.Add(String.Format("contributor - No se encontro con Code : {0}", itemGODEO.PartitionKey));
                                    }
                                    else
                                    {
                                        //Valida si el participante esta habilitado en produccion
                                        if (othersElectronicDocumentsService.QualifiedContributor(
                                        new Domain.Sql.OtherDocElecContributorOperations()
                                        {
                                            OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                            SoftwareId = new Guid(item1.SoftwareId),
                                            OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                            Deleted = false
                                        }, new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },

                                        sqlConnectionStringProd))
                                        {
                                            QualificationProd = true;
                                            messages.Add(String.Format("OtherDocElecContributorOperations - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, OtherDocElecContributorId : {3}, SoftwareId : {4}, No pasa set de pruebas, Habilitado en producción", item1.PartitionKey, item1.RowKey, item1.OperationModeId, itemGODEO.OtherDocElecContributorId, item1.SoftwareId));
                                        }
                                    }
                                }
                                if (!QualificationProd)
                                {
                                    itemGODEO.State = "En pruebas";
                                    itemGODEO.Timestamp = DateTime.Now;
                                    //Actualiza GlobalOtherDocElecOperation
                                    if (data.ModeTest == "0")
                                        arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                    messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "En pruebas"));
                                    if (data.ModeTest == "0")
                                    {
                                        arrayTasks.Add(Task.Run(() =>
                                        {
                                            //Actualiza othersDocsElecSoftware
                                            var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item1.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.InProcess, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                            if (_guid.Result == Guid.Empty)
                                                messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado En proceso", item1.SoftwareId));
                                            else
                                                messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item1.SoftwareId));
                                        }));
                                        arrayTasks.Add(Task.Run(() =>
                                        {
                                            //Actualiza OtherDocElecContributorOperations
                                            var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item1.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Test, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                            if (_ContributorOperationsId.Result == 0)
                                                messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado En pruebas", item1.SoftwareId));
                                            else
                                                messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item1.SoftwareId));
                                        }));
                                    }
                                    else
                                    {
                                        messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado En proceso", item1.SoftwareId));
                                        messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado En pruebas", item1.SoftwareId));
                                    }
                                }
                            }
                        }
                    }

                    //aceptados menores al requerido y aceptados faltantes mayores al faltante de total requerido
                    foreach (var item2 in listGlobalTestSetOthersDocumentsResult.Where(gtsodr => (!gtsodr.Deleted) &&
                        (gtsodr.TotalDocumentAccepted < gtsodr.TotalDocumentAcceptedRequired) &&
                        (gtsodr.TotalDocumentsRejected > (gtsodr.TotalDocumentRequired - gtsodr.TotalDocumentAcceptedRequired))))
                    {
                        ListNitsProcess.Add(item2.PartitionKey);
                        QualificationProd = false;
                        if (item2.Status != 2)
                        {
                            _StateOld = item2.State;
                            item2.Status = 2;
                            item2.StatusDescription = "Rechazado";
                            item2.State = "Rechazado";
                            item2.Timestamp = DateTime.Now;
                            //Actualiza GlobalTestSetOthersDocumentsResult
                            if (data.ModeTest == "0")
                                arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item2));
                            messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item2.PartitionKey, item2.RowKey, item2.RowKey.Split('|')[0], _StateOld, "Rechazado"));
                        }
                        itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item2.PartitionKey && nso.Item2 == item2.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item2.RowKey.Split('|')[0]).FirstOrDefault();
                        if (itemTuple != null && itemTuple.Item4 != "En pruebas")
                        {
                            itemGODEO = listGlobalOtherDocElecOperationByType.Where(godeo => godeo.PartitionKey == item2.PartitionKey &&
                                godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3).FirstOrDefault();
                            if (itemGODEO != null)
                            {
                                if (itemGODEO.State == "Habilitado")
                                {
                                    contributor = contributorService.GetByCode(itemGODEO.PartitionKey);
                                    if (contributor == null)
                                    {
                                        //para que no cambie estado
                                        QualificationProd = true;
                                        messages.Add(String.Format("contributor - No se encontro con Code : {0}", itemGODEO.PartitionKey));
                                    }
                                    else
                                    {
                                        //Valida si el participante esta habilitado en produccion
                                        if (othersElectronicDocumentsService.QualifiedContributor(
                                        new Domain.Sql.OtherDocElecContributorOperations()
                                        {
                                            OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                            SoftwareId = new Guid(item2.SoftwareId),
                                            OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                            Deleted = false
                                        }, new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },
                                        sqlConnectionStringProd))
                                        {
                                            QualificationProd = true;
                                            messages.Add(String.Format("OtherDocElecContributorOperations - OtherDocElecContributorId : {0}, SoftwareId : {1}, No pasa set de pruebas, Habilitado en producción", itemGODEO.OtherDocElecContributorId, item2.SoftwareId));
                                        }
                                    }
                                }
                                if (!QualificationProd)
                                {
                                    itemGODEO.State = "En pruebas";
                                    itemGODEO.Timestamp = DateTime.Now;
                                    //Actualiza GlobalOtherDocElecOperation
                                    if (data.ModeTest == "0")
                                        arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                    messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "En pruebas"));
                                    if (data.ModeTest == "0")
                                    {
                                        arrayTasks.Add(Task.Run(() =>
                                        {
                                            //Actualiza othersDocsElecSoftware
                                            var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item2.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Rejected, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                            if (_guid.Result == Guid.Empty)
                                                messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Rechazado", item2.SoftwareId));
                                            else
                                                messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item2.SoftwareId));
                                        }));
                                        arrayTasks.Add(Task.Run(() =>
                                        {
                                            //Actualiza OtherDocElecContributorOperations
                                            var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item2.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Cancelado, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                            if (_ContributorOperationsId.Result == 0)
                                                messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Cancelado (Rechazado)", item2.SoftwareId));
                                            else
                                                messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item2.SoftwareId));
                                        }));
                                    }
                                    else
                                    {
                                        messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado Rechazado", item2.SoftwareId));
                                        messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado Cancelado (Rechazado)", item2.SoftwareId));
                                    }
                                }
                            }
                        }
                    }
                }

                    //trae los nits no procesados
                    IEnumerable<string> listNitsNoProcess = (from item3 in nits
                                        join item4 in ListNitsProcess on item3 equals item4 into ResultProcess
                                        from item5 in ResultProcess.DefaultIfEmpty()
                                        where item5 is null
                                        select item3).AsEnumerable();
                    if (listNitsNoProcess.Count() > 0)
                    {
                        //consulta los registros en produccion por el nit, ya estan habilitados
                        IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperationProd = TableManagerGlobalOtherDocElecOperationProd.GetRowsContainsInPartitionKeys<GlobalOtherDocElecOperation>(listNitsNoProcess);
                        var listGlobalOtherDocElecOperationProdNoDeleted = listGlobalOtherDocElecOperationProd.Where(godeo => (!godeo.Deleted));
                        //Arma una lista de filtros por PartitionKey, RowKey, OperationModeId
                        List<Dictionary<string, string>> nitSwIdOperModProd = new List<Dictionary<string, string>>();
                        listGlobalOtherDocElecOperationProdNoDeleted.ToList().ForEach(f => nitSwIdOperModProd.Add( new Dictionary<string, string>() { { "PartitionKey", string.Format("'{0}'",f.PartitionKey) }, 
                            { "RowKey", string.Format("'{0}'",f.RowKey) }, {  "OperationModeId", string.Format("{0}",f.OperationModeId) } }));
                        if (nitSwIdOperModProd.Count() > 0)
                        {
                            //consulta los registros en habilitacion por PartitionKey, RowKey, OperationModeId
                            var listGlobalOtherDocElecOperationHab = TableManagerGlobalOtherDocElecOperation.GetRowsContainsInAnyFilter<GlobalOtherDocElecOperation>(nitSwIdOperModProd);
                            nitSwIdOperMod = null;
                            nitSwIdOperMod = listGlobalOtherDocElecOperationHab.Select(godeo => new Tuple<string, string, int, string>(godeo.PartitionKey, godeo.RowKey, godeo.OperationModeId, godeo.State)).ToList();
                            if (nitSwIdOperMod.Count() > 0)
                            {
                                var listGlobalTestSetOthersDocumentsResultHab = TableManagerGlobalTestSetOthersDocumentsResult.GetRowsContainsInPartitionRowKey<GlobalTestSetOthersDocumentsResult>(nitSwIdOperMod).ToList();
                                //alguno de los los documentos aceptados mayores o iguales al requerido y total aceptados y rechazados mayores o iguales al total requerido
                                foreach (var item6 in listGlobalTestSetOthersDocumentsResultHab//.Where(gtsodr => 
                                    //((gtsodr.ElectronicPayrollAjustmentAccepted >= gtsodr.ElectronicPayrollAjustmentAcceptedRequired) ||
                                    //(gtsodr.OthersDocumentsAccepted >= gtsodr.OthersDocumentsAcceptedRequired)) &&
                                    //(gtsodr.TotalDocumentAccepted >= gtsodr.TotalDocumentAcceptedRequired))
                                    )
                                {
                                    _StateOld = item6.State;
                                    
                                    item6.Status = 1;
                                    item6.StatusDescription = "Aceptado";
                                    item6.State = "Aceptado";
                                    item6.Timestamp = DateTime.Now;
                                    item6.Deleted = false;
                                    //Actualiza GlobalTestSetOthersDocumentsResult
                                    if (data.ModeTest == "0")
                                        arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item6));
                                    messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item6.PartitionKey, item6.RowKey, item6.RowKey.Split('|')[0], _StateOld, "Aceptado"));
                                    itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item6.PartitionKey && nso.Item2 == item6.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item6.RowKey.Split('|')[0]).FirstOrDefault();
                                    if (itemTuple != null)
                                    {
                                        itemGODEO = listGlobalOtherDocElecOperationHab.Where(godeo => godeo.PartitionKey == item6.PartitionKey &&
                                            godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3).FirstOrDefault();
                                        if (itemGODEO != null)
                                        {
                                            itemGODEO.State = "Habilitado";
                                            itemGODEO.Timestamp = DateTime.Now;
                                            itemGODEO.Deleted = false;
                                            //Actualiza GlobalOtherDocElecOperation
                                            if (data.ModeTest == "0")
                                                arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                            messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "Habilitado"));
                                            if (data.ModeTest == "0")
                                            {
                                                arrayTasks.Add(Task.Run(() =>
                                                {
                                                    //Actualiza othersDocsElecSoftware
                                                    var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item6.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                                    if (_guid.Result == Guid.Empty)
                                                        messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Aceptado", item6.SoftwareId));
                                                    else
                                                        messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item6.SoftwareId));
                                                }));
                                                arrayTasks.Add(Task.Run(() =>
                                                {
                                                    //Actualiza OtherDocElecContributorOperations
                                                    var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item6.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                                    if (_ContributorOperationsId.Result == 0)
                                                        messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Habilitado", item6.SoftwareId));
                                                    else
                                                        messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item6.SoftwareId));
                                                }));
                                            }
                                            else
                                            {
                                                messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado Aceptado", item6.SoftwareId));
                                                messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado Habilitado", item6.SoftwareId));
                                            }

                                        contributor = contributorService.GetByCode(itemGODEO.PartitionKey);
                                        if (contributor == null)
                                            messages.Add(String.Format("contributor - No se encontro con Code : {0}", itemGODEO.PartitionKey));
                                        else
                                        {
                                            //si no esta habilitado en produccion, se envia para su habilitacion
                                            if (!othersElectronicDocumentsService.QualifiedContributor(
                                                new Domain.Sql.OtherDocElecContributorOperations()
                                                {
                                                    
                                                    OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                                    SoftwareId = new Guid(item6.SoftwareId),
                                                    OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                                    Deleted = false, 
                                                },
                                                new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey  },
                                                sqlConnectionStringProd ))
                                            {
                                                if (data.ModeTest == "0")
                                                {
                                                    arrayTasks.Add(Task.Run(async () =>
                                                    {
                                                        Domain.Sql.OtherDocElecSoftware software = softwareService.GetOtherDocSoftware(new Guid(item6.SoftwareId));
                                                        if (software == null)
                                                            messages.Add(String.Format("OtherDocElecSoftware - No se encontro con SoftwareId : {0}", item6.SoftwareId));
                                                        else
                                                        {
                                                            #region migracion SQL
                                                            try
                                                            {
                                                                var requestObject = new
                                                                {
                                                                    code = itemGODEO.PartitionKey,
                                                                    contributorId = contributor.Id,
                                                                    contributorTypeId = itemGODEO.ContributorTypeId,
                                                                    softwareId = itemGODEO.RowKey,
                                                                    softwareType = itemGODEO.OperationModeId,
                                                                    softwareUser = software.SoftwareUser,
                                                                    softwarePassword = software.SoftwarePassword,
                                                                    pin = software.Pin,
                                                                    url = software.Url,
                                                                    softwareName = software.Name,
                                                                    enabled = othersElectronicDocumentsService.QualifiedContributor(
                                                                        new Domain.Sql.OtherDocElecContributorOperations()
                                                                        {
                                                                            OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                                                            SoftwareId = new Guid(item6.SoftwareId),
                                                                            OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                                                            Deleted = false
                                                                        }, new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },

                                                                        string.Empty),
                                                                    contributorOpertaionModeId = itemGODEO.OperationModeId
                                                                };


                                                                string functionPath = ConfigurationManager.GetValue("SendToActivateOtherDocumentContributorUrl");

                                                                var activation = await ApiHelpers.ExecuteRequestAsync<SendToActivateContributorResponse>(functionPath, requestObject);

                                                                if (activation.Success)
                                                                {
                                                                    messages.Add(String.Format("OtherDocElecContributor - ContributorId : {0}, ContributorTypeId : {1}, SoftwareId : {2},  SoftwareType : {3}, Se Activo",
                                                                    contributor.Id, itemGODEO.ContributorTypeId, itemGODEO.RowKey, itemGODEO.OperationModeId));

                                                                    var guid = Guid.NewGuid().ToString();
                                                                    var contributorActivation = new GlobalContributorActivation(contributor.Code, guid)
                                                                    {
                                                                        Success = true,
                                                                        ContributorCode = Convert.ToString(itemGODEO.OtherDocElecContributorId),
                                                                        ContributorTypeId = Convert.ToInt32(itemGODEO.ContributorTypeId),
                                                                        OperationModeId = Convert.ToInt32(itemGODEO.OperationModeId),
                                                                        OperationModeName = "OTHERDOCUMENTS",
                                                                        SentToActivateBy = "Function",
                                                                        SoftwareId = itemGODEO.RowKey,
                                                                        SendDate = DateTime.UtcNow,
                                                                        TestSetId = item6.Id,
                                                                        Request = JsonConvert.SerializeObject(requestObject)
                                                                    };
                                                                    var contAct = await contributorActivationTableManager.InsertOrUpdateAsync(contributorActivation);
                                                                    if (contAct)
                                                                        messages.Add(String.Format("GlobalContributorActivation - ContributorCode : {0}, ContributorTypeId : {1}, OperationModeId : {2},  SoftwareId : {3}, Se Activo",
                                                                        itemGODEO.OtherDocElecContributorId, itemGODEO.ContributorTypeId, itemGODEO.OperationModeId, itemGODEO.RowKey));
                                                                    else
                                                                        messages.Add(String.Format("GlobalContributorActivation - ContributorCode : {0}, ContributorTypeId : {1}, OperationModeId : {2},  SoftwareId : {3}, NO Se Activo",
                                                                        itemGODEO.OtherDocElecContributorId, itemGODEO.ContributorTypeId, itemGODEO.OperationModeId, itemGODEO.RowKey));
                                                                }
                                                                else
                                                                    messages.Add(String.Format("SendToActivateOtherDocumentContributorUrl - ContributorId : {0}, ContributorTypeId : {1}, SoftwareId : {2},  SoftwareType : {3}, Se presento el siguiente error : {4}",
                                                                    contributor.Id, itemGODEO.ContributorTypeId, itemGODEO.RowKey, itemGODEO.OperationModeId, activation.Message));
                                                            }

                                                            catch (Exception ex)
                                                            {
                                                                messages.Add(String.Format("OtherDocElecContributor - Error al enviar a activar contributor con id : {0}", contributor.Id));
                                                                log.Error($"Error al enviar a activar OtherDocument contribuyente con Code {itemGODEO.PartitionKey} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);

                                                            }
                                                            #endregion
                                                            //}
                                                        }
                                                    }));
                                                }
                                                else
                                                    messages.Add(String.Format("OtherDocElecContributor - OtherDocElecContributorId : {0} , SoftwareId : {1}, SE ENVIARIA HABILITAR EN PRODUCCION", itemGODEO.OtherDocElecContributorId, item6.SoftwareId));
                                            }
                                            else
                                                messages.Add(String.Format("OtherDocElecContributor - OtherDocElecContributorId : {0} , SoftwareId : {1}, YA ESTA HABILITADO EN PRODUCCION", itemGODEO.OtherDocElecContributorId, item6.SoftwareId));
                                        }

                                    }
                                    }
                                }

                                //si total de aceptados es menor al total de aceptados requeridos
                                //foreach (var item7 in listGlobalTestSetOthersDocumentsResultHab//.Where(gtsodr => (gtsodr.TotalDocumentAccepted < gtsodr.TotalDocumentAcceptedRequired))
                                //    )
                                //{
                                //    _StateOld = item7.Deleted ? "Eliminado": "No Eliminado";
                                //    item7.Status = 1;
                                //    item7.StatusDescription = "Aceptado";
                                //    item7.State = "Aceptado";
                                //    item7.Timestamp = DateTime.Now;
                                //    item7.Deleted = false;
                                //    //Actualiza GlobalTestSetOthersDocumentsResult
                                //    if (data.ModeTest == "0")
                                //        arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item7));
                                //    messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, DeletedOld : {3}, DeletedNew : {4}", item7.PartitionKey, item7.RowKey, item7.RowKey.Split('|')[0], _StateOld, "No Eliminado"));
                                //    itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item7.PartitionKey && nso.Item2 == item7.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item7.RowKey.Split('|')[0]).FirstOrDefault();
                                //    if (itemTuple != null)
                                //    {
                                //        itemGODEO = listGlobalOtherDocElecOperationHab.Where(godeo => godeo.PartitionKey == item7.PartitionKey &&
                                //            godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3).FirstOrDefault();
                                //        if (itemGODEO != null)
                                //        {
                                //            _StateOld = itemGODEO.Deleted ? "Eliminado" : "No Eliminado";
                                //            itemGODEO.State = "Habilitado";
                                //            itemGODEO.Timestamp = DateTime.Now;
                                //            itemGODEO.Deleted = false;
                                //            //Actualiza GlobalOtherDocElecOperation
                                //            if (data.ModeTest == "0")
                                //                arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                //            messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, DeletedOld : {3}, DeletedNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, _StateOld, "No Eliminado"));
                                //            if (data.ModeTest == "0")
                                //            {
                                //                arrayTasks.Add(Task.Run(() =>
                                //                {
                                //                    //Actualiza othersDocsElecSoftware
                                //                    var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item7.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.Accepted);
                                //                    if (_guid.Result == Guid.Empty)
                                //                        messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Aceptado", item7.SoftwareId));
                                //                    else
                                //                        messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item7.SoftwareId));
                                //                }));
                                //                arrayTasks.Add(Task.Run(() =>
                                //                {
                                //                    //Actualiza OtherDocElecContributorOperations
                                //                    var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { 
                                //                        OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item7.SoftwareId), 
                                //                        OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado, Deleted = false }, Domain.Common.OtherDocElecState.Habilitado);
                                //                    if (_ContributorOperationsId.Result == 0)
                                //                        messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Habilitado", item7.SoftwareId));
                                //                    else
                                //                        messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item7.SoftwareId));
                                //                }));
                                //            }
                                //            else
                                //            {
                                //                messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado En Proceso", item7.SoftwareId));
                                //                messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado En Pruebas", item7.SoftwareId));
                                //            }

                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                                        

                Task.WhenAll(arrayTasks).Wait();
                
                if (messages.Count > 0)
                    response.Message = string.Format("Se actualizo los siguientes registros: {0} {1}", "\n\r", string.Join("\n\r", messages));

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
            [JsonProperty(PropertyName = "nits")]
            public string Nits { get; set; }

            [JsonProperty(PropertyName = "modeTest")]
            public string ModeTest { get; set; }
        }

        class SendToActivateContributorResponse
        {
            [JsonProperty(PropertyName = "success")]
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }

            [JsonProperty(PropertyName = "detail")]
            public string Detail { get; set; }

            [JsonProperty(PropertyName = "trace")]
            public string Trace { get; set; }

            [JsonProperty(PropertyName = "testSetId")]
            public string TestSetId { get; set; }
        }

    }
}
