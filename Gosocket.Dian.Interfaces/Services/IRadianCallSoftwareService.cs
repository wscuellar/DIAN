using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianCallSoftwareService
    {
        RadianSoftware Get(Guid id);
        List<Software> GetSoftwares(int contributorId);

        Guid CreateSoftware(int radianContributorId, string softwareName, string url, string pin, string createdBy);
        Guid DeleteSoftware(Guid id);
    }
}