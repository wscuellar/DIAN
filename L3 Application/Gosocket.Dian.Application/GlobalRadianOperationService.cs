using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class GlobalRadianOperationService : IGlobalRadianOperationService
    {

        private readonly TableManager globalSoftware = new TableManager("GlobalSoftware");
        private readonly TableManager globalRadianOperations = new TableManager("GlobalRadianOperations");

        #region GlobalRadianOperation

        public bool Insert(GlobalRadianOperations item, RadianSoftware software)
        {
            GlobalSoftware soft = new GlobalSoftware(software.Id.ToString(), software.Id.ToString())
            {
                Id = software.Id,
                Pin = software.Pin,
                Timestamp = DateTime.Now,
                StatusId = 1
            };
            ;
            return SoftwareAdd(soft) && globalRadianOperations.InsertOrUpdate(item);
        }

        public bool Update(GlobalRadianOperations item)
        {
            return globalRadianOperations.InsertOrUpdate(item);
        }

        public List<GlobalRadianOperations> OperationList(string code)
        {
            return globalRadianOperations.FindByPartition<GlobalRadianOperations>(code);
        }

        public GlobalRadianOperations GetOperation(string code, Guid softwareId)
        {
            return globalRadianOperations.Find<GlobalRadianOperations>(code, softwareId.ToString());
        }

        public bool IsActive(string code, Guid softwareId)
        {
            GlobalRadianOperations item = globalRadianOperations.Find<GlobalRadianOperations>(code, softwareId.ToString());
            return item.RadianStatus == Domain.Common.RadianState.Habilitado.ToString();
        }


        #endregion

        #region GlobalSoftware

        public bool SoftwareAdd(GlobalSoftware item)
        {
            return globalSoftware.InsertOrUpdate(item);
        }

        public GlobalRadianOperations EnableParticipantRadian(string code, string softwareId)
        {
            GlobalRadianOperations operation = globalRadianOperations.Find<GlobalRadianOperations>(code, softwareId.ToString());
            if (operation.RadianStatus != Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Test))
                return new GlobalRadianOperations();
            operation.RadianStatus = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Habilitado);
            _ = Update(operation);
            return operation;
        }


        #endregion

    }
}
