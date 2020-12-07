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

        public bool Insert(GlobalRadianOperations item)
        {
            return globalRadianOperations.Insert(item);
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

        #endregion

    }
}
