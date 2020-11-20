using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class RadianCallSoftwareService : IRadianCallSoftwareService
    {
        private readonly SoftwareService _softwareService = new SoftwareService();

        public Software Get(Guid id)
        {
            return _softwareService.Get(id);
        }

        public List<Software> GetSoftwares(int contributorId)
        {
            return _softwareService.GetSoftwares(contributorId);
        }
    }
}
