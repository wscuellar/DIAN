using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class ContributorService
    {
        SqlDBContext sqlDBContext;
        //private static StackExchange.Redis.IDatabase cache;

        public ContributorService()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public List<Contributor> GetBillerContributors(int page, int length)
        {
            var query = sqlDBContext.Contributors.Where(c => c.ContributorTypeId == (int)Domain.Common.ContributorType.Biller).OrderBy(c => c.AcceptanceStatusId).Skip(page * length).Take(length);
            //var filtered = query.ToList().Where(c => !c.Deleted);
            return query.ToList();
        }

        public List<Contributor> GetProviderContributors(int page, int length)
        {
            var query = sqlDBContext.Contributors.Where(c => c.ContributorTypeId == (int)Domain.Common.ContributorType.Provider).OrderBy(c => c.AcceptanceStatusId).Skip(page * length).Take(length);
            //var filtered = query.ToList().Where(c => !c.Deleted);
            return query.ToList();
        }

        public List<Contributor> GetParticipantContributors(int page, int length)
        {
            var query = sqlDBContext.Contributors.OrderBy(c => c.AcceptanceStatusId).Skip(page * length).Take(length);
            return query.ToList();
        }

        public List<Contributor> GetContributors(int type, int page, int length)
        {
            var query = sqlDBContext.Contributors.Where(c => c.ContributorTypeId == type).OrderBy(c => c.AcceptanceStatusId).Skip(page * length).Take(length);
            return query.ToList();
        }

        public List<Contributor> GetContributors(string code, int status, int page, int length, int? contributorType)
        {
            var query = sqlDBContext.Contributors.Where(c => !c.Deleted
                         && (string.IsNullOrEmpty(code) || c.Code == code)
                         && (status == -1 || c.AcceptanceStatusId == status)
                         && (contributorType == -1 || c.ContributorTypeId == contributorType)
                         ).OrderByDescending(c => c.Code).Skip(page * length).Take(length);

            return query.ToList();
        }

        public List<Contributor> GetContributorsByType(int contributorType)
        {
            using (var context = new SqlDBContext())
            {
                var contributors = context.Contributors.Include("Softwares").Where(x => x.ContributorTypeId == contributorType).ToList();
                return contributors.Where(c => !c.Deleted).ToList();
            }
        }

        public IEnumerable<Contributor> GetContributorsByIds(List<int> ids)
        {
            return sqlDBContext.Contributors.Where(c => !c.Deleted
                         && ids.Contains(c.Id));
        }

        public IEnumerable<Contributor> GetContributors(int contributorTypeId)
        {
            return sqlDBContext.Contributors.Where(c => !c.Deleted && c.ContributorTypeId == contributorTypeId);
        }

        public IEnumerable<Contributor> GetContributors(int contributorTypeId, int statusId)
        {
            return sqlDBContext.Contributors.Where(c => !c.Deleted && c.ContributorTypeId == contributorTypeId && c.AcceptanceStatusId == statusId).OrderBy(c => c.AcceptanceStatusId);
        }

        public Contributor Get(int id)
        {
            return sqlDBContext.Contributors.FirstOrDefault(x => x.Id == id);
            //using (var context = new SqlDBContext())
            //{
            //    return context.Contributors.FirstOrDefault(x => x.Id == id);
            //}
        }

        public Contributor Get(int id, string connectionString)
        {
            using (var context = new SqlDBContext(connectionString))
            {
                return context.Contributors.FirstOrDefault(x => x.Id == id);
            }
        }

        public List<Contributor> GetContributorsByAcceptanceStatusId(int status)
        {
            using (var context = new SqlDBContext())
            {
                return context.Contributors.Where(x => x.AcceptanceStatusId == status).ToList();
            }
        }

        public List<Contributor> GetContributorsByAcceptanceStatusesId(int[] statuses, string connectionString = null)
        {
            var ctx = string.IsNullOrEmpty(connectionString) ? new SqlDBContext() : new SqlDBContext(connectionString);
            using (var context = ctx)
            {
                return context.Contributors.Where(x => statuses.Contains(x.AcceptanceStatusId)).ToList();
            }
        }

        public int GetCountContributorsByAcceptanceStatusId(int status)
        {
            using (var context = new SqlDBContext())
            {
                return context.Contributors.Count(x => x.AcceptanceStatusId == status);
            }
        }

        public Contributor ObsoleteGet(int id)
        {
            using (var context = new SqlDBContext())
            {
                return context.Contributors.Include("ContributorType").Include("OperationMode").Include("Provider").Include("Clients")
                    .Include("AcceptanceStatus").Include("Softwares").Include("Softwares.AcceptanceStatusSoftware").Include("ContributorFiles")
                    .Include("ContributorFiles.ContributorFileStatus").Include("ContributorFiles.ContributorFileType").FirstOrDefault(x => x.Id == id);
            }
        }

        public Contributor GetContributorFiles(int id)
        {
            using (var context = new SqlDBContext())
            {
                return context.Contributors.Include("ContributorFiles").Include("ContributorFiles.ContributorFileStatus").FirstOrDefault(x => x.Id == id);
            }
        }

        public Contributor GetByCode(string code)
        {
            return sqlDBContext.Contributors.FirstOrDefault(p => p.Code == code);
        }

        public Contributor GetByCode(string code, string connectionString)
        {
            using (var context = new SqlDBContext(connectionString))
            {
                return context.Contributors.FirstOrDefault(p => p.Code == code);
            }
        }

        public List<Contributor> GetByCodes(string[] codes)
        {
            return sqlDBContext.Contributors.Where(p => codes.Contains(p.Code)).ToList();
        }

        public Contributor GetByCode(string code, int type)
        {
            return sqlDBContext.Contributors.FirstOrDefault(p => p.Code == code && p.ContributorTypeId == type && p.Status);
        }

        public object GetContributorByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public void Activate(Contributor contributor)
        {
            using (var context = new SqlDBContext())
            {
                var contributorInstance = context.Contributors.FirstOrDefault(c => c.Id == contributor.Id);
                if (contributorInstance != null)
                {
                    contributorInstance.AcceptanceStatusId = (int)Domain.Common.ContributorStatus.Enabled;
                    contributorInstance.ContributorTypeId = contributor.ContributorTypeId;
                    //contributorInstance.HabilitationDate = contributorInstance.HabilitationDate ?? contributor.HabilitationDate;
                    contributorInstance.Updated = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
        }

        public void SetHabilitationAndProductionDates(Contributor contributor)
        {
            using (var context = new SqlDBContext())
            {
                var contributorInstance = context.Contributors.FirstOrDefault(c => c.Id == contributor.Id);
                if (contributorInstance != null)
                {
                    contributorInstance.HabilitationDate = contributorInstance.HabilitationDate ?? contributor.HabilitationDate;
                    contributorInstance.ProductionDate = contributorInstance.ProductionDate ?? contributor.ProductionDate;
                    contributorInstance.Updated = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
        }

        public void SetHabilitationAndProductionDates(Contributor contributor, string connectionString = null)
        {
            using (var context = string.IsNullOrWhiteSpace(connectionString) ? new SqlDBContext() : new SqlDBContext(connectionString))
            {
                var contributorInstance = context.Contributors.FirstOrDefault(c => c.Id == contributor.Id);
                if (contributorInstance != null)
                {
                    contributorInstance.HabilitationDate = contributorInstance.HabilitationDate ?? contributor.HabilitationDate;
                    contributorInstance.ProductionDate = contributorInstance.ProductionDate ?? contributor.ProductionDate;
                    contributorInstance.Updated = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
        }

        public int AddOrUpdate(Contributor contributor)
        {
            using (var context = new SqlDBContext())
            {
                var contributorInstance = context.Contributors.FirstOrDefault(c => c.Id == contributor.Id);
                if (contributorInstance != null)
                {
                    contributorInstance.AcceptanceStatusId = contributor.AcceptanceStatusId != 0 ? contributor.AcceptanceStatusId : contributorInstance.AcceptanceStatusId;
                    contributorInstance.Name = contributor.Name;
                    contributorInstance.BusinessName = contributor.BusinessName;
                    contributorInstance.Email = contributor.Email;
                    contributorInstance.ExchangeEmail = contributor.ExchangeEmail;
                    contributorInstance.ContributorTypeId = contributor.ContributorTypeId;

                    if (contributor.ContributorFiles != null && contributor.ContributorFiles.Count() > 0)
                    {
                        foreach (var item in contributor.ContributorFiles)
                        {
                            if (item.Deleted)
                            {
                                var removedItem = contributorInstance.ContributorFiles.FirstOrDefault(f => f.Id == item.Id);
                                if (removedItem != null)
                                {
                                    contributorInstance.ContributorFiles.Remove(removedItem);
                                    context.ContributorFiles.Remove(removedItem);
                                }
                            }
                            else
                                context.ContributorFiles.Add(item);
                        }
                    }
                    context.Entry(contributorInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(contributor).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();
                return contributorInstance != null ? contributorInstance.Id : contributor.Id;
            }
        }

        public void AddOrUpdateConfiguration(Contributor contributor)
        {

            using (var context = new SqlDBContext())
            {
                var contributorInstance = context.Contributors.FirstOrDefault(c => c.Id == contributor.Id);
                if (contributorInstance != null)
                {
                    contributorInstance.ExchangeEmail = contributor.ExchangeEmail;
                    context.SaveChanges();
                }
            }
        }

        public void SetToEnabled(Contributor contributor)
        {
            using (var context = new SqlDBContext())
            {
                try
                {
                    var contributorInstance = context.Contributors.FirstOrDefault(c => c.Code == contributor.Code);
                    contributorInstance.AcceptanceStatusId = (int)Domain.Common.ContributorStatus.Enabled;
                    contributorInstance.HabilitationDate = contributorInstance.HabilitationDate ?? DateTime.UtcNow;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    var logger = new GlobalLogger("SetContributorToEnabled", contributor.Code)
                    {
                        Action = "SetToEnabled",
                        Controller = "",
                        Message = ex.Message,
                        RouteData = "",
                        StackTrace = ex.StackTrace
                    };
                    RegisterException(logger);
                }
            }
        }

        public void RemoveUserContributor(UserContributors userContributors)
        {
            UserContributors uc = sqlDBContext.UserContributors.FirstOrDefault(x => x.UserId == userContributors.UserId && x.ContributorId == userContributors.ContributorId);
            sqlDBContext.UserContributors.Remove(uc);
            sqlDBContext.SaveChanges();
        }

        public void AddUserContributor(UserContributors userContributor)
        {
            using (var context = new SqlDBContext())
            {
                context.Entry(userContributor).State = System.Data.Entity.EntityState.Added;
                context.SaveChanges();
            }
        }

        public List<UserContributors> GetUserContributors(int id)
        {
            using (var context = new SqlDBContext())
            {
                return context.UserContributors.Where(u => u.ContributorId == id).ToList();
            }
        }

        public IEnumerable<AcceptanceStatus> GetAcceptanceStatuses()
        {
            return sqlDBContext.AcceptanceStatuses;
        }

        public OperationMode GetOperationMode(int id)
        {
            using (var context = new SqlDBContext())
            {
                return context.OperationModes.FirstOrDefault(x => x.Id == id);
            }
        }

        #region ContributorFiles

        public bool AddOrUpdateContributorFile(ContributorFile contributorFile)
        {
            using (var context = new SqlDBContext())
            {
                var contributorFileInstance = context.ContributorFiles.FirstOrDefault(p => p.Id == contributorFile.Id);
                if (contributorFileInstance != null)
                {
                    contributorFileInstance.FileName = contributorFile.FileName;
                    contributorFileInstance.Updated = DateTime.Now;
                    contributorFileInstance.CreatedBy = contributorFile.CreatedBy;
                    contributorFileInstance.Status = contributorFile.Status;
                    contributorFileInstance.Comments = contributorFile.Comments;
                    context.Entry(contributorFileInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                    context.Entry(contributorFile).State = System.Data.Entity.EntityState.Added;

                context.SaveChanges();
                return true;
            }
        }

        public ContributorFile GetContributorFile(Guid id)
        {
            return sqlDBContext.ContributorFiles.FirstOrDefault(x => x.Id == id);
        }

        public void AddContributorFileHistory(ContributorFileHistory contributorFileHistory)
        {
            using (var context = new SqlDBContext())
            {
                context.Entry(contributorFileHistory).State = System.Data.Entity.EntityState.Added;
                context.SaveChanges();
            }
        }

        public IEnumerable<ContributorFileHistory> GetContributorFileHistories(Guid id)
        {
            return sqlDBContext.ContributorFileHistories.Include("ContributorFileStatus").Where(p => p.ContributorFileId == id);
        }

        public List<ContributorFileType> GetMandatoryContributorFileTypes()
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorFileTypes.Where(f => f.Mandatory).ToList();
            }
        }

        public List<ContributorFileType> GetNotRequiredContributorFileTypes()
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorFileTypes.Where(f => !f.Mandatory).ToList();
            }
        }

        public List<ContributorFileStatus> GetContributorFileStatuses()
        {
            using (var context = new SqlDBContext())
            {
                return context.ContributorFileStatuses.ToList();
            }
        }
        #endregion


        private void RegisterException(GlobalLogger logger)
        {
            var tableManager = new TableManager("GlobalLogger");
            tableManager.InsertOrUpdate(logger);
        }
    }
}