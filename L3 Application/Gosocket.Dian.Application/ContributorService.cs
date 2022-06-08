using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class ContributorService : IContributorService
    {
        SqlDBContext sqlDBContext;
        private static readonly TableManager tableManager = new TableManager("GlobalLogger");
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
            //return sqlDBContext.Contributors.FirstOrDefault(x => x.Id == id);
            using (var context = new SqlDBContext())
            {
                return context.Contributors
                    .Include("ContributorOperations").AsNoTracking()
                    .Include("Softwares").AsNoTracking()
                    .FirstOrDefault(x => x.Id == id);
            }
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
            using (var context = new SqlDBContext())
            {
                return context.Contributors.Include("AcceptanceStatus").Include("ContributorOperations").AsNoTracking().FirstOrDefault(p => p.Code == code);                
            }
        }

        public RadianContributorOperation GetRadianOperations(int radianContributorId, string softwareId)
        {
            Guid SoftId = new Guid(softwareId);
            return sqlDBContext.RadianContributorOperations.FirstOrDefault(p => p.RadianContributorId == radianContributorId && p.SoftwareId == SoftId);
        }

        public OtherDocElecContributorOperations GetOtherDocOperations(int OtherDocContributorId, string softwareId)
        {
            Guid SoftId = new Guid(softwareId);
            return sqlDBContext.OtherDocElecContributorOperations.FirstOrDefault(p => p.OtherDocElecContributorId == OtherDocContributorId && p.SoftwareId == SoftId);
        }

        public List<OtherDocElecContributor> GetOtherDocElecContributor(int OtherDocContributorId)
        {
            
            return sqlDBContext.OtherDocElecContributors.Where(p => p.ContributorId == OtherDocContributorId && p.OtherDocElecOperationModeId==3).ToList();
        }

        public List<OtherDocElecContributor> GetOtherDocElecContributorPermisos(int OtherDocContributorId)
        {

            return sqlDBContext.OtherDocElecContributors.Where(p => p.ContributorId == OtherDocContributorId ).ToList();
        }

        public ContributorOperations GetContributorOperations(int OtherDocContributorId)
        {

            ContributorOperations contributorOp = null;
            var re = (from c in sqlDBContext.ContributorOperations
                      where c.ContributorId == OtherDocContributorId
                      select c).FirstOrDefault();

            if (re != null)
            {
                contributorOp = new ContributorOperations()
                {
                    Id = re.Id,
                    ContributorId = re.ContributorId,
                    OperationModeId = re.OperationModeId,
                    ProviderId = re.ProviderId,
                    SoftwareId = re.SoftwareId,
                    Deleted = re.Deleted,
                    Timestamp = re.Timestamp,
                    ContributorTypeId = re.ContributorTypeId
                };
            }

            return contributorOp;
        }
        public bool GetOtherDocElecContributorOperations(int OtherDocContributorId)
        {

            var resp = sqlDBContext.OtherDocElecContributorOperations.Where(p => p.OtherDocElecContributorId == OtherDocContributorId && p.Deleted==false).Count();
            return resp > 0 ? true : false;
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
                var contributorInstance = context.Contributors.FirstOrDefault(c => c.Code == contributor.Code);
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
                var r = context.OperationModes.FirstOrDefault(x => x.Id == id);
                if (r == null)
                    return new OperationMode();
                else
                    return r;
            }
        }

        /// <summary>
        /// Retornar todos los Modos de Operación
        /// </summary>
        /// <returns></returns>
        public List<OperationMode> GetOperationModes()
        {
            using (var context = new SqlDBContext())
            {
                return context.OperationModes.ToList();
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
            
            tableManager.InsertOrUpdate(logger);
        }


        /// <summary>
        /// Consulta contribuyentes del catalogo que sean habilitados (AcceptanceStatusId == 4) que esten activo y no eliminados
        /// con software en produccion habilitado y no eliminado
        /// con operaciones con software propio no eliminado
        /// </summary>
        /// <param name="contributorid"></param>
        /// <returns></returns>
        public List<Software> GetBaseSoftwareForRadian(int contributorid)
        {
            using (var context = new SqlDBContext())
            {
                return (from c in context.Contributors
                        from s in c.Softwares
                        join cp in context.ContributorOperations on s.Id equals cp.SoftwareId
                        where
                            c.Id == contributorid
                        && c.Status
                        && !c.Deleted
                        && s.Status
                        && !s.Deleted
                        && cp.OperationModeId == 2
                        && !cp.Deleted
                        select s).ToList();
            }
        }


        public void SetToEnabledOtherDocElecContributor(string softwareId, int softwareType, int otherDocElecContributorId)
        {
            using (var context = new SqlDBContext())
            {
                //Ubicar el participante que se habilitara
                OtherDocElecContributor otherDocElec = context.OtherDocElecContributors.FirstOrDefault(t => t.Id == otherDocElecContributorId);

                if (otherDocElec != null)
                {
                    //Cambiar estado habilitado
                    otherDocElec.State = Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Habilitado);
                    otherDocElec.Step = 3;
                    otherDocElec.Update = System.DateTime.Now;
                    Guid softId = new Guid(softwareId);

                    
                    OtherDocElecSoftware soft = context.OtherDocElecSoftwares.FirstOrDefault(t => t.Id.ToString().Equals(softId.ToString(), StringComparison.OrdinalIgnoreCase));
                    soft.OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted;

                    //Se ubica la operacion del participante para habilitarla
                    OtherDocElecContributorOperations otherDocOperation = context.OtherDocElecContributorOperations.FirstOrDefault(
                                                 t => t.OtherDocElecContributorId == otherDocElec.Id
                                                 && t.SoftwareType == softwareType
                                                 && t.SoftwareId.ToString().Trim().ToLower().Equals(softId.ToString().Trim().ToLower(), StringComparison.OrdinalIgnoreCase)
                                                 && t.OperationStatusId == (int)Domain.Common.OtherDocElecState.Test
                                                 && !t.Deleted
                                                 );


                    if (otherDocOperation != null)
                        otherDocOperation.OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado; //Aceptada la operacion.

                    context.SaveChanges();
                }
            }
        }

        public void SetToEnabledRadian(int contributorId, int contributorTypeId, string softwareId, int softwareType)
        {
            using (var context = new SqlDBContext())
            {
                //Ubicar el participante que se habilitara
                RadianContributor radianc = context.RadianContributors.FirstOrDefault(t => t.ContributorId == contributorId
                                        && t.RadianContributorTypeId == contributorTypeId);

                if (radianc != null)
                {
                    //Cambiar estado habilitado
                    radianc.RadianState = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Habilitado);
                    radianc.Step = 4;
                    radianc.Update = System.DateTime.Now;
                    Guid softId = new Guid(softwareId);

                    //Si es software propio lo habilita.
                    if (radianc.RadianOperationModeId == (int)Domain.Common.RadianOperationMode.Direct)
                    {
                        RadianSoftware soft = context.RadianSoftwares.FirstOrDefault(t => t.Id.ToString().Equals(softId.ToString(), StringComparison.OrdinalIgnoreCase));
                        soft.RadianSoftwareStatusId = (int)Domain.Common.RadianSoftwareStatus.Accepted;
                    }

                    //Se ubica la operacion del participante para habilitarla
                    RadianContributorOperation radianOperation = context.RadianContributorOperations.FirstOrDefault(
                                                 t => t.RadianContributorId == radianc.Id
                                                 && t.SoftwareType == softwareType
                                                 && t.SoftwareId.ToString().Trim().ToLower().Equals(softId.ToString().Trim().ToLower(), StringComparison.OrdinalIgnoreCase)
                                                 && t.OperationStatusId == (int)Domain.Common.RadianState.Test
                                                 && !t.Deleted 
                                                 );
                    if (radianOperation != null)
                        radianOperation.OperationStatusId = (int)Domain.Common.RadianState.Habilitado; //Aceptada la operacion.

                    context.SaveChanges();
                }
            }
        }

        public void OperationRejectOtherDoc(int otherDocElecContributorId, string softwareId, int softwareType)
        {
            using (var context = new SqlDBContext())
            {
                OtherDocElecContributor otherDoc = context.OtherDocElecContributors.FirstOrDefault(t => t.Id == otherDocElecContributorId);

                OtherDocElecContributorOperations otherDocOperation = context.OtherDocElecContributorOperations.FirstOrDefault(
                                                t => t.OtherDocElecContributorId == otherDoc.Id
                                                && t.SoftwareType == softwareType
                                                && t.SoftwareId.ToString().Trim().ToLower().Equals(softwareId.ToString().Trim().ToLower(), StringComparison.OrdinalIgnoreCase)
                                                && !t.Deleted
                                                );
                if (otherDocOperation != null)
                {
                    if (
                        !(
                            otherDoc.ElectronicDocumentId == (int)Domain.Common.ElectronicsDocuments.ElectronicEquivalent &&
                            otherDocOperation.OperationStatusId == (int)Domain.Common.OtherDocElecState.Habilitado
                        )
                    )
                    {
                        otherDocOperation.OperationStatusId = (int)Domain.Common.OtherDocElecState.Cancelado; //cancelo = rechazo.
                    }
                }
                     
                context.SaveChanges();
            }
        }



        public void OperationUpdate(int contributorId, int contributorTypeId, string softwareId, int softwareType, Domain.Common.RadianState radianState)
        {
            using (var context = new SqlDBContext())
            {
                RadianContributor radianc = context.RadianContributors.FirstOrDefault(t => t.ContributorId == contributorId
                                      && t.RadianContributorTypeId == contributorTypeId);

                RadianContributorOperation radianOperation = context.RadianContributorOperations.FirstOrDefault(
                                                t => t.RadianContributorId == radianc.Id
                                                && t.SoftwareType == softwareType
                                                && t.SoftwareId.ToString().Trim().ToLower().Equals(softwareId.ToString().Trim().ToLower(), StringComparison.OrdinalIgnoreCase)
                                                && !t.Deleted
                                                );
                if (radianOperation != null)
                    radianOperation.OperationStatusId = (int)radianState; //cancelo = rechazo.

                context.SaveChanges();
            }
        }

        #region Funciones Radian para Migracion


        /// <summary>
        /// Metodo que devuelve un objeto RadianContributor desde SQL
        /// </summary>
        /// <param name="radianContributorId">Id a ubicar</param>
        /// <returns>Un objeto RadianCotributor buscado</returns>
        public RadianContributor GetRadian(int contributorId, int contributorTypeId)
        {
            using (var context = new SqlDBContext())
            {
                return context.RadianContributors.FirstOrDefault(rc => rc.ContributorId == contributorId && rc.RadianContributorTypeId == contributorTypeId);
            }
        }

        /// <summary>
        /// Metodo que devuelve un objeto OtherDocElecContributor desde SQL
        /// </summary>
        /// <param name="radianContributorId">Id a ubicar</param>
        /// <returns>Un objeto RadianCotributor buscado</returns>
        public OtherDocElecContributor GetOtherDocElecContributorID(int contributorId, int contributorTypeId, int operationModeId)
        {
            using (var context = new SqlDBContext())
            {
                return context.OtherDocElecContributors.FirstOrDefault(rc => rc.ContributorId == contributorId 
                && rc.OtherDocElecContributorTypeId == contributorTypeId
                && rc.OtherDocElecOperationModeId == operationModeId);
            }
        }

        /// <summary>
        /// Metodo que devuelve un objeto OtherDocElecContributor desde SQL
        /// </summary>
        /// <param name="radianContributorId">Id a ubicar</param>
        /// <returns>Un objeto RadianCotributor buscado</returns>
        public OtherDocElecContributor GetOtherDocElecContributorById(int otherDocElecContributorId, bool enabled)
        {
            using (var context = new SqlDBContext())
            {
                if (enabled)
                {
                    return context.OtherDocElecContributors.FirstOrDefault(rc => rc.Id == otherDocElecContributorId
                     && rc.State == "Habilitado");
                }
                else
                {
                    return context.OtherDocElecContributors.FirstOrDefault(rc => rc.Id == otherDocElecContributorId);
                }
            }
        }

        /// <summary>
        /// Metodo que activa el RadianContributor en Produccion, lo ubica y cambia los Estados necesarios
        /// </summary>
        /// <param name="contributor">Los datos del radian Contributor a actualizar</param>
        public void ActivateRadian(RadianContributor contributor)
        {
            using (var context = new SqlDBContext())
            {
                var contributorInstance = context.RadianContributors.FirstOrDefault(c => c.Id == contributor.Id);
                if (contributorInstance != null)
                {
                    contributorInstance.RadianState = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Habilitado);
                    contributorInstance.RadianContributorTypeId = contributor.RadianContributorTypeId;
                    contributorInstance.Update = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Metodo que activa el RadianContributor en Produccion, lo ubica y cambia los Estados necesarios
        /// </summary>
        /// <param name="contributor">Los datos del radian Contributor a actualizar</param>
        public void ActivateOtherDocument(OtherDocElecContributor contributor)
        {
            using (var context = new SqlDBContext())
            {
                var contributorInstance = context.OtherDocElecContributors.FirstOrDefault(c => c.Id == contributor.Id);
                if (contributorInstance != null)
                {
                    contributorInstance.State = Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Habilitado);
                    contributorInstance.OtherDocElecContributorTypeId = contributor.OtherDocElecContributorTypeId;
                    contributorInstance.Update = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
        }

        public int AddOrUpdateRadianContributor(RadianContributor radianContributor)
        {
            using (var context = new SqlDBContext())
            {
                RadianContributor radianContributorInstance =
                    context.RadianContributors.FirstOrDefault(c => c.Id == radianContributor.Id);

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
                    radianContributor.Step = 4;
                    radianContributor.Update = DateTime.Now;
                    context.Entry(radianContributor).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();

                return radianContributorInstance != null ? radianContributorInstance.Id : radianContributor.Id;
            }
        }

        public int AddOrUpdateOtherDocContributor(OtherDocElecContributor otherDocContributor)
        {
            using (var context = new SqlDBContext())
            {
                OtherDocElecContributor otherDocContributorInstance =
                    context.OtherDocElecContributors.FirstOrDefault(c => c.Id == otherDocContributor.Id);


                if (otherDocContributorInstance != null)
                {
                    otherDocContributorInstance.OtherDocElecContributorTypeId = otherDocContributor.OtherDocElecContributorTypeId;
                    otherDocContributorInstance.Update = DateTime.Now;
                    otherDocContributorInstance.State = otherDocContributor.State;
                    otherDocContributorInstance.OtherDocElecOperationModeId = otherDocContributor.OtherDocElecOperationModeId;
                    otherDocContributorInstance.CreatedBy = otherDocContributor.CreatedBy;
                    otherDocContributorInstance.Description = otherDocContributor.Description;
                    otherDocContributorInstance.Step = otherDocContributor.Step == 0 ? 1 : otherDocContributor.Step;
                    otherDocContributorInstance.ElectronicDocumentId = otherDocContributor.ElectronicDocumentId;


                    context.Entry(otherDocContributorInstance).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    otherDocContributor.Step = 3;
                    otherDocContributor.Update = DateTime.Now;
                    context.Entry(otherDocContributor).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();

                return otherDocContributorInstance != null ? otherDocContributorInstance.Id : 0;
            }
        }


        public int AddRadianOperation(RadianContributorOperation operation)
        {
            int affectedRecords = 0;
            using (var context = new SqlDBContext())
            {
                context.Entry(operation).State = System.Data.Entity.EntityState.Added;
                affectedRecords = context.SaveChanges();
            }
            return affectedRecords > 0 ? operation.Id : 0;
        }

        public int AddOtherDocOperation(OtherDocElecContributorOperations operation)
        {
            int affectedRecords = 0;
            using (var context = new SqlDBContext())
            {
                context.Entry(operation).State = System.Data.Entity.EntityState.Added;
                affectedRecords = context.SaveChanges();
            }
            return affectedRecords > 0 ? operation.Id : 0;
        }

        public bool UpdateRadianOperation(RadianContributorOperation contributorOperation)
        {
            using (var context = new SqlDBContext())
            {
                var radianContributorOperationInstance = context.RadianContributorOperations.FirstOrDefault(c => c.Id == contributorOperation.Id);

                radianContributorOperationInstance.OperationStatusId = contributorOperation.OperationStatusId;
                radianContributorOperationInstance.RadianContributorId = contributorOperation.RadianContributorId;
                radianContributorOperationInstance.SoftwareId = contributorOperation.SoftwareId;
                radianContributorOperationInstance.SoftwareType = contributorOperation.SoftwareType;
                radianContributorOperationInstance.Deleted = contributorOperation.Deleted;
                radianContributorOperationInstance.Timestamp = System.DateTime.Now;
                context.Entry(radianContributorOperationInstance).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                return true;
            }
        }

        public bool UpdateOtherDocOperation(OtherDocElecContributorOperations contributorOperation)
        {
            using (var context = new SqlDBContext())
            {
                var otherDocContributorOperationInstance = context.OtherDocElecContributorOperations.FirstOrDefault(c => c.Id == contributorOperation.Id);

                otherDocContributorOperationInstance.OperationStatusId = contributorOperation.OperationStatusId;
                otherDocContributorOperationInstance.OtherDocElecContributorId = contributorOperation.OtherDocElecContributorId;
                otherDocContributorOperationInstance.SoftwareId = contributorOperation.SoftwareId;
                otherDocContributorOperationInstance.SoftwareType = contributorOperation.SoftwareType;
                otherDocContributorOperationInstance.Deleted = contributorOperation.Deleted;
                otherDocContributorOperationInstance.Timestamp = System.DateTime.Now;
                context.Entry(otherDocContributorOperationInstance).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                return true;
            }
        }
       
        #endregion

        public Contributor GetContributorById(int Id, int contributorTypeId)
        {
            Contributor contributor = null;
            var re = (from c in sqlDBContext.Contributors
                      where c.Id == Id
                      select c).FirstOrDefault();

            if (re != null)
            {
                contributor = new Contributor()
                {
                    Id = re.Id,
                    Code = re.Code,
                    Name = re.Name,
                    BusinessName = re.BusinessName,
                    Email = re.Email,
                    StartDate = re.StartDate,
                    EndDate = re.EndDate,
                    StartDateNumber = re.StartDateNumber,
                    AcceptanceStatus = re.AcceptanceStatus,
                    AcceptanceStatusId = re.AcceptanceStatusId,
                    Status = re.Status,
                    Deleted = re.Deleted,
                    Timestamp = re.Timestamp,
                    Updated = re.Updated,
                    CreatedBy = re.CreatedBy,
                    ContributorTypeId = re.ContributorTypeId,
                    OperationModeId = re.OperationModeId,
                    ProviderId = re.ProviderId,
                    PrincipalActivityCode = re.PrincipalActivityCode,
                    PersonType = re.PersonType,
                    HabilitationDate = re.HabilitationDate,
                    ProductionDate = re.ProductionDate,
                    StatusRut = re.StatusRut,
                    ExchangeEmail = re.ExchangeEmail
                };
            }

            return contributor;
        }

    }
}