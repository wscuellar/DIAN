using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{

    public interface IRadianContributorRepository
    {

        bool GetParticipantWithActiveProcess(int contributorId);
        RadianContributor Get(Expression<Func<RadianContributor, bool>> expression);
        PagedResult<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);
        PagedResult<RadianContributor> ListByDateDesc(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);
        int AddOrUpdate(RadianContributor radianContributor);
        
        void RemoveRadianContributor(RadianContributor radianContributor);
        PagedResult<RadianCustomerList> CustomerList(int id, string code, string radianState, int page = 0, int length = 0);
        List<RadianContributor> ActiveParticipantsWithSoftware(int radianContributorTypeId);

        List<RadianSoftware> RadianSoftwareByParticipante(int radianContributorId);
    }

}