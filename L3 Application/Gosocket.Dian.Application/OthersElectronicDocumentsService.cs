using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class OthersElectronicDocumentsService : IOthersElectronicDocumentsService
    {
        private SqlDBContext sqlDBContext;
        private readonly IContributorService _contributorService;
        private readonly IOthersDocsElecContributorService _othersDocsElecContributorService;
        
        private readonly IOthersDocsElecSoftwareService _othersDocsElecSoftwareService;
        private readonly IOthersDocsElecContributorRepository _othersDocsElecContributorRepository;
        private readonly IOthersDocsElecContributorOperationRepository _othersDocsElecContributorOperationRepository;

        public OthersElectronicDocumentsService(IContributorService contributorService,
            IOthersDocsElecSoftwareService othersDocsElecSoftwareService,
            IOthersDocsElecContributorService othersDocsElecContributorService,
            IOthersDocsElecContributorOperationRepository othersDocsElecContributorOperationRepository,
            IOthersDocsElecContributorRepository othersDocsElecContributorRepositor)
        {
            _contributorService = contributorService;
            _othersDocsElecContributorService = othersDocsElecContributorService;
            _othersDocsElecSoftwareService = othersDocsElecSoftwareService;
            _othersDocsElecContributorRepository = othersDocsElecContributorRepositor;
            _othersDocsElecContributorOperationRepository = othersDocsElecContributorOperationRepository;
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public ResponseMessage Validation(string userCode, string Accion, int ElectronicDocumentId, string complementeTexto, int ContributorIdType)
        {
            return new ResponseMessage(TextResources.FailedValidation, TextResources.alertType);
        }


        public ResponseMessage AddOtherDocElecContributorOperation(OtherDocElecContributorOperations ContributorOperation, OtherDocElecSoftware software, bool isInsert, bool validateOperation)
        {
            //if (testSet == null)
            //    return new ResponseMessage(TextResources.ModeWithoutTestSet, TextResources.alertType, 500);

            if (validateOperation)
            {
                List<OtherDocElecContributorOperations> currentOperations = 
                    _othersDocsElecContributorOperationRepository.List(t => t. OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId
                                                                    && t.SoftwareType == ContributorOperation.SoftwareType
                                                                    && t.OperationStatusId != (int)OtherDocElecState.Habilitado
                                                                    && !t.Deleted);
                if (currentOperations.Any())
                    return new ResponseMessage(TextResources.OperationFailOtherInProcess, TextResources.alertType, 500);
            }

            OtherDocElecContributor Contributor = _othersDocsElecContributorRepository.Get(t => t.Id ==  ContributorOperation.OtherDocElecContributorId);
            OtherDocElecContributorOperations existingOperation = _othersDocsElecContributorOperationRepository.Get(t => t. OtherDocElecContributorId ==  ContributorOperation.OtherDocElecContributorId && t.SoftwareId == ContributorOperation.SoftwareId && !t.Deleted);
            if (existingOperation != null)
                return new ResponseMessage(TextResources.ExistingSoftware, TextResources.alertType, 500);

            if (isInsert)
            {
                OtherDocElecSoftware soft = _othersDocsElecSoftwareService.CreateSoftware(software);
                ContributorOperation.SoftwareId = soft.Id;
            }

            ContributorOperation.OperationStatusId = (int)(Contributor.State == OtherDocElecState.Habilitado.GetDescription() ? (int)OtherDocElecState.Test : (int)RadianState.Registrado);
            int operationId = _othersDocsElecContributorOperationRepository.Add(ContributorOperation);
            existingOperation = _othersDocsElecContributorOperationRepository.Get(t => t.Id == operationId);

            //ApplyTestSet(radianContributorOperation, testSet, radianContributor, existingOperation);

            return new ResponseMessage(TextResources.SuccessSoftware, TextResources.alertType);
        }

    }
}
