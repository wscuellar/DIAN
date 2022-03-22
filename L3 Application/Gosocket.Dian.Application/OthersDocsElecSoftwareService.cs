using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Gosocket.Dian.Domain;

namespace Gosocket.Dian.Application
{
    public class OthersDocsElecSoftwareService : IOthersDocsElecSoftwareService
    {
        private readonly SoftwareService _softwareService = new SoftwareService();

        public readonly IOthersDocsElecSoftwareRepository _othersDocsElecSoftwareRepository;

        private static readonly TableManager tableManager = new TableManager("GlobalLogger");

        public OthersDocsElecSoftwareService(IOthersDocsElecSoftwareRepository othersDocsElecSoftwareRepository)
        {
            _othersDocsElecSoftwareRepository = othersDocsElecSoftwareRepository;
        }
         

        public OtherDocElecSoftware Get(Guid id)
        {
            return _othersDocsElecSoftwareRepository.Get(t => t.Id == id);
        }

        public OtherDocElecSoftware GetBySoftwareId(Guid id)
        {
            return _othersDocsElecSoftwareRepository.Get(t => t.SoftwareId == id);
        }

        public OtherDocElecSoftware GetBySoftwareIdV2(Guid id)
        {
            var softwareOtherDoc = _othersDocsElecSoftwareRepository.Get(t => t.SoftwareId == id);
            if (softwareOtherDoc != null) return softwareOtherDoc;

            var software = _softwareService.Get(id);
            return Map(software);
        }

        private OtherDocElecSoftware Map(Software software)
        {
            if (software is null) return null;
            return new OtherDocElecSoftware(software, 0, "");
        }

        public List<Software> GetSoftwares(int contributorId)
        {
            return _softwareService.GetSoftwares(contributorId);
        }

        public List<OtherDocElecSoftware> List(int ContributorId)
        {
            return _othersDocsElecSoftwareRepository.List(t => t.OtherDocElecContributorId == ContributorId, 0, 0).Results;
        }


        public OtherDocElecSoftware CreateSoftware(OtherDocElecSoftware software)
        {
            software.Id = _othersDocsElecSoftwareRepository.AddOrUpdate(software);
            return software;
        }


        public Guid DeleteSoftware(Guid id)
        {
            OtherDocElecSoftware software = _othersDocsElecSoftwareRepository.Get(t => t.Id == id);
            software.Status = false;
            software.Deleted = true;
            return _othersDocsElecSoftwareRepository.AddOrUpdate(software);
        }

        public void SetToProduction(OtherDocElecSoftware software)
        {
            try
            {
                using (var context = new SqlDBContext())
                {
                    var softwareInstance = context.OtherDocElecSoftwares.FirstOrDefault(c => c.Id == software.Id);
                    softwareInstance.OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var logger = new GlobalLogger("Other Docs Elec - SetSoftwareToProduction", software.Id.ToString())
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

        private void RegisterException(GlobalLogger logger)
        {
            
            tableManager.InsertOrUpdate(logger);
        }

        public string GetSoftwareStatusName(int id)
        {
            return _othersDocsElecSoftwareRepository.GetSoftwareStatusName(id);
        }

        public List<OtherDocElecSoftware> GetSoftwaresByProviderTechnologicalServices(int contributorId, 
            int electronicDocumentId, 
            int contributorTypeId,
            string state)
        {
            return _othersDocsElecSoftwareRepository.List(t => t.OtherDocElecContributor.ContributorId == contributorId &&
                t.OtherDocElecContributor.ElectronicDocumentId == electronicDocumentId &&
                t.OtherDocElecContributor.OtherDocElecContributorTypeId == contributorTypeId &&
                t.OtherDocElecContributor.State == state && t.Deleted == false, 0, 0).Results;
        }
    }
}
