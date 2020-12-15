using Gosocket.Dian.Domain.Sql;
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

        List<Gosocket.Dian.Domain.Sql.OtherDocElecOperationMode> GetOperationModes();

        OtherDocElecContributor CreateContributor(int contributorId, Domain.Common.OtherDocElecState State, Domain.Common.OtherDocElecContributorType ContributorType, Domain.Common.OtherDocElecOperationMode OperationMode, string createdBy);
    }
}
