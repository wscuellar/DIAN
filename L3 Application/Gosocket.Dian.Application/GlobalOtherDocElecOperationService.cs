using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class GlobalOtherDocElecOperationService : IGlobalOtherDocElecOperationService
    {

        private readonly ITableManager globalSoftware = new TableManager("GlobalSoftware");
        private readonly ITableManager globalOtherDocElecOperation = new TableManager("GlobalOtherDocElecOperation");

        public GlobalOtherDocElecOperationService() { }

        public GlobalOtherDocElecOperationService(ITableManager _globalSoftware, ITableManager _globalOtherDocElecOperation) {
            globalOtherDocElecOperation = _globalOtherDocElecOperation;
            globalSoftware = _globalSoftware;
        }

        #region GlobalOperation

        public bool Insert(GlobalOtherDocElecOperation item, OtherDocElecSoftware software)
        {
            GlobalSoftware soft = new GlobalSoftware(software.SoftwareId.ToString(), software.SoftwareId.ToString())
            {
                Id = software.Id,
                Pin = software.Pin,
                Timestamp = DateTime.Now,
                StatusId = 1
            };
            return SoftwareAdd(soft) && globalOtherDocElecOperation.InsertOrUpdate(item);
        }


        public bool Update(GlobalOtherDocElecOperation item)
        {
            return globalOtherDocElecOperation.InsertOrUpdate(item);
        }

        public List<GlobalOtherDocElecOperation> OperationList(string code)
        {
            return globalOtherDocElecOperation.FindByPartition<GlobalOtherDocElecOperation>(code);
        }

        public GlobalOtherDocElecOperation GetOperation(string code, Guid softwareId)
        {
            return globalOtherDocElecOperation.Find<GlobalOtherDocElecOperation>(code, softwareId.ToString());
        }

        public GlobalOtherDocElecOperation GetOperationByElectronicDocumentId(string code, Guid softwareId, int electronicDocumentId)
        {
            return globalOtherDocElecOperation.FindBy<GlobalOtherDocElecOperation>(code, softwareId.ToString())
                .FirstOrDefault(t => t.ElectronicDocumentId == electronicDocumentId);
        }

        public bool IsActive(string code, Guid softwareId)
        {
            GlobalOtherDocElecOperation item = globalOtherDocElecOperation.FindSoftwareRowKey<GlobalOtherDocElecOperation>(code, softwareId.ToString());
            return item.State == Domain.Common.OtherDocElecState.Habilitado.ToString();
        }

        public GlobalOtherDocElecOperation EnableParticipantOtherDocument(string code, string softwareId, OtherDocElecContributor otherDocElecContributor)
        {
            GlobalOtherDocElecOperation operation = globalOtherDocElecOperation.FindSoftwareId<GlobalOtherDocElecOperation>(code, softwareId.ToString());
            if (operation.State != Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Test))
                return new GlobalOtherDocElecOperation();

            operation.State = Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Habilitado);

            if (otherDocElecContributor.OtherDocElecContributorTypeId == (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider)
                operation.TecnologicalSupplier = otherDocElecContributor.OtherDocElecContributorTypeId == (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider;

            _ = Update(operation);

            return operation;
        }


        #endregion

        #region GlobalSoftware

        public bool SoftwareAdd(GlobalSoftware item)
        {
            return globalSoftware.InsertOrUpdate(item);
        }

        public GlobalOtherDocElecOperation EnableParticipant(string code, string softwareId)
        {
            GlobalOtherDocElecOperation operation = globalOtherDocElecOperation.Find<GlobalOtherDocElecOperation>(code, softwareId.ToString());
            if (operation.State != Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Test))
                return new GlobalOtherDocElecOperation();
            operation.State = Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Habilitado);
            _ = Update(operation);
            return operation;
        }

        public bool Delete(string code, string softwareId)
        {
            GlobalOtherDocElecOperation operation = globalOtherDocElecOperation.Find<GlobalOtherDocElecOperation>(code, softwareId);
            return globalOtherDocElecOperation.Delete(operation);
        }

        #endregion

    }
}
