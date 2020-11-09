﻿using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.Application
{
    public class RadianContributorService : IRadianContributorService
    {
        private readonly IContributorService _contributorService;
        private readonly IRadianContributorRepository _radianContributorRepository;

        SqlDBContext sqlDBContext;
        //private static StackExchange.Redis.IDatabase cache;

        public RadianContributorService()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public RadianContributorService(IContributorService contributorService, IRadianContributorRepository radianContributorRepository )
        {
            _contributorService = contributorService;
            _radianContributorRepository = radianContributorRepository;
        }


        public NameValueCollection Summary(string userCode)
        {
            NameValueCollection collection = new NameValueCollection();
            Domain.Contributor contributor = _contributorService.GetByCode(userCode);
            if (contributor == null) return collection;

            List<Domain.RadianContributor> radianContributor = _radianContributorRepository.List(t => t.ContributorId == contributor.Id && t.RadianState != "Cancelado");
            string rcontributorTypes = radianContributor?.Aggregate("", (current, next) => current + ", " + next.RadianContributorTypeId.ToString());
            collection.Add("ContributorId", contributor.Id.ToString());
            collection.Add("ContributorTypeId", contributor.ContributorTypeId.ToString());
            collection.Add("Active", contributor.Status.ToString());
            collection.Add("WithSoft", (contributor.Softwares?.Count > 0).ToString());
            collection.Add("ExistInRadian", rcontributorTypes);
            return collection;
        }


        #region Repo 1
        /// <summary>
        /// Consulta los contribuyentes de radian.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="length"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public List<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0)
        {
            var query = sqlDBContext.RadianContributors.Where(expression).Include("Contributor").Include("RadianContributorType").Include("RadianOperationMode").Include("RadianContributorFile");
            if (page > 0 && length > 0)
            {
                query = query.Skip(page * length).Take(length);
            }
            return query.ToList();
        }

        /// <summary>
        /// Inserta y actualiza
        /// </summary>
        /// <param name="radianContributor"></param>
        /// <returns></returns>
        public int AddOrUpdate(RadianContributor radianContributor, string approveState = "")
        {
            using (var context = new SqlDBContext())
            {
                var radianContributorInstance = context.RadianContributors.FirstOrDefault(c => c.Id == radianContributor.Id);
                if (radianContributorInstance != null)
                {
                    radianContributorInstance.RadianContributorTypeId = radianContributor.RadianContributorTypeId;
                    radianContributorInstance.Update = DateTime.Now;
                    if (approveState != "")
                    {
                        radianContributorInstance.RadianState = approveState == "0" ? "En pruebas" : "Cancelado";
                    }
                    context.Entry(radianContributorInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(radianContributor).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();
                return radianContributorInstance != null ? radianContributorInstance.Id : radianContributor.Id;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="radianContributor"></param>
        public void RemoveRadianContributor(RadianContributor radianContributor)
        {
            RadianContributor rc = sqlDBContext.RadianContributors.FirstOrDefault(x => x.Id == radianContributor.Id);
            if (rc != null)
            {
                sqlDBContext.RadianContributors.Remove(rc);
                sqlDBContext.SaveChanges();
            }
        }

        #endregion

        #region Repo2

        public List<RadianContributorType> GetRadianContributorTypes(Expression<Func<RadianContributorType, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorTypes.Where(expression);
            return query.ToList();
        }

        #endregion

        #region Repo 3

        public List<RadianContributorFileStatus> GetRadianContributorFileStatus(Expression<Func<RadianContributorFileStatus, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorFileStatuses.Where(expression);
            return query.ToList();
        }
        #endregion

        #region Repo 4

        public List<RadianContributorFile> GetRadianContributorFile(Expression<Func<RadianContributorFile, bool>> expression)
        {
            var query = sqlDBContext.RadianContributorFiles.Where(expression);
            return query.ToList();
        }

        public Guid UpdateRadianContributorFile(RadianContributorFile radianContributorFile)
        {
            using (var context = new SqlDBContext())
            {
                var radianContributorFileInstance = context.RadianContributorFiles.FirstOrDefault(c => c.Id == radianContributorFile.Id);
                if (radianContributorFileInstance != null)
                {
                    radianContributorFileInstance.Status = radianContributorFile.Status;
                    context.Entry(radianContributorFileInstance).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                    return radianContributorFileInstance.Id;
                }
                else
                {
                    return radianContributorFile.Id;
                }

            }
        }
        #endregion

        #region Repo 6

        public Guid AddRegisterHistory(RadianContributorFileHistory radianContributorFileHistory)
        {
            using (var context = new SqlDBContext())
            {
                context.Entry(radianContributorFileHistory).State = System.Data.Entity.EntityState.Added;

                context.SaveChanges();
                return radianContributorFileHistory.Id;
            }
        } 
        #endregion

    }
}
