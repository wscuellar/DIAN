using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Text;

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

        public NameValueCollection Summary(string userCode)
        {
            NameValueCollection collection = new NameValueCollection();
            Domain.Contributor contributor = _contributorService.GetByCode(userCode);
            List<RadianContributor> radianContributors = _radianContributorRepository.List(t => t.ContributorId == contributor.Id && t.RadianState != "Cancelado").Results;
            if (radianContributors.Any())
                foreach (var radianContributor in radianContributors)
                {
                    string key = Enum.GetName(typeof(Domain.Common.RadianContributorType), radianContributor.RadianContributorTypeId);
                    collection.Add(key + "_RadianContributorTypeId", radianContributor.RadianContributorTypeId.ToString());
                    collection.Add(key + "_RadianOperationModeId", radianContributor.RadianOperationModeId.ToString());
                }
            if (contributor == null) return collection;
            collection.Add("ContributorId", contributor.Id.ToString());
            collection.Add("ContributorTypeId", contributor.ContributorTypeId.ToString());
            collection.Add("Active", contributor.Status.ToString());
            return collection;
        }

        public ResponseMessage RegistrationValidation(string userCode, Domain.Common.RadianContributorType radianContributorType, Domain.Common.RadianOperationMode radianOperationMode)
        {
            Contributor contributor = _contributorService.GetByCode(userCode);

            bool indirectElectronicBiller = radianContributorType == Domain.Common.RadianContributorType.ElectronicInvoice && radianOperationMode == Domain.Common.RadianOperationMode.Indirect;
            Software ownSoftware = _contributorService.GetBaseSoftwareForRadian(contributor.Id);
            if (!indirectElectronicBiller && ownSoftware == null)

                return new ResponseMessage(TextResources.ParticipantWithoutSoftware, TextResources.alertType);

            if (radianOperationMode == Domain.Common.RadianOperationMode.Direct)
            {
                bool otherActiveProcess = _radianContributorRepository.GetParticipantWithActiveProcess(contributor.Id, (int)radianContributorType);
                if (otherActiveProcess)
                    return new ResponseMessage(TextResources.OnlyActiveProcess, TextResources.alertType);
            }

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
            PagedResult<RadianContributor> radianContributors = _radianContributorRepository.List(t => t.RadianState != cancelState, page, size);
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


        public RadianAdmin ListParticipantsFilter(AdminRadianFilter filter, int page, int size)
        {
            string cancelState = RadianState.Cancelado.GetDescription();
            string stateDescriptionFilter = filter.RadianState == null ? string.Empty : filter.RadianState;
            DateTime? startDate = string.IsNullOrEmpty(filter.StartDate) ? null : (DateTime?)Convert.ToDateTime(filter.StartDate).Date;
            DateTime? endDate = string.IsNullOrEmpty(filter.EndDate) ? null : (DateTime?)Convert.ToDateTime(filter.EndDate).Date;

            var radianContributors = _radianContributorRepository.List(t => (t.Contributor.Code == filter.Code || filter.Code == null) &&
                                                                             (t.RadianContributorTypeId == filter.Type || filter.Type == 0) &&
                                                                             ((filter.RadianState == null && t.RadianState != cancelState) || t.RadianState == stateDescriptionFilter) &&
                                                                             (DbFunctions.TruncateTime(t.CreatedDate) >= startDate || !startDate.HasValue) &&
                                                                             (DbFunctions.TruncateTime(t.CreatedDate) <= endDate || !endDate.HasValue),
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
            List<RadianContributor> radianContributors;

            if (radianContributorType != 0)
                radianContributors = _radianContributorRepository.List(t => t.ContributorId == contributorId && t.RadianContributorTypeId == radianContributorType).Results;
            else
                radianContributors = _radianContributorRepository.List(t => t.Id == contributorId).Results;


            RadianAdmin radianAdmin = null;

            radianContributors.ForEach(c =>
            {

                List<RadianTestSetResult> testSet = _radianTestSetResultManager.GetAllTestSetResultByContributor(c.Id).ToList();
                foreach (var item in testSet)
                {
                    string[] parts = item.RowKey.Split('|');
                    item.SoftwareId = parts[1];
                    item.OperationModeName = Domain.Common.EnumHelper.GetEnumDescription(Enum.Parse(typeof(RadianOperationModeTestSet), parts[0]));
                }

                List<string> userIds = _contributorService.GetUserContributors(c.Contributor.Id).Select(u => u.UserId).ToList();

                List<RadianContributorFileType> fileTypes = _radianContributorFileTypeRepository.List(t => t.RadianContributorTypeId == c.RadianContributorTypeId && !t.Deleted);
                List<RadianContributorFile> newFiles = (from t in fileTypes
                                                        join f in c.RadianContributorFile.Where(t => !t.Deleted) on t.Id equals f.FileType into files
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
                        RadianOperationModeId = c.RadianOperationModeId
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
            List<RadianContributor> contributors = _radianContributorRepository.List(t => t.ContributorId == contributorId && t.RadianContributorTypeId == radianContributorTypeId && t.RadianState == actualState).Results;
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

            UpdateGlobalRadianOperation(radianContributorTypeId, competitor);

            _radianContributorRepository.AddOrUpdate(competitor);

            return true;

        }


        #region Private methods Cancel Participant

        private void UpdateGlobalRadianOperation(int radianContributorTypeId, RadianContributor competitor)
        {
            List<GlobalRadianOperations> radianOperations = _globalRadianOperationService.OperationList(competitor.Contributor.Code);
            if (radianOperations.Any())
            {
                List<GlobalRadianOperations> operations = radianOperations.Where(t => t.RadianContributorTypeId == radianContributorTypeId).ToList();
                if (competitor.RadianState == RadianState.Test.GetDescription())
                {
                    GlobalRadianOperations operation = radianOperations.OrderByDescending(t => t.Timestamp).FirstOrDefault(t => t.RadianContributorTypeId == radianContributorTypeId);
                    operation.Deleted = false;
                    operation.RadianStatus = competitor.RadianState;
                    _globalRadianOperationService.Update(operation);
                }
                else
                {
                    foreach (GlobalRadianOperations operation in operations)
                    {
                        operation.Deleted = true;
                        operation.RadianStatus = competitor.RadianState;
                        _globalRadianOperationService.Update(operation);
                    }
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

            List<RadianSoftware> softwares = _radianCallSoftwareService.List(competitor.Id);
            foreach (RadianSoftware software in softwares)
            {
                //Quita los software
                _radianCallSoftwareService.DeleteSoftware(software.Id);

                //Quitar la operacion.
                List<RadianContributorOperation> operations = _radianContributorOperationRepository.List(t => t.SoftwareId == software.Id && !t.Deleted);
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
            List<RadianContributorOperation> operations = _radianContributorOperationRepository.List(t => t.RadianContributorId == radiancontributorId && !t.Deleted && t.SoftwareType == softwareType && t.OperationStatusId == 1);
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
                Software ownSoftware = _contributorService.GetBaseSoftwareForRadian(contributorId);
                RadianSoftware radianSoftware = new RadianSoftware(ownSoftware, newRadianContributor.Id, createdBy);
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
            string idHistoryRegister = string.Empty;

            radianFileHistory.Id = Guid.NewGuid();
            idHistoryRegister = _radianContributorFileHistoryRepository.AddRegisterHistory(radianFileHistory).ToString();

            if (!string.IsNullOrEmpty(idHistoryRegister))
            {
                return new ResponseMessage($"Información registrada id: {idHistoryRegister}", "Guardado");
            }

            return new ResponseMessage($"El registro no pudo ser guardado", "Nulo");
        }

        public string GetAssociatedClients(int radianContributorId)
        {
            List<RadianCustomerList> customerLists = _radianContributorRepository.CustomerList(radianContributorId, string.Empty, string.Empty).Results;
            if (!customerLists.Any())
                return string.Empty;
            StringBuilder message = new StringBuilder();
            message.AppendFormat("<div class='htmlalert'><p>{0}</p>", TextResources.WithCustomerList);
            message.Append("<ul>");
            foreach (RadianCustomerList customer in customerLists)
            {
                message.AppendFormat("<li>{0}</li>", customer.Nit + "-" + customer.BussinessName);
            }
            message.Append("</ul></div>");
            return message.ToString();
        }



        public RadianTestSetResult GetSetTestResult(string code, string softwareId, string softwareType)
        {
            RadianOperationModeTestSet softwareTypeEnum = EnumHelper.GetValueFromDescription<RadianOperationModeTestSet>(softwareType);
            string key = ((int)softwareTypeEnum).ToString() + "|" + softwareId;
            return _radianTestSetResultManager.GetTestSetResult(code, key);
        }
    }
}
