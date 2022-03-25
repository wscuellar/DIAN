using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity; 
using System.Collections.Generic;
using System.Collections.Specialized;
using Gosocket.Dian.Domain;

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
        OtherDocElecOperationMode GetOperationModeById(int operationModeId);
        OtherDocElecContributor CreateContributor(int contributorId, Domain.Common.OtherDocElecState State, int ContributorType, int OperationMode, int ElectronicDocumentId, string createdBy);
        OtherDocElecContributor CreateContributorNew(int contributorId, Domain.Common.OtherDocElecState State, int ContributorType, int OperationMode, int ElectronicDocumentId, string createdBy);
        List<OtherDocElecContributor> ValidateExistenciaContribuitor(int ContributorId, int contributorTypeId, int OperationModeId, string state);
        bool ValidateSoftwareActive(int ContributorId, int ContributorTypeId, int OperationModeId, int stateSofware);
        PagedResult<OtherDocsElectData> List(int contributorId, int contributorTypeId, int operationModeId, int electronicDocumentId);
        PagedResult<OtherDocsElectData> List3(int contributorId, int contributorTypeId, int electronicDocumentId);
        PagedResult<OtherDocsElectData> List2(int contributorId);
        int NumHabilitadosOtherDocsElect(int contributorId);
        OtherDocsElectData GetCOntrinutorODE(int Id);

    
        /// <summary>
        /// Cancelar un registro en la tabla OtherDocElecContributor
        /// </summary>
        /// <param name="contributorId">OtherDocElecContributorId</param>
        /// <param name="description">Motivo por el cual se hace la cancelación</param>
        /// <returns></returns>
        ResponseMessage CancelRegister(int contributorId,string description);
        GlobalTestSetOthersDocuments GetTestResult(int OperatonModeId, int ElectronicDocumentId);
        List<OtherDocElecContributor> GetDocElecContributorsByContributorId(int contributorId);
        List<Contributor> GetTechnologicalProviders(int contributorId, int electronicDocumentId, int contributorTypeId, string state);

        bool HabilitarParaSincronizarAProduccion(int Id, string Estado);
    }
}