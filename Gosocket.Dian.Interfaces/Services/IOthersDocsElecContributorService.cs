using Gosocket.Dian.Domain.Entity;
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

        List<Gosocket.Dian.Domain.Sql.OtherDocElecOperationMode> GetOperationModes();
        /// <summary>
        /// Cancelar un registro en la tabla OtherDocElecContributor
        /// </summary>
        /// <param name="contributorId">OtherDocElecContributorId</param>
        /// <param name="description">Motivo por el cual se hace la cancelación</param>
        /// <returns></returns>
        ResponseMessage CancelRegister(int contributorId,string description);
    }
}
