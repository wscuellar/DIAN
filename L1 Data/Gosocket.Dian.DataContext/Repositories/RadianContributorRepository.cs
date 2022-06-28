using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
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
                .Include("RadianContributorType")
                .Include("RadianOperationMode")
                .Include("RadianContributorFile")
                .Include("RadianContributorOperations");

            return query.FirstOrDefault();
        }

        public bool GetParticipantWithActiveProcess(int contributorId)
        {
            List<RadianContributor> participants = (from p in sqlDBContext.RadianContributors.Where(t => t.ContributorId == contributorId && t.RadianState != "Cancelado")
                                                    join o in sqlDBContext.RadianContributorOperations on p.Id equals o.RadianContributorId
                                                    where 
                                                         o.OperationStatusId < 4
                                                        && !o.Deleted
                                                    select p).ToList();
            return participants.Any();

        }



        public PagedResult<RadianCustomerList> CustomerList(int id, string code, string radianState, int page = 0, int length = 0)
        {
            var query = (from rc in sqlDBContext.RadianContributors
                         join rcop in sqlDBContext.RadianContributorOperations on new { id = rc.Id, st = 1 } equals new { id = rcop.RadianContributorId, st = rcop.SoftwareType }
                         join rs in sqlDBContext.RadianSoftwares on new { s = rcop.SoftwareId, c = rc.ContributorId } equals new { s = rs.Id, c = rs.ContributorId }
                         join rcop2 in sqlDBContext.RadianContributorOperations on rs.Id equals rcop2.SoftwareId
                         join rc2 in sqlDBContext.RadianContributors on rcop2.RadianContributorId equals rc2.Id
                         join c in sqlDBContext.Contributors on rc2.ContributorId equals c.Id
                         where rcop2.SoftwareType != 1
                         && rc2.RadianState != "Cancelado"
                         && rc.Id == id
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


        /// <summary>
        /// Consulta los contribuyentes de radian.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="length"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public PagedResult<RadianContributor> ListByDateDesc(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0)
        {
            IQueryable<RadianContributor> query = sqlDBContext.RadianContributors.Where(expression)
                .Include("Contributor")
                .Include("RadianContributorType")
                .Include("RadianOperationMode")
                .Include("RadianContributorFile")
                .Include("RadianContributorOperations");
            return query.Paginate(page, length, t => t.Update, true);
        }


        public List<RadianContributor> ActiveParticipantsWithSoftware(int radianContributorTypeId)
        {
            string radianStatus = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Habilitado);
            return (from c in sqlDBContext.RadianContributors.Include("Contributor").Where(t => t.RadianState == radianStatus && t.RadianContributorTypeId == radianContributorTypeId).ToList()
                    join ope in sqlDBContext.RadianContributorOperations on c.Id equals ope.RadianContributorId
                    join s in sqlDBContext.RadianSoftwares on ope.SoftwareId equals s.Id
                    where ope.SoftwareType == (int)Domain.Common.RadianOperationModeTestSet.OwnSoftware
                    && ope.OperationStatusId == 4 //que haya pasado pruebas.
                    select c).Distinct().ToList();
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
                    radianContributorInstance.IsActive = radianContributor.IsActive;

                    context.Entry(radianContributorInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    radianContributor.Step = 1;
                    radianContributor.Update = DateTime.Now;
                    radianContributor.IsActive = true;
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

        public List<RadianSoftware> RadianSoftwareByParticipante(int radianContributorId)
        {
            int softwareSt = (int)RadianSoftwareStatus.Accepted;
            return (
                    from c in sqlDBContext.RadianContributors.Where(t => t.Id == radianContributorId).Include("Contributor").ToList()
                    join ope in sqlDBContext.RadianContributorOperations on c.Id equals ope.RadianContributorId
                    join s in sqlDBContext.RadianSoftwares on ope.SoftwareId equals s.Id
                    where s.RadianSoftwareStatusId == softwareSt
                    select s
                    ).Distinct().ToList();
        }
    }
}
