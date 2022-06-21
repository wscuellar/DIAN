using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public interface ITableManager
    {
        CloudTable CloudTable { get; set; }

        bool Delete(DynamicTableEntity entity);
        bool Delete(TableEntity entity);
        bool Exist<T>(string PartitionKey, string RowKey) where T : ITableEntity, new();
        T ExistTarifa<T>(string partitionKey, string tarifa) where T : ITableEntity, new();
        DynamicTableEntity Find(string partitionKey, string rowKey);
        T Find<T>(string partitionKey, string rowKey) where T : ITableEntity, new();
        IEnumerable<T> FindBy<T>(string partitionKey, string rowKey) where T : ITableEntity, new();
        IEnumerable<T> FindAll<T>() where T : ITableEntity, new();
        IEnumerable<T> FindAll<T>(int take, ref TableContinuationToken continuationToken) where T : ITableEntity, new();
        IEnumerable<T> FindAll<T>(string partitionKey) where T : ITableEntity, new();
        T FindByCode<T>(string code) where T : ITableEntity, new();
        List<T> FindByContributorIdWithPagination<T>(int contributorId) where T : ITableEntity, new();
        T FindByDocumentKey<T>(string partitionKey, string rowKey, string documentKey) where T : ITableEntity, new();
        T FindByGlobalDocumentId<T>(string globalDocumentId) where T : ITableEntity, new();
        T FindByGlobalOtherDocumentTestId<T>(string Id) where T : ITableEntity, new();
        List<DynamicTableEntity> FindByPartition(string partitionKey);
        List<DynamicTableEntity> FindByPartition(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo, int take = 1000);
        List<DynamicTableEntity> FindByPartition(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo, Dictionary<string, string> fields, int take = 1000);
        List<T> FindByPartition<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindByPartition<T>(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo) where T : ITableEntity, new();
        List<DynamicTableEntity> FindByPartitionWithPagination(string partitionKey);
        List<T> FindByPartitionWithPagination<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindByPartitionWithPagination<T>(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo) where T : ITableEntity, new();        
        T FindByTestSetId<T>(string partitionKey, string testSetId) where T : ITableEntity, new();       
        T FindDocumentReferenceAttorney<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindDocumentReferenceAttorney<T>(string rowKey, string senderCode) where T : ITableEntity, new();
        List<T> FindDocumentReferenceAttorneyByCUFEList<T>(string rowKey) where T : ITableEntity, new();
        List<T> FindDocumentReferenceAttorneyFaculitity<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindDocumentReferenceAttorneyList<T>(string partitionKey) where T : ITableEntity, new();       
        List<T> FindDocumentRegisterAR<T>(string providerCode, string documentTypeId, string serieandNumber) where T : ITableEntity, new();
        List<T> FindDocumentSenderCodeIssueAttorney<T>(string issuerAttorney, string senderCode) where T : ITableEntity, new();        
        T FindGlobalEvent<T>(string partitionKey, string rowKey, string documentTypeId) where T : ITableEntity, new();
        List<T> FindGlobalOtherDocElecOperationByPartition_RowKey_Deleted_State<T>(string partitionKey, string rowKey, bool deleted, string state) where T : ITableEntity, new();
        T FindGlobalPayrollByCUNE<T>(string cune) where T : ITableEntity, new();
        List<T> FindGlobalPayrollByMonth_EnumerationRange_EmployeeDocType_EmployeeDocNumber_FirstSurname_EmployeeSalaryRange_EmployerCity<T>(int take, DateTime? monthStart, DateTime? monthEnd, double? enumerationStart, double? enumerationEnd, string employeeDocType, string employeeDocNumber, string firstSurname, double? employeeSalaryStart, double? employeeSalaryEnd, string employeeCity) where T : ITableEntity, new();
        T FindGlobalTestOtherDocumentId<T>(string partitionkey, string testSetId) where T : ITableEntity, new();
        T FindhByCufeExchange<T>(string partitionKey, bool Active) where T : ITableEntity, new();
        T FindhByCufeSenderAttorney<T>(string rowKey, string senderCode, string issuerAttorney) where T : ITableEntity, new();       
        T FindhByPartitionKeyRadianStatus<T>(string partitionKey, bool deleted, string softwareId) where T : ITableEntity, new();
        T FindhByRadianStatus<T>(string partitionKey, bool deleted, string radianStatus) where T : ITableEntity, new();
        List<DynamicTableEntity> FindhByTimeStamp(DateTime timeStampFrom, DateTime timeStampTo);
        List<T> FindOthersDocumentsNitTestSetId<T>(string PartitionKey, string Id) where T : ITableEntity, new();
        T FindOthersDocumentsResult<T>(string partitionKey, string softwareId) where T : ITableEntity, new();
        List<T> FindpartitionKey<T>(string partitionKey) where T : ITableEntity, new();
        T FindSoftwareId<T>(string partitionKey, string softwareId) where T : ITableEntity, new();
        T FindSoftwareRowKey<T>(string partitionKey, string softwareId) where T : ITableEntity, new();
        List<DynamicTableEntity> FindStartsWithByPartition(string startsWithPattern, DateTime timeStampFrom, DateTime timeStampTo, int take = 1000);
        List<DynamicTableEntity> FindStartsWithByPartition(string startsWithPattern, DateTime timeStampFrom, DateTime timeStampTo, Dictionary<string, string> fields, int take = 1000);
        List<DynamicTableEntity> FindWithinPartitionRange(string partitionLowerBound, string partitionUpperBound);
        List<DynamicTableEntity> FindWithinPartitionRangeStartsWithByRowKey(string partitionLowerBound, string partitionUpperBound, string startsWithPattern);
        List<DynamicTableEntity> FindWithinPartitionStartsWithByRowKey(string startsWithPattern);
        List<DynamicTableEntity> FindWithinPartitionStartsWithByRowKey(string partitionKey, string startsWithPattern);
        List<T> GetOthersDocuments<T>(string partitionKey, string rowKey) where T : ITableEntity, new();
        Tuple<IEnumerable<T>, TableContinuationToken> GetRangeRows<T>(int take, TableContinuationToken continuationToken) where T : ITableEntity, new();
        Tuple<IEnumerable<T>, TableContinuationToken> GetRangeRows<T>(string PartitionKey, int take, TableContinuationToken continuationToken) where T : ITableEntity, new();
        IEnumerable<T> GetRowsContainsInPartitionKeys<T>(IEnumerable<string> partitionKeys) where T : ITableEntity, new();
        List<T> globalDocPayrollRegisterByPartitionKey_DocumentNumber<T>(string partitionKey, string numeroDocumento) where T : ITableEntity, new();
        List<T> globalDocPayrollRegisterByPartitionKey_SerieAndNumnber<T>(string partitionKey, string serieAndNumnber) where T : ITableEntity, new();

        bool Insert(DynamicTableEntity entity);
        bool Insert(DynamicTableEntity entity, string rowKey2);
        bool Insert(DynamicTableEntity[] entitys, string partitionKey = null);
        bool Insert(TableEntity entity);
        bool InsertOrUpdate(TableEntity entity);
        Task<bool> InsertOrUpdateAsync(TableEntity entity);
        CloudTable Query();
        bool Update(DynamicTableEntity entity);
        bool Update(DynamicTableEntity entity, string partitionKey, string rowKey);
        bool Update(TableEntity entity);
        bool Update(TableEntity entity, string partitionKey, string rowKey);
        List<T> FindFirstSurNameByPartition<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindhByPartitionKeyRadianState<T>(string partitionKey, bool deleted, string softwareId) where T : ITableEntity, new();        
    }
}
