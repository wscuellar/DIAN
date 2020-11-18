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
        /// Resumen de los contribuyentes de radian
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        NameValueCollection Summary(string userCode);

        ResponseMessage RegistrationValidation(string userCode, Domain.Common.RadianContributorType radianContributorType, Domain.Common.RadianOperationMode radianOperationMode);

        /// <summary>
        /// Consulta de participantes de radian en estado Registrado
        /// </summary>
        /// <param name="page">Numero de la pagina</param>
        /// <param name="size">Tamaño de la pagina</param>
        /// <returns></returns>
        RadianAdmin ListParticipants(int page, int size);

        RadianAdmin ContributorSummary(int contributorId);

        

        bool ChangeParticipantStatus(int contributorId, string approveState);

        RadianAdmin ListParticipantsFilter(AdminRadianFilter filter, int page, int size);

        Guid UpdateRadianContributorFile(RadianContributorFile radianContributorFile);

        void CreateContributor(int contributorId, Domain.Common.RadianState radianState, Domain.Common.RadianContributorType radianContributorType, Domain.Common.RadianOperationMode radianOperationMode, string createdBy);

        List<RadianContributorFile> RadianContributorFileList(string id);

        RadianOperationMode GetOperationMode(int id);

        List<Domain.RadianOperationMode> OperationModeList();
        
    }
}