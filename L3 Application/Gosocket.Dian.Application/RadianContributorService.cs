using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
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
    public class RadianContributorService : IRadianContributorService
    {
        private readonly IContributorService _contributorService;
        private readonly IRadianContributorRepository _radianContributorRepository;
        private readonly IRadianContributorTypeRepository _radianContributorTypeRepository;
        private readonly IRadianContributorFileRepository _radianContributorFileRepository;
        private readonly IRadianContributorFileTypeRepository _radianContributorFileTypeRepository;
        private readonly IRadianContributorOperationRepository _radianContributorOperationRepository;
        private readonly IRadianCallSoftwareService _radianCallSoftwareService;
        private readonly IRadianTestSetResultManager _radianTestSetResultManager;
        private readonly IRadianOperationModeRepository _radianOperationModeRepository;
        private readonly IRadianContributorFileHistoryRepository _radianContributorFileHistoryRepository;
        private readonly IGlobalRadianOperationService _globalRadianOperationService;

        public RadianContributorService(IContributorService contributorService,
            IRadianContributorRepository radianContributorRepository,
            IRadianContributorTypeRepository radianContributorTypeRepository,
            IRadianContributorFileRepository radianContributorFileRepository,
            IRadianContributorOperationRepository radianContributorOperationRepository,
            IRadianTestSetResultManager radianTestSetResultManager,
            IRadianOperationModeRepository radianOperationModeRepository,
            IRadianContributorFileHistoryRepository radianContributorFileHistoryRepository,
            IGlobalRadianOperationService globalRadianOperationService,
            IRadianContributorFileTypeRepository radianContributorFileTypeRepository,
            IRadianCallSoftwareService radianCallSoftwareService)
        {
            _contributorService = contributorService;
            _radianContributorRepository = radianContributorRepository;
            _radianContributorTypeRepository = radianContributorTypeRepository;
            _radianContributorFileRepository = radianContributorFileRepository;
            _radianTestSetResultManager = radianTestSetResultManager;
            _radianOperationModeRepository = radianOperationModeRepository;
            _radianContributorFileHistoryRepository = radianContributorFileHistoryRepository;
            _globalRadianOperationService = globalRadianOperationService;
            _radianContributorFileTypeRepository = radianContributorFileTypeRepository;
            _radianCallSoftwareService = radianCallSoftwareService;
            _radianContributorOperationRepository = radianContributorOperationRepository;
        }

        #region Registro de participantes
        public Domain.Contributor GetContributor(int contributorId)
        {
            return _contributorService.Get(contributorId);
        }

        public NameValueCollection Summary(int contributorId)
        {
            NameValueCollection collection = new NameValueCollection();
            Domain.Contributor contributor = _contributorService.Get(contributorId);
            if (contributor == null) return collection;
            List<RadianContributor> radianContributors = _radianContributorRepository.List(t => t.ContributorId == contributor.Id && t.RadianState != "Cancelado").Results;
            if (radianContributors.Any())
                foreach (var radianContributor in radianContributors)
                {
                    string key = Enum.GetName(typeof(Domain.Common.RadianContributorType), radianContributor.RadianContributorTypeId);
                    collection.Add(key + "_RadianContributorTypeId", radianContributor.RadianContributorTypeId.ToString());
                    collection.Add(key + "_RadianOperationModeId", radianContributor.RadianOperationModeId.ToString());
                }
            collection.Add("ContributorId", contributor.Id.ToString());
            collection.Add("ContributorTypeId", contributor.ContributorTypeId.ToString());
            collection.Add("Active", contributor.Status.ToString());
            return collection;
        }

        public ResponseMessage RegistrationValidation(int contributorId, Domain.Common.RadianContributorType radianContributorType, Domain.Common.RadianOperationMode radianOperationMode)
        {
            Contributor contributor = _contributorService.Get(contributorId);
            bool indirectElectronicBiller = radianContributorType == Domain.Common.RadianContributorType.ElectronicInvoice && radianOperationMode == Domain.Common.RadianOperationMode.Indirect;

            if (!indirectElectronicBiller && GetSoftwareOwn(contributor.Id) == null)
                return new ResponseMessage(TextResources.ParticipantWithoutSoftware, TextResources.alertType);

            bool otherActiveProcess = _radianContributorRepository.GetParticipantWithActiveProcess(contributor.Id);
            if (otherActiveProcess)
                return new ResponseMessage(TextResources.OperationFailOtherInProcess, TextResources.alertType);

            string cancelEvent = RadianState.Cancelado.GetDescription();
            int radianType = (int)radianContributorType;
            RadianContributor record = _radianContributorRepository.Get(t => t.ContributorId == contributor.Id && t.RadianContributorTypeId == radianType);
            if (record != null && record.RadianState != cancelEvent)
                return new ResponseMessage(TextResources.RegisteredParticipant, TextResources.redirectType);

            if (radianContributorType == Domain.Common.RadianContributorType.TechnologyProvider && (contributor.ContributorTypeId != (int)Domain.Common.ContributorType.Provider || !contributor.Status))
                return new ResponseMessage(TextResources.TechnologProviderDisabled, TextResources.alertType);

            if (radianContributorType == Domain.Common.RadianContributorType.ElectronicInvoice)
                return new ResponseMessage(TextResources.ElectronicInvoice_Confirm, TextResources.confirmType);

            if (radianContributorType == Domain.Common.RadianContributorType.TechnologyProvider)
                return new ResponseMessage(TextResources.TechnologyProvider_Confirm, TextResources.confirmType);

            if (radianContributorType == Domain.Common.RadianContributorType.TradingSystem)
                return new ResponseMessage(TextResources.TradingSystem_Confirm, TextResources.confirmType);

            if (radianContributorType == Domain.Common.RadianContributorType.Factor)
                return new ResponseMessage(TextResources.Factor_Confirm, TextResources.confirmType);

            return new ResponseMessage(TextResources.FailedValidation, TextResources.alertType);
        }

        #endregion

        public RadianAdmin ListParticipants(int page, int size)
        {
            string cancelState = RadianState.Cancelado.GetDescription();
            PagedResult<RadianContributor> radianContributors = _radianContributorRepository.ListByDateDesc(t => t.RadianState != cancelState, page, size);
            List<Domain.RadianContributorType> radianContributorType = _radianContributorTypeRepository.List(t => true);
            RadianAdmin radianAdmin = new RadianAdmin()
            {
                Contributors = radianContributors.Results.Select(c =>
               new RedianContributorWithTypes()
               {
                   Id = c.Id,
                   Code = c.Contributor.Code,
                   TradeName = c.Contributor.Name,
                   BusinessName = c.Contributor.BusinessName,
                   AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name,
                   RadianState = c.RadianState,
                   RadianContributorId = c.Id,
                   CreatedDate = c.CreatedDate,
                   Update = c.Update
               }).ToList(),
                Types = radianContributorType,
                RowCount = radianContributors.RowCount,
                CurrentPage = radianContributors.CurrentPage
            };

            return radianAdmin;
        }


        public RadianAdmin ListParticipantsFilter(AdminRadianFilter filter, int page, int size)
        {
            string cancelState = filter.RadianState == null ? RadianState.Cancelado.GetDescription() : string.Empty;
            string stateDescriptionFilter = filter.RadianState == null ? string.Empty : filter.RadianState;
            DateTime? startDate = string.IsNullOrEmpty(filter.StartDate) ? null : (DateTime?)Convert.ToDateTime(filter.StartDate).Date;
            DateTime? endDate = string.IsNullOrEmpty(filter.EndDate) ? null : (DateTime?)Convert.ToDateTime(filter.EndDate).Date;

            PagedResult<RadianContributor> radianContributors = _radianContributorRepository.ListByDateDesc(t => (t.Contributor.Code == filter.Code || filter.Code == null) &&
                                                                             (t.RadianContributorTypeId == filter.Type || filter.Type == 0) &&
                                                                             ((filter.RadianState == null && t.RadianState != cancelState) || t.RadianState == stateDescriptionFilter) &&
                                                                             (DbFunctions.TruncateTime(t.Update) >= startDate || !startDate.HasValue) &&
                                                                             (DbFunctions.TruncateTime(t.Update) <= endDate || !endDate.HasValue),
            page, size);
            List<Domain.RadianContributorType> radianContributorType = _radianContributorTypeRepository.List(t => true);
            RadianAdmin radianAdmin = new RadianAdmin()
            {
                Contributors = radianContributors.Results.Select(c =>
               new RedianContributorWithTypes()
               {
                   Id = c.Id,
                   Code = c.Contributor.Code,
                   TradeName = c.Contributor.Name,
                   BusinessName = c.Contributor.BusinessName,
                   AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name,
                   RadianState = c.RadianState,
                   RadianContributorId = c.Id
               }).ToList(),
                Types = radianContributorType,
                RowCount = radianContributors.RowCount,
                CurrentPage = radianContributors.CurrentPage
            };
            return radianAdmin;
        }

        public RadianAdmin ContributorSummary(int contributorId, int radianContributorType = 0)
        {
            List<RadianContributor> radianContributors = (radianContributorType != 0) ?
                                _radianContributorRepository.List(t => t.ContributorId == contributorId && t.RadianContributorTypeId == radianContributorType).Results :
                                _radianContributorRepository.List(t => t.Id == contributorId).Results;

            RadianAdmin radianAdmin = null;
            radianContributors.ForEach(c =>
             {

                 List<RadianTestSetResult> testSet = _radianTestSetResultManager.GetAllTestSetResultByContributor(c.Id).ToList();
                 testSet = testSet.Where(t => t.ContributorTypeId == c.RadianContributorTypeId.ToString() && !t.Deleted).ToList();
                 string softwareTypeName = c.RadianOperationModeId == 1 ?
                                            RadianOperationModeTestSet.OwnSoftware.GetDescription() :
                                            Domain.Common.EnumHelper.GetEnumDescription(Enum.Parse(typeof(RadianOperationModeTestSet), c.RadianContributorTypeId.ToString()));
                 foreach (var item in testSet)
                 {
                     string[] parts = item.RowKey.Split('|');
                     item.OperationModeName = string.IsNullOrEmpty(item.OperationModeName) ? softwareTypeName : item.OperationModeName;
                     item.SoftwareId = parts[1];
                 }
                 List<string> userIds = _contributorService.GetUserContributors(c.Contributor.Id).Select(u => u.UserId).ToList();
                 List<RadianContributorFileType> fileTypes = _radianContributorFileTypeRepository.List(t => t.RadianContributorTypeId == c.RadianContributorTypeId && !t.Deleted);
                 List<RadianContributorFile> newFiles = (from t in fileTypes
                                                         join f in c.RadianContributorFile.Where(t => t.RadianContributorId == c.Id && !t.Deleted) on t.Id equals f.FileType into files
                                                         from fl in files.DefaultIfEmpty(new RadianContributorFile()
                                                         {
                                                             FileName = t.Name,
                                                             FileType = t.Id,
                                                             Status = 0,
                                                             RadianContributorFileType = t,
                                                             RadianContributorFileStatus = new RadianContributorFileStatus()
                                                             {
                                                                 Id = 0,
                                                                 Name = "Pendiente"
                                                             }
                                                         })
                                                         select fl).ToList();

                 radianAdmin = new RadianAdmin()
                 {
                     Contributor = new RedianContributorWithTypes()
                     {
                         RadianContributorId = c.Id,
                         Id = c.Contributor.Id,
                         Code = c.Contributor.Code,
                         TradeName = c.Contributor.Name,
                         BusinessName = c.Contributor.BusinessName,
                         AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name,
                         Email = c.Contributor.Email,
                         Update = c.Update,
                         RadianState = c.RadianState,
                         AcceptanceStatusId = c.Contributor.AcceptanceStatus.Id,
                         CreatedDate = c.CreatedDate,
                         Step = c.Step,
                         RadianContributorTypeId = c.RadianContributorTypeId,
                         RadianOperationModeId = c.RadianOperationModeId,
                         IsActive = c.IsActive                         
                     },
                     Files = newFiles,
                     FileTypes = fileTypes,
                     Tests = testSet,
                     LegalRepresentativeIds = userIds,
                     Type = c.RadianContributorType
                 };
             });

            return radianAdmin;
        }

        public bool ChangeParticipantStatus(int contributorId, string newState, int radianContributorTypeId, string actualState, string description)
        {
            List<RadianContributor> contributors = _radianContributorRepository.List(t => t.ContributorId == contributorId
                                                                                    && t.RadianContributorTypeId == radianContributorTypeId
                                                                                    && t.RadianState == actualState).Results;
            if (!contributors.Any())
                return false;

            RadianContributor competitor = contributors.FirstOrDefault();
            competitor.RadianState = newState;
            competitor.Description = description;
            if (newState == RadianState.Test.GetDescription()) competitor.Step = 3;
            if (newState == RadianState.Habilitado.GetDescription()) competitor.Step = 4;
            if (newState == RadianState.Cancelado.GetDescription()) competitor.Step = 1;

            if (competitor.RadianState == RadianState.Cancelado.GetDescription())
                CancelParticipant(competitor);

            UpdateGlobalRadianOperation(competitor);

            _radianContributorRepository.AddOrUpdate(competitor);

            return true;

        }


        #region Private methods Cancel Participant

        private void UpdateGlobalRadianOperation(RadianContributor competitor)
        {
            RadianContributorOperation radianOperation = competitor.RadianContributorOperations.FirstOrDefault(t => t.OperationStatusId == (int)Domain.Common.RadianState.Registrado || t.OperationStatusId == (int)Domain.Common.RadianState.Test);
            if (radianOperation == null)
                return;

            GlobalRadianOperations operation = _globalRadianOperationService.GetOperation(competitor.Contributor.Code, radianOperation.SoftwareId);
            if (operation != null)
            {
                if (competitor.RadianState == RadianState.Test.GetDescription())
                {
                    operation.Deleted = false;
                    operation.RadianState = competitor.RadianState;
                    _globalRadianOperationService.Update(operation);
                }
                else
                {
                    operation.Deleted = true;
                    operation.RadianState = competitor.RadianState;
                    _globalRadianOperationService.Update(operation);
                }
            }
        }

        private void CancelParticipant(RadianContributor competitor)
        {
            //Quita los archivos
            List<RadianContributorFile> files = _radianContributorFileRepository.List(t => t.RadianContributorId == competitor.Id);
            if (files.Any())
                foreach (RadianContributorFile file in files)
                {
                    file.Deleted = true;
                    file.Status = 3;
                    _radianContributorFileRepository.AddOrUpdate(file);

                    RadianContributorFileHistory radianFileHistory = new RadianContributorFileHistory();
                    radianFileHistory.Id = Guid.NewGuid();
                    radianFileHistory.Timestamp = System.DateTime.Now;
                    radianFileHistory.CreatedBy = file.CreatedBy;
                    radianFileHistory.FileName = file.FileName;
                    radianFileHistory.Comments = file.Comments;
                    radianFileHistory.CreatedBy = file.CreatedBy;
                    radianFileHistory.Status = file.Status;
                    radianFileHistory.RadianContributorFileId = file.Id;
                    _ = _radianContributorFileHistoryRepository.AddRegisterHistory(radianFileHistory);

                }

            foreach(var radianContributorOperations in competitor.RadianContributorOperations)
            {               

                //Quita los software
                _radianCallSoftwareService.DeleteSoftwareCancelaRegistro(radianContributorOperations.SoftwareId);

                //Quitar la operacion.
                List<RadianContributorOperation> operations = _radianContributorOperationRepository.List(t => t.SoftwareId == radianContributorOperations.SoftwareId 
                && t.Id == radianContributorOperations.Id && t.RadianContributorId == competitor.Id);
                foreach (RadianContributorOperation operation in operations)
                {
                    operation.OperationStatusId = (int)RadianState.Cancelado;
                    operation.Deleted = true;
                    _radianContributorOperationRepository.Update(operation);
                }

            }           
        }

        public void UpdateRadianOperation(int radiancontributorId, int softwareType)
        {
            List<RadianContributorOperation> operations = _radianContributorOperationRepository.List(t => t.RadianContributorId == radiancontributorId 
            && !t.Deleted && t.SoftwareType == softwareType && t.OperationStatusId == 1);
            foreach (RadianContributorOperation operation in operations)
            {
                operation.OperationStatusId = (int)RadianState.Test;
                _radianContributorOperationRepository.Update(operation);
            }
        }
        #endregion


        public bool ChangeContributorStep(int radianContributorId, int step)
        {
            RadianContributor radianContributor = _radianContributorRepository.Get(t => t.Id == radianContributorId);

            if (radianContributor == null)
                return false;

            radianContributor.Step = step;
            _radianContributorRepository.AddOrUpdate(radianContributor);
            return true;
        }

        public Guid UpdateRadianContributorFile(RadianContributorFile radianContributorFile)
        {
            return _radianContributorFileRepository.Update(radianContributorFile);
        }

        public RadianContributor CreateContributor(int contributorId, RadianState radianState, Domain.Common.RadianContributorType radianContributorType, Domain.Common.RadianOperationMode radianOperationMode, string createdBy)
        {
            RadianContributor existing = _radianContributorRepository.Get(t => t.ContributorId == contributorId && t.RadianContributorTypeId == (int)radianContributorType);

            RadianContributor newRadianContributor = new RadianContributor()
            {
                Id = existing != null ? existing.Id : 0,
                ContributorId = contributorId,
                CreatedBy = createdBy,
                RadianContributorTypeId = (int)radianContributorType,
                RadianOperationModeId = (int)radianOperationMode,
                RadianState = radianState.GetDescription(),
                CreatedDate = existing != null ? existing.CreatedDate : DateTime.Now
            };
            newRadianContributor.Id = _radianContributorRepository.AddOrUpdate(newRadianContributor);
            if (radianOperationMode == Domain.Common.RadianOperationMode.Direct)
            {
                Software ownSoftware = GetSoftwareOwn(contributorId);
                RadianSoftware radianSoftware = new RadianSoftware(ownSoftware, contributorId, createdBy);
                newRadianContributor.RadianSoftwares = new List<RadianSoftware>() { radianSoftware };
            }

            return newRadianContributor;
        }

        public List<RadianContributorFile> RadianContributorFileList(string id)
        {
            return _radianContributorFileRepository.List(t => t.Id.ToString() == id);
        }

        public Domain.RadianOperationMode GetOperationMode(int id)
        {
            return _radianOperationModeRepository.Get(t => t.Id == id);
        }

        public List<Domain.RadianOperationMode> OperationModeList()
        {
            return _radianOperationModeRepository.List(t => true);
        }

        public ResponseMessage AddFileHistory(RadianContributorFileHistory radianFileHistory)
        {
            radianFileHistory.Timestamp = DateTime.Now;
            radianFileHistory.Id = Guid.NewGuid();
            Guid idHistoryRegister = _radianContributorFileHistoryRepository.AddRegisterHistory(radianFileHistory);
            
            if (idHistoryRegister != Guid.Empty)
            {
                return new ResponseMessage($"Información registrada id: {idHistoryRegister}", "Guardado");
            }

            return new ResponseMessage($"El registro no pudo ser guardado", "Nulo");
        }

        public int GetAssociatedClients(int radianContributorId)
        {
            List<RadianCustomerList> customerLists = _radianContributorRepository.CustomerList(radianContributorId, string.Empty, string.Empty).Results;
            return customerLists.Count;
        }

        public RadianTestSetResult GetSetTestResult(string code, string softwareId, int type)
        {
            string key = type.ToString() + "|" + softwareId;
            return _radianTestSetResultManager.GetTestSetResult(code, key);
        }

        #region Private Methods

        /// <summary>
        /// Calcula el software propio heredado de catalogo
        /// </summary>
        /// <param name="contributorId">Identificador del contribuyente</param>
        /// <returns></returns>
        private Software GetSoftwareOwn(int contributorId)
        {
            List<Software> ownSoftwares = _contributorService.GetBaseSoftwareForRadian(contributorId);
            if (!ownSoftwares.Any())
                return null;
            List<string> softwares = ContributorSoftwareAcceptedList(contributorId);

            return (from os in ownSoftwares
                    join s in softwares on os.Id.ToString() equals s
                    select os).OrderByDescending(t => t.Timestamp).FirstOrDefault();
        }

        /// <summary>
        /// Listado de software que pasaron el set de pruebas en estado aceptado de catalogo
        /// </summary>
        /// <param name="contributorId">Identificador del contribuyente</param>
        /// <returns></returns>
        private List<string> ContributorSoftwareAcceptedList(int contributorId)
        {
            Contributor contributor = _contributorService.Get(contributorId);
            var contributorOperations = contributor.ContributorOperations.Where(o => !o.Deleted);
            var testSetResults = _radianTestSetResultManager.GetTestSetResulByCatalog(contributor.Code);

            List<string> softwareAccepted = new List<string>();
            foreach (var item in contributorOperations)
            {
                if (item.SoftwareId != null)
                {
                    GlobalTestSetResult testset = GetTestSetResult(testSetResults, item, contributor.ContributorTypeId.Value);
                    if (testset != null && ((TestSetStatus)testset.Status) == TestSetStatus.Accepted)
                        softwareAccepted.Add(testset.SoftwareId);
                }
            }

            return softwareAccepted;
        }

        /// <summary>
        /// Rutina heredada de catalogo para determinar su set de pruebas
        /// </summary>
        /// <param name="testSetResults">Listado de set de pruebas</param>
        /// <param name="operation">Operacion</param>
        /// <param name="ContributorTypeId">tipo de contribuyente</param>
        /// <returns>Set de prubas que uso el contribuyente para la operacion en cuestion.</returns>
        private GlobalTestSetResult GetTestSetResult(List<GlobalTestSetResult> testSetResults, ContributorOperations operation, int ContributorTypeId)
        {
            string softwareId = operation.SoftwareId.Value.ToString();
            GlobalTestSetResult testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{operation.ContributorTypeId}|{softwareId}" && t.OperationModeId == operation.OperationModeId);
            if (testSetResult != null)
                return testSetResult;

            testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{ContributorTypeId}|{softwareId}" && t.OperationModeId == operation.OperationModeId);

            if (testSetResult == null)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && t.RowKey == $"{(int)Domain.Common.ContributorType.Zero}|{operation.SoftwareId}" && t.OperationModeId == operation.OperationModeId);

            if (testSetResult == null && ContributorTypeId == (int)Domain.Common.ContributorType.Provider)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && int.Parse(t.ContributorTypeId) == (int)Domain.Common.ContributorType.Provider && t.SoftwareId == softwareId && t.OperationModeId == operation.OperationModeId);

            if (testSetResult == null && ContributorTypeId == (int)Domain.Common.ContributorType.Provider)
                testSetResult = testSetResults.FirstOrDefault(t => !t.Deleted && int.Parse(t.ContributorTypeId) == (int)Domain.Common.ContributorType.Biller && t.SoftwareId == softwareId && t.OperationModeId == operation.OperationModeId);

            return testSetResult;
        }

        public byte[] DownloadContributorFile(string code, string fileName, out string contentType)
        {
            string fileNameURL = code + "/" + StringTools.MakeValidFileName(fileName);
            //var fileManager = new FileManager(ConfigurationManager.GetValue("GlobalStorage"));
            var fileManager = new FileManager();
            return fileManager.GetBytes("radiancontributor-files", fileNameURL, out contentType);
        }

        public RadianContributor ChangeContributorActiveRequirement(int radianContributorId)
        {
            RadianContributor radianContributor = _radianContributorRepository.Get(t => t.Id == radianContributorId);

            if (radianContributor == null)
                return radianContributor;

            radianContributor.IsActive = radianContributor.IsActive ? false : true;
            _radianContributorRepository.AddOrUpdate(radianContributor);
            return radianContributor;
        }

        #endregion

    }
}
