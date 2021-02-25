﻿using Gosocket.Dian.DataContext;
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
using Gosocket.Dian.Infrastructure;

namespace Gosocket.Dian.Application
{
    public class OthersDocsElecContributorService : IOthersDocsElecContributorService
    {
        private static readonly TableManager testSetManager = new TableManager("GlobalTestSetOthersDocuments");
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

        public OtherDocElecContributor CreateContributor(int contributorId, OtherDocElecState State,
           int ContributorType, int OperationMode, int ElectronicDocumentId, string createdBy)
        {

            //int contributorId = _contributorService.GetByCode(userCode).Id;
            OtherDocElecContributor existing = _othersDocsElecContributorRepository.Get(t => t.ContributorId == contributorId
                                                                                     && t.OtherDocElecContributorTypeId == ContributorType
                                                                                     && t.OtherDocElecOperationModeId == OperationMode
                                                                                     && t.ElectronicDocumentId == ElectronicDocumentId
                                                                                     && t.State!="Aceptado");

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

        public PagedResult<OtherDocsElectData> List(int contributorId, int contributorTypeId, int operationModeId)
        {
            IQueryable<OtherDocsElectData> query = (from oc in sqlDBContext.OtherDocElecContributors
                                                    join s in sqlDBContext.OtherDocElecSoftwares on oc.Id equals s.OtherDocElecContributorId
                                                    join oco in sqlDBContext.OtherDocElecContributorOperations on oc.Id equals oco.OtherDocElecContributorId
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
                                                    select new OtherDocsElectData()
                                                    {
                                                        Id = oc.Id,
                                                        ContributorId = oc.ContributorId,
                                                        OperationMode = ope.Name,
                                                        ContributorType = oty.Name,
                                                        Software = s.Name,
                                                        PinSW = s.Pin,
                                                        SoftwareId = s.Id.ToString(),
                                                        StateSoftware = ocs.Name,
                                                        StateContributor = oc.State,
                                                        CreatedDate = oc.CreatedDate,
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
                              ProviderId = s.ProviderId,
                              Step = oc.Step,
                              State = oc.State,
                              SoftwareId = s.Id.ToString(),
                              SoftwareIdBase=s.SoftwareId
                          }).Distinct().FirstOrDefault();

            List<string> userIds = _contributorService.GetUserContributors(entity.ContributorId).Select(u => u.UserId).ToList();
            entity.LegalRepresentativeIds = userIds;

            return entity;
        }

        /// <summary>
        /// Cancelar un registro en la tabla OtherDocElecContributor
        /// </summary>
        /// <param name="contributorId">OtherDocElecContributorId</param>
        /// <param name="description">Motivo por el cual se hace la cancelación</param>
        /// <returns></returns>
        public ResponseMessage CancelRegister(int contributorId, string description)
        {
            ResponseMessage result = new ResponseMessage();

            var contributor = sqlDBContext.OtherDocElecContributors.FirstOrDefault(c => c.Id == contributorId);

            if (contributor != null)
            {
                contributor.State = "Cancelado";
                contributor.Update = DateTime.Now;
                contributor.Description = description;

                sqlDBContext.Entry(contributor).State = System.Data.Entity.EntityState.Modified;

                int re1 = sqlDBContext.SaveChanges();
                result.Code = System.Net.HttpStatusCode.OK.GetHashCode();
                result.Message = "Se cancelo el registro exitosamente";

                if (re1 > 0) //Update operations state
                {
                    var contriOpera = sqlDBContext
                                        .OtherDocElecContributorOperations
                                        .FirstOrDefault(c => c.OtherDocElecContributorId == contributorId && c.Deleted == false);
                    if (contriOpera != null)
                    {
                        contriOpera.Deleted = true;

                        sqlDBContext.Entry(contriOpera).State = System.Data.Entity.EntityState.Modified;

                        int re2 = sqlDBContext.SaveChanges();

                        if (re2 > 0) //Update operations SUCCESS
                        {
                            var contriSoftware = sqlDBContext
                                                    .OtherDocElecSoftwares
                                                    .FirstOrDefault(c => c.OtherDocElecContributorId == contributorId && c.Id == contriOpera.SoftwareId);

                            if (contriSoftware != null)
                            {
                                contriSoftware.Deleted = true;
                                contriSoftware.Status = true;
                                contriSoftware.Updated = DateTime.Now;

                                sqlDBContext.Entry(contriOpera).State = System.Data.Entity.EntityState.Modified;

                                int re3 = sqlDBContext.SaveChanges();

                                if (re3 > 0) //Update Software SUCCESS
                                {

                                }
                            }
                        }
                    }

                }
            }
            else
            {
                //sqlDBContext.Entry(contributor).State = System.Data.Entity.EntityState.Added;
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

        public OtherDocElecContributor GetContributorSoftwareInProcess(int contributorId, int statusId)
        {
            return _othersDocsElecContributorRepository
                .Get(x => x.ContributorId == contributorId && x.OtherDocElecSoftwares
                    .Any(y => y.OtherDocElecSoftwareStatusId == statusId));
        }
    }
}
