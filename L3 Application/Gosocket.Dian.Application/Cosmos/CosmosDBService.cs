using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application.Cosmos
{
    public class CosmosDBService
    {
        private readonly static object lockInstance = new object();
        private readonly static object lockUpdate = new object();
        //Assign an id for your database & collection 
        private static readonly string databaseId = ConfigurationManager.GetValue("CosmosDbDataBaseId");
        //Read the DocumentDB endpointUrl and authorizationKeys from config
        //These values are available from the Azure Management Portal on the DocumentDB Account Blade under "Keys"
        //NB > Keep these values in a safe & secure location. Together they provide Administrative access to your DocDB account
        private static readonly string endpointUrl = ConfigurationManager.GetValue("CosmosDbEndpointUrl");
        private static readonly string authorizationKey = ConfigurationManager.GetValue("CosmosDbAuthorizationKey");
        private static readonly string collectionId = ConfigurationManager.GetValue("CosmosDbCollectionID");
        //private static readonly Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

        private static readonly string environment = ConfigurationManager.GetValue("Environment");
        private static DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey);
        public Database Database { get; set; }

        private static Dictionary<string, CosmosDBService> instances = new Dictionary<string, CosmosDBService>();
        private static Dictionary<string, DocumentCollection> collections = new Dictionary<string, DocumentCollection>();

        //public static CosmosDBService Instance()
        //{
        //    var collectionName = GetCollectionName();
        //    CosmosDBService instance = null;

        //    if (instances.ContainsKey(collectionName) && collections.ContainsKey(collectionName))
        //        instance = instances[collectionName];

        //    if (instance == null || (instance != null && collections[collectionName] == null))
        //    {
        //        lock (lockInstance)
        //        {
        //            if (instance != null && collections[collectionName] != null)
        //                return instance;

        //            var instance2 = new CosmosDBService
        //            {
        //                Database = GetNewDatabaseAsync(databaseId).Result
        //            };
        //            instance = instance2;

        //            instances[collectionName] = instance;
        //            collections[collectionName] = GetOrCreateCollectionAsync(instance2.Database, collectionName).Result;
        //        }
        //    }

        //    return instance;
        //}

        public static CosmosDBService Instance(DateTime documentDate)
        {
            var collectionName = GetCollectionName(documentDate);
            CosmosDBService instance = null;

            if (instances.ContainsKey(collectionName) && collections.ContainsKey(collectionName))
                instance = instances[collectionName];

            if (instance == null || (instance != null && collections[collectionName] == null))
            {
                lock (lockInstance)
                {
                    if (instance != null && collections[collectionName] != null)
                        return instance;

                    var instance2 = new CosmosDBService
                    {
                        Database = GetNewDatabaseAsync(databaseId).Result
                    };
                    instance = instance2;

                    instances[collectionName] = instance;
                    collections[collectionName] = GetOrCreateCollectionAsync(instance2.Database, collectionName).Result;
                }
            }

            return instance;
        }

        private static string GetCollectionName()
        {
            return ConfigurationManager.GetValue("CosmosDbCollectionID");
        }
        private static string GetCollectionName(DateTime date)
        {
            if (environment == "Prod")
            {
                if (date.Year < 2010)
                    return "0000";

                return date.Year.ToString();
            }

            return "0000";
        }

        /// <summary>
        /// Get a Database for this id.
        /// </summary>
        /// <param id="id">The id of the Database to create.</param>
        /// <returns>The created Database object</returns>
        private static async Task<Database> GetNewDatabaseAsync(string id)
        {
            {
                Database database = client.CreateDatabaseQuery().Where(c => c.Id == id).ToArray().FirstOrDefault();

                if (database != null)
                {
                    return database;
                }

                database = await client.CreateDatabaseAsync(new Database { Id = id });
                return database;
            }
        }

        public async Task<GlobalDataDocument> CreateDocumentAsync(GlobalDataDocument document)
        {
            var collectionName = GetCollectionName(document.EmissionDate);
            if (!collections.ContainsKey(collectionName))
                Instance(document.EmissionDate);

            var result = await client.UpsertDocumentAsync(collections[collectionName].SelfLink, document);

            return document;
        }

        public GlobalDataDocument CreateDocument(GlobalDataDocument document)
        {
            var collectionName = GetCollectionName(document.EmissionDate);
            if (!collections.ContainsKey(collectionName))
                Instance(document.EmissionDate);

            string discriminator = document.DocumentKey.ToString().Substring(0, 2);
            document.PartitionKey = $"co|{document.EmissionDate.Day.ToString().PadLeft(2, '0')}|{discriminator}";
            var result = client.CreateDocumentAsync(collections[collectionName].SelfLink, document).Result;

            return document;
        }

        private static async Task<DocumentCollection> GetOrCreateCollectionAsync(Database db, string id)
        {
            {
                DocumentCollection collection = client.CreateDocumentCollectionQuery(db.SelfLink).Where(c => c.Id == id).ToArray().FirstOrDefault();

                if (collection == null)
                {
                    IndexingPolicy optimalQueriesIndexingPolicy = new IndexingPolicy();
                    optimalQueriesIndexingPolicy.IncludedPaths.Add(new IncludedPath
                    {
                        Path = "/*",
                        Indexes = new System.Collections.ObjectModel.Collection<Index>()
                        {
                            new RangeIndex(DataType.Number) { Precision = -1 },
                            new RangeIndex(DataType.String) { Precision = -1 }
                        },
                    });
                    PartitionKeyDefinition pk = new PartitionKeyDefinition() { Paths = { "/PartitionKey" } };
                    RequestOptions requestOptions = new RequestOptions { OfferThroughput = 400 };
                    DocumentCollection collectionDefinition = new DocumentCollection { Id = id };
                    collectionDefinition.IndexingPolicy = optimalQueriesIndexingPolicy;
                    collectionDefinition.PartitionKey = pk;
                    collection = await DocumentClientHelper.CreateDocumentCollectionWithRetriesAsync(client, db, collectionDefinition, requestOptions);
                }
                return collection;
            }
        }

        public async Task<GlobalDataDocument> GetAsync(string id, string partitionKey, DateTime date)
        {

            var collectionName = GetCollectionName(date);
            var collectionLink = collections[collectionName].SelfLink;

            var options = new FeedOptions()
            {
                EnableCrossPartitionQuery = true,
            };

            IOrderedQueryable<GlobalDataDocument> query = null;

            query = (IOrderedQueryable<GlobalDataDocument>)
                 client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                 .Where(e => e.id == id
                 && e.PartitionKey == partitionKey).AsEnumerable();
            var result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();
            return result.FirstOrDefault();
        }

        public async Task<List<GlobalDataDocument>> ReadDocumentsByLastDateTimeUpdateAsync(GlobalDataDocumentModel model)
        {
            var documents = new List<GlobalDataDocument>();
            var collectionName = GetCollectionName(model.EmissionDate);
            var collectionLink = collections[collectionName].SelfLink;

            var options = new FeedOptions()
            {
                MaxItemCount = 1000,
                EnableCrossPartitionQuery = true,
                RequestContinuation = null
            };

            var partitionKeys = GeneratePartitionKeys(model.LastDateTimeUpdate, model.UtcNow);
            IOrderedQueryable<GlobalDataDocument> query = null;

            do
            {
                query = (IOrderedQueryable<GlobalDataDocument>)client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                        .Where(e => partitionKeys.Contains(e.PartitionKey) && e.GenerationTimeStamp >= model.LastDateTimeUpdate).AsDocumentQuery();
                var result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();
                options.RequestContinuation = result.ResponseContinuation;
                documents.AddRange(result.ToList());

            } while (((IDocumentQuery<GlobalDataDocument>)query).HasMoreResults);
            return documents;
        }

        public async Task<GlobalDataDocument> ReadDocumentAsync(string documentKey, string partitionKey, DateTime date)
        {

            var collectionName = GetCollectionName(date);
            var collectionLink = collections[collectionName].SelfLink;

            var options = new FeedOptions()
            {
                EnableCrossPartitionQuery = true,
            };

            IOrderedQueryable<GlobalDataDocument> query = null;

            query = (IOrderedQueryable<GlobalDataDocument>)
                 client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                 .Where(e => e.PartitionKey == partitionKey
                 && e.DocumentKey == documentKey).AsEnumerable();
            var result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();
            return result.FirstOrDefault();
        }

        public async Task<Tuple<bool, string, List<GlobalDataDocument>>> ReadDocumentsAsync(string continuationToken, DateTime? from, DateTime? to,
        int status, string documentTypeId, string senderCode, string receiverCode, string providerCode, int maxItemCount, string documentKey, string referenceType, List<string> pks = null)
        {

            if (!from.HasValue || !to.HasValue)
            {
                from = to.Value.AddMonths(-1);
                to = DateTime.Now.Date;
            }

            int fromNumber = int.Parse(from.Value.ToString("yyyyMMdd"));
            int toNumber = int.Parse(to.Value.ToString("yyyyMMdd"));

            string documentTypeOption1 = "";
            string documentTypeOption2 = "";
            switch (documentTypeId)
            {
                case "01": documentTypeOption1 = "1"; documentTypeOption2 = "1"; break;
                case "02": documentTypeOption1 = "2"; documentTypeOption2 = "2"; break;
                case "03": documentTypeOption1 = "3"; documentTypeOption2 = "3"; break;
                case "07": documentTypeOption1 = "7"; documentTypeOption2 = "91"; break;
                case "08": documentTypeOption1 = "8"; documentTypeOption2 = "92"; break;
            }

            string referenceTypeOption1 = "";
            string referenceTypeOption2 = "";
            switch (referenceType)
            {
                case "07": referenceTypeOption1 = "7"; referenceTypeOption2 = "91"; break;
                case "08": referenceTypeOption1 = "8"; referenceTypeOption2 = "92"; break;
            }

            var collectionName = GetCollectionName(to.Value);
            var collectionLink = collections[collectionName].SelfLink;

            var options = new FeedOptions()
            {
                MaxItemCount = maxItemCount,
                EnableCrossPartitionQuery = true,
                RequestContinuation = continuationToken
            };

            IOrderedQueryable<GlobalDataDocument> query = null;

            if (pks != null && !string.IsNullOrEmpty(documentKey))
            {
                query = (IOrderedQueryable<GlobalDataDocument>)
                client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                .Where(e => pks.Contains(e.PartitionKey)
                && e.DocumentKey == documentKey).AsDocumentQuery();
            }
            else
            {
                var partitionKeys = GeneratePartitionKeys(from.Value, to.Value);
                query = (IOrderedQueryable<GlobalDataDocument>)
                client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                .Where(e => partitionKeys.Contains(e.PartitionKey)
                && e.EmissionDateNumber >= fromNumber && e.EmissionDateNumber <= toNumber
                && (status == 0 || e.ValidationResultInfo.Status == status)
                && (documentTypeId == "00" || e.DocumentTypeId == documentTypeId || e.DocumentTypeId == documentTypeOption1 || e.DocumentTypeId == documentTypeOption2)
                && (referenceType == "00" || e.References.Any(r => r.DocumentTypeId == referenceType) || e.References.Any(r => r.DocumentTypeId == referenceTypeOption1) || e.References.Any(r => r.DocumentTypeId == referenceTypeOption2))
                && (senderCode == null || e.SenderCode == senderCode)
                && (receiverCode == null || e.ReceiverCode == receiverCode)
                && (providerCode == null || e.TechProviderInfo.TechProviderCode == providerCode)
                ).OrderByDescending(e => e.Timestamp).AsDocumentQuery();
            }


            var result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();
            return Tuple.Create(((IDocumentQuery<GlobalDataDocument>)query).HasMoreResults, result.ResponseContinuation, result.ToList());
        }

        public GlobalDataDocument UpdateDocument(GlobalDataDocument document)
        {
            try
            {
                lock (lockUpdate)
                {
                    var collectionName = GetCollectionName(document.EmissionDate);
                    if (!collections.ContainsKey(collectionName))
                        Instance(document.EmissionDate);

                    string discriminator = document.GlobalDocumentId.ToString().Substring(0, 2);
                    document.PartitionKey = $"co|{document.EmissionDate.Day.ToString().PadLeft(2, '0')}|{discriminator}";
                    var result = client.UpsertDocumentAsync(collections[collectionName].SelfLink, document).Result;

                    return document;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateDocumentAsync(DocumentTagMessage documentTagMessage)
        {
            try
            {
                var collectionName = GetCollectionName(documentTagMessage.Date);
                if (!collections.ContainsKey(collectionName))
                    Instance(documentTagMessage.Date);

                var documentTag = Mapper<DocumentTagMessage, DocumentTag>(documentTagMessage);

                string discriminator = documentTagMessage.DocumentKey.Substring(0, 2);
                var partitionKey = $"co|{documentTagMessage.Date.Day.ToString().PadLeft(2, '0')}|{discriminator}";

                var document = await ReadDocumentAsync(documentTagMessage.DocumentKey, partitionKey, documentTagMessage.Date);
                if (document == null) return;
                    //throw new Exception("Document not found " + documentTag.Value);
                
                if (document.DocumentTags != null && document.DocumentTags.Any(x => x.Code == documentTag.Code && x.Value == documentTag.Value))
                    return;

                if (document.DocumentTags == null)
                    document.DocumentTags = new List<DocumentTag>();

                document.DocumentTags.Add(documentTag);
                await client.UpsertDocumentAsync(collections[collectionName].SelfLink, document);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task MigrateCollection(string collectionName, int offerThroughput)
        {
            var collectionLink = collections[collectionName].SelfLink;

            Offer offer = client.CreateOfferQuery()
                      .Where(r => r.ResourceLink == collectionLink)
                      .AsEnumerable()
                      .SingleOrDefault();

            offer = new OfferV2(offer, offerThroughput);

            //Now persist these changes to the database by replacing the original resource
            await client.ReplaceOfferAsync(offer);
        }

        #region Utils
        public static List<string> GeneratePartitionKeys(DateTime from, DateTime to)
        {
            List<string> partitionKeys = new List<string>();
            List<string> dayList = new List<string>();
            var permutations = GetPermutations(new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" }, 2);

            foreach (DateTime date in EachDay(from, to))
                dayList.Add($"co|{date.Day.ToString().PadLeft(2, '0')}");

            var distinctDays = dayList.Distinct().ToList();
            foreach (var day in distinctDays)
                foreach (var p in permutations)
                    partitionKeys.Add(day + "|" + string.Join("", p));

            return partitionKeys;
        }
        private static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }
        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list,
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        #endregion

        #region Mapper
        private T1 Mapper<T, T1>(T sourceObj)
        {
            string json = JsonConvert.SerializeObject(sourceObj);
            T1 m = JsonConvert.DeserializeObject<T1>(json);
            return m;
        }
        #endregion
    }

    #region Models
    public class GlobalDataDocumentModel
    {
        public string DocumentTypeId { get; set; }
        public DateTime EmissionDate { get; set; }
        public DateTime LastDateTimeUpdate { get; set; }
        public DateTime UtcNow { get; set; }
        public string ReceiverCode { get; set; }
        public string SerieAndNumber { get; set; }
        public string SenderCode { get; set; }
        public double TotalAmount { get; set; }
    }
    #endregion
}
