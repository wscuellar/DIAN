using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalOtherDocElecOperation : TableEntity
    {
        public GlobalOtherDocElecOperation() { }

        public GlobalOtherDocElecOperation(string code, string softwareId) : base(code, softwareId)
        {
            PartitionKey = code; // track id zip
            RowKey = softwareId; // track id xml
        }

        public int OtherDocElecContributorId { get; set; }
        public bool Transmitter { get; set; }

        public bool TecnologicalSupplier { get; set; }

        public int OperationModeId { get; set; }

        public int ElectronicDocumentId { get; set; }

        public int ContributorTypeId { get; set; }

        public string  SoftwareId { get; set; }
        public string State { get; set; }

        public bool Deleted { get; set; }

        //public bool UpdateOtherDocument(GlobalOtherDocElecOperation item)
        //{
        //    return globalGlobalOtherDocElecOperation.InsertOrUpdate(item);
        //}

        //public GlobalOtherDocElecOperation EnableParticipantOtherDocument(string code, string softwareId)
        //{
        //    GlobalOtherDocElecOperation operation = globalGlobalOtherDocElecOperation.Find<GlobalOtherDocElecOperation>(code, softwareId.ToString());
        //    if (operation.State != Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Test))
        //        return new GlobalOtherDocElecOperation();
        //    operation.State = Domain.Common.EnumHelper.GetDescription(Domain.Common.RadianState.Habilitado);
        //    _ = UpdateOtherDocument(operation);
        //    return operation;
        //}
    }

}
