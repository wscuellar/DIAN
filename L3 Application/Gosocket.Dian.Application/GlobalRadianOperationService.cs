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

        private readonly ITableManager globalSoftware = new TableManager("GlobalSoftware");
        private readonly ITableManager globalRadianOperations = new TableManager("GlobalRadianOperations");

        public GlobalRadianOperationService() { }

        public GlobalRadianOperationService(ITableManager _globalSoftware, ITableManager _globalRadianOperations) {
            globalSoftware = _globalSoftware;
            globalRadianOperations = _globalRadianOperations;
        }


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
            return item.RadianState == Domain.Common.RadianState.Habilitado.ToString();
        }


        #endregion

        #region GlobalSoftware

        public bool SoftwareAdd(GlobalSoftware item)
        {
            return globalSoftware.InsertOrUpdate(item);
        }

        public GlobalRadianOperations EnableParticipantRadian(string code, string softwareId, RadianContributor radianContributor)
        {
            GlobalRadianOperations operation = globalRadianOperations.Find<GlobalRadianOperations>(code, softwareId.ToString());
            if (operation.RadianState != Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Test))
                return new GlobalRadianOperations();
            operation.RadianState = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Habilitado);
            if (radianContributor.RadianOperationModeId == (int)Domain.Common.RadianOperationMode.Indirect)
                operation.IndirectElectronicInvoicer = radianContributor.RadianOperationModeId == (int)Domain.Common.RadianOperationMode.Indirect;
            if (radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.ElectronicInvoice)
                operation.ElectronicInvoicer = radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.ElectronicInvoice;
            if (radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.TechnologyProvider)
                operation.TecnologicalSupplier = radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.TechnologyProvider;
            if (radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.TradingSystem)
                operation.NegotiationSystem = radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.TradingSystem;
            if (radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.Factor)
                operation.Factor = radianContributor.RadianContributorTypeId == (int)Domain.Common.RadianContributorType.Factor;

            _ = Update(operation);
            return operation;
        }

        public bool Delete(string code, string v)
        {
            GlobalRadianOperations operation = globalRadianOperations.Find<GlobalRadianOperations>(code, v);
            return globalRadianOperations.Delete(operation);
        }


        #endregion

    }
}
