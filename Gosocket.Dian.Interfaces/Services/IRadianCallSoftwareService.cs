using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianCallSoftwareService
    {
        Software Get(Guid id);
        List<Software> GetSoftwares(int contributorId);
    }
}