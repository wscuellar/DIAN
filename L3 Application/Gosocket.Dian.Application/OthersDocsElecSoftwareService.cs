﻿using Gosocket.Dian.DataContext;
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
        public OthersDocsElecSoftwareService(IOthersDocsElecSoftwareRepository othersDocsElecSoftwareRepository)
        {
            _othersDocsElecSoftwareRepository = othersDocsElecSoftwareRepository;
        }
         

        public OtherDocElecSoftware Get(Guid id)
        {
            return _othersDocsElecSoftwareRepository.Get(t => t.Id == id);
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
            var tableManager = new TableManager("GlobalLogger");
            tableManager.InsertOrUpdate(logger);
        }
 
    }
}