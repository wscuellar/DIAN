using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianContributorService
    {
        /// <summary>
        /// Obtiene un participante por el id
        /// </summary>
        /// <param name="contributorId"></param>
        /// <returns></returns>
        Domain.Contributor GetContributor(int contributorId);
        /// <summary>
        /// Resumen de los contribuyentes de radian
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        NameValueCollection Summary(int contributorId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contributorId"></param>
        /// <param name="radianContributorType"></param>
        /// <param name="radianOperationMode"></param>
        /// <returns></returns>
        ResponseMessage RegistrationValidation(int contributorId, Domain.Common.RadianContributorType radianContributorType, Domain.Common.RadianOperationMode radianOperationMode);

        /// <summary>
        /// Consulta de participantes de radian en estado Registrado
        /// </summary>
        /// <param name="page">Numero de la pagina</param>
        /// <param name="size">Tamaño de la pagina</param>
        /// <returns></returns>
        RadianAdmin ListParticipants(int page, int size);
        RadianAdmin ContributorSummary(int contributorId, int radianContributorType = 0);
        bool ChangeParticipantStatus(int contributorId, string newState, int radianContributorTypeId, string actualState, string description);
        void UpdateRadianOperation(int radiancontributorId, int softwareType);
        RadianAdmin ListParticipantsFilter(AdminRadianFilter filter, int page, int size);
        Guid UpdateRadianContributorFile(RadianContributorFile radianContributorFile);
        RadianContributor CreateContributor(int contributorId, Domain.Common.RadianState radianState, Domain.Common.RadianContributorType radianContributorType, Domain.Common.RadianOperationMode radianOperationMode, string createdBy);
        List<RadianContributorFile> RadianContributorFileList(string id);
        RadianOperationMode GetOperationMode(int id);
        List<Domain.RadianOperationMode> OperationModeList();
        bool ChangeContributorStep(int radianContributorId, int step);
        ResponseMessage AddFileHistory(RadianContributorFileHistory radianFileHistory);
        int GetAssociatedClients(int radianContributorId);
        RadianTestSetResult GetSetTestResult(string code, string softwareId, int type);
        byte[] DownloadContributorFile(string code, string fileName, out string contentType);

        RadianContributor ChangeContributorActiveRequirement(int radianContributorId);
    }
}