using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces
{
    public interface IRadianContributorService
    {

        /// <summary>
        /// Resumen de los contribuyentes de radian
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        NameValueCollection Summary(string userCode);

        /// <summary>
        /// Consulta de participantes de radian en estado Registrado
        /// </summary>
        /// <param name="page">Numero de la pagina</param>
        /// <param name="size">Tamaño de la pagina</param>
        /// <returns></returns>
        RadianAdmin ListParticipants(int page, int size);

        RadianAdmin ListParticipantsFilter(AdminRadianFilter filter, int page, int size);

        int AddOrUpdate(RadianContributor radianContributor, string approveState);
        List<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);
        void RemoveRadianContributor(RadianContributor radianContributor);
        List<RadianContributorType> GetRadianContributorTypes(Expression<Func<RadianContributorType, bool>> expression);
        List<RadianContributorFileStatus> GetRadianContributorFileStatus(Expression<Func<RadianContributorFileStatus, bool>> expression);
        Guid UpdateRadianContributorFile(RadianContributorFile radianContributorFile);
        List<RadianContributorFile> GetRadianContributorFile(Expression<Func<RadianContributorFile, bool>> expression);
    }
}