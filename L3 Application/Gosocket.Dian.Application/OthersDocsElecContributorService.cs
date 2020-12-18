using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
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
using Gosocket.Dian.DataContext.Middle;

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
            Contributor contributor = _contributorService.GetByCode(userCode);
            List<OtherDocElecContributor> LContributors = _othersDocsElecContributorRepository.List(t => t.ContributorId == contributor.Id && t.State != "Cancelado").Results;
            if (LContributors.Any())
                foreach (var itemContributor in LContributors)
                {
                    string key = Enum.GetName(typeof(Domain.Common.OtherDocElecContributorType), itemContributor.OtherDocElecContributorTypeId);
                    collection.Add(key + "_OtherDocElecContributorTypeId", itemContributor.OtherDocElecContributorTypeId.ToString());
                    collection.Add(key + "_OtherDocElecOperationModeId", itemContributor.OtherDocElecOperationModeId.ToString());
                }
            if (contributor == null) return collection;
            collection.Add("ContributorId", contributor.Id.ToString());
            collection.Add("ContributorTypeId", contributor.ContributorTypeId.ToString());
            collection.Add("Active", contributor.Status.ToString());
            return collection;
        }

        public OtherDocElecContributor CreateContributor(string userCode, OtherDocElecState State,
            Domain.Common.OtherDocElecContributorType ContributorType,
            Domain.Common.OtherDocElecOperationMode OperationMode, int ElectronicDocumentId, string createdBy)
        {

            int contributorId = _contributorService.GetByCode(userCode).Id;
            OtherDocElecContributor existing = _othersDocsElecContributorRepository.Get(t => t.ContributorId == contributorId
                                                                                     && t.OtherDocElecContributorTypeId == (int)ContributorType
                                                                                     && t.OtherDocElecOperationModeId == (int)OperationMode
                                                                                     && t.ElectronicDocumentId == ElectronicDocumentId);

            OtherDocElecContributor newContributor = new OtherDocElecContributor()
            {
                Id = existing != null ? existing.Id : 0,
                ContributorId = contributorId,
                CreatedBy = createdBy,
                OtherDocElecContributorTypeId = (int)ContributorType,
                OtherDocElecOperationModeId = (int)OperationMode,
                ElectronicDocumentId = ElectronicDocumentId,
                State = State.GetDescription(),
                CreatedDate = existing != null ? existing.CreatedDate : DateTime.Now
            };
            newContributor.Id = _othersDocsElecContributorRepository.AddOrUpdate(newContributor);

            //Software ownSoftware = GetSoftwareOwn(contributorId);
            //OtherDocElecSoftware odeSoftware = new OtherDocElecSoftware(ownSoftware, newContributor.Id, createdBy);
            //newContributor.OtherDocElecSoftwares = new List<OtherDocElecSoftware>() { odeSoftware };

            return newContributor;
        }

        private Software GetSoftwareOwn(int contributorId)
        {
            List<Software> ownSoftwares = _contributorService.GetBaseSoftwareForRadian(contributorId);
            if (!ownSoftwares.Any())
                return null;
            List<string> softwares = ContributorSoftwareAcceptedList(contributorId);

            return (from os in ownSoftwares
                    join s in softwares on os.Id.ToString() equals s
                    select os).OrderByDescending(t => t.Timestamp).FirstOrDefault();
        }


        private List<string> ContributorSoftwareAcceptedList(int contributorId)
        {
            Contributor contributor = _contributorService.Get(contributorId);
            var contributorOperations = contributor.ContributorOperations.Where(o => !o.Deleted);
            //var testSetResults = _radianTestSetResultManager.GetTestSetResulByCatalog(contributor.Code);

            List<string> softwareAccepted = new List<string>();
            foreach (var item in contributorOperations)
            {
                //GlobalTestSetResult testset = GetTestSetResult(testSetResults, item, contributor.ContributorTypeId.Value);
                // if (((TestSetStatus)testset.Status) == TestSetStatus.Accepted)
                // softwareAccepted.Add(testset.SoftwareId);
            }

            return softwareAccepted;
        }

        public List<OtherDocElecContributor> ValidateExistenciaContribuitor(int ContributorId, int OperationModeId, string state)
        {
            return _othersDocsElecContributorRepository.List(t => t.ContributorId == ContributorId
                                                                                      && t.OtherDocElecOperationModeId == OperationModeId
                                                                                      && t.State != state).Results;

        }

        public bool ValidateSoftwareActive(int ContributorId, int ContributorTypeId, int OperationModeId, int stateSofware)
        {
            return _othersDocsElecContributorRepository.GetParticipantWithActiveProcess(ContributorId, ContributorTypeId, OperationModeId, stateSofware);

        }


        public PagedResult<OtherDocsElectList> List(string userCode, int OperationModeId)
        {
            Contributor contributor = _contributorService.GetByCode(userCode);



            IQueryable<OtherDocsElectList> query = (from oc in sqlDBContext.OtherDocElecContributors
                                                    join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
                                                    join oco in sqlDBContext.OtherDocElecContributorOperations on s.Id equals oco.SoftwareId
                                                    join ocs in sqlDBContext.OtherDocElecSoftwareStatus on s.OtherDocElecSoftwareStatusId equals ocs.Id
                                                    join ope in sqlDBContext.OtherDocElecOperationModes on oc.OtherDocElecOperationModeId equals ope.Id
                                                    join oty in sqlDBContext.OtherDocElecContributorTypes on oc.OtherDocElecContributorTypeId equals oty.Id
                                                    join eld in sqlDBContext.ElectronicDocuments on oc.ElectronicDocumentId equals eld.Id
                                                    where oc.ContributorId == contributor.Id
                                                     && oc.State != "Cancelado"

                                                    select new OtherDocsElectList()
                                                    {
                                                        Id = oc.Id,
                                                        ContributorId = oc.ContributorId,
                                                        OperationMode = ope.Name,
                                                        ContibutorType = oty.Name,
                                                        Software =s.Name,
                                                        PinSW =s.Pin,
                                                        StateSoftware =ocs.Name, 
                                                        StateContributor =oc.State,
                                                        CreatedDate = oc.CreatedDate,
                                                        ElectronicDoc= eld.Name,
                                                        Url =s.Url, 
                                                    }).Distinct();
            return query.Paginate(0, 100, t => t.Id.ToString());
 
        }

    }
}
