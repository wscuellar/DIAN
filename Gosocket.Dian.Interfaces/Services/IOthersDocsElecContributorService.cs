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
    }
}
