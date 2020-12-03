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
        private readonly IContributorService _contributorService;
        private readonly IRadianContributorService _radianContributorService;
        private readonly IRadianContributorFileTypeService _radianContributorFileTypeService;
        private readonly IRadianContributorOperationRepository _radianContributorOperationRepository;
        private readonly IRadianContributorFileRepository _radianContributorFileRepository;
        private readonly IRadianContributorFileHistoryRepository _radianContributorFileHistoryRepository;
        private readonly IContributorOperationsService _contributorOperationsService;
        private readonly IRadianTestSetResultService _radianTestSetResultService;
        private readonly IRadianCallSoftwareService _radianCallSoftwareService;

        public RadianAprovedService(IRadianContributorRepository radianContributorRepository,
                                    IRadianTestSetService radianTestSetService,
                                    IRadianContributorService radianContributorService,
                                    IRadianContributorFileTypeService radianContributorFileTypeService,
                                    IRadianContributorOperationRepository radianContributorOperationRepository,
                                    IRadianContributorFileRepository radianContributorFileRepository,
                                    IRadianContributorFileHistoryRepository radianContributorFileHistoryRepository,
                                    IContributorOperationsService contributorOperationsService,
                                    IRadianTestSetResultService radianTestSetResultService, IContributorService contributorService, IRadianCallSoftwareService radianCallSoftwareService)
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
            _contributorService = contributorService;
            _radianCallSoftwareService = radianCallSoftwareService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radianContributorId"></param>
        /// <param name="softwareId"></param>
        /// <returns></returns>
        public Tuple<string, string> FindNamesContributorAndSoftware(int radianContributorId, string softwareId)
        {
            string radianContributorName = "No se encontró el contribuyente";
            string softwareName = "No hay software asociado al contribuyente";

            RadianContributor radianContributor = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId);

            if (radianContributor != null)
            {
                radianContributorName = radianContributor.Contributor.Name;
                Software software = radianContributor
                    .Contributor
                    .Softwares
                    .FirstOrDefault(s => s.Id.ToString() == softwareId);

                if (software != null)
                {
                    softwareName = software.Name;
                }
            }

            Tuple<string, string> data = Tuple.Create(radianContributorName, softwareName);

            return data;
        }

        public List<RadianContributor> ListContributorByType(int radianContributorTypeId)
        {
            return _radianContributorRepository.List(t => t.RadianContributorTypeId == radianContributorTypeId).Results;
        }

        // Manquip
        public List<Software> ListSoftwareByContributor(int radianContributorId)
        {
            List<Software> softwares = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId)
                .Contributor
                .Softwares.ToList();

            return softwares;
        }


        public RadianContributor GetRadianContributor(int radianContributorId)
        {
            RadianContributor radianContributor = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId);

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
            return _radianContributorService.ContributorSummary(contributorId, radianContributorType);
        }

        public Software SoftwareByContributor(int contributorId)
        {
            List<ContributorOperations> contributorOperations = _contributorOperationsService
                .GetContributorOperations(contributorId);

            if (contributorOperations == null)
                return default;

            return contributorOperations.FirstOrDefault(t => !t.Deleted && t.OperationModeId == (int)Domain.Common.OperationMode.Own && t.Software != null && t.Software.Status)?.Software ?? default;
        }

        public List<RadianContributorFileType> ContributorFileTypeList(int radianContributorTypeId)
        {
            List<RadianContributorFileType> contributorTypeList = _radianContributorFileTypeService.FileTypeList()
                .Where(ft => ft.RadianContributorTypeId == radianContributorTypeId && !ft.Deleted).ToList();

            return contributorTypeList;
        }

        public ResponseMessage OperationDelete(int radianContributorOperationId)
        {
            RadianContributorOperation operation = _radianContributorOperationRepository.Get(t => t.Id == radianContributorOperationId);
            RadianSoftware software = _radianCallSoftwareService.Get(operation.SoftwareId);
            if (software.RadianSoftwareStatusId == (int)RadianSoftwareStatus.Accepted && operation.SoftwareType == (int)RadianOperationModeTestSet.OwnSoftware)
                return new ResponseMessage() { Message = "El software ya se encuentra en uso" };

            if (operation.SoftwareType == (int)RadianOperationModeTestSet.OwnSoftware)
                _ = _radianCallSoftwareService.DeleteSoftware(software.Id);

            return _radianContributorOperationRepository.Update(radianContributorOperationId);

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

        public int AddRadianContributorOperation(RadianContributorOperation radianContributorOperation, string url, string softwareName, string pin, string createdBy)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(softwareName))
                radianContributorOperation.SoftwareId = _radianCallSoftwareService.CreateSoftware(radianContributorOperation.RadianContributorId, softwareName, url, pin, createdBy);

            RadianContributorOperation existingsoft = _radianContributorOperationRepository.Get(t => t.RadianContributorId == radianContributorOperation.RadianContributorId && t.SoftwareId == radianContributorOperation.SoftwareId && !t.Deleted);
            if (existingsoft == null)
                result = _radianContributorOperationRepository.Add(radianContributorOperation);

            if (result > 0)
            {
                RadianContributor radianContributor = _radianContributorRepository.Get(t => t.Id == radianContributorOperation.RadianContributorId);
                RadianTestSet testSet = _radianTestSetService.GetTestSet(radianContributor.RadianContributorTypeId.ToString(), radianContributor.RadianContributorTypeId.ToString());
                if (testSet != null)
                {
                    Contributor contributor = radianContributor.Contributor;
                    string key = radianContributor.RadianContributorTypeId.ToString() + "|" + radianContributorOperation.SoftwareId;
                    RadianTestSetResult setResult = new RadianTestSetResult(contributor.Code, key)
                    {
                        TotalDocumentRequired = testSet.TotalDocumentAcceptedRequired,
                        ReceiptNoticeTotalRequired = testSet.ReceiptNoticeTotalAcceptedRequired,
                        ReceiptServiceTotalRequired = testSet.ReceiptServiceTotalAcceptedRequired,
                        ExpressAcceptanceTotalRequired = testSet.ExpressAcceptanceTotalAcceptedRequired,
                        AutomaticAcceptanceTotalRequired = testSet.AutomaticAcceptanceTotalAcceptedRequired,
                        RejectInvoiceTotalRequired = testSet.RejectInvoiceTotalAcceptedRequired,
                        ApplicationAvailableTotalRequired = testSet.ApplicationAvailableTotalRequired,
                        ApplicationAvailableTotalAcceptedRequired = testSet.ApplicationAvailableTotalAcceptedRequired,
                        EndorsementTotalRequired = testSet.EndorsementTotalRequired,
                        EndorsementTotalAcceptedRequired = testSet.EndorsementTotalAcceptedRequired,
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
            }

            return result;
        }

        public RadianContributorOperationWithSoftware ListRadianContributorOperations(int radianContributorId)
        {
            RadianContributorOperationWithSoftware radianContributorOperationWithSoftware = new RadianContributorOperationWithSoftware();
            radianContributorOperationWithSoftware.RadianContributorOperations = _radianContributorOperationRepository.List(t => t.RadianContributorId == radianContributorId && t.Deleted == false);
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
        public List<RadianSoftware> SoftwareList(int radianContributorId)
        {
            List<RadianContributor> participants;
            int softwareStatus = (int)RadianSoftwareStatus.Accepted;
            participants = _radianContributorRepository.List(t => t.Id == radianContributorId && t.RadianSoftwares.Any(x => x.Status && x.RadianSoftwareStatusId == softwareStatus)).Results;
            return participants.Select(t => t.RadianSoftwares).Aggregate(new List<RadianSoftware>(), (list, source) =>
            {
                list.AddRange(source.Where(t => t.RadianSoftwareStatusId == softwareStatus));
                return list;
            }).Distinct().ToList();
        }

        public RadianSoftware GetSoftware(Guid id)
        {
            return _radianCallSoftwareService.Get(id);
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

    }
}
