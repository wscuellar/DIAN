﻿using Gosocket.Dian.Common.Resources;
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
                                    IGlobalRadianOperationService globalRadianOperationService)
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
                return default;

            return contributorOperations.FirstOrDefault(t => !t.Deleted && t.OperationModeId == (int)Domain.Common.OperationMode.Own && t.Software != null && t.Software.Status)?.Software ?? default;
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
            return _radianContributorOperationRepository.Delete(operationToDelete.Id);
        }

        public ResponseMessage UploadFile(Stream fileStream, string code, RadianContributorFile radianContributorFile)
        {
            string fileName = StringTools.MakeValidFileName(radianContributorFile.FileName);
            var fileManager = new FileManager(ConfigurationManager.GetValue("GlobalStorage"));
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
            string idHistoryRegister = string.Empty;

            radianContributorFileHistory.Id = Guid.NewGuid();
            idHistoryRegister = _radianContributorFileHistoryRepository.AddRegisterHistory(radianContributorFileHistory).ToString();

            if (!string.IsNullOrEmpty(idHistoryRegister))
            {
                return new ResponseMessage($"Información registrada id: {idHistoryRegister}", "Guardado");
            }

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


        public RadianTestSet GetTestResult(string softwareType)
        {
            return _radianTestSetService.GetTestSet(softwareType, softwareType);
        }

        public ResponseMessage AddRadianContributorOperation(RadianContributorOperation radianContributorOperation, RadianSoftware software, RadianTestSet testSet, bool isInsert, bool validateOperation)
        {
            int result = 0;

            if (testSet == null)
                return new ResponseMessage(TextResources.ModeWithoutTestSet, TextResources.alertType, 500);

            if (validateOperation)
            {
                List<RadianContributorOperation> currentOperations = _radianContributorOperationRepository.List(t => t.RadianContributorId == radianContributorOperation.RadianContributorId && t.SoftwareType == radianContributorOperation.SoftwareType && t.OperationStatusId != (int)RadianState.Habilitado && !t.Deleted);
                if (currentOperations.Any())
                    return new ResponseMessage(TextResources.OperationFailOtherInProcess, TextResources.alertType, 500);
            }

            RadianContributor radianContributor = _radianContributorRepository.Get(t => t.Id == radianContributorOperation.RadianContributorId);
            RadianContributorOperation existingOperation = _radianContributorOperationRepository.Get(t => t.RadianContributorId == radianContributorOperation.RadianContributorId && t.SoftwareId == radianContributorOperation.SoftwareId && !t.Deleted);
            if (existingOperation != null)
                return new ResponseMessage(TextResources.ExistingSoftware, TextResources.alertType, 500);

            if (isInsert)
            {
                RadianSoftware soft = _radianCallSoftwareService.CreateSoftware(software);
                radianContributorOperation.SoftwareId = soft.Id;
            }

            radianContributorOperation.OperationStatusId = (int)(radianContributor.RadianState == RadianState.Habilitado.GetDescription() ? RadianState.Test : RadianState.Registrado);
            result = _radianContributorOperationRepository.Add(radianContributorOperation);
            existingOperation = _radianContributorOperationRepository.Get(t => t.Id == result);

            Contributor contributor = radianContributor.Contributor;
            GlobalRadianOperations operation = new GlobalRadianOperations(contributor.Code, existingOperation.SoftwareId.ToString())
            {
                RadianStatus = radianContributor.RadianState == RadianState.Habilitado.GetDescription() ? RadianState.Test.GetDescription() : RadianState.Registrado.GetDescription(),
                SoftwareType = existingOperation.SoftwareType,
                RadianContributorTypeId = radianContributor.RadianContributorTypeId,
                Deleted = false
            };

            if (_globalRadianOperationService.Insert(operation, existingOperation.Software))
            {
                string key = existingOperation.SoftwareType.ToString() + "|" + radianContributorOperation.SoftwareId;
                RadianTestSetResult setResult = new RadianTestSetResult(contributor.Code, key)
                {
                    Id = Guid.NewGuid().ToString(),
                    ContributorId = radianContributor.Id,
                    ContributorTypeId = radianContributor.RadianContributorTypeId.ToString(),
                    TotalDocumentRequired = testSet.TotalDocumentAcceptedRequired,
                    ReceiptNoticeTotalRequired = testSet.ReceiptNoticeTotalAcceptedRequired,
                    ReceiptServiceTotalRequired = testSet.ReceiptServiceTotalAcceptedRequired,
                    ExpressAcceptanceTotalRequired = testSet.ExpressAcceptanceTotalAcceptedRequired,
                    AutomaticAcceptanceTotalRequired = testSet.AutomaticAcceptanceTotalAcceptedRequired,
                    RejectInvoiceTotalRequired = testSet.RejectInvoiceTotalAcceptedRequired,
                    ApplicationAvailableTotalRequired = testSet.ApplicationAvailableTotalRequired,
                    ApplicationAvailableTotalAcceptedRequired = testSet.ApplicationAvailableTotalAcceptedRequired,
                    EndorsementPropertyTotalRequired = testSet.EndorsementPropertyTotalRequired,
                    EndorsementPropertyTotalAcceptedRequired = testSet.EndorsementPropertyTotalAcceptedRequired,
                    EndorsementProcurementTotalRequired = testSet.EndorsementProcurementTotalRequired,
                    EndorsementProcurementTotalAcceptedRequired = testSet.EndorsementProcurementTotalAcceptedRequired,
                    EndorsementGuaranteeTotalRequired = testSet.EndorsementGuaranteeTotalRequired,
                    EndorsementGuaranteeTotalAcceptedRequired = testSet.EndorsementGuaranteeTotalAcceptedRequired,
                    EndorsementCancellationTotalRequired = testSet.EndorsementCancellationTotalRequired,
                    EndorsementCancellationTotalAcceptedRequired = testSet.EndorsementCancellationTotalAcceptedRequired,
                    GuaranteeTotalRequired = testSet.GuaranteeTotalRequired,
                    GuaranteeTotalAcceptedRequired = testSet.GuaranteeTotalAcceptedRequired,
                    ElectronicMandateTotalRequired = testSet.ElectronicMandateTotalRequired,
                    ElectronicMandateTotalAcceptedRequired = testSet.ElectronicMandateTotalAcceptedRequired,
                    EndMandateTotalRequired = testSet.EndMandateTotalRequired,
                    EndMandateTotalAcceptedRequired = testSet.EndMandateTotalAcceptedRequired,
                    PaymentNotificationTotalRequired = testSet.PaymentNotificationTotalRequired,
                    PaymentNotificationTotalAcceptedRequired = testSet.PaymentNotificationTotalAcceptedRequired,
                    CirculationLimitationTotalRequired = testSet.CirculationLimitationTotalRequired,
                    CirculationLimitationTotalAcceptedRequired = testSet.CirculationLimitationTotalAcceptedRequired,
                    EndCirculationLimitationTotalRequired = testSet.EndCirculationLimitationTotalRequired,
                    EndCirculationLimitationTotalAcceptedRequired = testSet.EndCirculationLimitationTotalAcceptedRequired
                };
                _ = _radianTestSetResultService.InsertTestSetResult(setResult);

            }

            return new ResponseMessage(TextResources.SuccessSoftware, TextResources.alertType);
        }

        public RadianContributorOperationWithSoftware ListRadianContributorOperations(int radianContributorId)
        {
            RadianContributorOperationWithSoftware radianContributorOperationWithSoftware = new RadianContributorOperationWithSoftware();
            radianContributorOperationWithSoftware.RadianContributorOperations = _radianContributorOperationRepository.List(t => t.RadianContributorId == radianContributorId && !t.Deleted);
            radianContributorOperationWithSoftware.Softwares = radianContributorOperationWithSoftware.RadianContributorOperations.Select(t => t.Software).ToList();
            return radianContributorOperationWithSoftware;
        }

        public RadianTestSetResult RadianTestSetResultByNit(string nit)
        {
            return _radianTestSetResultService.GetTestSetResultByNit(nit).FirstOrDefault();
        }

        /// <summary>
        /// Metodo encargado de filtrar los software disponibles de acuerdo el modo de seleccion.
        /// </summary>
        /// <param name="contributorId"></param>
        /// <param name="contributorTypeId"></param>
        /// <param name="operationMode"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public List<RadianSoftware> SoftwareList(int radianContributorId, RadianSoftwareStatus softwareStatus)
        {

            List<RadianContributor> participants;
            int softStatus = (int)softwareStatus;
            if (softwareStatus == 0)
                participants = _radianContributorRepository.List(t => t.Id == radianContributorId && t.RadianSoftwares.Any(x => x.Status)).Results;
            else
                participants = _radianContributorRepository.List(t => t.Id == radianContributorId && t.RadianSoftwares.Any(x => x.Status && x.RadianSoftwareStatusId == softStatus)).Results;
            return participants.Select(t => t.RadianSoftwares).Aggregate(new List<RadianSoftware>(), (list, source) =>
            {
                if (softwareStatus == 0)
                    list.AddRange(source);
                else
                    list.AddRange(source.Where(t => t.RadianSoftwareStatusId == softStatus));

                return list;
            }).Distinct().ToList();
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
            List<RadianContributor> participants;
            if (softwareType == RadianOperationModeTestSet.OwnSoftware)
                participants = _radianContributorRepository.List(t => t.ContributorId == contributorId && t.RadianContributorTypeId == contributorTypeId && t.Contributor.BusinessName.Contains(term)).Results;
            else
            {
                string radianState = RadianState.Habilitado.GetDescription();
                participants = _radianContributorRepository.List(t => t.RadianState == radianState && t.RadianContributorTypeId == (int)softwareType && t.Contributor.BusinessName.Contains(term)).Results;

            }
            return participants.Distinct().ToList();
        }

        public PagedResult<RadianCustomerList> CustormerList(int radianContributorId, string code, RadianState radianState, int page, int pagesize)
        {
            string radianStateText = radianState != RadianState.none ? radianState.GetDescription() : string.Empty;
            PagedResult<RadianCustomerList> customers = _radianContributorRepository.CustomerList(radianContributorId, code, radianStateText, page, pagesize);
            return customers;
        }

        public PagedResult<RadianContributorFileHistory> FileHistoryFilter(string fileName, string initial, string end, int page, int pagesize)
        {
            DateTime initialDate, endDate;
            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(initial) && DateTime.TryParse(initial, out initialDate) && !string.IsNullOrEmpty(end) && DateTime.TryParse(end, out endDate))
                return _radianContributorFileHistoryRepository.List(t => t.FileName.Contains(fileName) && t.Timestamp >= initialDate.Date && t.Timestamp <= endDate.Date, page, pagesize);

            if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(initial) && DateTime.TryParse(initial, out initialDate) && !string.IsNullOrEmpty(end) && DateTime.TryParse(end, out endDate))
                return _radianContributorFileHistoryRepository.List(t => t.Timestamp >= initialDate.Date && t.Timestamp <= endDate.Date, page, pagesize);

            if (!string.IsNullOrEmpty(fileName))
                return _radianContributorFileHistoryRepository.List(t => t.FileName.Contains(fileName), page, pagesize);

            return _radianContributorFileHistoryRepository.List(t => true, page, pagesize);
        }

        public void DeleteSoftware(Guid softwareId)
        {
            _radianCallSoftwareService.DeleteSoftware(softwareId);
        }

        public List<RadianContributorOperation> OperationsBySoftwareId(Guid id)
        {
            return _radianContributorOperationRepository.List(t => t.SoftwareId == id);
        }
    }
}
