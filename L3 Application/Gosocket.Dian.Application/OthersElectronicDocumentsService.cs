using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class OthersElectronicDocumentsService : IOthersElectronicDocumentsService
    {
        private readonly SqlDBContext sqlDBContext;
        private readonly IContributorService _contributorService;
        private readonly IOthersDocsElecContributorService _othersDocsElecContributorService;
        private readonly IOthersDocsElecSoftwareService _othersDocsElecSoftwareService;
        private readonly IGlobalOtherDocElecOperationService _globalOtherDocElecOperationService;
        private readonly ITestSetOthersDocumentsResultService _testSetOthersDocumentsResultService;

        private readonly IOthersDocsElecContributorRepository _othersDocsElecContributorRepository;
        private readonly IOthersDocsElecContributorOperationRepository _othersDocsElecContributorOperationRepository;
        private readonly IEquivalentElectronicDocumentRepository _equivalentElectronicDocumentRepository;

        public OthersElectronicDocumentsService(IContributorService contributorService,
            IOthersDocsElecSoftwareService othersDocsElecSoftwareService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IOthersDocsElecContributorOperationRepository othersDocsElecContributorOperationRepository,
            IOthersDocsElecContributorRepository othersDocsElecContributorRepositor,
            IGlobalOtherDocElecOperationService globalOtherDocElecOperationService,
            ITestSetOthersDocumentsResultService testSetOthersDocumentsResultService, 
            IEquivalentElectronicDocumentRepository equivalentElectronicDocumentRepository)
        {
            _contributorService = contributorService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _othersDocsElecSoftwareService = othersDocsElecSoftwareService;
            _othersDocsElecContributorRepository = othersDocsElecContributorRepositor;
            _othersDocsElecContributorOperationRepository = othersDocsElecContributorOperationRepository;
            _globalOtherDocElecOperationService = globalOtherDocElecOperationService;
            _testSetOthersDocumentsResultService = testSetOthersDocumentsResultService;

            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
            _equivalentElectronicDocumentRepository = equivalentElectronicDocumentRepository;
        }

        public ResponseMessage Validation(string userCode, string Accion, int ElectronicDocumentId, string complementeTexto, int ContributorIdType)
        {
            return new ResponseMessage(TextResources.FailedValidation, TextResources.alertType);
        }


        public ResponseMessage AddOtherDocElecContributorOperation(OtherDocElecContributorOperations ContributorOperation, OtherDocElecSoftware software, bool isInsert, bool validateOperation)
        {
            OtherDocElecContributor Contributor = _othersDocsElecContributorRepository.Get(t => t.Id == ContributorOperation.OtherDocElecContributorId);
            GlobalTestSetOthersDocuments testSet = null;
            testSet = _othersDocsElecContributorService.GetTestResult(Contributor.OtherDocElecOperationModeId, Contributor.ElectronicDocumentId);
            if (testSet == null)
                return new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500);

            if (validateOperation)
            {
                List<OtherDocElecContributorOperations> currentOperations =
                    _othersDocsElecContributorOperationRepository.List(t => t.OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId
                                                                    && t.SoftwareType == ContributorOperation.SoftwareType
                                                                    && t.OperationStatusId == (int)OtherDocElecState.Test
                                                                    && !t.Deleted);
                if (currentOperations.Any())
                    return new ResponseMessage(TextResources.OperationFailOtherInProcess, TextResources.alertType, 500);
            }

            OtherDocElecContributorOperations existingOperation = _othersDocsElecContributorOperationRepository.Get(t => t.OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId && t.SoftwareId == ContributorOperation.SoftwareId && !t.Deleted);
            if (existingOperation != null)
                return new ResponseMessage(TextResources.ExistingSoftware, TextResources.alertType, 500);

            _othersDocsElecSoftwareService.CreateSoftware(software);

            int operationId = _othersDocsElecContributorOperationRepository.Add(ContributorOperation);

            existingOperation = _othersDocsElecContributorOperationRepository.Get(t => t.Id == operationId);
            // se asigna el nuevo set de pruebas...
            ApplyTestSet(ContributorOperation, testSet, Contributor, existingOperation, software);

            return new ResponseMessage(TextResources.SuccessSoftware, TextResources.alertType);
        }


        public ResponseMessage AddOtherDocElecContributorOperationNew(OtherDocElecContributorOperations ContributorOperation, OtherDocElecSoftware software, bool isInsert, bool validateOperation,int ContributorId,int ContributorIdType,int OperationModeId)
        {
            OtherDocElecContributor Contributor = _othersDocsElecContributorRepository.Get(t => t.Id == ContributorOperation.OtherDocElecContributorId);
            GlobalTestSetOthersDocuments testSet = null;
            testSet = _othersDocsElecContributorService.GetTestResult(Contributor.OtherDocElecOperationModeId, Contributor.ElectronicDocumentId);
            if (testSet == null)
                return new ResponseMessage(TextResources.ModeElectroniDocWithoutTestSet, TextResources.alertType, 500);

            if (validateOperation)
            {
                List<OtherDocElecContributorOperations> currentOperations =
                    _othersDocsElecContributorOperationRepository.List(t => t.OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId
                                                                    && t.SoftwareType == ContributorOperation.SoftwareType
                                                                    && t.OperationStatusId == (int)OtherDocElecState.Test
                                                                    && !t.Deleted);
                if (currentOperations.Any())
                    return new ResponseMessage(TextResources.OperationFailOtherInProcess, TextResources.alertType, 500);
            }

            OtherDocElecContributorOperations existingOperation = _othersDocsElecContributorOperationRepository.Get(t => t.OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId && t.SoftwareId == ContributorOperation.SoftwareId && !t.Deleted);
            if (existingOperation != null)
                return new ResponseMessage(TextResources.ExistingSoftware, TextResources.alertType, 500);
            PagedResult<OtherDocsElectData> List = _othersDocsElecContributorService.List3(ContributorId, 2, Contributor.ElectronicDocumentId);
            PagedResult<OtherDocsElectData> List2 = _othersDocsElecContributorService.List3(ContributorId, 1, Contributor.ElectronicDocumentId);

            if (List.Results.Any(x=>x.StateSoftware=="2")|| List2.Results.Any(x => x.StateSoftware == "2"))
                return new ResponseMessage("No se puede asociar modo de operación, ya que tiene uno en pruebas", TextResources.alertType, 500);
            if (List.Results.Any(x => x.StateSoftware == "4") || List2.Results.Any(x => x.StateSoftware == "4"))
                return new ResponseMessage("No se puede asociar modo de operación, ya que tiene uno Rechazado", TextResources.alertType, 500);


            _othersDocsElecSoftwareService.CreateSoftware(software);
            int operationId = _othersDocsElecContributorOperationRepository.Add(ContributorOperation);
            

            existingOperation = _othersDocsElecContributorOperationRepository.Get(t => t.Id == operationId);
            // se asigna el nuevo set de pruebas...
            ApplyTestSet(ContributorOperation, testSet, Contributor, existingOperation, software);

            return new ResponseMessage(TextResources.SuccessSoftware, TextResources.alertType);
        }
        public bool ChangeParticipantStatus(int contributorId, string newState, int ContributorTypeId, string actualState, string description)
        {
            List<OtherDocElecContributor> contributors = _othersDocsElecContributorRepository.List(t => t.Id == contributorId
                                                         && t.State == actualState).Results;
            if (!contributors.Any())
                return false;

            OtherDocElecContributor entity = contributors.FirstOrDefault();
            entity.State = newState;
            entity.Description = description;

            if (newState == OtherDocElecState.Test.GetDescription()) entity.Step = 2;
            if (newState == OtherDocElecState.Habilitado.GetDescription()) entity.Step = 3;
            if (newState == OtherDocElecState.Cancelado.GetDescription()) entity.Step = 1;

            if (entity.State == OtherDocElecState.Cancelado.GetDescription())
                CancelParticipant(entity);

            entity.Update = DateTime.Now;

            _othersDocsElecContributorRepository.AddOrUpdate(entity);

            return true;

        }

        public bool ChangeContributorStep(int ContributorId, int step)
        {
            OtherDocElecContributor entity = _othersDocsElecContributorRepository.Get(t => t.Id == ContributorId);

            if (entity == null) return false;
            entity.Step = step;
            _othersDocsElecContributorRepository.AddOrUpdate(entity);
            return true;
        }

        public PagedResult<OtherDocElecCustomerList> CustormerList(int ContributorId, string code, OtherDocElecState State, int page, int pagesize)
        {
            string StateText = State != OtherDocElecState.none ? State.GetDescription() : string.Empty;
            PagedResult<OtherDocElecCustomerList> customers = _othersDocsElecContributorRepository.CustomerList(ContributorId, code, StateText, page, pagesize);
            return customers;
        }

        public ResponseMessage OperationDelete(int ODEContributorId)
        {
            OtherDocElecContributorOperations operationToDelete = _othersDocsElecContributorOperationRepository.Get(t => t.Id == ODEContributorId);

            OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(operationToDelete.SoftwareId);
            if (software != null && software.OtherDocElecSoftwareStatusId == (int)OtherDocElecSoftwaresStatus.Accepted)
                return new ResponseMessage() { Message = "El software encuentra en estado aceptado.", Code = 500 };

            var result = _othersDocsElecContributorOperationRepository.Delete(operationToDelete.Id);
            if (software != null)
                _othersDocsElecSoftwareService.DeleteSoftware(software.Id);

            return result;
        }

        public ResponseMessage ValidaSoftwareDelete(int ODEContributorId)
        {
            OtherDocElecContributorOperations operationToDelete = _othersDocsElecContributorOperationRepository.Get(t => t.Id == ODEContributorId);

            OtherDocElecSoftware software = _othersDocsElecSoftwareService.Get(operationToDelete.SoftwareId);
            if (software != null && software.OtherDocElecSoftwareStatusId == (int)OtherDocElecSoftwaresStatus.Accepted)
                return new ResponseMessage() { Message = "El software encuentra en estado aceptado.", Code = 500 };
 
            return null;
        }

        #region Private methods Cancel Participant

        private void CancelParticipant(OtherDocElecContributor entity)
        {

            List<OtherDocElecSoftware> softwares = _othersDocsElecSoftwareService.List(entity.Id);
            foreach (OtherDocElecSoftware software in softwares)
            {
                //Quita los software
                _othersDocsElecSoftwareService.DeleteSoftware(software.Id);

                //Quitar la operacion.
                List<OtherDocElecContributorOperations> operations = _othersDocsElecContributorOperationRepository.List(t => t.SoftwareId == software.Id && !t.Deleted);
                foreach (OtherDocElecContributorOperations operation in operations)
                {
                    operation.OperationStatusId = (int)OtherDocElecState.Cancelado;
                    operation.Deleted = true;
                    _othersDocsElecContributorOperationRepository.Update(operation);
                }
            }
        }

        private void ApplyTestSet(OtherDocElecContributorOperations ODEOperation, GlobalTestSetOthersDocuments testSet, OtherDocElecContributor ODEContributor, OtherDocElecContributorOperations existingOperation, OtherDocElecSoftware software)
        {
            Contributor contributor = ODEContributor.Contributor;
            bool contributorIsOfe = contributor.ContributorTypeId == (int)Domain.Common.ContributorType.Biller;
            bool electronicDocumentIsSupport = ODEContributor.ElectronicDocumentId == (int)ElectronicsDocuments.SupportDocument;

            GlobalOtherDocElecOperation operation = _globalOtherDocElecOperationService.GetOperationByElectronicDocumentId(contributor.Code, software.SoftwareId, ODEContributor.ElectronicDocumentId);
            if (operation == null)
                operation = new GlobalOtherDocElecOperation(contributor.Code, software.SoftwareId.ToString());

            if (ODEContributor.OtherDocElecContributorTypeId == (int)Domain.Common.OtherDocElecContributorType.Transmitter)
                operation.Transmitter = true;
            if (ODEContributor.OtherDocElecContributorTypeId == (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                operation.TecnologicalSupplier = true;

            operation.OtherDocElecContributorId = ODEContributor.Id;
            operation.OperationModeId = ODEContributor.OtherDocElecOperationModeId;

            operation.SoftwareId = existingOperation.SoftwareId.ToString();
            operation.ElectronicDocumentId = ODEContributor.ElectronicDocumentId;
            operation.OtherDocElecContributorId = ODEContributor.Id;
            operation.State = (contributorIsOfe && electronicDocumentIsSupport) ? OtherDocElecState.Habilitado.GetDescription() : OtherDocElecState.Test.GetDescription();
            operation.ContributorTypeId = ODEContributor.OtherDocElecContributorTypeId;
            operation.Deleted = false;

            if (_globalOtherDocElecOperationService.Insert(operation, existingOperation.Software))
            {
                RegisterTestSet(testSet, ODEContributor, existingOperation, software, contributor, contributorIsOfe, electronicDocumentIsSupport);
                bool electronicDocumentIsEquivalent = ODEContributor.ElectronicDocumentId == (int)ElectronicsDocuments.ElectronicEquivalent;
                if (electronicDocumentIsEquivalent)
                {
                    List<EquivalentElectronicDocument> equivalentsDocuments = _equivalentElectronicDocumentRepository.GetEquivalentElectronicDocuments();
                    foreach (var item in equivalentsDocuments)
                    {
                        RegisterTestSet(testSet, ODEContributor, existingOperation, software, contributor, contributorIsOfe, electronicDocumentIsSupport, item.Id);
                    }
                }
            }
        }

        private void RegisterTestSet(GlobalTestSetOthersDocuments testSet, OtherDocElecContributor ODEContributor, OtherDocElecContributorOperations existingOperation, OtherDocElecSoftware software, Contributor contributor, bool contributorIsOfe, bool electronicDocumentIsSupport, int? equivalentElectronicDocumentId = null)
        {
            string key = existingOperation.SoftwareType.ToString() + "|" + software.SoftwareId.ToString() + (!equivalentElectronicDocumentId.HasValue ? "" : $"|{equivalentElectronicDocumentId.Value}");
            GlobalTestSetOthersDocumentsResult setResult = new GlobalTestSetOthersDocumentsResult(contributor.Code, key)
            {
                Id = Guid.NewGuid().ToString(),
                OtherDocElecContributorId = ODEContributor.Id,
                State = (contributorIsOfe && electronicDocumentIsSupport) ? TestSetStatus.Accepted.GetDescription() : TestSetStatus.InProcess.GetDescription(),
                Status = (contributorIsOfe && electronicDocumentIsSupport) ? (int)TestSetStatus.Accepted : (int)TestSetStatus.InProcess,
                StatusDescription = (contributorIsOfe && electronicDocumentIsSupport) ? TestSetStatus.Accepted.GetDescription() : TestSetStatus.InProcess.GetDescription(),
                ContributorTypeId = ODEContributor.OtherDocElecContributorTypeId.ToString(),
                OperationModeName = ((Domain.Common.OtherDocElecOperationMode)ODEContributor.OtherDocElecOperationModeId).GetDescription(),
                ElectronicDocumentId = ODEContributor.ElectronicDocumentId,
                EquivalentElectronicDocumentId = equivalentElectronicDocumentId,
                SoftwareId = software.Id.ToString(),
                ProviderId = software.ProviderId,
                // Totales Generales
                TotalDocumentRequired = (contributorIsOfe && electronicDocumentIsSupport) ? 0 : testSet.TotalDocumentRequired,
                TotalDocumentAcceptedRequired = (contributorIsOfe && electronicDocumentIsSupport) ? 0 : testSet.TotalDocumentAcceptedRequired,
                TotalDocumentSent = 0,
                TotalDocumentAccepted = 0,
                TotalDocumentsRejected = 0,
                // EndTotales Generales

                // OthersDocuments
                OthersDocumentsRequired = (contributorIsOfe && electronicDocumentIsSupport) ? 0 : testSet.OthersDocumentsRequired,
                OthersDocumentsAcceptedRequired = (contributorIsOfe && electronicDocumentIsSupport) ? 0 : testSet.OthersDocumentsAcceptedRequired,
                TotalOthersDocumentsSent = 0,
                OthersDocumentsAccepted = 0,
                OthersDocumentsRejected = 0,
                //End OthersDocuments

                //ElectronicPayrollAjustment
                ElectronicPayrollAjustmentRequired = (contributorIsOfe && electronicDocumentIsSupport) ? 0 : testSet.ElectronicPayrollAjustmentRequired,
                ElectronicPayrollAjustmentAcceptedRequired = (contributorIsOfe && electronicDocumentIsSupport) ? 0 : testSet.ElectronicPayrollAjustmentAcceptedRequired,
                TotalElectronicPayrollAjustmentSent = 0,
                ElectronicPayrollAjustmentAccepted = 0,
                ElectronicPayrollAjustmentRejected = 0,
                //EndElectronicPayrollAjustment
            };
            // insert...
            _ = _testSetOthersDocumentsResultService.InsertTestSetResult(setResult);
        }
        #endregion

        public OtherDocElecContributorOperations GetOtherDocElecContributorOperationBySoftwareId(Guid softwareId)
        {
            return this._othersDocsElecContributorOperationRepository.Get(t => t.SoftwareId == softwareId);
        }

        public bool UpdateOtherDocElecContributorOperation(OtherDocElecContributorOperations model)
        {
            return _othersDocsElecContributorOperationRepository.Update(model);
        }

        public OtherDocElecContributorOperations GetOtherDocElecContributorOperationById(int id)
        {
            return this._othersDocsElecContributorOperationRepository.Get(t => t.Id == id);
        }

        public OtherDocElecContributorOperations GetOtherDocElecContributorOperationByDocEleContributorId(int id)
        {
            return this._othersDocsElecContributorOperationRepository.Get(t => t.OtherDocElecContributorId == id);
        }

        public List<OtherDocElecContributorOperations> GetOtherDocElecContributorOperationsListByDocElecContributorId(int id)
        {
            return this._othersDocsElecContributorOperationRepository.List(t => t.OtherDocElecContributorId == id);
        }
    }
}
