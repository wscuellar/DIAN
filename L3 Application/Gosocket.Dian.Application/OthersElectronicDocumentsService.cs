﻿using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class OthersElectronicDocumentsService : IOthersElectronicDocumentsService
    {
        private readonly SqlDBContext sqlDBContext;
        private static readonly TableManager tableManager = new TableManager("GlobalLogger");
        private readonly IContributorService _contributorService;
        private readonly IOthersDocsElecContributorService _othersDocsElecContributorService;
        private readonly IOthersDocsElecSoftwareService _othersDocsElecSoftwareService;
        private readonly IGlobalOtherDocElecOperationService _globalOtherDocElecOperationService;
        private readonly ITestSetOthersDocumentsResultService _testSetOthersDocumentsResultService;

        private readonly IOthersDocsElecContributorRepository _othersDocsElecContributorRepository;
        private readonly IOthersDocsElecContributorOperationRepository _othersDocsElecContributorOperationRepository;

        public OthersElectronicDocumentsService(IContributorService contributorService,
            IOthersDocsElecSoftwareService othersDocsElecSoftwareService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IOthersDocsElecContributorOperationRepository othersDocsElecContributorOperationRepository,
            IOthersDocsElecContributorRepository othersDocsElecContributorRepositor,
            IGlobalOtherDocElecOperationService globalOtherDocElecOperationService,
            ITestSetOthersDocumentsResultService testSetOthersDocumentsResultService)
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
        }

        public OthersElectronicDocumentsService()
        { }

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
            GlobalOtherDocElecOperation operation = _globalOtherDocElecOperationService.GetOperation(contributor.Code, software.SoftwareId);
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
            operation.State = OtherDocElecState.Test.GetDescription();
            operation.ContributorTypeId = ODEContributor.OtherDocElecContributorTypeId;
            operation.Deleted = false;

            if (_globalOtherDocElecOperationService.Insert(operation, existingOperation.Software))
            {
                string key = existingOperation.SoftwareType.ToString() + "|" + software.SoftwareId.ToString();
                GlobalTestSetOthersDocumentsResult setResult = new GlobalTestSetOthersDocumentsResult(contributor.Code, key)
                {
                    Id = Guid.NewGuid().ToString(),
                    OtherDocElecContributorId = ODEContributor.Id,
                    State = TestSetStatus.InProcess.GetDescription(),
                    Status = (int)TestSetStatus.InProcess,
                    StatusDescription = TestSetStatus.InProcess.GetDescription(),
                    ContributorTypeId = ODEContributor.OtherDocElecContributorTypeId.ToString(),
                    OperationModeName = ((Domain.Common.OtherDocElecOperationMode)ODEContributor.OtherDocElecOperationModeId).GetDescription(),
                    ElectronicDocumentId = ODEContributor.ElectronicDocumentId,
                    SoftwareId = software.Id.ToString(),
                    ProviderId = software.ProviderId,
                    // Totales Generales
                    TotalDocumentRequired = testSet.TotalDocumentRequired,
                    TotalDocumentAcceptedRequired = testSet.TotalDocumentAcceptedRequired,
                    TotalDocumentSent = 0,
                    TotalDocumentAccepted = 0,
                    TotalDocumentsRejected = 0,
                    // EndTotales Generales

                    // OthersDocuments
                    OthersDocumentsRequired = testSet.OthersDocumentsRequired,
                    OthersDocumentsAcceptedRequired = testSet.OthersDocumentsAcceptedRequired,
                    TotalOthersDocumentsSent = 0,
                    OthersDocumentsAccepted = 0,
                    OthersDocumentsRejected = 0,
                    //End OthersDocuments

                    //ElectronicPayrollAjustment
                    ElectronicPayrollAjustmentRequired = testSet.ElectronicPayrollAjustmentRequired,
                    ElectronicPayrollAjustmentAcceptedRequired = testSet.ElectronicPayrollAjustmentAcceptedRequired,
                    TotalElectronicPayrollAjustmentSent = 0,
                    ElectronicPayrollAjustmentAccepted = 0,
                    ElectronicPayrollAjustmentRejected = 0,
                    //EndElectronicPayrollAjustment
                };
                // insert...
                _ = _testSetOthersDocumentsResultService.InsertTestSetResult(setResult);
            }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="operationStatus"></param>
        /// <returns></returns>
        public async Task<int> UpdateOtherDocElecContributorOperationStatusId(OtherDocElecContributorOperations filters, Domain.Common.OtherDocElecState operationStatus)
        {
            try
            {
                OtherDocElecContributorOperations contributorOperation = null;
                using (var context = new SqlDBContext())
                {
                    contributorOperation = context.OtherDocElecContributorOperations.FirstOrDefault(c => c.OtherDocElecContributorId == filters.OtherDocElecContributorId &&
                    c.SoftwareId == filters.SoftwareId);
                    contributorOperation.Deleted = filters.Deleted;
                    if (contributorOperation.OperationStatusId != (int)operationStatus)
                        contributorOperation.OperationStatusId = filters.OperationStatusId;
                    await context.SaveChangesAsync();
                }
                return contributorOperation.Id;
            }
            catch (Exception ex)
            {
                var logger = new GlobalLogger("Other Elec Doc - ContributorOperationStatusId", filters.Id.ToString())
                {
                    Action = "Update",
                    Controller = "",
                    Message = ex.Message,
                    RouteData = "",
                    StackTrace = ex.StackTrace
                };
                RegisterException(logger);
                return 0;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="otherDocElecContributorPar"></param>
        /// <param name="sqlConnectionStringProd"></param>
        /// <returns></returns>
        public bool QualifiedContributor(OtherDocElecContributorOperations filters, OtherDocElecContributor otherDocElecContributorPar, 
            string sqlConnectionStringProd)
        {
            bool resultado = false;
            if (string.IsNullOrEmpty(sqlConnectionStringProd))
                sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnection");

            using (var context = new SqlDBContext(sqlConnectionStringProd))
            {
                var contributor = context.Contributors.FirstOrDefault(co => co.Code == otherDocElecContributorPar.Description);
                if (contributor != null)
                {
                    var otherDocElecContributor = context.OtherDocElecContributors.FirstOrDefault(c => c.ContributorId == contributor.Id &&
                            c.OtherDocElecOperationModeId == otherDocElecContributorPar.OtherDocElecOperationModeId && c.State == "Habilitado" && c.Step == 3);
                    if (otherDocElecContributor != null)
                    {
                        filters.OtherDocElecContributorId = otherDocElecContributor.Id;
                        //resultado = true;
                        OtherDocElecContributorOperations contributorOperation = context.OtherDocElecContributorOperations.FirstOrDefault(co =>
                        co.OtherDocElecContributorId == filters.OtherDocElecContributorId &&
                        co.SoftwareId == filters.SoftwareId && co.Deleted == filters.Deleted && co.OperationStatusId == filters.OperationStatusId);
                        if (contributorOperation != null)
                        {
                            var otherdocelecsoftware = context.OtherDocElecSoftwares.FirstOrDefault(s => s.Id == contributorOperation.SoftwareId && (!s.Deleted) && s.Status &&
                            s.OtherDocElecSoftwareStatusId == (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted);
                            if (otherdocelecsoftware != null)
                            {
                                resultado = true;
                            }
                        }
                    }
                }
            }
            return resultado;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        private void RegisterException(GlobalLogger logger)
        {

            tableManager.InsertOrUpdate(logger);
        }
    }
}
