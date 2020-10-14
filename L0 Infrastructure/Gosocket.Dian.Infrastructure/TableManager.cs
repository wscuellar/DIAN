using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure
{
    public class TableManager
    {
        public CloudTable CloudTable { get; set; }

        public TableManager(string tableName, bool createIfNotExists = true)
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.GetValue("GlobalStorage"));
            var tableClient = account.CreateCloudTableClient();

            CloudTable = tableClient.GetTableReference(tableName);

            if (createIfNotExists)
                CloudTable.CreateIfNotExists();
        }

        public TableManager(string tableName, string connectionString, bool createIfNotExists = true)
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
            catch
            {
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
            catch(Exception ex)
            {
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
        public List<T> FindDocumentReferenced<T>(string documentReferencedKey) where T : ITableEntity, new()
        {
            var query = new TableQuery<T>();

            var prefixCondition = TableQuery.GenerateFilterCondition("DocumentReferencedKey",
                QueryComparisons.Equal, documentReferencedKey);

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
    }
}
