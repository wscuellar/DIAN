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
        DynamicTableEntity Find(string partitionKey, string rowKey);
        T Find<T>(string partitionKey, string rowKey) where T : ITableEntity, new();
        IEnumerable<T> FindAll<T>() where T : ITableEntity, new();
        IEnumerable<T> FindAll<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindByContributorIdWithPagination<T>(int contributorId) where T : ITableEntity, new();
        List<DynamicTableEntity> FindByPartition(string partitionKey);
        List<DynamicTableEntity> FindByPartition(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo, int take = 1000);
        List<DynamicTableEntity> FindByPartition(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo, Dictionary<string, string> fields, int take = 1000);
        List<T> FindByPartition<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindByPartition<T>(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo) where T : ITableEntity, new();
        List<DynamicTableEntity> FindByPartitionWithPagination(string partitionKey);
        List<T> FindByPartitionWithPagination<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindByPartitionWithPagination<T>(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo) where T : ITableEntity, new();
        List<T> FindDocumentReferenceAttorney<T>(string partitionKey, string rowKey, string issueAtorney, string senderCode) where T : ITableEntity, new();
        List<T> FindDocumentReferenceAttorneyFaculitity<T>(string partitionKey) where T : ITableEntity, new();
        List<T> FindDocumentReferenced<T>(string documentReferencedKey, string documentTypeId) where T : ITableEntity, new();
        List<T> FindDocumentReferenced_EventCode_TypeId<T>(string documentReferencedKey, string documentTypeId, string eventCode) where T : ITableEntity, new();
        List<T> FindDocumentReferenced_TypeId<T>(string documentReferencedKey, string documentTypeId) where T : ITableEntity, new();
        List<DynamicTableEntity> FindhByTimeStamp(DateTime timeStampFrom, DateTime timeStampTo);
        List<T> FindpartitionKey<T>(string partitionKey) where T : ITableEntity, new();
        List<DynamicTableEntity> FindStartsWithByPartition(string startsWithPattern, DateTime timeStampFrom, DateTime timeStampTo, int take = 1000);
        List<DynamicTableEntity> FindStartsWithByPartition(string startsWithPattern, DateTime timeStampFrom, DateTime timeStampTo, Dictionary<string, string> fields, int take = 1000);
        List<DynamicTableEntity> FindWithinPartitionRange(string partitionLowerBound, string partitionUpperBound);
        List<DynamicTableEntity> FindWithinPartitionRangeStartsWithByRowKey(string partitionLowerBound, string partitionUpperBound, string startsWithPattern);
        List<DynamicTableEntity> FindWithinPartitionStartsWithByRowKey(string startsWithPattern);
        List<DynamicTableEntity> FindWithinPartitionStartsWithByRowKey(string partitionKey, string startsWithPattern);
        Tuple<IEnumerable<T>, TableContinuationToken> GetRangeRows<T>(int take, TableContinuationToken continuationToken) where T : ITableEntity, new();
        Tuple<IEnumerable<T>, TableContinuationToken> GetRangeRows<T>(string PartitionKey, int take, TableContinuationToken continuationToken) where T : ITableEntity, new();
        IEnumerable<T> GetRowsContainsInPartitionKeys<T>(IEnumerable<string> partitionKeys) where T : ITableEntity, new();
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
    }
}