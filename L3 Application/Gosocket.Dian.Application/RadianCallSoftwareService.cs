using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianCallSoftwareService : IRadianCallSoftwareService
    {
        private readonly SoftwareService _softwareService = new SoftwareService();

        public readonly IRadianSoftwareRepository _RadianSoftwareRepository;
        private static readonly TableManager tableManager = new TableManager("GlobalLogger");
        public RadianCallSoftwareService(IRadianSoftwareRepository radianSoftwareRepository)
        {
            _RadianSoftwareRepository = radianSoftwareRepository;
        }



        public RadianSoftware Get(Guid id)
        {
            return _RadianSoftwareRepository.Get(t => t.Id == id);
        }

        public List<Software> GetSoftwares(int contributorId)
        {
            return _softwareService.GetSoftwares(contributorId);
        }

        public List<RadianSoftware> List(int id)
        {
            return _RadianSoftwareRepository.List(t => t.ContributorId == id, 0, 0).Results;
        }

        public List<RadianSoftware> List(Guid id)
        {
            return _RadianSoftwareRepository.List(t => t.Id == id, 0, 0).Results;
        }

        public RadianSoftware CreateSoftware(RadianSoftware software)
        {
            software.Id = _RadianSoftwareRepository.AddOrUpdate(software);
            return software;
        }


        public Guid DeleteSoftware(Guid id)
        {
            RadianSoftware software = _RadianSoftwareRepository.Get(t => t.Id == id);
            software.Status = false;
            software.Deleted = true;
            return _RadianSoftwareRepository.AddOrUpdate(software);
        }
        
        public Guid DeleteSoftwareCancelaRegistro(Guid id)
        {
            RadianSoftware software = _RadianSoftwareRepository.Get(t => t.Id == id && t.RadianSoftwareStatusId == (int)Domain.Common.RadianSoftwareStatus.InProcess);
            if(software != null)
            {
                software.Status = false;
                software.Deleted = true;
                return _RadianSoftwareRepository.AddOrUpdate(software);
            }
            return id;
        }

        public void SetToProduction(RadianSoftware software)
        {
            try
            {
                using (var context = new SqlDBContext())
                {
                    var softwareInstance = context.RadianSoftwares.FirstOrDefault(c => c.Id == software.Id);
                    softwareInstance.RadianSoftwareStatusId = (int)Domain.Common.RadianSoftwareStatus.Accepted;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var logger = new GlobalLogger("Radian - SetSoftwareToProduction", software.Id.ToString())
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

    }
}
