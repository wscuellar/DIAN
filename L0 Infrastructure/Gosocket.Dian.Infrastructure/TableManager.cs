using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public class TableManager : ITableManager
    {
        public CloudTable CloudTable { get; set; }


        private static Lazy<CloudTableClient> lazyClient = new Lazy<CloudTableClient>(InitializeTableClient);
        public static CloudTableClient tableClient => lazyClient.Value;

        private static CloudTableClient InitializeTableClient()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var tableClient = account.CreateCloudTableClient();
            return tableClient;
        }

        public TableManager(string tableName, bool createIfNotExists = false)
        {            

            CloudTable = tableClient.GetTableReference(tableName);

            if (createIfNotExists)
                CloudTable.CreateIfNotExists();
        }

        public TableManager(string tableName, string connectionString, bool createIfNotExists = false)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable = tableClient.GetTableReference(tableName);

            if (createIfNotExists)
                CloudTable.CreateIfNotExists();
        }

        public bool Delete(TableEntity entity)
        {
            try
            {
                var operationToDelete = TableOperation.Delete(entity);
                CloudTable.Execute(operationToDelete);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Delete(DynamicTableEntity entity)
        {
            try
            {
                var operationToDelete = TableOperation.Delete(entity);
                CloudTable.Execute(operationToDelete);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Insert(TableEntity entity)
        {
            try
            {
                var operationToInsert = TableOperation.Insert(entity);
                CloudTable.Execute(operationToInsert);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool InsertOrUpdate(TableEntity entity)
        {
            try
            {
                var operationToInsert = TableOperation.InsertOrReplace(entity);
                CloudTable.Execute(operationToInsert);
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return false;
            }
        }

        public async Task<bool> InsertOrUpdateAsync(TableEntity entity)
        {
            try
            {
                var operationToInsert = TableOperation.InsertOrReplace(entity);
                await CloudTable.ExecuteAsync(operationToInsert);
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return false;
            }
        }

        public bool Insert(DynamicTableEntity entity)
        {
            try
            {
                var operationToInsert = TableOperation.Insert(entity);
                CloudTable.Execute(operationToInsert);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Insert(DynamicTableEntity[] entitys, string partitionKey = null)
        {
            try
            {
                var batch = new TableBatchOperation();

                foreach (var entity in entitys)
                {
                    if (!string.IsNullOrEmpty(partitionKey))
                        entity.PartitionKey = partitionKey;
                    batch.InsertOrReplace(entity);
                }

                CloudTable.ExecuteBatch(batch);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Insert(DynamicTableEntity entity, string rowKey2)
        {
            try
            {
                var batch = new TableBatchOperation();
                batch.InsertOrReplace(entity);
                entity.RowKey = rowKey2;
                batch.InsertOrReplace(entity);
                CloudTable.ExecuteBatch(batch);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Update(TableEntity entity)
        {
            try
            {
                CloudTable.Execute(TableOperation.Replace(entity));
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return false;
            }
        }

        public bool Update(TableEntity entity, string partitionKey, string rowKey)
        {
            try
            {
                if (!Delete(entity))
                    return false;

                entity.PartitionKey = partitionKey;
                entity.RowKey = rowKey;
                return Insert(entity);
            }
            catch
            {
                return false;
            }
        }

        public bool Update(DynamicTableEntity entity)
        {
            try
            {
                CloudTable.Execute(TableOperation.Replace(entity));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Update(DynamicTableEntity entity, string partitionKey, string rowKey)
        {
            try
            {
                if (!Delete(entity))
                    return false;

                entity.PartitionKey = partitionKey;
                entity.RowKey = rowKey;
                return Insert(entity);
            }
            catch
            {
                return false;
            }
        }

        public CloudTable Query()
        {
            return CloudTable;
        }

        public bool Exist<T>(string PartitionKey, string RowKey) where T : ITableEntity, new()
        {
            try
            {
                var query = CloudTable.CreateQuery<T>().Where(x => x.PartitionKey == PartitionKey && x.RowKey == RowKey).Select(x => x).Take(1).AsTableQuery();
                var tableQueryResult = query.ExecuteSegmented(null, new TableRequestOptions());

                if (tableQueryResult.Count() == 0)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public T ExistTarifa<T>(string partitionKey, string tarifa) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("Tarifa",
                    QueryComparisons.Equal,
                    tarifa));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }


        public IEnumerable<T> FindAll<T>(string partitionKey) where T : ITableEntity, new()
        {
            TableContinuationToken token = null;
            var items = new List<T>();
            do
            {
                var data = GetRangeRows<T>(partitionKey, 1000, token);
                token = data.Item2;
                items.AddRange(data.Item1);
            }
            while (token != null);
            return items;
        }

        public IEnumerable<T> FindAll<T>() where T : ITableEntity, new()
        {
            TableContinuationToken token = null;
            var items = new List<T>();
            do
            {
                var data = GetRangeRows<T>(1000, token);
                token = data.Item2;
                items.AddRange(data.Item1);
            }
            while (token != null);
            return items;
        }

        public IEnumerable<T> FindAll<T>(int take, ref TableContinuationToken continuationToken) where T : ITableEntity, new()
        {
            var items = new List<T>();

            var data = GetRangeRows<T>(take, continuationToken);
            continuationToken = data.Item2;
            items.AddRange(data.Item1);

            return items;
        }

        public List<DynamicTableEntity> FindWithinPartitionStartsWithByRowKey(string partitionKey, string startsWithPattern)
        {
            var query = new TableQuery();

            var length = startsWithPattern.Length - 1;
            var lastChar = startsWithPattern[length];

            var nextLastChar = (char)(lastChar + 1);

            var startsWithEndPattern = startsWithPattern.Substring(0, length) + nextLastChar;

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.GreaterThanOrEqual,
                    startsWithPattern),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.LessThan,
                    startsWithEndPattern)
                );

            var filterString = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                prefixCondition
                );

            var entities = CloudTable.ExecuteQuery(query.Where(filterString));

            return entities.ToList();
        }

        public List<DynamicTableEntity> FindWithinPartitionRangeStartsWithByRowKey(
            string partitionLowerBound, string partitionUpperBound, string startsWithPattern)
        {
            var query = new TableQuery();

            var length = startsWithPattern.Length - 1;
            var lastChar = startsWithPattern[length];

            var nextLastChar = (char)(lastChar + 1);

            var startsWithEndPattern = startsWithPattern.Substring(0, length) + nextLastChar;

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.GreaterThanOrEqual,
                    startsWithPattern),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.LessThan,
                    startsWithEndPattern)
                );

            var partitionFilterString = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    partitionLowerBound),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.LessThanOrEqual,
                    partitionUpperBound)
                );

            var filterString = TableQuery.CombineFilters(partitionFilterString, TableOperators.And, prefixCondition);
            var entities = CloudTable.ExecuteQuery(query.Where(filterString));

            return entities.ToList();
        }

        public List<DynamicTableEntity> FindWithinPartitionRange(string partitionLowerBound, string partitionUpperBound)
        {
            var query = new TableQuery();

            var partitionFilterString = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    partitionLowerBound),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.LessThanOrEqual,
                    partitionUpperBound)
                );

            var entities = CloudTable.ExecuteQuery(query.Where(partitionFilterString));

            return entities.ToList();
        }

        public List<DynamicTableEntity> FindWithinPartitionStartsWithByRowKey(string startsWithPattern)
        {
            var query = new TableQuery();

            var length = startsWithPattern.Length - 1;
            var lastChar = startsWithPattern[length];

            var nextLastChar = (char)(lastChar + 1);

            var startsWithEndPattern = startsWithPattern.Substring(0, length) + nextLastChar;

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    startsWithPattern),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.LessThan,
                    startsWithEndPattern)
                );

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public List<T> FindByPartition<T>(string partitionKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var entities = CloudTable.ExecuteQuery(query);

            return entities.ToList();
        }

        public List<T> FindFirstSurNameByPartition<T>(string partitionKey) where T : ITableEntity, new()
        {
            var query = CloudTable.CreateQuery<T>().Where(x => x.PartitionKey == partitionKey).Select(x => x).AsTableQuery();

            var entities = CloudTable.ExecuteQuery(query);

            return entities.ToList();
        }

        public T FindByGlobalOtherDocumentTestId<T>(string Id) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("Id", QueryComparisons.Equal, Id));

            var entities = CloudTable.ExecuteQuery(query);

            return entities.FirstOrDefault();
        }

        public T FindByDocumentKey<T>(string partitionKey, string rowKey, string documentKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
             TableQuery.GenerateFilterCondition("PartitionKey",
                 QueryComparisons.Equal,
                 partitionKey),
             TableOperators.And,
             TableQuery.GenerateFilterCondition("RowKey",
                 QueryComparisons.Equal,
                 rowKey));

            prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, TableQuery.GenerateFilterCondition("DocumentKey",
             QueryComparisons.Equal,
             documentKey));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();

        }

        public T Find<T>(string partitionKey, string rowKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    rowKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public T FindPartitionKey<T>(string partitionKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey);

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public IEnumerable<T> FindBy<T>(string partitionKey, string rowKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    rowKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }


        public T FindSoftwareId<T>(string partitionKey, string softwareId) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("SoftwareId",
                    QueryComparisons.Equal,
                    softwareId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public T FindOthersDocumentsResult<T>(string partitionKey, string softwareId) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("SoftwareId",
                    QueryComparisons.Equal,
                    softwareId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public T FindSoftwareRowKey<T>(string partitionKey, string softwareId) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    softwareId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }


        public T FindGlobalTestOtherDocumentId<T>(string partitionKey, string testSetId) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("Id",
                    QueryComparisons.Equal,
                    testSetId));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public List<T> FindDocumentRegisterAR<T>(string providerCode, string documentTypeId, string serieandNumber) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    providerCode),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("DocumentTypeId",
                    QueryComparisons.Equal,
                    documentTypeId));

            prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, TableQuery.GenerateFilterCondition("SerieAndNumber",
              QueryComparisons.Equal,
              serieandNumber));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public List<T> FindpartitionKey<T>(string partitionKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var entities = CloudTable.ExecuteQuery(query);

            return entities.ToList();
        }

        public List<T> FindDocumentReferenceAttorneyFaculitity<T>(string partitionKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var entities = CloudTable.ExecuteQuery(query);

            return entities.ToList();
        }

        public List<T> FindDocumentFaculitityEvent<T>(string eventCode) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, eventCode));

            var entities = CloudTable.ExecuteQuery(query);

            return entities.ToList();
        }


        public T FindDocumentReferenceAttorney<T>(string partitionKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));


            var entities = CloudTable.ExecuteQuery(query);

            return entities.FirstOrDefault();
        }

        public List<T> FindDocumentReferenceAttorneyList<T>(string partitionKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));


            var entities = CloudTable.ExecuteQuery(query);

            return entities.ToList();
        }

        
        public List<T> FindOthersDocumentsNitTestSetId<T>(string PartitionKey, string Id) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    PartitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("Id",
                    QueryComparisons.Equal,
                    Id));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }       

        public List<T> FindDocumentReferenceAttorney<T>(string rowKey, string senderCode) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    rowKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("SenderCode",
                    QueryComparisons.Equal,
                    senderCode));
            prefixCondition = TableQuery.CombineFilters(
                prefixCondition,
                TableOperators.And,
                TableQuery.GenerateFilterConditionForBool("Active",
                    QueryComparisons.Equal,
                    true));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public List<T> FindDocumentSenderCodeIssueAttorney<T>(string issuerAttorney, string senderCode) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("IssuerAttorney",
                    QueryComparisons.Equal,
                    issuerAttorney),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("SenderCode",
                    QueryComparisons.Equal,
                    senderCode));
            prefixCondition = TableQuery.CombineFilters(
              prefixCondition,
              TableOperators.And,
              TableQuery.GenerateFilterConditionForBool("Active",
                  QueryComparisons.Equal,
                  true));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public List<T> FindByPartition<T>(string partitionKey, DateTime timeStampFrom, DateTime timeStampTo)
            where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var entities = CloudTable.ExecuteQuery(query);

            return entities.ToList();
        }

        public List<T> FindByPartitionWithPagination<T>(string partitionKey) where T : ITableEntity, new()
        {
            var query =
                new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                    partitionKey)).Take(1000).AsTableQuery();

            var results = new List<T>();
            var queryResult = CloudTable.ExecuteQuerySegmented(query, null,
                new TableRequestOptions { PayloadFormat = TablePayloadFormat.Json });

            while (queryResult.Results.Any())
            {
                results.AddRange(queryResult.Results);
                if (queryResult.ContinuationToken == null) break;

                queryResult = CloudTable.ExecuteQuerySegmented(query, queryResult.ContinuationToken,
                    new TableRequestOptions { PayloadFormat = TablePayloadFormat.Json });

                Thread.Sleep(100);
            }

            return results;
        }


        public List<T> FindByContributorIdWithPagination<T>(int contributorId) where T : ITableEntity, new()
        {
            var query =
                new TableQuery<T>().Where(TableQuery.GenerateFilterConditionForInt("ContributorId", QueryComparisons.Equal,
                    contributorId)).Take(1000).AsTableQuery();

            var results = new List<T>();
            var queryResult = CloudTable.ExecuteQuerySegmented(query, null,
                new TableRequestOptions { PayloadFormat = TablePayloadFormat.Json });

            while (queryResult.Results.Any())
            {
                results.AddRange(queryResult.Results);
                if (queryResult.ContinuationToken == null) break;

                queryResult = CloudTable.ExecuteQuerySegmented(query, queryResult.ContinuationToken,
                    new TableRequestOptions { PayloadFormat = TablePayloadFormat.Json });

                Thread.Sleep(100);
            }

            return results;
        }

        public List<DynamicTableEntity> FindByPartitionWithPagination(string partitionKey)
        {
            var query =
                new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                    partitionKey)).Take(1000);

            var results = new List<DynamicTableEntity>();
            var queryResult = CloudTable.ExecuteQuerySegmented(query, null);

            while (queryResult.Results.Any())
            {
                results.AddRange(queryResult.Results);
                if (queryResult.ContinuationToken == null) break;

                queryResult = CloudTable.ExecuteQuerySegmented(query, queryResult.ContinuationToken);

                Thread.Sleep(100);
            }

            return results;
        }

        public List<T> FindByPartitionWithPagination<T>(string partitionKey,
            DateTime timeStampFrom, DateTime timeStampTo) where T : ITableEntity, new()
        {
            var partitionFilterString =
           TableQuery.GenerateFilterCondition("PartitionKey",
              QueryComparisons.Equal, partitionKey);

            var prefixCondition =
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual,
                        timeStampFrom), TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, timeStampTo));

            var filterString = TableQuery.CombineFilters(partitionFilterString, TableOperators.And, prefixCondition);
            var query =
                new TableQuery<T>().Where(filterString).Take(1000).AsTableQuery();

            var results = new List<T>();
            var queryResult = CloudTable.ExecuteQuerySegmented(query, null,
                new TableRequestOptions { PayloadFormat = TablePayloadFormat.Json });

            while (queryResult.Results.Any())
            {
                results.AddRange(queryResult.Results);
                if (queryResult.ContinuationToken == null) break;

                queryResult = CloudTable.ExecuteQuerySegmented(query, queryResult.ContinuationToken,
                    new TableRequestOptions { PayloadFormat = TablePayloadFormat.Json });

                Thread.Sleep(100);
            }

            return results;
        }

        public List<DynamicTableEntity> FindByPartition(string partitionKey)
        {
            var query = new TableQuery();

            var prefixCondition =
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal, partitionKey);

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public List<DynamicTableEntity> FindByPartition(string partitionKey,
            DateTime timeStampFrom, DateTime timeStampTo, int take = 1000)
        {
            return FindByPartition(partitionKey, timeStampFrom, timeStampTo, new Dictionary<string, string>(), take);
        }

        public List<DynamicTableEntity> FindByPartition(string partitionKey,
            DateTime timeStampFrom, DateTime timeStampTo, Dictionary<string, string> fields, int take = 1000)
        {
            var query = new TableQuery();

            var partitionFilterString =
            TableQuery.GenerateFilterCondition("PartitionKey",
               QueryComparisons.Equal, partitionKey);

            var prefixCondition =
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual,
                        timeStampFrom), TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, timeStampTo));

            var filterString = TableQuery.CombineFilters(partitionFilterString, TableOperators.And, prefixCondition);

            foreach (var field in fields)
            {
                prefixCondition =
                    TableQuery.GenerateFilterCondition(field.Key,
                        QueryComparisons.Equal, field.Value);

                filterString = TableQuery.CombineFilters(filterString, TableOperators.And, prefixCondition);
            }

            var entities = CloudTable.ExecuteQuery(query.Where(filterString).Take(take));

            return entities.ToList();
        }

        public List<DynamicTableEntity> FindStartsWithByPartition(string startsWithPattern,
            DateTime timeStampFrom, DateTime timeStampTo, int take = 1000)
        {
            return FindStartsWithByPartition(startsWithPattern, timeStampFrom, timeStampTo, new Dictionary<string, string>(), take);
        }

        public List<DynamicTableEntity> FindStartsWithByPartition(string startsWithPattern,
            DateTime timeStampFrom, DateTime timeStampTo, Dictionary<string, string> fields, int take = 1000)
        {
            var query = new TableQuery();

            var length = startsWithPattern.Length - 1;
            var lastChar = startsWithPattern[length];

            var nextLastChar = (char)(lastChar + 1);

            var startsWithEndPattern = startsWithPattern.Substring(0, length) + nextLastChar;

            var partitionFilterString = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    startsWithPattern),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.LessThan,
                    startsWithEndPattern)
                );

            var prefixCondition =
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual,
                        timeStampFrom), TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, timeStampTo));

            var filterString = TableQuery.CombineFilters(partitionFilterString, TableOperators.And, prefixCondition);

            foreach (var field in fields)
            {
                prefixCondition =
                    TableQuery.GenerateFilterCondition(field.Key,
                        QueryComparisons.Equal, field.Value);

                filterString = TableQuery.CombineFilters(filterString, TableOperators.And, prefixCondition);
            }

            var entities = CloudTable.ExecuteQuery(query.Where(filterString).Take(take));

            return entities.ToList();
        }

        public List<DynamicTableEntity> FindhByTimeStamp(DateTime timeStampFrom, DateTime timeStampTo)
        {
            var query = new TableQuery();

            var prefixCondition =
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual,
                        timeStampFrom), TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, timeStampTo));


            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public DynamicTableEntity Find(string partitionKey, string rowKey)
        {
            var query = new TableQuery();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    rowKey));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }       

        public T FindhByRadianStatus<T>(string partitionKey, bool deleted, string radianStatus) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForBool("Deleted",
                    QueryComparisons.Equal,
                    deleted));

            var RadianStateHabilitado = TableQuery.GenerateFilterCondition("RadianState",
                QueryComparisons.Equal,
                radianStatus);

            var RadianStatePruebas = TableQuery.GenerateFilterCondition("RadianState",
                QueryComparisons.Equal,
                "En pruebas");

            prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, TableQuery.CombineFilters(RadianStateHabilitado,
             TableOperators.Or,
             RadianStatePruebas));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public T FindhByPartitionKeyRadianStatus<T>(string partitionKey, bool deleted, string softwareId) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForBool("Deleted",
                    QueryComparisons.Equal,
                    deleted));
            prefixCondition = TableQuery.CombineFilters(
                prefixCondition,
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    softwareId));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }


        public List<T> FindhByPartitionKeyRadianState<T>(string partitionKey, bool deleted, string radianState) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForBool("Deleted",
                    QueryComparisons.Equal,
                    deleted));
            prefixCondition = TableQuery.CombineFilters(
                prefixCondition,
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RadianState",
                    QueryComparisons.Equal,
                    radianState));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public T FindhByCufeExchange<T>(string partitionKey, bool Active) where T : ITableEntity, new()
        {

            var query = new TableQuery<T>();
            var prefixCondition = TableQuery.CombineFilters(
               TableQuery.GenerateFilterCondition("PartitionKey",
                   QueryComparisons.Equal,
                   partitionKey),
               TableOperators.And,
                TableQuery.GenerateFilterConditionForBool("Active",
                    QueryComparisons.Equal,
                    Active));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public T FindhByCufeSenderAttorney<T>(string rowKey, string senderCode, string issuerAttorney) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    rowKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("SenderCode",
                    QueryComparisons.Equal,
                    senderCode));
            prefixCondition = TableQuery.CombineFilters(
                prefixCondition,
                TableOperators.And,
                TableQuery.GenerateFilterCondition("IssuerAttorney",
                    QueryComparisons.Equal,
                    issuerAttorney));


            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public T FindByTestSetId<T>(string partitionKey, string testSetId) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("Id",
                    QueryComparisons.Equal,
                    testSetId));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }



        public T FindByGlobalDocumentId<T>(string globalDocumentId) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("GlobalDocumentId", QueryComparisons.Equal, globalDocumentId));

            var entities = CloudTable.ExecuteQuery(query);

            return entities.FirstOrDefault();
        }


        public Tuple<IEnumerable<T>, TableContinuationToken> GetRangeRows<T>(int take, TableContinuationToken continuationToken) where T : ITableEntity, new()
        {
            var query = CloudTable.CreateQuery<T>().Where(x => x.PartitionKey != "").Take(take).AsTableQuery();
            var tableQueryResult = query.ExecuteSegmented(continuationToken, new TableRequestOptions());
            continuationToken = tableQueryResult.ContinuationToken;
            return new Tuple<IEnumerable<T>, TableContinuationToken>(tableQueryResult.Results, continuationToken);
        }

        public Tuple<IEnumerable<T>, TableContinuationToken> GetRangeRows<T>(string PartitionKey, int take, TableContinuationToken continuationToken) where T : ITableEntity, new()
        {
            var query = CloudTable.CreateQuery<T>().Where(x => x.PartitionKey == PartitionKey).Take(take).AsTableQuery();
            var tableQueryResult = query.ExecuteSegmented(continuationToken, new TableRequestOptions());
            continuationToken = tableQueryResult.ContinuationToken;
            return new Tuple<IEnumerable<T>, TableContinuationToken>(tableQueryResult.Results, continuationToken);
        }

        public IEnumerable<T> GetRowsContainsInPartitionKeys<T>(IEnumerable<string> partitionKeys) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();
            var filter = string.Join($" {TableOperators.Or} ", partitionKeys.Select(p => TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, p)));
            var entities = CloudTable.ExecuteQuery(query.Where(filter));
            return entities;
        }

        /// <summary>
        /// Buscar Set de Pruebas - Otros Documentos por Id del Documento Electronico y el Id del Modo de Operación
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partitionKey">Id del documento electronico</param>
        /// <param name="rowKey">Id del modo de operación</param>
        /// <returns></returns>
        public List<T> GetOthersDocuments<T>(string partitionKey, string rowKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", //ElectronicDocumentId
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", //OperationModeId
                    QueryComparisons.Equal,
                    rowKey));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public T FindGlobalEvent<T>(string partitionKey, string rowKey, string documentTypeId) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    rowKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey));

            prefixCondition = TableQuery.CombineFilters(
                prefixCondition,
                TableOperators.And,
                TableQuery.GenerateFilterCondition("DocumentTypeId",
                    QueryComparisons.Equal,
                    documentTypeId));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.FirstOrDefault();
        }

        public List<T> FindGlobalOtherDocElecOperationByPartition_RowKey_Deleted_State<T>(string partitionKey, string rowKey, bool deleted, string state) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    rowKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey));

            prefixCondition = TableQuery.CombineFilters(
                prefixCondition,
                TableOperators.And,
                TableQuery.GenerateFilterConditionForBool("Deleted",
                    QueryComparisons.Equal,
                    deleted));

            prefixCondition = TableQuery.CombineFilters(
                prefixCondition,
                TableOperators.And,
                TableQuery.GenerateFilterCondition("State",
                    QueryComparisons.Equal,
                    state));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }

        public T FindGlobalPayrollByCUNE<T>(string cune) where T : ITableEntity, new()
        {
            //El CUNE es el mismo PartitionKey en la tabla GlobalDocPayroll
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, cune));
            return CloudTable.ExecuteQuery(query).FirstOrDefault();
        }

        public List<T> FindGlobalPayrollByMonth_EnumerationRange_EmployeeDocType_EmployeeDocNumber_FirstSurname_EmployeeSalaryRange_EmployerCity<T>
            (int take, DateTime? monthStart, DateTime? monthEnd, double? enumerationStart, double? enumerationEnd, string employeeDocType,
            string employeeDocNumber, string firstSurname, double? employeeSalaryStart, double? employeeSalaryEnd, string employeeCity) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();
            string prefixCondition = null;

            if (enumerationStart.HasValue)
            {
                var tempPrefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterConditionForDouble("Consecutivo", QueryComparisons.GreaterThanOrEqual,
                    enumerationStart.Value), TableOperators.And,
                TableQuery.GenerateFilterConditionForDouble("Consecutivo", QueryComparisons.LessThanOrEqual, enumerationEnd.Value));

                if (prefixCondition == null) prefixCondition = tempPrefixCondition;
                else prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, tempPrefixCondition);
            }

            if (monthStart.HasValue)
            {
                var tempPrefixCondition = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("FechaPagoInicio", QueryComparisons.GreaterThanOrEqual,
                        monthStart.Value.Date), TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate("FechaPagoInicio", QueryComparisons.LessThanOrEqual, monthEnd.Value.Date));

                // por alguna razón el tiempo lo inicia con 05:...
                tempPrefixCondition = tempPrefixCondition.Replace("T05", "T00");

                if (prefixCondition == null) prefixCondition = tempPrefixCondition;
                else prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, tempPrefixCondition);
            }

            if (!string.IsNullOrWhiteSpace(employeeDocType))
            {
                var tempPrefixCondition = TableQuery.GenerateFilterCondition("TipoDocumento", QueryComparisons.Equal, employeeDocType);

                if (prefixCondition == null) prefixCondition = tempPrefixCondition;
                else prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, tempPrefixCondition);
            }

            if (!string.IsNullOrWhiteSpace(employeeDocNumber))
            {
                var tempPrefixCondition = TableQuery.GenerateFilterCondition("NumeroDocumento", QueryComparisons.Equal, employeeDocNumber);

                if (prefixCondition == null) prefixCondition = tempPrefixCondition;
                else prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, tempPrefixCondition);
            }

            if (!string.IsNullOrWhiteSpace(firstSurname))
            {
                var tempPrefixCondition = TableQuery.GenerateFilterCondition("PrimerApellido", QueryComparisons.Equal, firstSurname);

                if (prefixCondition == null) prefixCondition = tempPrefixCondition;
                else prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, tempPrefixCondition);
            }

            if (employeeSalaryStart.HasValue)
            {
                var tempPrefixCondition = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDouble("Sueldo", QueryComparisons.GreaterThanOrEqual,
                        employeeSalaryStart.Value), TableOperators.And,
                    TableQuery.GenerateFilterConditionForDouble("Sueldo", QueryComparisons.LessThanOrEqual, employeeSalaryEnd.Value));

                if (prefixCondition == null) prefixCondition = tempPrefixCondition;
                else prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, tempPrefixCondition);
            }

            if (!string.IsNullOrWhiteSpace(employeeCity))
            {
                var tempPrefixCondition = TableQuery.GenerateFilterCondition("LugarTrabajoMunicipioCiudad", QueryComparisons.Equal, employeeCity);

                if (prefixCondition == null) prefixCondition = tempPrefixCondition;
                else prefixCondition = TableQuery.CombineFilters(prefixCondition, TableOperators.And, tempPrefixCondition);
            }

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition)).Take(take).OrderByDescending(x => x.Timestamp);

            return entities.ToList();
        }

        public T FindByCode<T>(string code) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("Code", QueryComparisons.Equal, code));
            return CloudTable.ExecuteQuery(query).FirstOrDefault();
        }

        public List<T> globalDocPayrollRegisterByPartitionKey_DocumentNumber<T>(string partitionKey, string numeroDocumento) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("NumeroDocumento",
                    QueryComparisons.Equal,
                    numeroDocumento));

            return CloudTable.ExecuteQuery(query.Where(prefixCondition)).ToList();
        }

        public List<T> globalDocPayrollRegisterByPartitionKey_SerieAndNumnber<T>(string partitionKey, string serieAndNumnber) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal,
                    partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    serieAndNumnber));

            return CloudTable.ExecuteQuery(query.Where(prefixCondition)).ToList();
        }

        // FindDocumentReferenceAttorneyList
        public List<T> FindDocumentReferenceAttorneyByCUFEList<T>(string rowKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal,
                    rowKey),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForBool("Active",
                    QueryComparisons.Equal,
                    true));

            var entities = CloudTable.ExecuteQuery(query.Where(prefixCondition));

            return entities.ToList();
        }
    }
}