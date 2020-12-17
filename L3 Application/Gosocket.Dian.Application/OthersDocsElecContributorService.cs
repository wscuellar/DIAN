using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class OthersDocsElecContributorService : IOthersDocsElecContributorService
    {
        private SqlDBContext sqlDBContext;
        private readonly IContributorService _contributorService;
        private readonly IOthersDocsElecContributorRepository _othersDocsElecContributorRepository;

        public OthersDocsElecContributorService(IContributorService contributorService, IOthersDocsElecContributorRepository othersDocsElecContributorRepository)
        {
            _contributorService = contributorService;
            _othersDocsElecContributorRepository = othersDocsElecContributorRepository;
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }


        public List<Gosocket.Dian.Domain.Sql.OtherDocElecOperationMode> GetOperationModes()
        {
            using (var context = new SqlDBContext())
            {
                return context.OtherDocElecOperationModes.ToList();
            }
        }

        public NameValueCollection Summary(string userCode)
        {
            NameValueCollection collection = new NameValueCollection();
            Domain.Contributor contributor = _contributorService.GetByCode(userCode);
            List<OtherDocElecContributor> LContributors = _othersDocsElecContributorRepository.List(t => t.ContributorId == contributor.Id && t.State != "Cancelado").Results;
            if (LContributors.Any())
                foreach (var radianContributor in LContributors)
                {
                    string key = Enum.GetName(typeof(Domain.Common.RadianContributorType), radianContributor. OtherDocElecContributorTypeId);
                    collection.Add(key + "_OtherDocElecContributorTypeId", radianContributor.OtherDocElecContributorTypeId.ToString());
                    collection.Add(key + "_OtherDocElecOperationModeId", radianContributor.OtherDocElecOperationModeId.ToString());
                }
            if (contributor == null) return collection;
            collection.Add("ContributorId", contributor.Id.ToString());
            collection.Add("ContributorTypeId", contributor.ContributorTypeId.ToString());
            collection.Add("Active", contributor.Status.ToString());
            return collection;
        }
    }
}
