using Gosocket.Dian.DataContext;
using Gosocket.Dian.DataContext.Middle;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class OthersDocsElecContributorService : IOthersDocsElecContributorService
    {
        private static readonly TableManager testSetManager = new TableManager("GlobalTestSetOthersDocuments");
        private readonly SqlDBContext sqlDBContext;
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

        public Gosocket.Dian.Domain.Sql.OtherDocElecOperationMode GetOperationModeById(int operationModeId)
        {
            using (var context = new SqlDBContext())
            {
                return context.OtherDocElecOperationModes.FirstOrDefault(t => t.Id == operationModeId);
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

        public OtherDocElecContributor CreateContributor(int contributorId, OtherDocElecState State,
           int ContributorType, int OperationMode, int ElectronicDocumentId, string createdBy)
        {
            var state = OtherDocElecState.Cancelado.GetDescription();
            OtherDocElecContributor existing = _othersDocsElecContributorRepository.Get(t => t.ContributorId == contributorId
                                                                                     && t.OtherDocElecContributorTypeId == ContributorType
                                                                                     && t.OtherDocElecOperationModeId == OperationMode
                                                                                     && t.ElectronicDocumentId == ElectronicDocumentId
                                                                                     && t.State != state);
            // Si ya esta habilitado, no se debe actualizar el estado...
            if (existing != null && existing.State == OtherDocElecState.Habilitado.GetDescription()) return existing;

            OtherDocElecContributor newContributor = new OtherDocElecContributor()
            {
                Id = existing != null ? existing.Id : 0,
                ContributorId = contributorId,
                CreatedBy = createdBy,
                OtherDocElecContributorTypeId = ContributorType,
                OtherDocElecOperationModeId = OperationMode,
                ElectronicDocumentId = ElectronicDocumentId,
                State = State.GetDescription(),
                CreatedDate = existing != null ? existing.CreatedDate : DateTime.Now
            };

            newContributor.Id = _othersDocsElecContributorRepository.AddOrUpdate(newContributor);

            return newContributor;
        }

        public OtherDocElecContributor CreateContributorNew(int contributorId, OtherDocElecState State,
           int ContributorType, int OperationMode, int ElectronicDocumentId, string createdBy)
        {
            var state = OtherDocElecState.Cancelado.GetDescription();
            var state2 = OtherDocElecState.Habilitado.GetDescription();
            OtherDocElecContributor existing = _othersDocsElecContributorRepository.Get(t => t.ContributorId == contributorId
                                                                                     && t.OtherDocElecContributorTypeId == ContributorType
                                                                                     && t.OtherDocElecOperationModeId == OperationMode
                                                                                     && t.ElectronicDocumentId == ElectronicDocumentId
                                                                                     && t.State != state && t.State != state2);
            // Si ya esta habilitado, no se debe actualizar el estado...
            //if (existing != null && existing.State == OtherDocElecState.Habilitado.GetDescription()) return existing;

            OtherDocElecContributor newContributor = new OtherDocElecContributor()
            {
                Id = existing != null ? existing.Id : 0,
                ContributorId = contributorId,
                CreatedBy = createdBy,
                OtherDocElecContributorTypeId = ContributorType,
                OtherDocElecOperationModeId = OperationMode,
                ElectronicDocumentId = ElectronicDocumentId,
                State = State.GetDescription(),
                CreatedDate = existing != null ? existing.CreatedDate : DateTime.Now
            };

            newContributor.Id = _othersDocsElecContributorRepository.AddOrUpdate(newContributor);

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

            List<string> softwareAccepted = new List<string>();
            foreach (var item in contributorOperations)
            {
                //GlobalTestSetResult testset = GetTestSetResult(testSetResults, item, contributor.ContributorTypeId.Value);
                // if (((TestSetStatus)testset.Status) == TestSetStatus.Accepted)
                // softwareAccepted.Add(testset.SoftwareId);
            }

            return softwareAccepted;
        }

        public List<OtherDocElecContributor> ValidateExistenciaContribuitor(int ContributorId, int contributorTypeId, int OperationModeId, string state)
        {
            return _othersDocsElecContributorRepository.List(t => t.ContributorId == ContributorId && 
                                                                  t.OtherDocElecContributorTypeId == contributorTypeId &&
                                                                  t.OtherDocElecOperationModeId == OperationModeId && 
                                                                  t.State != state).Results;
        }

        public bool ValidateSoftwareActive(int ContributorId, int ContributorTypeId, int OperationModeId, int stateSofware)
        {
            return _othersDocsElecContributorRepository.GetParticipantWithActiveProcess(ContributorId, ContributorTypeId, OperationModeId, stateSofware);
        }

        public PagedResult<OtherDocsElectData> List(int contributorId, int contributorTypeId, int operationModeId, int electronicDocumentId)
        {
            //IQueryable<OtherDocsElectData> query = (from oc in sqlDBContext.OtherDocElecContributors
            //                                        join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
            //                                        join oco in sqlDBContext.OtherDocElecContributorOperations on s.Id equals oco.SoftwareId
            //                                        join ocs in sqlDBContext.OtherDocElecSoftwareStatus on s.OtherDocElecSoftwareStatusId equals ocs.Id
            //                                        join ope in sqlDBContext.OtherDocElecOperationModes on oc.OtherDocElecOperationModeId equals ope.Id
            //                                        join oty in sqlDBContext.OtherDocElecContributorTypes on oc.OtherDocElecContributorTypeId equals oty.Id
            //                                        join eld in sqlDBContext.ElectronicDocuments on oc.ElectronicDocumentId equals eld.Id
            //                                        where oc.ContributorId == contributorId
            //                                            && oc.OtherDocElecContributorTypeId == contributorTypeId
            //                                            && oc.OtherDocElecOperationModeId == operationModeId
            //                                            && oc.State != "Cancelado"
            //                                            && s.Deleted == false
            //                                            && oco.Deleted == false
            //                                        select new OtherDocsElectData()
            //                                        {
            //                                            Id = oc.Id,
            //                                            ContributorId = oc.ContributorId,
            //                                            OperationMode = ope.Name,
            //                                            ContributorType = oty.Name,
            //                                            Software = s.Name,
            //                                            PinSW = s.Pin,
            //                                            SoftwareId = s.SoftwareId.ToString(),
            //                                            //StateSoftware = ocs.Name,
            //                                            StateSoftware = oco.OperationStatusId.ToString(),
            //                                            StateContributor = oc.State,
            //                                            CreatedDate = oc.CreatedDate,
            //                                            ElectronicDoc = eld.Name,
            //                                            Url = s.Url,
            //                                        }).Distinct();
            //return query.Paginate(0, 100, t => t.Id.ToString());

            IQueryable<OtherDocsElectData> query = (from oc in sqlDBContext.OtherDocElecContributors
                                                    join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
                                                    join oco in sqlDBContext.OtherDocElecContributorOperations on s.Id equals oco.SoftwareId
                                                    join ocs in sqlDBContext.OtherDocElecSoftwareStatus on s.OtherDocElecSoftwareStatusId equals ocs.Id
                                                    join ope in sqlDBContext.OtherDocElecOperationModes on oc.OtherDocElecOperationModeId equals ope.Id
                                                    join oty in sqlDBContext.OtherDocElecContributorTypes on oc.OtherDocElecContributorTypeId equals oty.Id
                                                    join eld in sqlDBContext.ElectronicDocuments on oc.ElectronicDocumentId equals eld.Id
                                                    where oc.ContributorId == contributorId
                                                        && oc.OtherDocElecContributorTypeId == contributorTypeId
                                                        && oc.OtherDocElecOperationModeId == operationModeId
                                                        && oc.State != "Cancelado"
                                                        && s.Deleted == false
                                                        && oco.Deleted == false
                                                        && eld.Id == electronicDocumentId
                                                    select new OtherDocsElectData()
                                                    {
                                                        //Id = oc.Id,
                                                        Id = oco.Id,
                                                        ContributorId = oc.ContributorId,
                                                        OperationMode = ope.Name,
                                                        ContributorType = oty.Name,
                                                        Software = s.Name,
                                                        PinSW = s.Pin,
                                                        SoftwareId = s.SoftwareId.ToString(),
                                                        StateSoftware = oco.OperationStatusId.ToString(),
                                                        StateContributor = oc.State,
                                                        //CreatedDate = oc.CreatedDate,
                                                        CreatedDate = s.SoftwareDate.Value,
                                                        ElectronicDoc = eld.Name,
                                                        Url = s.Url,
                                                    }).Distinct();
            return query.Paginate(0, 100, t => t.Id.ToString());
        }

        public PagedResult<OtherDocsElectData> List2(int contributorId)
        {
            IQueryable<OtherDocsElectData> query = (from oc in sqlDBContext.OtherDocElecContributors
                                                    join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
                                                    join oco in sqlDBContext.OtherDocElecContributorOperations on s.Id equals oco.SoftwareId
                                                    join ocs in sqlDBContext.OtherDocElecSoftwareStatus on s.OtherDocElecSoftwareStatusId equals ocs.Id
                                                    join ope in sqlDBContext.OtherDocElecOperationModes on oc.OtherDocElecOperationModeId equals ope.Id
                                                    join oty in sqlDBContext.OtherDocElecContributorTypes on oc.OtherDocElecContributorTypeId equals oty.Id
                                                    join eld in sqlDBContext.ElectronicDocuments on oc.ElectronicDocumentId equals eld.Id
                                                    where oc.ContributorId == contributorId
                                                        && oc.State != "Cancelado"
                                                        && s.Deleted == false
                                                        && oco.Deleted == false
                                                    select new OtherDocsElectData()
                                                    {
                                                        //Id = oc.Id,
                                                        Id = oco.Id,
                                                        ContributorId = oc.ContributorId,
                                                        OperationMode = ope.Name,
                                                        ContributorType = oty.Name,
                                                        Software = s.Name,
                                                        PinSW = s.Pin,
                                                        SoftwareId = s.SoftwareId.ToString(),
                                                        StateSoftware = oco.OperationStatusId.ToString(),
                                                        StateContributor = oc.State,
                                                        //CreatedDate = oc.CreatedDate,
                                                        CreatedDate = s.SoftwareDate.Value,
                                                        ElectronicDoc = eld.Name,
                                                        Url = s.Url,
                                                    }).Distinct();
            return query.Paginate(0, 100, t => t.Id.ToString());
        }

        public OtherDocsElectData GetCOntrinutorODE(int Id)
        {
            var entity = (from oc in sqlDBContext.OtherDocElecContributors
                          join ope in sqlDBContext.OtherDocElecOperationModes on oc.OtherDocElecOperationModeId equals ope.Id
                          join oty in sqlDBContext.OtherDocElecContributorTypes on oc.OtherDocElecContributorTypeId equals oty.Id
                          join eld in sqlDBContext.ElectronicDocuments on oc.ElectronicDocumentId equals eld.Id                          
                          join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
                          join odeco in sqlDBContext.OtherDocElecContributorOperations on new { colA = s.Id, colB = oc.Id } equals new { colA = odeco.SoftwareId, colB = odeco.OtherDocElecContributorId }                                                                             
                          where oc.Id == Id
                           && oc.State != "Cancelado"
                           && s.Deleted == false

                          select new OtherDocsElectData()
                          {
                              Id = oc.Id,
                              ContributorId = oc.ContributorId,
                              OperationMode = ope.Name,
                              ContributorType = oty.Name,
                              StateContributor = oc.State,
                              CreatedDate = oc.CreatedDate,
                              ElectronicDoc = eld.Name,
                              OperationModeId = oc.OtherDocElecOperationModeId,
                              ContributorTypeId = oc.OtherDocElecContributorTypeId,
                              ElectronicDocId = oc.ElectronicDocumentId,
                              ProviderId = s.ProviderId == 0 ? oc.ContributorId : s.ProviderId,
                              Step = oc.Step,
                              State = oc.State,
                              SoftwareId = s.Id.ToString(),
                              SoftwareIdBase=s.SoftwareId
                          }).Distinct().ToList();

            if (entity.Any())
            {
                OtherDocsElectData _entity = entity.Where(e => e.ProviderId != 0).FirstOrDefault();
                if (_entity != null)
                {
                    List<string> userIds = _contributorService.GetUserContributors(_entity.ContributorId).Select(u => u.UserId).ToList();
                    _entity.LegalRepresentativeIds = userIds;
                    return _entity;
                }
            }            

            return new OtherDocsElectData();
        }

        public PagedResult<OtherDocsElectData> List3(int contributorId, int contributorTypeId, int electronicDocumentId)
        {
            //IQueryable<OtherDocsElectData> query = (from oc in sqlDBContext.OtherDocElecContributors
            //                                        join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
            //                                        join oco in sqlDBContext.OtherDocElecContributorOperations on s.Id equals oco.SoftwareId
            //                                        join ocs in sqlDBContext.OtherDocElecSoftwareStatus on s.OtherDocElecSoftwareStatusId equals ocs.Id
            //                                        join ope in sqlDBContext.OtherDocElecOperationModes on oc.OtherDocElecOperationModeId equals ope.Id
            //                                        join oty in sqlDBContext.OtherDocElecContributorTypes on oc.OtherDocElecContributorTypeId equals oty.Id
            //                                        join eld in sqlDBContext.ElectronicDocuments on oc.ElectronicDocumentId equals eld.Id
            //                                        where oc.ContributorId == contributorId
            //                                            && oc.OtherDocElecContributorTypeId == contributorTypeId
            //                                            && oc.OtherDocElecOperationModeId == operationModeId
            //                                            && oc.State != "Cancelado"
            //                                            && s.Deleted == false
            //                                            && oco.Deleted == false
            //                                        select new OtherDocsElectData()
            //                                        {
            //                                            Id = oc.Id,
            //                                            ContributorId = oc.ContributorId,
            //                                            OperationMode = ope.Name,
            //                                            ContributorType = oty.Name,
            //                                            Software = s.Name,
            //                                            PinSW = s.Pin,
            //                                            SoftwareId = s.SoftwareId.ToString(),
            //                                            //StateSoftware = ocs.Name,
            //                                            StateSoftware = oco.OperationStatusId.ToString(),
            //                                            StateContributor = oc.State,
            //                                            CreatedDate = oc.CreatedDate,
            //                                            ElectronicDoc = eld.Name,
            //                                            Url = s.Url,
            //                                        }).Distinct();
            //return query.Paginate(0, 100, t => t.Id.ToString());

            IQueryable<OtherDocsElectData> query = (from oc in sqlDBContext.OtherDocElecContributors
                                                    join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
                                                    join oco in sqlDBContext.OtherDocElecContributorOperations on new { SoftwareId = s.Id, s.OtherDocElecContributorId  } equals new { oco.SoftwareId, oco.OtherDocElecContributorId } 
                                                    join ocs in sqlDBContext.OtherDocElecSoftwareStatus on s.OtherDocElecSoftwareStatusId equals ocs.Id
                                                    join ope in sqlDBContext.OtherDocElecOperationModes on oc.OtherDocElecOperationModeId equals ope.Id
                                                    join oty in sqlDBContext.OtherDocElecContributorTypes on oc.OtherDocElecContributorTypeId equals oty.Id
                                                    join eld in sqlDBContext.ElectronicDocuments on oc.ElectronicDocumentId equals eld.Id
                                                    where oc.ContributorId == contributorId
                                                        && oc.OtherDocElecContributorTypeId == contributorTypeId
                                                        && oc.State != "Cancelado"
                                                        && s.Deleted == false
                                                        && oco.Deleted == false
                                                        && eld.Id == electronicDocumentId
                                                    select new OtherDocsElectData()
                                                    {
                                                        //Id = oc.Id,
                                                        Id = oco.Id,
                                                        ContributorId = oc.ContributorId,
                                                        OperationMode = ope.Name,
                                                        ContributorType = oty.Name,
                                                        Software = s.Name,
                                                        PinSW = s.Pin,
                                                        SoftwareId = s.SoftwareId.ToString(),
                                                        StateSoftware = oco.OperationStatusId.ToString(),
                                                        StateContributor = oc.State,
                                                        //CreatedDate = oc.CreatedDate,
                                                        CreatedDate = s.SoftwareDate.Value,
                                                        ElectronicDoc = eld.Name,
                                                        Url = s.Url,
                                                    }).Distinct();
            return query.Paginate(0, 100, t => t.Id.ToString());
        }

        /// <summary>
        /// Cancelar un registro en la tabla OtherDocElecContributor
        /// </summary>
        /// <param name="contributorId">OtherDocElecContributorId</param>
        /// <param name="description">Motivo por el cual se hace la cancelación</param>
        /// <returns></returns>
        public ResponseMessage CancelRegister(int operationId, string description)
        {
            ResponseMessage result = new ResponseMessage();

            var operation = sqlDBContext.OtherDocElecContributorOperations
                .Include(t => t.OtherDocElecContributor.Contributor.OtherDocElecContributors)
                .FirstOrDefault(c => c.Id == operationId);
            if (operation != null)
            {
                operation.Deleted = true;

                sqlDBContext.Entry(operation).State = System.Data.Entity.EntityState.Modified;

                int re1 = sqlDBContext.SaveChanges();
                result.Code = System.Net.HttpStatusCode.OK.GetHashCode();
                result.Message = "Se canceló el registro exitosamente";

                result.ExistOperationModeAsociated = operation.OtherDocElecContributor.Contributor.OtherDocElecContributors
                    .Any(t => t.ElectronicDocumentId == operation.OtherDocElecContributor.ElectronicDocumentId && t.OtherDocElecContributorOperations.Any(x => !x.Deleted));

                if (re1 > 0) //Update operations state
                {
                    var software = sqlDBContext.OtherDocElecSoftwares
                        .FirstOrDefault(c => c.OtherDocElecContributorId == operation.OtherDocElecContributorId && c.Id == operation.SoftwareId);

                    if (software != null)
                    {
                        software.Deleted = true;
                        software.Status = false;
                        software.Updated = DateTime.Now;

                        sqlDBContext.Entry(software).State = System.Data.Entity.EntityState.Modified;

                        int re3 = sqlDBContext.SaveChanges();

                        if (re3 > 0) //Update Software SUCCESS
                        {

                        }
                    }
                }
            }
            else
            {
                result.Code = System.Net.HttpStatusCode.NotFound.GetHashCode();
                result.Message = System.Net.HttpStatusCode.NotFound.ToString();
            }

            return result;
        }

        public GlobalTestSetOthersDocuments GetTestResult(int OperatonModeId, int ElectronicDocumentId)
        {
            var _OperationMode = sqlDBContext.OtherDocElecOperationModes.FirstOrDefault(c => c.Id == OperatonModeId);
            var res = testSetManager.GetOthersDocuments<GlobalTestSetOthersDocuments>(ElectronicDocumentId.ToString(), _OperationMode.OperationModeId.ToString());
            if (res != null && res.Count > 0)
                return res.FirstOrDefault();
            else
                return null;
        }

        public List<OtherDocElecContributor> GetDocElecContributorsByContributorId(int contributorId)
        {
            return _othersDocsElecContributorRepository
                .List(x => x.ContributorId == contributorId && x.OtherDocElecContributorOperations.Any(z => z.Deleted == false), 0, 0)
                .Results
                .Distinct()
                .ToList();
        }

        public List<Contributor> GetTechnologicalProviders(int contributorId, int electronicDocumentId, int contributorTypeId, string state)
        {
            return this._othersDocsElecContributorRepository.GetTechnologicalProviders(contributorId, electronicDocumentId, contributorTypeId, state);
        }

        public int NumHabilitadosOtherDocsElect(int contributorId)
        { 
            IQueryable<OtherDocsElectData> query = (from oc in sqlDBContext.OtherDocElecContributors
                                                    join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
                                                    join oco in sqlDBContext.OtherDocElecContributorOperations on s.Id equals oco.SoftwareId
                                                    join ocs in sqlDBContext.OtherDocElecSoftwareStatus on s.OtherDocElecSoftwareStatusId equals ocs.Id
                                                    join ope in sqlDBContext.OtherDocElecOperationModes on oc.OtherDocElecOperationModeId equals ope.Id
                                                    join oty in sqlDBContext.OtherDocElecContributorTypes on oc.OtherDocElecContributorTypeId equals oty.Id
                                                    join eld in sqlDBContext.ElectronicDocuments on oc.ElectronicDocumentId equals eld.Id
                                                    where oc.ContributorId == contributorId
                                                        && oc.State != "Cancelado"
                                                        && s.Deleted == false
                                                        && oco.Deleted == false
                                                        && oco.OperationStatusId == (int)OtherDocElecState.Habilitado
                                                    select new OtherDocsElectData()
                                                    {
                                                        //Id = oc.Id,
                                                        Id = oco.Id,
                                                        ContributorId = oc.ContributorId,
                                                        OperationMode = ope.Name,
                                                        ContributorType = oty.Name,
                                                        Software = s.Name,
                                                        PinSW = s.Pin,
                                                        SoftwareId = s.SoftwareId.ToString(),
                                                        StateSoftware = oco.OperationStatusId.ToString(),
                                                        StateContributor = oc.State,
                                                        //CreatedDate = oc.CreatedDate,
                                                        CreatedDate = s.SoftwareDate.Value,
                                                        ElectronicDoc = eld.Name,
                                                        Url = s.Url,
                                                    }).Distinct();
            return query.Count();
        } 


        public bool HabilitarParaSincronizarAProduccion(int Id, string Estado)
        {
            var entity = sqlDBContext.OtherDocElecContributors.Where(t => t.Id == Id).FirstOrDefault();
            entity.State = Estado;
            entity.OtherDocElecContributorOperations
                .FirstOrDefault(t => !t.Deleted)
                .OperationStatusId = Estado == "Habilitado" ? (int)OtherDocElecState.Habilitado : (int)OtherDocElecState.Test;

            int filasAfectadas = sqlDBContext.SaveChanges();
            return filasAfectadas > 0;
        }
    }
}
