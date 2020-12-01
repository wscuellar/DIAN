using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class RadianCallSoftwareService : IRadianCallSoftwareService
    {
        private readonly SoftwareService _softwareService = new SoftwareService();

        public readonly IRadianSoftwareRepository _RadianSoftwareRepository;
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

        public Guid CreateSoftware(int radianContributorId, string softwareName,string url, string pin,string createdBy)
        {
            RadianSoftware radianSoftware = new RadianSoftware()
            {
                Name = softwareName,
                Pin = pin,
                Deleted = false,
                Status = true,
                CreatedBy = createdBy,
                SoftwareDate = System.DateTime.Now,
                Timestamp = System.DateTime.Now,
                Updated = System.DateTime.Now,
                Url = url,
                RadianContributorId = radianContributorId
            };
            return _RadianSoftwareRepository.AddOrUpdate(radianSoftware);
        }


        public Guid DeleteSoftware(Guid id)
        {
            RadianSoftware software = _RadianSoftwareRepository.Get(t => t.Id == id);
            software.Status = false;
            software.Deleted = true;
            return _RadianSoftwareRepository.AddOrUpdate(software);
        }
    }
}
