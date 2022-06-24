using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianCallSoftwareService
    {
        RadianSoftware Get(Guid id);
        List<Software> GetSoftwares(int contributorId);
        RadianSoftware CreateSoftware(RadianSoftware software);
        Guid DeleteSoftware(Guid id);
        void SetToProduction(RadianSoftware software);
        List<RadianSoftware> List(int id);
        Guid DeleteSoftwareCancelaRegistro(Guid id);       
    }
}