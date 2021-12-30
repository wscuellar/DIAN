using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class SoftwareService
    {
        SqlDBContext sqlDBContext;
        public SoftwareService()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public void AddOrUpdate(Software software)
        {
            using (var context = new SqlDBContext())
            {
                var softwareInstance = context.Softwares.FirstOrDefault(c => c.Id == software.Id);
                if (softwareInstance != null)
                {
                    softwareInstance.Deleted = software.Deleted;
                    softwareInstance.Name = software.Name;
                    softwareInstance.ContributorId = software.ContributorId;
                    softwareInstance.SoftwareUser = software.SoftwareUser;
                    softwareInstance.SoftwarePassword = software.SoftwarePassword;
                    softwareInstance.Url = software.Url;
                    softwareInstance.Updated = DateTime.UtcNow;
                    softwareInstance.AcceptanceStatusSoftwareId = software.AcceptanceStatusSoftwareId;
                    context.Entry(softwareInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(software).State = System.Data.Entity.EntityState.Added;
                }
                context.SaveChanges();
            }
        }

        public void SetToProduction(Software software)
        {
            try
            {
                using (var context = new SqlDBContext())
                {
                    var softwareInstance = context.Softwares.FirstOrDefault(c => c.Id == software.Id);
                    softwareInstance.AcceptanceStatusSoftwareId = software.AcceptanceStatusSoftwareId;
                    softwareInstance.AcceptanceStatusSoftwareId = (int)Domain.Common.SoftwareStatus.Production;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var logger = new GlobalLogger("SetSoftwareToProduction", software.Id.ToString())
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

        public IEnumerable<Software> GetSoftwares(string pin, int? contributorId, int page, int length)
        {
            var query = sqlDBContext.Softwares.Include("Contributor").Where(s => !s.Deleted
                        && (string.IsNullOrEmpty(pin) || s.Pin == pin)
                        && (contributorId == null || s.ContributorId == contributorId)
                        ).OrderByDescending(s => s.Id).Skip(page * length).Take(length);
            return query;
        }

        public Software Get(Guid id)
        {
            using (var context = new SqlDBContext())
            {
                return context.Softwares.AsNoTracking().FirstOrDefault(x => x.Id == id);
            }
                
        }

        public Software GetAndContributorAndAcceptanceStatus(Guid id)
        {
            using (var context = new SqlDBContext())
            {
                return context.Softwares.Include("AcceptanceStatusSoftware").Include("Contributor").AsNoTracking().FirstOrDefault(x => x.Id == id);
            }
        }


        public RadianSoftware GetByRadian(Guid id)
        {
            return sqlDBContext.RadianSoftwares.FirstOrDefault(x => x.Id == id);
        }


        public OtherDocElecSoftware GetByOtherDoc(Guid softwareId)
        {            
            using (var context = new SqlDBContext())
            {
                return context.OtherDocElecSoftwares.FirstOrDefault(x => x.SoftwareId == softwareId);
            }
        }


        public IEnumerable<Software> GetAll()
        {
            return sqlDBContext.Softwares;
        }

        public List<AcceptanceStatusSoftware> GetAcceptanceStatuses()
        {
            using (var context = new SqlDBContext())
            {
                return context.AcceptanceStatusesSoftware.ToList();
            }
        }

        public List<AcceptanceStatusSoftware> GetAcceptanceStatusesSoftware()
        {
            try
            {
                using (var context = sqlDBContext)
                {
                    return context.AcceptanceStatusesSoftware.ToList();
                }
            }
            catch (Exception)
            {
                return new List<AcceptanceStatusSoftware>();
            }
        }
        public List<Software> GetSoftwares(int contributorId)
        {
            using (var context = new SqlDBContext())
            {
                return context.Softwares.Where(p => !p.Deleted
                                                   && p.ContributorId == contributorId
                                                   && p.Status
                                                   && p.AcceptanceStatusSoftwareId != (int)Domain.Common.SoftwareStatus.Inactive).ToList();
            }
        }

        /// <summary>
        /// Buscar/Listar los Software por id del contribuyente que no esten eliminados y que esten activos o inactivos
        /// </summary>
        /// <param name="contributorId"></param>
        /// <param name="state">True. Activos, False: inactivos</param>
        /// <returns></returns>
        public List<Software> GetSoftwaresByContributorAndState(int contributorId, bool state)
        {
            using (var context = new SqlDBContext())
            {
                return context.Softwares.Where(p => !p.Deleted
                                                   && p.ContributorId == contributorId
                                                   && p.Status == state).ToList();
            }
        }


        private void RegisterException(GlobalLogger logger)
        {
            var tableManager = new TableManager("GlobalLogger");
            tableManager.InsertOrUpdate(logger);
        }

        #region RADIAN 


        public RadianSoftware GetRadianSoftware(Guid id)
        {
            return sqlDBContext.RadianSoftwares.FirstOrDefault(x => x.Id == id);
        }

        public void AddOrUpdateRadianSoftware(RadianSoftware software)
        {
            using (var context = new SqlDBContext())
            {
                var softwareInstance = context.RadianSoftwares.FirstOrDefault(c => c.Id == software.Id);
                if (softwareInstance != null)
                {
                    softwareInstance.Deleted = software.Deleted;
                    softwareInstance.Name = software.Name;
                    softwareInstance.ContributorId = software.ContributorId;
                    softwareInstance.SoftwareUser = software.SoftwareUser;
                    softwareInstance.SoftwarePassword = software.SoftwarePassword;
                    softwareInstance.Url = software.Url;
                    softwareInstance.Updated = DateTime.UtcNow;
                    softwareInstance.RadianSoftwareStatusId  = software.RadianSoftwareStatusId;
                    context.Entry(softwareInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(software).State = System.Data.Entity.EntityState.Added;
                }
                context.SaveChanges();
            }
        }

        #endregion

        #region OtherDocumentos

        public OtherDocElecSoftware GetOtherDocSoftware(Guid id)
        {
            using (var context = new SqlDBContext())
            {
                return context.OtherDocElecSoftwares.FirstOrDefault(x => x.Id == id);
            }
        }

        public void AddOrUpdateOtherDocSoftware(OtherDocElecSoftware software)
        {
            using (var context = new SqlDBContext())
            {
                var softwareInstance = context.OtherDocElecSoftwares.FirstOrDefault(c => c.Id == software.Id);

                if (softwareInstance != null)
                {
                    softwareInstance.Deleted = software.Deleted;
                    softwareInstance.Name = software.Name;
                    softwareInstance.OtherDocElecContributorId = software.OtherDocElecContributorId;
                    softwareInstance.SoftwareUser = software.SoftwareUser;
                    softwareInstance.SoftwarePassword = software.SoftwarePassword;
                    softwareInstance.Url = software.Url;
                    softwareInstance.Updated = DateTime.UtcNow;
                    softwareInstance.OtherDocElecSoftwareStatusId = software.OtherDocElecSoftwareStatusId;
                    softwareInstance.SoftwareId = software.SoftwareId;
                    softwareInstance.ProviderId = software.ProviderId;
                    context.Entry(softwareInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(software).State = System.Data.Entity.EntityState.Added;
                }
                context.SaveChanges();
            }
        }

        #endregion
    }
}
