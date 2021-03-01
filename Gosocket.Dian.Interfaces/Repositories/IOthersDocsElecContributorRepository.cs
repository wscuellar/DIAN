using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Gosocket.Dian.Domain;

namespace Gosocket.Dian.Interfaces.Repositories
{
    public interface IOthersDocsElecContributorRepository
    {
        OtherDocElecContributor Get(Expression<Func<OtherDocElecContributor, bool>> expression);
        PagedResult<OtherDocElecContributor> List(Expression<Func<OtherDocElecContributor, bool>> expression, int page = 0, int length = 0);
        int AddOrUpdate(OtherDocElecContributor othersDocsElecContributor);
        void RemoveOthersDocsElecContributor(OtherDocElecContributor othersDocsElecContributor);
        bool GetParticipantWithActiveProcess(int contributorId, int contributorTypeId, int OperationModeId, int statusSowftware);
        PagedResult<OtherDocElecCustomerList> CustomerList(int id, string code, string State, int page = 0, int length = 0);
        List<Contributor> GetTechnologicalProviders(int contributorId, int electronicDocumentId, int contributorTypeId, string state);
    }
}