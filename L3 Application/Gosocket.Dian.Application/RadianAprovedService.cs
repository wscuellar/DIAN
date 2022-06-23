using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Gosocket.Dian.Services.Utils.Helpers;

namespace Gosocket.Dian.Application
{
    public class RadianAprovedService : IRadianApprovedService
    {
        private readonly IRadianContributorRepository _radianContributorRepository;
        private readonly IRadianTestSetService _radianTestSetService;
        private readonly IRadianContributorService _radianContributorService;
        private readonly IRadianContributorFileTypeService _radianContributorFileTypeService;
        private readonly IRadianContributorOperationRepository _radianContributorOperationRepository;
        private readonly IRadianContributorFileRepository _radianContributorFileRepository;
        private readonly IRadianContributorFileHistoryRepository _radianContributorFileHistoryRepository;
        private readonly IContributorOperationsService _contributorOperationsService;
        private readonly IRadianTestSetResultService _radianTestSetResultService;
        private readonly IRadianCallSoftwareService _radianCallSoftwareService;
        private readonly IGlobalRadianOperationService _globalRadianOperationService;
        private readonly IGlobalAuthorizationService _globalAuthorizationService;

        public RadianAprovedService(IRadianContributorRepository radianContributorRepository,
                                    IRadianTestSetService radianTestSetService,
                                    IRadianContributorService radianContributorService,
                                    IRadianContributorFileTypeService radianContributorFileTypeService,
                                    IRadianContributorOperationRepository radianContributorOperationRepository,
                                    IRadianContributorFileRepository radianContributorFileRepository,
                                    IRadianContributorFileHistoryRepository radianContributorFileHistoryRepository,
                                    IContributorOperationsService contributorOperationsService,
                                    IRadianTestSetResultService radianTestSetResultService,
                                    IRadianCallSoftwareService radianCallSoftwareService,
                                    IGlobalRadianOperationService globalRadianOperationService,
                                    IGlobalAuthorizationService globalAuthorizationService)
        {
            _radianContributorRepository = radianContributorRepository;
            _radianTestSetService = radianTestSetService;
            _radianContributorService = radianContributorService;
            _radianContributorFileTypeService = radianContributorFileTypeService;
            _radianContributorOperationRepository = radianContributorOperationRepository;
            _radianContributorFileRepository = radianContributorFileRepository;
            _radianContributorFileHistoryRepository = radianContributorFileHistoryRepository;
            _contributorOperationsService = contributorOperationsService;
            _radianTestSetResultService = radianTestSetResultService;
            _radianCallSoftwareService = radianCallSoftwareService;
            _globalRadianOperationService = globalRadianOperationService;
            _globalAuthorizationService = globalAuthorizationService;
        }


        public List<RadianContributor> ListContributorByType(int radianContributorTypeId)
        {
            return _radianContributorRepository.List(t => t.RadianContributorTypeId == radianContributorTypeId).Results;
        }


        public RadianContributor GetRadianContributor(int radianContributorId)
        {
            RadianContributor radianContributor = _radianContributorRepository
                .Get(rc => rc.Id == radianContributorId);

            return radianContributor;
        }

        public List<RadianContributorFile> ListContributorFiles(int radianContributorId)
        {
            RadianContributor radianContributor = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId);

            return radianContributor.RadianContributorFile.ToList();
        }

        public RadianAdmin ContributorSummary(int contributorId, int radianContributorType)
        {
            RadianAdmin result = _radianContributorService.ContributorSummary(contributorId, radianContributorType);
            return result;
        }

        public Software SoftwareByContributor(int contributorId)
        {
            List<ContributorOperations> contributorOperations = _contributorOperationsService
                .GetContributorOperations(contributorId);

            if (contributorOperations == null)
                return new Software();

            return contributorOperations.FirstOrDefault(t => !t.Deleted && t.OperationModeId == (int)Domain.Common.OperationMode.Own && t.Software != null && t.Software.Status)?.Software ?? new Software();
        }

        public List<RadianContributorFileType> ContributorFileTypeList(int typeId)
        {
            List<RadianContributorFileType> contributorTypeList = _radianContributorFileTypeService.FileTypeList()
                .Where(ft => ft.RadianContributorTypeId == typeId && !ft.Deleted).ToList();

            return contributorTypeList;
        }


        public ResponseMessage OperationDelete(RadianContributorOperation operationToDelete)
        {
            RadianContributor participant = _radianContributorRepository.Get(t => t.Id == operationToDelete.RadianContributorId);
            _globalRadianOperationService.Delete(participant.Contributor.Code, operationToDelete.SoftwareId.ToString());
            return _radianContributorOperationRepository.Delete(operationToDelete.Id);
        }

        public ResponseMessage OperationDelete(int radianContributorOperationId)
        {
            RadianContributorOperation operationToDelete = _radianContributorOperationRepository.Get(t => t.Id == radianContributorOperationId);
            if (operationToDelete.SoftwareType == (int)RadianOperationModeTestSet.OwnSoftware)
            {
                RadianSoftware software = _radianCallSoftwareService.Get(operationToDelete.SoftwareId);
                if (software != null && software.RadianSoftwareStatusId == (int)RadianSoftwareStatus.Accepted)
                    return new ResponseMessage() { Message = "El software encuentra en estado aceptado." };

                _radianCallSoftwareService.DeleteSoftware(operationToDelete.SoftwareId);
            }

            RadianContributor participant = _radianContributorRepository.Get(t => t.Id == operationToDelete.RadianContributorId);
            _globalRadianOperationService.Delete(participant.Contributor.Code, operationToDelete.SoftwareId.ToString());

            //-----------------marca como elimnado el set de prueba.
            RadianContributor radianContributor = _radianContributorRepository.Get(t => t.Id == operationToDelete.RadianContributor.Id);
            string key = participant.RadianContributorTypeId + "|" + operationToDelete.SoftwareId.ToString();
            RadianTestSetResult oper = _radianTestSetResultService.GetTestSetResult(radianContributor.Contributor.Code, key);
            oper.Deleted = true;
            _radianTestSetResultService.InsertTestSetResult(oper);
            //------------------

            return _radianContributorOperationRepository.Delete(operationToDelete.Id);
        }

        public ResponseMessage UploadFile(Stream fileStream, string code, RadianContributorFile radianContributorFile)
        {
            string fileName = StringTools.MakeValidFileName(radianContributorFile.FileName);
            //var fileManager = new FileManager(ConfigurationManager.GetValue("GlobalStorage"));
            var fileManager = new FileManager();
            
            bool result = fileManager.Upload("radiancontributor-files", code.ToLower() + "/" + fileName, fileStream);
            string idFile = string.Empty;

            if (result)
            {
                idFile = _radianContributorFileRepository.AddOrUpdate(radianContributorFile);
                return new ResponseMessage($"{idFile}", "Guardado");
            }

            return new ResponseMessage($"{string.Empty}", "Nulo");
        }


        public ResponseMessage AddFileHistory(RadianContributorFileHistory radianContributorFileHistory)
        {
            radianContributorFileHistory.Timestamp = DateTime.Now;
            radianContributorFileHistory.Id = Guid.NewGuid();
            Guid idHistoryRegister = _radianContributorFileHistoryRepository.AddRegisterHistory(radianContributorFileHistory);
            if (idHistoryRegister != Guid.Empty)
                return new ResponseMessage($"Información registrada id: {idHistoryRegister}", "Guardado");

            return new ResponseMessage($"El registro no pudo ser guardado", "Nulo");
        }

        public ResponseMessage UpdateRadianContributorStep(int radianContributorId, int radianContributorStep)
        {
            bool updated = _radianContributorService.ChangeContributorStep(radianContributorId, radianContributorStep);

            if (updated)
            {
                return new ResponseMessage($"Paso actualizado", "Actualizado");
            }

            return new ResponseMessage($"El registro no pudo ser actualizado", "Nulo");
        }

        public int RadianContributorId(int contributorId, int contributorTypeId, string state)
        {
            return _radianContributorRepository.Get(c => c.ContributorId == contributorId && c.RadianContributorTypeId == contributorTypeId && c.RadianState == state).Id;
        }


        public RadianTestSet GetTestSet(string softwareType)
        {
            return _radianTestSetService.GetTestSet(softwareType, softwareType);
        }

        /// <summary>
        /// Construye la operacion asignando un software a un participante de radian
        /// </summary>
        /// <param name="radianContributorOperation">Operacion que se aplica</param>
        /// <param name="software">Software que se asocia</param>
        /// <param name="testSet">Set de pruebas que se asigna</param>
        /// <param name="isInsert">Si el software es para insertasr</param>
        /// <param name="validateOperation">True = si valida que exita la operacion.</param>
        /// <returns>ResponseMessage</returns>
        public async Task<ResponseMessage> AddRadianContributorOperation(RadianContributorOperation radianContributorOperation, RadianSoftware software, RadianTestSet testSet, bool isInsert, bool validateOperation)
        {
            if (testSet == null)
                return new ResponseMessage(TextResources.ModeWithoutTestSet, TextResources.alertType, 500);

            RadianContributor radianContributor = _radianContributorRepository.Get(t => t.Id == radianContributorOperation.RadianContributorId);

            if (validateOperation)
            {
                bool otherActiveProcess = _radianContributorRepository.GetParticipantWithActiveProcess(radianContributor.ContributorId);
                if (otherActiveProcess)
                    return new ResponseMessage(TextResources.OperationFailOtherInProcess, TextResources.alertType, 500);
            }

            RadianContributorOperation existingOperation = _radianContributorOperationRepository.Get(t => t.RadianContributorId == radianContributorOperation.RadianContributorId && t.SoftwareId == radianContributorOperation.SoftwareId && !t.Deleted);
            if (existingOperation != null)
                return new ResponseMessage(TextResources.ExistingSoftware, TextResources.alertType, 500);

            if (isInsert)
            {
                software.Url = ConfigurationManager.GetValue("WebServiceUrl");
                RadianSoftware soft = _radianCallSoftwareService.CreateSoftware(software);
                radianContributorOperation.SoftwareId = soft.Id;
                radianContributorOperation.Software = soft;
            }
            
            if (radianContributorOperation.OperationStatusId != (int)RadianState.Test)
                radianContributorOperation.OperationStatusId = (int)(radianContributor.RadianState == RadianState.Habilitado.GetDescription() ? RadianState.Test : RadianState.Registrado);

            int operationId = _radianContributorOperationRepository.Add(radianContributorOperation);

            if ((!isInsert) && (radianContributorOperation.Software == null || radianContributorOperation.Software.Id == Guid.Empty))
            {
                if (software.Id == Guid.Empty)
                {
                    software = _radianCallSoftwareService.Get(radianContributorOperation.SoftwareId);
                    radianContributorOperation.Software = software;
                }
                else
                    radianContributorOperation.Software = software;
            }

            existingOperation = _radianContributorOperationRepository.Get(t => t.Id == operationId);

            ApplyTestSet(radianContributorOperation, testSet, radianContributor, existingOperation);

            if (radianContributor.RadianOperationModeId == (int)Gosocket.Dian.Domain.Common.RadianOperationMode.Indirect)
            {
                var response = await SyncToProductionAsync(radianContributorOperation, radianContributor);
                if (response != null)
                {
                    if (response.Success)
                        return new ResponseMessage(string.Format("{0}{1}{2}",response.Message, Environment.NewLine, TextResources.SuccessSoftware), TextResources.alertType);
                    else
                        return new ResponseMessage(response.Message, TextResources.alertType, 500);
                }
                else
                    return new ResponseMessage("Error al enviar a activar contribuyente a producción.", TextResources.alertType, 500);
            }
            
            return new ResponseMessage(TextResources.SuccessSoftware, TextResources.alertType);
        }

        private void ApplyTestSet(RadianContributorOperation radianContributorOperation, RadianTestSet testSet, RadianContributor radianContributor, RadianContributorOperation existingOperation)
        {
            Contributor contributor = radianContributor.Contributor;
            GlobalRadianOperations operation = _globalRadianOperationService.GetOperation(contributor.Code, existingOperation.SoftwareId);
            if (operation == null)
                operation = new GlobalRadianOperations(contributor.Code, existingOperation.SoftwareId.ToString());

            operation.RadianState = radianContributor.RadianState == RadianState.Habilitado.GetDescription() ? RadianState.Test.GetDescription() : RadianState.Registrado.GetDescription();
            operation.SoftwareType = existingOperation.SoftwareType;
            operation.RadianContributorTypeId = radianContributor.RadianContributorTypeId;
            operation.Deleted = false;
            operation.IndirectElectronicInvoicer = (radianContributor.RadianOperationModeId == (int)Gosocket.Dian.Domain.Common.RadianOperationMode.Indirect);
            if (operation.IndirectElectronicInvoicer)
                operation.RadianState = RadianState.Habilitado.GetDescription();

            if (_globalRadianOperationService.Insert(operation, existingOperation.Software))
            {
                string key = radianContributor.RadianContributorTypeId.ToString() + "|" + radianContributorOperation.SoftwareId;
                string sType = radianContributor.RadianOperationModeId == 1 ? "1" : operation.SoftwareType.ToString();
                
                RadianTestSetResult setResult = new RadianTestSetResult(contributor.Code, key)
                {
                    Id = Guid.NewGuid().ToString(),
                    ContributorId = radianContributor.Id,
                    State = TestSetStatus.InProcess.GetDescription(),
                    Status = (int)TestSetStatus.InProcess,
                    StatusDescription = testSet.Description,
                    SoftwareId = radianContributorOperation.SoftwareId.ToString(),
                    ContributorTypeId = radianContributor.RadianContributorTypeId.ToString(),
                    OperationModeId = radianContributor.RadianOperationModeId,
                    OperationModeName = Domain.Common.EnumHelper.GetEnumDescription((Enum.Parse(typeof(Domain.Common.RadianOperationModeTestSet), sType))),

                    // Totales Generales
                    TotalDocumentRequired = testSet.TotalDocumentRequired,
                    TotalDocumentAcceptedRequired = testSet.TotalDocumentAcceptedRequired,
                    // Acuse de recibo
                    ReceiptNoticeTotalRequired = testSet.ReceiptNoticeTotalRequired,
                    ReceiptNoticeTotalAcceptedRequired = testSet.ReceiptNoticeTotalAcceptedRequired,
                    //Recibo del bien
                    ReceiptServiceTotalRequired = testSet.ReceiptServiceTotalRequired,
                    ReceiptServiceTotalAcceptedRequired = testSet.ReceiptServiceTotalAcceptedRequired,
                    // Aceptación expresa
                    ExpressAcceptanceTotalRequired = testSet.ExpressAcceptanceTotalRequired,
                    ExpressAcceptanceTotalAcceptedRequired = testSet.ExpressAcceptanceTotalAcceptedRequired,
                    //Manifestación de aceptación
                    AutomaticAcceptanceTotalRequired = testSet.AutomaticAcceptanceTotalRequired,
                    AutomaticAcceptanceTotalAcceptedRequired = testSet.AutomaticAcceptanceTotalAcceptedRequired,
                    //Rechazo factura electrónica
                    RejectInvoiceTotalRequired = testSet.RejectInvoiceTotalRequired,
                    RejectInvoiceTotalAcceptedRequired = testSet.RejectInvoiceTotalAcceptedRequired,
                    // Solicitud disponibilización
                    ApplicationAvailableTotalRequired = testSet.ApplicationAvailableTotalRequired,
                    ApplicationAvailableTotalAcceptedRequired = testSet.ApplicationAvailableTotalAcceptedRequired,
                    // Endoso en Propiedad
                    EndorsementPropertyTotalRequired = testSet.EndorsementPropertyTotalRequired,
                    EndorsementPropertyTotalAcceptedRequired = testSet.EndorsementPropertyTotalAcceptedRequired,
                    // Endoso en Procuracion
                    EndorsementProcurementTotalRequired = testSet.EndorsementProcurementTotalRequired,
                    EndorsementProcurementTotalAcceptedRequired = testSet.EndorsementProcurementTotalAcceptedRequired,
                    // Endoso en Garantia
                    EndorsementGuaranteeTotalRequired = testSet.EndorsementGuaranteeTotalRequired,
                    EndorsementGuaranteeTotalAcceptedRequired = testSet.EndorsementGuaranteeTotalAcceptedRequired,
                    // Cancelación de endoso
                    EndorsementCancellationTotalRequired = testSet.EndorsementCancellationTotalRequired,
                    EndorsementCancellationTotalAcceptedRequired = testSet.EndorsementCancellationTotalAcceptedRequired,
                    // Avales
                    GuaranteeTotalRequired = testSet.GuaranteeTotalRequired,
                    GuaranteeTotalAcceptedRequired = testSet.GuaranteeTotalAcceptedRequired,
                    // Mandato electrónico
                    ElectronicMandateTotalRequired = testSet.ElectronicMandateTotalRequired,
                    ElectronicMandateTotalAcceptedRequired = testSet.ElectronicMandateTotalAcceptedRequired,
                    // Terminación mandato
                    EndMandateTotalRequired = testSet.EndMandateTotalRequired,
                    EndMandateTotalAcceptedRequired = testSet.EndMandateTotalAcceptedRequired,
                    // Notificación de pago
                    PaymentNotificationTotalRequired = testSet.PaymentNotificationTotalRequired,
                    PaymentNotificationTotalAcceptedRequired = testSet.PaymentNotificationTotalAcceptedRequired,
                    // Limitación de circulación
                    CirculationLimitationTotalRequired = testSet.CirculationLimitationTotalRequired,
                    CirculationLimitationTotalAcceptedRequired = testSet.CirculationLimitationTotalAcceptedRequired,
                    // Terminación limitación 
                    EndCirculationLimitationTotalRequired = testSet.EndCirculationLimitationTotalRequired,
                    EndCirculationLimitationTotalAcceptedRequired = testSet.EndCirculationLimitationTotalAcceptedRequired,
                    //Reporte para el pago
                    ReportForPaymentTotalRequired = testSet.ReportForPaymentTotalRequired,
                    ReportForPaymentTotalAcceptedRequired = testSet.ReportForPaymentTotalAcceptedRequired,
                    //Transferencia de los derechos económicos 
                    TransferEconomicRightsTotalRequired = testSet.TransferEconomicRightsTotalRequired,
                    TransferEconomicRightsTotalAcceptedRequired = testSet.TransferEconomicRightsTotalAcceptedRequired,
                    //Notificación al deudor sobre la transferencia de los derechos económicos
                    NotificationDebtorOfTransferEconomicRightsTotalRequired = testSet.NotificationDebtorOfTransferEconomicRightsTotalRequired,
                    NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired = testSet.NotificationDebtorOfTransferEconomicRightsTotalAcceptedRequired,
                    //Pago de la transferencia de los derechos económicos  
                    PaymentOfTransferEconomicRightsTotalRequired = testSet.PaymentOfTransferEconomicRightsTotalRequired,
                    PaymentOfTransferEconomicRightsTotalAcceptedRequired = testSet.PaymentOfTransferEconomicRightsTotalAcceptedRequired,
                    //Endoso con efectos de cesión ordinaria
                    EndorsementWithEffectOrdinaryAssignmentTotalRequired = testSet.EndorsementWithEffectOrdinaryAssignmentTotalRequired,
                    EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired = testSet.EndorsementWithEffectOrdinaryAssignmentTotalAcceptedRequired,
                    //Protesto
                    ObjectionTotalRequired = testSet.ObjectionTotalRequired,
                    ObjectionTotalAcceptedRequired = testSet.ObjectionTotalAcceptedRequired,
                };
                if (radianContributor.RadianOperationModeId == (int)Gosocket.Dian.Domain.Common.RadianOperationMode.Indirect)
                {
                    setResult.Status = (int)TestSetStatus.Accepted;
                    setResult.State = TestSetStatus.Accepted.GetDescription();
                }

                _ = _radianTestSetResultService.InsertTestSetResult(setResult);

            }
        }

        public RadianContributorOperationWithSoftware ListRadianContributorOperations(int radianContributorId)
        {
            RadianContributorOperationWithSoftware radianContributorOperationWithSoftware = new RadianContributorOperationWithSoftware();
            radianContributorOperationWithSoftware.RadianContributorOperations = _radianContributorOperationRepository.List(t => t.RadianContributorId == radianContributorId && !t.Deleted);
            radianContributorOperationWithSoftware.Softwares = radianContributorOperationWithSoftware.RadianContributorOperations.Select(t => t.Software).ToList();
            return radianContributorOperationWithSoftware;
        }

        public RadianTestSetResult RadianTestSetResultByNit(string nit, string idTestSet)
        {
            return _radianTestSetResultService.GetTestSetResultByNit(nit).FirstOrDefault(t => t.Id == idTestSet);
        }

        /// <summary>
        /// Metodo encargado de filtrar los software disponibles de acuerdo el modo de seleccion.
        /// </summary>
        /// <param name="contributorId"></param>
        /// <param name="contributorTypeId"></param>
        /// <param name="operationMode"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public List<RadianSoftware> SoftwareList(int radianContributorId)
        {
            return _radianContributorRepository.RadianSoftwareByParticipante(radianContributorId);
        }

        public RadianSoftware GetSoftware(Guid id)
        {
            return _radianCallSoftwareService.Get(id);
        }

        public RadianSoftware GetSoftware(int radianContributorId, int softwareType)
        {
            RadianContributorOperation radianContributorOperation = _radianContributorOperationRepository.Get(t => t.RadianContributorId == radianContributorId && t.SoftwareType == softwareType);
            return GetSoftware(radianContributorOperation.SoftwareId);
        }

        public List<RadianContributor> AutoCompleteProvider(int contributorId, int contributorTypeId, RadianOperationModeTestSet softwareType, string term)
        {
            List<RadianContributor> participants = (softwareType == RadianOperationModeTestSet.OwnSoftware) ?
                             _radianContributorRepository.List(t => t.ContributorId == contributorId && t.RadianContributorTypeId == contributorTypeId && t.Contributor.BusinessName.Contains(term)).Results :
                             _radianContributorRepository.ActiveParticipantsWithSoftware((int)softwareType);

            return participants.Distinct().ToList();
        }

        public PagedResult<RadianCustomerList> CustormerList(int radianContributorId, string code, RadianState radianState, int page, int pagesize)
        {
            string radianStateText = radianState != RadianState.none ? radianState.GetDescription() : string.Empty;
            PagedResult<RadianCustomerList> customers = _radianContributorRepository.CustomerList(radianContributorId, code, radianStateText, page, pagesize);
            return customers;
        }

        public PagedResult<RadianContributorFileHistory> FileHistoryFilter(int radiancontributorId, string fileName, string initial, string end, int page, int pagesize)
        {
            DateTime initialDate, endDate;
            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(initial) && DateTime.TryParse(initial, out initialDate) && !string.IsNullOrEmpty(end) && DateTime.TryParse(end, out endDate))
                return _radianContributorFileHistoryRepository.HistoryByParticipantList(radiancontributorId, t => t.FileName.Contains(fileName) && t.Timestamp >= initialDate.Date && t.Timestamp <= endDate.Date, page, pagesize);

            if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(initial) && DateTime.TryParse(initial, out initialDate) && !string.IsNullOrEmpty(end) && DateTime.TryParse(end, out endDate))
                return _radianContributorFileHistoryRepository.HistoryByParticipantList(radiancontributorId, t => t.Timestamp >= initialDate.Date && t.Timestamp <= endDate.Date, page, pagesize);

            if (!string.IsNullOrEmpty(fileName))
                return _radianContributorFileHistoryRepository.HistoryByParticipantList(radiancontributorId, t => t.FileName.Contains(fileName), page, pagesize);

            return _radianContributorFileHistoryRepository.HistoryByParticipantList(radiancontributorId, t => true, page, pagesize);
        }

        public void DeleteSoftware(Guid softwareId)
        {
            _radianCallSoftwareService.DeleteSoftware(softwareId);
        }

        public List<RadianContributorOperation> OperationsBySoftwareId(Guid id)
        {
            return _radianContributorOperationRepository.List(t => t.SoftwareId == id);
        }

        public bool ResetRadianOperation(int radianOperationId)
        {
            RadianContributorOperation operation = _radianContributorOperationRepository.Get(t => t.Id == radianOperationId);
            operation.OperationStatusId = (int)RadianState.Test; //=en proceso
            return _radianContributorOperationRepository.Update(operation);
        }


        private async Task<GlobalContributorActivation> SyncToProductionAsync(RadianContributorOperation radianContributorOperation, RadianContributor radianContributor)
        {
            var pk = radianContributor.Contributor.Code.ToString();
            var rk = radianContributor.RadianContributorTypeId.ToString() + "|" + radianContributorOperation.SoftwareId;
            RadianTestSetResult testSetResult = _radianTestSetResultService.GetTestSetResult(pk, rk);
            if (testSetResult != null)
            {
                var data = new RadianActivationRequest();
                data.Code = radianContributor.Contributor.Code; 
                data.ContributorId = radianContributor.ContributorId; 
                data.ContributorTypeId = radianContributor.RadianContributorTypeId;  
                data.Pin = radianContributorOperation.Software.Pin;
                data.SoftwareId = radianContributorOperation.SoftwareId.ToString();
                data.SoftwareName = radianContributorOperation.Software.Name; 
                data.SoftwarePassword = radianContributorOperation.Software.SoftwarePassword; 
                data.SoftwareType = radianContributorOperation.SoftwareType.ToString();
                data.SoftwareUser = radianContributorOperation.Software.SoftwareUser; 
                data.TestSetId = testSetResult.Id;
                data.Url = radianContributorOperation.Software.Url;
                data.Enabled = true;

                var function = ConfigurationManager.GetValue("SendToActivateRadianOperationUrl");
                var response = await ApiHelpers.ExecuteRequestAsync<GlobalContributorActivation>(function, data);
                if (response.Success)
                {
                    Contributor contributorSoftware = _radianContributorService.GetContributor(radianContributorOperation.Software.ContributorId);
                    if (contributorSoftware != null)
                    {
                        //Se inserta en GlobalAuthorization
                        var auth = _globalAuthorizationService.Find(contributorSoftware.Code, data.Code);
                        if (auth == null)
                            _globalAuthorizationService.InsertOrUpdate(new GlobalAuthorization(contributorSoftware.Code, data.Code));

                    }
                }
                return response;
                
            }
            return new GlobalContributorActivation() { Success = false, Message = "No se encontró el set de pruebas" };
        }
    }

    class RadianActivationRequest
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "contributorId")]
        public int ContributorId { get; set; }

        [JsonProperty(PropertyName = "contributorTypeId")]
        public int ContributorTypeId { get; set; }

        [JsonProperty(PropertyName = "softwareId")]
        public string SoftwareId { get; set; }

        [JsonProperty(PropertyName = "softwareType")]
        public string SoftwareType { get; set; }

        [JsonProperty(PropertyName = "softwareUser")]
        public string SoftwareUser { get; set; }

        [JsonProperty(PropertyName = "softwarePassword")]
        public string SoftwarePassword { get; set; }

        [JsonProperty(PropertyName = "pin")]
        public string Pin { get; set; }

        [JsonProperty(PropertyName = "softwareName")]
        public string SoftwareName { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "testSetId")]
        public string TestSetId { get; set; }

        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }
    }
}
