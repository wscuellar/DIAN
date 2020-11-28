﻿using Gosocket.Dian.Common.Resources;
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

namespace Gosocket.Dian.Application
{
    public class RadianContributorService : IRadianContributorService
    {
        private readonly IContributorService _contributorService;
        private readonly IRadianContributorRepository _radianContributorRepository;
        private readonly IRadianContributorTypeRepository _radianContributorTypeRepository;
        private readonly IRadianContributorFileRepository _radianContributorFileRepository;
        private readonly IRadianTestSetResultManager _radianTestSetResultManager;
        private readonly IRadianOperationModeRepository _radianOperationModeRepository;
        private readonly IRadianContributorFileHistoryRepository _radianContributorFileHistoryRepository;
        private readonly IRadianSoftwareRepository _radianSoftwareRepository;

        public RadianContributorService(IContributorService contributorService,
            IRadianContributorRepository radianContributorRepository, 
            IRadianContributorTypeRepository radianContributorTypeRepository, 
            IRadianContributorFileRepository radianContributorFileRepository, 
            IRadianTestSetResultManager radianTestSetResultManager, 
            IRadianOperationModeRepository radianOperationModeRepository, 
            IRadianContributorFileHistoryRepository radianContributorFileHistoryRepository,
            IRadianSoftwareRepository radianSoftwareRepository)
        {
            _contributorService = contributorService;
            _radianContributorRepository = radianContributorRepository;
            _radianContributorTypeRepository = radianContributorTypeRepository;
            _radianContributorFileRepository = radianContributorFileRepository;
            _radianTestSetResultManager = radianTestSetResultManager;
            _radianOperationModeRepository = radianOperationModeRepository;
            _radianContributorFileHistoryRepository = radianContributorFileHistoryRepository;
            _radianSoftwareRepository = radianSoftwareRepository;
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
            if (contributor == null || contributor.AcceptanceStatusId != 4)
                return new ResponseMessage(TextResources.NonExistentParticipant, TextResources.alertType);

            bool indirectElectronicBiller = radianContributorType == Domain.Common.RadianContributorType.ElectronicInvoice && radianOperationMode == Domain.Common.RadianOperationMode.Indirect;
            if (!indirectElectronicBiller)
            {
                Software ownSoftware = _contributorService.GetBaseSoftwareForRadian(contributor.Id);
                if (ownSoftware == null)
                    return new ResponseMessage(TextResources.ParticipantWithoutSoftware, TextResources.alertType);
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
            string cancelState = Domain.Common.RadianState.Cancelado.GetDescription();
            PagedResult<RadianContributor> radianContributors = _radianContributorRepository.List(t => t.RadianState != cancelState, page, size);
            List<Domain.RadianContributorType> radianContributorType = _radianContributorTypeRepository.List(t => true);
            RadianAdmin radianAdmin = new RadianAdmin()
            {
                Contributors = radianContributors.Results.Select(c =>
               new RedianContributorWithTypes()
               {
                   Id = c.Contributor.Id,
                   Code = c.Contributor.Code,
                   TradeName = c.Contributor.Name,
                   BusinessName = c.Contributor.BusinessName,
                   AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name
               }).ToList(),
                Types = radianContributorType,
                RowCount = radianContributors.RowCount,
                CurrentPage = radianContributors.CurrentPage
            };
            return radianAdmin;
        }


        public RadianAdmin ListParticipantsFilter(AdminRadianFilter filter, int page, int size)
        {
            string cancelState = Domain.Common.RadianState.Cancelado.GetDescription();
            string stateDescriptionFilter = filter.RadianState == null ? string.Empty : filter.RadianState.GetDescription();
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
                   Id = c.Contributor.Id,
                   Code = c.Contributor.Code,
                   TradeName = c.Contributor.Name,
                   BusinessName = c.Contributor.BusinessName,
                   AcceptanceStatusName = c.Contributor.AcceptanceStatus.Name
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

            if(radianContributorType!=0)
                radianContributors = _radianContributorRepository.List(t => t.ContributorId == contributorId && t.RadianContributorTypeId == radianContributorType).Results;
            else
                radianContributors = _radianContributorRepository.List(t => t.ContributorId == contributorId).Results;

            List<RadianTestSetResult> testSet = _radianTestSetResultManager.GetAllTestSetResultByContributor(contributorId).ToList();
            List<string> userIds = _contributorService.GetUserContributors(contributorId).Select(u => u.UserId).ToList();

            RadianAdmin radianAdmin = null;

            radianContributors.ForEach(c =>
            {
                radianAdmin = new RadianAdmin()
                {
                    Contributor = new RedianContributorWithTypes()
                    {
                        RadianContributorId =  c.Id,
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
                    Files = c.RadianContributorFile.ToList(),
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

            if (contributors.Any())
            {
                var radianContributor = contributors.FirstOrDefault();

                if (newState != "")
                    radianContributor.RadianState = newState == "0" ? RadianState.Test.GetDescription() : RadianState.Cancelado.GetDescription();
                radianContributor.Description = description;

                _radianContributorRepository.AddOrUpdate(radianContributor);
                return true;
            }

            return false;
        }

        public bool ChangeContributorStep(int radianContributorId, int step)
        {
            RadianContributor radianContributor = _radianContributorRepository.Get(t => t.Id == radianContributorId);

            if (radianContributor != null)
            {
                radianContributor.Step = step;

                _radianContributorRepository.AddOrUpdate(radianContributor);
                return true;
            }

            return false;
        }

        public Guid UpdateRadianContributorFile(RadianContributorFile radianContributorFile)
        {
            return _radianContributorFileRepository.Update(radianContributorFile);
        }

        public void CreateContributor(int contributorId, RadianState radianState, Domain.Common.RadianContributorType radianContributorType, Domain.Common.RadianOperationMode radianOperationMode, string createdBy)
        {
            RadianContributor existing = _radianContributorRepository.Get(t => t.ContributorId == contributorId && t.RadianContributorTypeId == (int)radianContributorType);

            RadianContributor newRadianContributor = new Domain.RadianContributor()
            {
                Id = existing != null ? existing.Id : 0,
                ContributorId = contributorId,
                CreatedBy = createdBy,
                RadianContributorTypeId = (int)radianContributorType,
                RadianOperationModeId = (int)radianOperationMode,
                RadianState = radianState.GetDescription(),
                CreatedDate = existing != null ? existing.CreatedDate : System.DateTime.Now
            };
            int id = _radianContributorRepository.AddOrUpdate(newRadianContributor);
            if(radianOperationMode == Domain.Common.RadianOperationMode.Direct)
            {
                Software ownSoftware = _contributorService.GetBaseSoftwareForRadian(contributorId);
                RadianSoftware radianSoftware = new RadianSoftware(ownSoftware,id,createdBy);
                _radianSoftwareRepository.AddOrUpdate(radianSoftware);
                //se hereda el software anterior a radian.
            }
            

            newRadianContributor.Id = id;
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
    }
}
