using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IGlobalRadianOperationService
    {
        GlobalRadianOperations GetOperation(string code, Guid softwareId);
        bool Insert(GlobalRadianOperations item, RadianSoftware software);
        bool Update(GlobalRadianOperations item);
        bool IsActive(string code, Guid softwareId);
        List<GlobalRadianOperations> OperationList(string code);
        bool SoftwareAdd(GlobalSoftware item);
        GlobalRadianOperations EnableParticipantRadian(string code, string softwareId, RadianContributor radianContributor);
        bool Delete(string code, string v);
    }
}