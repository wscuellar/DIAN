﻿using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorRepository : IRadianContributorRepository
    {

        private readonly SqlDBContext sqlDBContext;

        public RadianContributorRepository()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public RadianContributor Get(Expression<Func<RadianContributor, bool>> expression)
        {
            IQueryable<RadianContributor> query = sqlDBContext.RadianContributors.Where(expression)
                .Include("Contributor")
                .Include("RadianSoftwares")
                .Include("RadianSoftwares.RadianContributorOperations")
                .Include("RadianContributorType")
                .Include("RadianOperationMode")
                .Include("RadianContributorFile")
                .Include("RadianContributorOperations");

            return query.FirstOrDefault();
        }

        public bool GetParticipantWithActiveProcess(int contributorId, int contributorTypeId)
        {
            List<RadianContributor> participants = (from p in sqlDBContext.RadianContributors.Where(t => t.ContributorId == contributorId)
                                                    join o in sqlDBContext.RadianContributorOperations on p.Id equals o.RadianContributorId
                                                    where p.RadianContributorTypeId != contributorTypeId
                                                    && p.RadianOperationModeId == 1
                                                    && o.OperationStatusId < 4
                                                    select p).ToList();
            return participants.Any();

        }



        public PagedResult<RadianCustomerList> CustomerList(int id, string code, string radianState, int page = 0, int length = 0)
        {
            IQueryable<RadianCustomerList> query = (from rc in sqlDBContext.RadianContributors
                                                    join s in sqlDBContext.RadianSoftwares on rc.Id equals s.RadianContributorId
                                                    join rco in sqlDBContext.RadianContributorOperations on s.Id equals rco.SoftwareId
                                                    join rc2 in sqlDBContext.RadianContributors on rco.RadianContributorId equals rc2.Id
                                                    join c in sqlDBContext.Contributors on rc2.ContributorId equals c.Id
                                                    where rc.Id == id
                                                    && rco.SoftwareType != 1
                                                    && rc2.RadianState != "Cancelado"
                                                    && (string.IsNullOrEmpty(code) || c.Code == code)
                                                    && (string.IsNullOrEmpty(radianState) || rc2.RadianState == radianState)

                                                    select new RadianCustomerList()
                                                    {
                                                        Id = rc2.Id,
                                                        BussinessName = c.BusinessName,
                                                        Nit = c.Code,
                                                        RadianState = rc2.RadianState,
                                                        Page = page,
                                                        Length = length
                                                    }).Distinct();
            return query.Paginate(page, length, t => t.Id.ToString());
        }

        /// <summary>
        /// Consulta los contribuyentes de radian.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="length"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public PagedResult<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0)
        {
            IQueryable<RadianContributor> query = sqlDBContext.RadianContributors.Where(expression)
                .Include("Contributor")
                .Include("RadianContributorType")
                .Include("RadianOperationMode")
                .Include("RadianContributorFile")
                .Include("RadianContributorOperations");
            return query.Paginate(page, length, t => t.Id.ToString());
        }



        public List<RadianContributor> ActiveParticipantsWithSoftware(int radianContributorTypeId)
        {
            string radianStatus = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Habilitado);
            int softwareStatus = (int)Domain.Common.RadianSoftwareStatus.Accepted;
            List<RadianContributor> query = sqlDBContext.RadianContributors.Where(t => t.RadianContributorTypeId == radianContributorTypeId && t.RadianState == radianStatus).Include("Contributor").ToList();
            query = (from rc in query
                     join s in sqlDBContext.RadianSoftwares.Where(t => t.RadianSoftwareStatusId == softwareStatus) on rc.Id equals s.RadianContributorId
                     select rc).ToList();
            return query;
        }

        /// <summary>
        /// Inserta y actualiza
        /// </summary>
        /// <param name="radianContributor"></param>
        /// <returns></returns>
        public int AddOrUpdate(RadianContributor radianContributor)
        {
            using (var context = new SqlDBContext())
            {
                RadianContributor radianContributorInstance = context.RadianContributors.FirstOrDefault(c => c.Id == radianContributor.Id);

                if (radianContributorInstance != null)
                {
                    radianContributorInstance.RadianContributorTypeId = radianContributor.RadianContributorTypeId;
                    radianContributorInstance.Update = DateTime.Now;
                    radianContributorInstance.RadianState = radianContributor.RadianState;
                    radianContributorInstance.RadianOperationModeId = radianContributor.RadianOperationModeId;
                    radianContributorInstance.CreatedBy = radianContributor.CreatedBy;
                    radianContributorInstance.Description = radianContributor.Description;
                    radianContributorInstance.Step = radianContributor.Step == 0 ? 1 : radianContributor.Step;

                    context.Entry(radianContributorInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    radianContributor.Step = 1;
                    radianContributor.Update = DateTime.Now;
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
    }
}
