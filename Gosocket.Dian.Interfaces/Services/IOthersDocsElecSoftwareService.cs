using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IOthersDocsElecSoftwareService
    {
        OtherDocElecSoftware Get(Guid id);
        List<Software> GetSoftwares(int contributorId);
        OtherDocElecSoftware CreateSoftware(OtherDocElecSoftware software);
        Guid DeleteSoftware(Guid id);
        void SetToProduction(OtherDocElecSoftware software);
        List<OtherDocElecSoftware> List(int id);
    }
}