using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class OthersDocsElecContributorRepository : IOthersDocsElecContributorRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public OthersDocsElecContributorRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public OtherDocElecContributor Get(Expression<Func<OtherDocElecContributor, bool>> expression)
        {
            IQueryable<OtherDocElecContributor> query = sqlDBContext.OtherDocElecContributors.Where(expression)
                .Include("Contributor")
                .Include("OtherDocElecSoftwares")
                .Include("OtherDocElecSoftwares.OtherDocElecContributorOperations")
                .Include("OtherDocElecContributorType")
                .Include("OtherDocElecOperationMode")
                .Include("OtherDocElecContributorOperations");

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Inserta y actualiza
        /// </summary>
        /// <param name="radianContributor"></param>
        /// <returns></returns>
        public int AddOrUpdate(OtherDocElecContributor othersDocsElecContributor)
        {
            using (var context = new SqlDBContext())
            {
                OtherDocElecContributor ContributorInstance =
                    context.OtherDocElecContributors.FirstOrDefault(c => c.Id == othersDocsElecContributor.Id);

                if (ContributorInstance != null)
                {
                    ContributorInstance.OtherDocElecContributorTypeId = othersDocsElecContributor.OtherDocElecContributorTypeId;
                    ContributorInstance.Update = DateTime.Now;
                    ContributorInstance.State = othersDocsElecContributor.State;
                    ContributorInstance.ElectronicDocumentId = othersDocsElecContributor.ElectronicDocumentId;
                    ContributorInstance.OtherDocElecOperationModeId = othersDocsElecContributor.OtherDocElecOperationModeId;
                    ContributorInstance.CreatedBy = othersDocsElecContributor.CreatedBy;
                    ContributorInstance.Description = othersDocsElecContributor.Description;
                    ContributorInstance.Step = othersDocsElecContributor.Step == 0 ? 1 : othersDocsElecContributor.Step;

                    context.Entry(ContributorInstance).State = EntityState.Modified;
                }
                else
                {
                    othersDocsElecContributor.Step = 1;
                    othersDocsElecContributor.Update = DateTime.Now;
                    context.Entry(othersDocsElecContributor).State = EntityState.Added;
                }

                context.SaveChanges();

                return othersDocsElecContributor.Id;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="radianContributor"></param>
        public void RemoveOthersDocsElecContributor(OtherDocElecContributor othersDocsElecContributor)
        {
            OtherDocElecContributor rc = sqlDBContext.OtherDocElecContributors.FirstOrDefault(x => x.Id == othersDocsElecContributor.Id);
            if (rc != null)
            {
                sqlDBContext.OtherDocElecContributors.Remove(rc);
                sqlDBContext.SaveChanges();
            }
        }


        public PagedResult<OtherDocElecContributor> List(Expression<Func<OtherDocElecContributor, bool>> expression, int page = 0, int length = 0)
        {
            IQueryable<OtherDocElecContributor> query = sqlDBContext.OtherDocElecContributors.Where(expression)
             .Include("Contributor")
             .Include("OtherDocElecSoftwares")
             .Include("OtherDocElecSoftwares.OtherDocElecContributorOperations")
             .Include("OtherDocElecContributorType")
             .Include("OtherDocElecOperationMode")
             .Include("OtherDocElecContributorOperations");

            return query.Paginate(page, length, t => t.Id.ToString());
        }

        public bool GetParticipantWithActiveProcess(int contributorId, int contributorTypeId, int OperationModeId, int statusSowftware)
        {
            return (from p in sqlDBContext.OtherDocElecContributors.Where(t => t.ContributorId == contributorId)
                    join o in sqlDBContext.OtherDocElecSoftwares on p.Id equals o.OtherDocElecContributorId
                    where p.OtherDocElecOperationModeId == OperationModeId
                    && o.OtherDocElecSoftwareStatusId == statusSowftware
                    && o.Deleted == false
                    select p).ToList().Any();
        }

        public PagedResult<OtherDocElecCustomerList> CustomerList(int id, string code, string State, int page = 0, int length = 0)
        {
            IQueryable<OtherDocElecCustomerList> query = (from c in sqlDBContext.Contributors
                                                          join dec in sqlDBContext.OtherDocElecContributors on c.Id equals dec.ContributorId
                                                          join s in sqlDBContext.OtherDocElecSoftwares on dec.Id equals s.OtherDocElecContributorId
                                                          where dec.OtherDocElecOperationModeId == (int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider
                                                            && dec.State != "Cancelado"
                                                            && s.ProviderId == id
                                                            && (string.IsNullOrEmpty(code) || c.Code == code)
                                                            && (string.IsNullOrEmpty(State) || dec.State == State)
                                                          select new OtherDocElecCustomerList()
                                                          {
                                                              Id = c.Id,
                                                              BussinessName = c.BusinessName,
                                                              Nit = c.Code,
                                                              State = dec.State,
                                                              Page = page,
                                                              Length = length
                                                          }).Distinct();

            return query.Paginate(page, length, t => t.Id.ToString());
        }

        public List<Contributor> GetTechnologicalProviders(int contributorId, int electronicDocumentId, int contributorTypeId, string state)
        {
            return sqlDBContext.OtherDocElecContributors
                .Include("Contributor")
                .Where(x => x.ContributorId != contributorId &&
                        x.ElectronicDocumentId == electronicDocumentId &&
                        x.OtherDocElecContributorTypeId == contributorTypeId &&
                        x.State == state)
                .Select(x => x.Contributor).ToList();
        }
    }
}
