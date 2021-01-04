using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IOthersDocsElecContributorService
    {
        /// <summary>
        /// Resumen de los contribuyentes de ODE
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        NameValueCollection Summary(string userCode);

        List<OtherDocElecOperationMode> GetOperationModes();

        OtherDocElecContributor CreateContributor(string userCode, Domain.Common.OtherDocElecState State, int ContributorType, int OperationMode, int ElectronicDocumentId, string createdBy);

        List<OtherDocElecContributor> ValidateExistenciaContribuitor(int ContributorId, int OperationModeId, string state);

        bool ValidateSoftwareActive(int ContributorId, int ContributorTypeId, int OperationModeId, int stateSofware);

        PagedResult<OtherDocsElectData> List(string userCode, int OperationModeId);

        OtherDocsElectData GetCOntrinutorODE(int Id);

        /// <summary>
        /// Cancelar un registro en la tabla OtherDocElecContributor
        /// </summary>
        /// <param name="contributorId">OtherDocElecContributorId</param>
        /// <param name="description">Motivo por el cual se hace la cancelación</param>
        /// <returns></returns>
        ResponseMessage CancelRegister(int contributorId,string description);


        GlobalTestSetOthersDocuments GetTestResult(int OperatonModeId, int ElectronicDocumentId);

    }
}
