using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

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

        OtherDocElecContributor CreateContributor(string userCode, Domain.Common.OtherDocElecState State, Domain.Common.OtherDocElecContributorType ContributorType, Domain.Common.OtherDocElecOperationMode OperationMode, int ElectronicDocumentId, string createdBy);

        List<OtherDocElecContributor> ValidateExistenciaContribuitor(int ContributorId, int OperationModeId, string state);

        bool ValidateSoftwareActive(int ContributorId, int ContributorTypeId, int OperationModeId, int stateSofware);

        PagedResult<OtherDocsElectList> List(string userCode, int OperationModeId);

    }
}
