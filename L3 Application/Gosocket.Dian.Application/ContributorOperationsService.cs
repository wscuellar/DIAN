
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class ContributorOperationsService : IContributorOperationsService
    {
        public ContributorOperationsService()
        {

        }
        public List<ContributorOperations> Get(int contributorId, int page, int length)
        {
            using (var context = new SqlDBContext())
            {
                var query = context.ContributorOperations.Include("Software").Include("Contributor").Include("OperationMode").Where(co => co.ContributorId == contributorId).OrderByDescending(c => c.Id).Skip(page * length).Take(length);
                return query.ToList();
            }
        }
        public ContributorOperations Get(int id)
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorOperations.Include("OperationMode").Include("Contributor").Include("Software").FirstOrDefault(co => co.Id == id);
            }
        }
        public ContributorOperations GetForDelete(int id)
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorOperations.Include("Contributor").FirstOrDefault(co => co.Id == id);
            }
        }
        public ContributorOperations Get(int contributorId, int operationModeId, int? providerId, Guid? softwareId)
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorOperations.SingleOrDefault(co => co.ContributorId == contributorId && co.OperationModeId == operationModeId && co.ProviderId == providerId && co.SoftwareId == softwareId && !co.Deleted);
            }
        }

        public List<ContributorOperations> Find(int contributorId, int operationModeId, int? providerId, Guid? softwareId)
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorOperations.Where(co => co.ContributorId == contributorId && co.OperationModeId == operationModeId && co.ProviderId == providerId && co.SoftwareId == softwareId && !co.Deleted).ToList();
            }
        }

        public List<ContributorOperations> GetContributorOperations(int contributorId)
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorOperations.Include("Provider").Include("Software").Where(co => co.ContributorId == contributorId).ToList();
            }
        }

        public List<ContributorOperations> GetContributorOperations(int contributorId, string clientCode)
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorOperations.Include("Contributor").Include("Contributor.AcceptanceStatus").Where(co => co.ProviderId == contributorId && (clientCode == null || co.Contributor.Code == clientCode)).ToList();
            }
        }

        public Contributor GetContributor(int contributorId)
        {
            var context = new SqlDBContext();
            return context.Contributors.Include("ContributorOperations").Include("ContributorOperations.OperationMode").Include("ContributorOperations.Software").Include("ContributorOperations.Software.AcceptanceStatusSoftware").SingleOrDefault(co => co.Id == contributorId);
        }

        public int AddOrUpdate(ContributorOperations contributorOperations)
        {
            using (var context = new SqlDBContext())
            {
                var contributorOperationsInstance = context.ContributorOperations.FirstOrDefault(c => c.Id == contributorOperations.Id);
                if (contributorOperationsInstance != null)
                {
                    contributorOperationsInstance.Deleted = contributorOperations.Deleted;
                    context.Entry(contributorOperationsInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(contributorOperations).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();
                return contributorOperationsInstance != null ? contributorOperationsInstance.Id : contributorOperations.Id;
            }
        }

        public int AddOrUpdateRadianContributorOperation(RadianContributorOperations contributorOperations)
        {
            using (var context = new SqlDBContext())
            {
                var contributorOperationsInstance = context.RadianContributorOperations.FirstOrDefault(
                                                                                c => c.Id == contributorOperations.Id);
                if (contributorOperationsInstance != null)
                {
                    contributorOperationsInstance.Deleted = contributorOperations.Deleted;
                    context.Entry(contributorOperationsInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(contributorOperations).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();
                return contributorOperationsInstance != null ? contributorOperationsInstance.Id : contributorOperations.Id;
            }
        }
    }
}
