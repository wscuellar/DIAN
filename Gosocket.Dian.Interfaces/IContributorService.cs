using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces
{
    public interface IContributorService
    {
        void Activate(Contributor contributor);
        void AddContributorFileHistory(ContributorFileHistory contributorFileHistory);
        int AddOrUpdate(Contributor contributor);
        void AddOrUpdateConfiguration(Contributor contributor);
        bool AddOrUpdateContributorFile(ContributorFile contributorFile);
        void AddUserContributor(UserContributors userContributor);
        Contributor Get(int id);
        Contributor Get(int id, string connectionString);
        IEnumerable<AcceptanceStatus> GetAcceptanceStatuses();
        List<Contributor> GetBillerContributors(int page, int length);
        Contributor GetByCode(string code);
        Contributor GetByCode(string code, int type);
        Contributor GetByCode(string code, string connectionString);
        List<Contributor> GetByCodes(string[] codes);
        object GetContributorByEmail(string email);
        ContributorFile GetContributorFile(Guid id);
        IEnumerable<ContributorFileHistory> GetContributorFileHistories(Guid id);
        Contributor GetContributorFiles(int id);

        List<OtherDocElecContributor> GetOtherDocElecContributorPermisos(int OtherDocContributorId);
        List<ContributorFileStatus> GetContributorFileStatuses();
        IEnumerable<Contributor> GetContributors(int contributorTypeId);
        IEnumerable<Contributor> GetContributors(int contributorTypeId, int statusId);
        List<Contributor> GetContributors(int type, int page, int length);
        List<Contributor> GetContributors(string code, int status, int page, int length, int? contributorType);
        List<Contributor> GetContributorsByAcceptanceStatusesId(int[] statuses, string connectionString = null);
        List<Contributor> GetContributorsByAcceptanceStatusId(int status);
        IEnumerable<Contributor> GetContributorsByIds(List<int> ids);
        List<Contributor> GetContributorsByType(int contributorType);
        int GetCountContributorsByAcceptanceStatusId(int status);
        List<ContributorFileType> GetMandatoryContributorFileTypes();
        List<ContributorFileType> GetNotRequiredContributorFileTypes();
        OperationMode GetOperationMode(int id);
        List<Contributor> GetParticipantContributors(int page, int length);
        List<Contributor> GetProviderContributors(int page, int length);
        List<UserContributors> GetUserContributors(int id);
        Contributor ObsoleteGet(int id);
        void RemoveUserContributor(UserContributors userContributors);
        void SetHabilitationAndProductionDates(Contributor contributor);
        void SetHabilitationAndProductionDates(Contributor contributor, string connectionString = null);
        void SetToEnabled(Contributor contributor);
        /// <summary>
        /// Retornar la lista de Modos de Operación
        /// </summary>
        /// <returns></returns>
        List<OperationMode> GetOperationModes();

        List<Software> GetBaseSoftwareForRadian(int contributorid);

        Contributor GetContributorById(int Id,int contributorTypeId);
    }
}