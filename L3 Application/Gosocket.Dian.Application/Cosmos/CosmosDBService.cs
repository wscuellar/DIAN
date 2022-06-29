using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Infrastructure;
using LinqKit;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
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


        private static readonly TelemetryConfiguration config = TelemetryConfiguration.CreateDefault();
        private static readonly TelemetryClient telemetryClient = new TelemetryClient(config);



        public Database Database { get; set; }

        private static Dictionary<string, CosmosDBService> instances = new Dictionary<string, CosmosDBService>();
        private static Dictionary<string, DocumentCollection> collections = new Dictionary<string, DocumentCollection>();

        // table manager instance
        private static readonly TableManager tableManager = new TableManager("GlobalDocValidatorDocumentMeta");

        private static readonly TableManager tableManagerTrackCosmos = new TableManager("TrackCosmos");

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
            var start = DateTime.UtcNow;
            query = (IOrderedQueryable<GlobalDataDocument>)
                 client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                 .Where(e => e.id == id
                 && e.PartitionKey == partitionKey).AsEnumerable();
            var result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();            
            TrackOperation("FACELCosmosGetAsync_1", start, query.ToString(), result.RequestCharge);            
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
                //RequestContinuation = null
            };

            
            IOrderedQueryable<GlobalDataDocument> query = null;

            do
            {
                
                
                var start = DateTime.UtcNow;
                

                query = (IOrderedQueryable<GlobalDataDocument>)client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                        .Where(e => e.GenerationTimeStamp >= model.LastDateTimeUpdate).AsDocumentQuery();
                var result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();
                options.RequestContinuation = result.ResponseContinuation;
                documents.AddRange(result.ToList());

                TrackOperation("FACELCosmosQuery_1", start, query.ToString(), result.RequestCharge);

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

            
            var start = DateTime.UtcNow;

            query = (IOrderedQueryable<GlobalDataDocument>)
                 client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                 .Where(e => e.PartitionKey == partitionKey
                 && e.DocumentKey == documentKey).AsEnumerable();
            var result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();

            TrackOperation("FACELCosmosQuery_2", start, query.ToString(), result.RequestCharge);

            return result.FirstOrDefault();
        }

        public Tuple<bool, string, List<GlobalDataDocument>> ReadFilterDocuments(ComosDbFilterRequest filter)
        {
            var collectionName = GetCollectionName(filter.EFrom);
            var collectionLink = collections[collectionName].SelfLink;

            

            if (string.IsNullOrEmpty(filter.ContinuationToken))
            {
                filter.ContinuationToken = null;
            }
            FeedOptions options;

            if (string.IsNullOrEmpty(filter.ContinuationToken))
            {
                options = new FeedOptions()
                {
                    MaxItemCount = filter.ResultMaxItemCount,
                    EnableCrossPartitionQuery = true
                };
            }
            else
            {
                options = new FeedOptions()
                {
                    MaxItemCount = filter.ResultMaxItemCount,
                    EnableCrossPartitionQuery = true,
                    RequestContinuation = filter.ContinuationToken
                };
            }

            var predicate = PredicateBuilder.New<GlobalDataDocument>();

            

            if (filter.EFrom != new DateTime(DateTime.UtcNow.Year, 1, 1) && filter.ETo.Date != new DateTime(DateTime.UtcNow.Year, 12, 31))
            {
                //int fromNumber = int.Parse(filter.EFrom.ToString("yyyyMMdd"));
                //int toNumber = int.Parse(filter.ETo.ToString("yyyyMMdd"));
                filter.ETo = new DateTime(filter.ETo.Year, filter.ETo.Month, filter.ETo.Day).AddDays(1).AddMilliseconds(-1); //para que sea hasta el ultimo milisegundo de ese dia
                //predicate = predicate.And(p => p.EmissionDateNumber >= fromNumber && p.EmissionDateNumber <= toNumber);
                predicate = predicate.And(p => p.ReceptionTimeStamp >= (filter.EFrom) && p.ReceptionTimeStamp <= (filter.ETo));
            }

            IQueryable<GlobalDataDocument> query = null;
            query = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options).Where(predicate);

            string ct = "";
            var resultDocs = new List<GlobalDataDocument>();
            while (query.AsDocumentQuery().HasMoreResults)
            {
                
                var start = DateTime.UtcNow;

                var result = query.AsDocumentQuery().ExecuteNextAsync<GlobalDataDocument>().Result;
                resultDocs.AddRange(SaveRest(result.GetEnumerator()));
                ct = result.ResponseContinuation;

                TrackOperation("FACELCosmosQuery_3", start, query.ToString(), result.RequestCharge);
            }

            return Tuple.Create(((IDocumentQuery<GlobalDataDocument>)query).HasMoreResults, ct, resultDocs.ToList());
        }

        public Tuple<long, long, List<GlobalDataDocument>> CountFilterDocuments(ComosDbFilterRequest filter)
        {
            var collectionName = GetCollectionName(filter.RFrom);
            var collectionLink = collections[collectionName].SelfLink;

            var options = new FeedOptions()
            {
                MaxItemCount = -1,
                EnableCrossPartitionQuery = true,
                RequestContinuation = null
            };

            

            var predicate = PredicateBuilder.New<GlobalDataDocument>();

            //Sender
            if (!string.IsNullOrEmpty(filter.SenderCode))
                predicate = predicate.And(p => p.SenderCode == filter.SenderCode.ToUpper());

            //Receiver
            if (!string.IsNullOrEmpty(filter.ReceiverCode))
                predicate = predicate.And(p => p.ReceiverCode == filter.ReceiverCode.ToUpper());

            //DocumentType
            if (!string.IsNullOrEmpty(filter.DocumentTypeId))
                predicate = predicate.And(p => p.DocumentTypeId == filter.DocumentTypeId);

            //PartitionKey
            //if (partitionKeys.Any())
            //    predicate = predicate.And(p => partitionKeys.Contains(p.PartitionKey));


            //Reception date
            if (filter.RFrom != new DateTime() && filter.RTo != new DateTime())
            {
                filter.RTo = new DateTime(filter.RTo.Year, filter.RTo.Month, filter.RTo.Day).AddDays(1).AddMilliseconds(-1);//para que sea hasta el ultimo milisegundo de ese di
                predicate = predicate.And(p => p.ReceptionTimeStamp >= filter.RFrom && p.ReceptionTimeStamp <= filter.RTo);
            }

            //Emission date
            //if (filter.EFrom != new DateTime() && filter.ETo != new DateTime())
            //{
            //    filter.ETo = new DateTime(filter.ETo.Year, filter.ETo.Month, filter.ETo.Day).AddDays(1).AddMilliseconds(-1);//para que sea hasta el ultimo milisegundo de ese di
            //    predicate = predicate.And(p => p.EmissionDate >= filter.EFrom && p.EmissionDate <= filter.ETo);
            //}

            //Status
            if (filter.Status.HasValue)
                predicate = predicate.And(p => p.ValidationResultInfo.Status == filter.Status.Value);

            var result = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options).Count(predicate);

            double total = 0;
            var count = result;
            if (filter.ReturnTotals)
                total = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options).Where(predicate).Sum(d => d.TotalAmount);

            var list = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options).Where(predicate).ToList();

            return Tuple.Create((long)count, (long)total, list);
        }

        private List<T> SaveRest<T>(IEnumerator<T> e)
        {
            var list = new List<T>();
            while (e.MoveNext())
            {
                list.Add(e.Current);
            }
            return list;

        }

        private bool Operations(List<Event> events, List<string> codes)
        {
            //1. Fecha del evento
            var eventC = events.FirstOrDefault(t => codes.Contains(t.Code));
            return events.Where(t => eventC.TimeStamp > t.TimeStamp && !codes.Contains(t.Code)).Any();
        }

        public async Task<Tuple<bool, string, List<GlobalDataDocument>>> ReadDocumentsAsync(string continuationToken,
                                                                                            DateTime? from,
                                                                                            DateTime? to,
                                                                                            int status,
                                                                                            string documentTypeId,
                                                                                            string senderCode,
                                                                                            string serieAndNumber,
                                                                                            string receiverCode,
                                                                                            string providerCode,
                                                                                            int maxItemCount,
                                                                                            string documentKey,
                                                                                            string referenceType,
                                                                                            List<string> pks = null)
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

            string collectionName = GetCollectionName(to.Value);
            string collectionLink = collections[collectionName].SelfLink;

            FeedOptions options;
            if (string.IsNullOrEmpty(continuationToken))
            {
                options = new FeedOptions()
                {
                    MaxItemCount = maxItemCount,
                    EnableCrossPartitionQuery = true
                };
            }
            else
            {
                options = new FeedOptions()
                {
                    MaxItemCount = maxItemCount,
                    EnableCrossPartitionQuery = true,
                    RequestContinuation = continuationToken
                };
            }

            IOrderedQueryable<GlobalDataDocument> query = null;

            
            var start = DateTime.UtcNow;

            if (pks != null && !string.IsNullOrEmpty(documentKey))
            {
                query = (IOrderedQueryable<GlobalDataDocument>)
                client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                .Where(e => pks.Contains(e.PartitionKey)
                && e.DocumentKey == documentKey).AsDocumentQuery();
            }
            else
            {
                

                query = (IOrderedQueryable<GlobalDataDocument>)client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                .Where(
                    e => e.EmissionDateNumber >= fromNumber && e.EmissionDateNumber <= toNumber
                    && (status == 0 || e.ValidationResultInfo.Status == status)
                    && (documentTypeId == "00"
                        || e.DocumentTypeId == documentTypeId
                        || e.DocumentTypeId == documentTypeOption1
                        || e.DocumentTypeId == documentTypeOption2)
                    && (referenceType == "00"
                        || e.References.Any(r => r.DocumentTypeId == referenceType)
                        || e.References.Any(r => r.DocumentTypeId == referenceTypeOption1)
                        || e.References.Any(r => r.DocumentTypeId == referenceTypeOption2))
                    && (senderCode == null || e.SenderCode == senderCode)
                    && (serieAndNumber == null || e.SerieAndNumber == serieAndNumber)
                    && (receiverCode == null || e.ReceiverCode == receiverCode)
                    && (providerCode == null || e.TechProviderInfo.TechProviderCode == providerCode)
                ).OrderByDescending(e => e.Timestamp).AsDocumentQuery();
            }

            FeedResponse<GlobalDataDocument> result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();

            TrackOperation ("FACELCosmosQuery_4", start, query.ToString(), result.RequestCharge);

            return Tuple.Create(((IDocumentQuery<GlobalDataDocument>)query).HasMoreResults, result.ResponseContinuation, result.ToList());
        }

        public async Task<(bool HasMoreResults, string Continuation, List<GlobalDataDocument> GlobalDocuments)>
            ReadDocumentsAsyncOrderByReception(string continuationToken,
                                               DateTime? from,
                                               DateTime? to,
                                               int status,
                                               string documentTypeId,
                                               string senderCode,
                                               string serieAndNumber,
                                               string receiverCode,
                                               string providerCode,
                                               int maxItemCount,
                                               string documentKey,
                                               string referenceType,
                                               List<string> pks = null,
                                               int radianStatus = 0)
        {
            FeedOptions options;

            if (string.IsNullOrEmpty(continuationToken))
            {
                options = new FeedOptions()
                {
                    MaxItemCount = maxItemCount,
                    EnableCrossPartitionQuery = true

                };
            }
            else
            {
                options = new FeedOptions()
                {
                    MaxItemCount = maxItemCount,
                    EnableCrossPartitionQuery = true,
                    RequestContinuation = continuationToken
                };
            }
            string collectionName = GetCollectionName(to.Value);
            string collectionLink = collections[collectionName].SelfLink;

            IOrderedQueryable<GlobalDataDocument> query = null;
            FeedResponse<GlobalDataDocument> result = null;
            List<string> radianStatusFilter = null;

            if (pks != null && !string.IsNullOrEmpty(documentKey))
            {
                //---Descarto los documentos que no pueden estar en la consulta asignando los que si.
                query = (IOrderedQueryable<GlobalDataDocument>)client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                    .Where(e => pks.Contains(e.PartitionKey) && e.DocumentKey == documentKey && (e.DocumentTypeId == "01" ||
                e.DocumentTypeId == "02" ||
                e.DocumentTypeId == "03" ||
                e.DocumentTypeId == "04" ||
                e.DocumentTypeId == "05" ||
                e.DocumentTypeId == "07" ||
                e.DocumentTypeId == "08" ||
                e.DocumentTypeId == "09" ||
                e.DocumentTypeId == "11" ||
                e.DocumentTypeId == "12" ||
                e.DocumentTypeId == "101"||
                e.DocumentTypeId == "91" ||
                e.DocumentTypeId == "92" ||
                e.DocumentTypeId == "20" ||
                e.DocumentTypeId == "25" ||
                e.DocumentTypeId == "27" ||
                e.DocumentTypeId == "32" ||
                e.DocumentTypeId == "35" ||
                e.DocumentTypeId == "40" ||
                e.DocumentTypeId == "50" ||
                e.DocumentTypeId == "55" ||
                e.DocumentTypeId == "45" ||
                e.DocumentTypeId == "94" ||
                e.DocumentTypeId == "60" 
                )).AsDocumentQuery();

                result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();
                return (((IDocumentQuery<GlobalDataDocument>)query).HasMoreResults,
                        result.ResponseContinuation,
                        GlobalDocuments: result.ToList());
            }

            if (!from.HasValue || !to.HasValue)
            {
                from = to.Value.AddMonths(-1);
                to = DateTime.Now.Date;
            }

            ExpressionStarter<GlobalDataDocument> predicate = PredicateBuilder.New<GlobalDataDocument>();

            int fromNumber = int.Parse($"{from.Value:yyyyMMdd}");
            int toNumber = int.Parse($"{to.Value:yyyyMMdd}");

            
            
            predicate = predicate.And(g => g.EmissionDateNumber >= fromNumber && g.EmissionDateNumber <= toNumber);

            if (status != 0)
                predicate = predicate.And(g => g.ValidationResultInfo.Status == status);

            if (!documentTypeId.Equals("00"))
            {
                switch (documentTypeId)
                {
                    case "01":
                    case "02":
                    case "03":
                        predicate = predicate.And(g => g.DocumentTypeId == documentTypeId
                                                  || g.DocumentTypeId == documentTypeId.Remove(0, 1));
                        break;
                    case "07":
                    case "08":
                        predicate = predicate.And(g => g.DocumentTypeId == documentTypeId
                                                 || g.DocumentTypeId == documentTypeId.Remove(0, 1)
                                                 || g.DocumentTypeId == (documentTypeId == "08" ? "92" : "91"));
                        break;
                    default:
                        predicate = predicate.And(g => g.DocumentTypeId == documentTypeId);
                        break;
                }
            }

            if (!referenceType.Equals("00"))
            {
                switch (referenceType)
                {
                    case "07":
                    case "08":
                        predicate = predicate.And(g => g.References.Any(r => r.DocumentTypeId == referenceType)
                                                  || g.References.Any(r => r.DocumentTypeId == referenceType.Remove(0, 1))
                                                  || g.References.Any(r => r.DocumentTypeId == (referenceType == "07" ? "91" : "92")));
                        break;
                    default:
                        predicate = predicate.And(g => g.References.Any(r => r.DocumentTypeId == referenceType));
                        break;
                }
            }

            if (senderCode != null)
                predicate = predicate.And(g => g.SenderCode == senderCode);

            if (serieAndNumber != null)
                predicate = predicate.And(g => g.SerieAndNumber == serieAndNumber);

            if (receiverCode != null)
                predicate = predicate.And(g => g.ReceiverCode == receiverCode);

            if (providerCode != null)
                predicate = predicate.And(g => g.TechProviderInfo.TechProviderCode == providerCode);

            //---------Descarta para los filtros los applicationresponse u otros documentos que no deban estar en la consulta.
            if (documentTypeId.Equals("00"))
            {
                predicate = predicate.And(g => g.DocumentTypeId == "01" ||
                g.DocumentTypeId == "02" ||
                g.DocumentTypeId == "03" ||
                g.DocumentTypeId == "04" ||
                g.DocumentTypeId == "05" ||
                g.DocumentTypeId == "07" ||
                g.DocumentTypeId == "08" ||
                g.DocumentTypeId == "09" ||
                g.DocumentTypeId == "11" ||
                g.DocumentTypeId == "12" ||
                g.DocumentTypeId == "101"||
                g.DocumentTypeId == "20" ||
                g.DocumentTypeId == "25" ||
                g.DocumentTypeId == "27" ||
                g.DocumentTypeId == "32" ||
                g.DocumentTypeId == "35" ||
                g.DocumentTypeId == "40" ||
                g.DocumentTypeId == "50" ||
                g.DocumentTypeId == "55" ||
                g.DocumentTypeId == "45" ||
                g.DocumentTypeId == "55" ||
                g.DocumentTypeId == "94" ||
                g.DocumentTypeId == "60"
                );
            }

            if (radianStatus > 0)
            {
                options.MaxItemCount = 10;
                switch (radianStatus)
                {
                    case 1: //Titulo Valor
                        predicate = predicate.And(g => g.Events.Any(a =>
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                 && (a.Code.Equals($"0{(int)EventStatus.Accepted}") || a.Code.Equals($"0{(int)EventStatus.AceptacionTacita}")))
                                                && g.Events.Any(ev => ev.Code.Equals($"0{(int)EventStatus.Received}"))
                                                && g.Events.Any(ev => ev.Code.Equals($"0{(int)EventStatus.Receipt}"))
                                                 );
                        break;
                    case 2: //Solicitud de disponilibilizacion

                        predicate = predicate.And(g => ( //Si tengo anulacion en mi ultima posicion de fecha, 
                                                    g.Events.Any(a =>
                                                                        a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                                    && a.Code.Equals($"0{(int)EventStatus.AnulacionLimitacionCirculacion}")
                                                        )
                                                &&
                                                     g.Events.Any(a => //si la tengo, quito la limitacion y la anulacion delimitacion
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.NegotiatedInvoice}") &&  //limitacion
                                                                                       !t.Code.Equals($"0{(int)EventStatus.AnulacionLimitacionCirculacion}") //anulacion
                                                                                       ).Max(b => b.TimeStamp)
                                                 && a.Code.Equals($"0{(int)EventStatus.SolicitudDisponibilizacion}"))
                                                     )
                                                     ||
                                                     ( //si en mi ultimpa osicion de fecah tengo una anulacion de endoso
                                                        g.Events.Any(a =>
                                                                        a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                                    && a.Code.Equals($"0{(int)EventStatus.InvoiceOfferedForNegotiation}")
                                                        )
                                                &&
                                                     g.Events.Any(a =>  //si tengo la anulacion de endoso, quito los endosos en procuracion y garantia
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.EndosoGarantia}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.EndosoProcuracion}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.InvoiceOfferedForNegotiation}")
                                                                                       ).Max(b => b.TimeStamp)
                                                 && a.Code.Equals($"0{(int)EventStatus.SolicitudDisponibilizacion}"))
                                                     )

                                                     ||
                                                     ( //si no tenemos anulacion o limitacion dejamos el flujo normal
                                                     g.Events.Any(a =>
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                 && a.Code.Equals($"0{(int)EventStatus.SolicitudDisponibilizacion}"))

                                                     )
                                                 );
                        break;
                    case 3: //endosado
                        predicate = predicate.And(g => ( //Si tengo anulacion en mi ultima posicion de fecha, 
                                                    g.Events.Any(a =>
                                                                        a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                                    && a.Code.Equals($"0{(int)EventStatus.AnulacionLimitacionCirculacion}")
                                                        )
                                                &&
                                                     g.Events.Any(a => //si la tengo, quito la limitacion y la anulacion de limitacion
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.NegotiatedInvoice}") &&  //limitacion
                                                                                       !t.Code.Equals($"0{(int)EventStatus.AnulacionLimitacionCirculacion}") //anulacion
                                                                                       ).Max(b => b.TimeStamp)
                                                 && (a.Code.Equals($"0{(int)EventStatus.EndosoGarantia}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndosoProcuracion}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndosoPropiedad}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndorsementWithEffectOrdinaryAssignment}")))
                                                     )
                                                     ||
                                                     ( //si en mi ultimpa osicion de fecah tengo una anulacion de endoso
                                                        g.Events.Any(a =>
                                                                        a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                                    && a.Code.Equals($"0{(int)EventStatus.InvoiceOfferedForNegotiation}")
                                                        )
                                                &&
                                                     g.Events.Any(a =>  //si tengo la anulacion de endoso, quito los endosos en procuracion y garantia
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.EndosoGarantia}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.EndosoProcuracion}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.InvoiceOfferedForNegotiation}")
                                                                                       ).Max(b => b.TimeStamp)
                                                 && (a.Code.Equals($"0{(int)EventStatus.EndosoGarantia}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndosoProcuracion}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndosoPropiedad}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndorsementWithEffectOrdinaryAssignment}")))
                                                     )

                                                     ||
                                                     ( //si no tenemos anulacion o limitacion dejamos el flujo normal
                                                     g.Events.Any(a =>
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                 && (a.Code.Equals($"0{(int)EventStatus.EndosoGarantia}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndosoProcuracion}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndosoPropiedad}")
                                                        || a.Code.Equals($"0{(int)EventStatus.EndorsementWithEffectOrdinaryAssignment}")))
                                                     )
                                                 );
                        break;
                    case 4: //pagado
                        predicate = predicate.And(g => g.Events.Any(a =>
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                 && a.Code.Equals($"0{(int)EventStatus.NotificacionPagoTotalParcial}")
                                                    || a.Code.Equals($"0{(int)EventStatus.PaymentOfTransferEconomicRights}"))
                                                 );
                        break;
                    case 5: //limitacion

                        predicate = predicate.And(g => ( //Si tengo anulacion en mi ultima posicion de fecha, 
                                                    g.Events.Any(a =>
                                                                        a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                                    && a.Code.Equals($"0{(int)EventStatus.AnulacionLimitacionCirculacion}")
                                                        )
                                                &&
                                                     g.Events.Any(a => //si la tengo, quito la limitacion y la anulacion de limitacion
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.NegotiatedInvoice}") &&  //limitacion
                                                                                       !t.Code.Equals($"0{(int)EventStatus.AnulacionLimitacionCirculacion}") //anulacion
                                                                                       ).Max(b => b.TimeStamp)
                                                 && a.Code.Equals($"0{(int)EventStatus.NegotiatedInvoice}"))
                                                     )
                                                     ||
                                                     ( //si en mi ultimpa osicion de fecah tengo una anulacion de endoso
                                                        g.Events.Any(a =>
                                                                        a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                            !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                                    && a.Code.Equals($"0{(int)EventStatus.InvoiceOfferedForNegotiation}")
                                                        )
                                                &&
                                                     g.Events.Any(a =>  //si tengo la anulacion de endoso, quito los endosos en procuracion y garantia
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.EndosoGarantia}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.EndosoProcuracion}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.InvoiceOfferedForNegotiation}")
                                                                                       ).Max(b => b.TimeStamp)
                                                 && a.Code.Equals($"0{(int)EventStatus.NegotiatedInvoice}"))
                                                     )

                                                     ||
                                                     ( //si no tenemos anulacion o limitacion dejamos el flujo normal
                                                     g.Events.Any(a =>
                                                     a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                       !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                 && a.Code.Equals($"0{(int)EventStatus.NegotiatedInvoice}"))

                                                     )
                                                 );
                        break;
                    case 6: //factura electronica
                        if (documentTypeId == "01")
                        {
                            predicate = predicate.And(g => (!g.Events.Any()) || (g.Events.Any() && !g.Events.Any(t => t.Code.Equals($"0{(int)EventStatus.Accepted}"))));
                        }
                        break;
                    case 7: //Transferida
                        predicate = predicate.And(g => g.Events.Any(a =>
                                                    a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                      !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                      !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                      !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                                && a.Code.Equals($"0{(int)EventStatus.TransferEconomicRights}"))
                                                );

                        break;
                    case 8: //Protestada
                        predicate = predicate.And(g => g.Events.Any(a =>
                                                   a.TimeStamp == g.Events.Where(t => !t.Code.Equals($"0{(int)EventStatus.Avales}") &&
                                                                                     !t.Code.Equals($"0{(int)EventStatus.Mandato}") &&
                                                                                     !t.Code.Equals($"0{(int)EventStatus.ValInfoPago}") &&
                                                                                     !t.Code.Equals($"0{(int)EventStatus.TerminacionMandato}")).Max(b => b.TimeStamp)
                                               && a.Code.Equals($"0{(int)EventStatus.Objection}"))
                                               );
                        break;
                    case 9://No Aplica
                        predicate = predicate.And(g => !(g.DocumentTypeId.Equals($"0{(int)DocumentType.AIUInvoice} ")));
                        predicate = predicate.And(g => !(g.DocumentTypeId.Equals($"0{(int)DocumentType.Invoice}")));
                        predicate = predicate.And(g => !(g.DocumentTypeId.Equals($"0{(int)DocumentType.MandateInvoice}")));
                        predicate = predicate.And(g => !g.DocumentTypeId.Equals(((int)DocumentType.MandateInvoice).ToString()));
                        break;
                }

                if (radianStatusFilter != null)
                    predicate = predicate.And(g => g.Events.Any(e => radianStatusFilter.Contains(e.Code)));
            }

            
            var start = DateTime.UtcNow;


            query = (IOrderedQueryable<GlobalDataDocument>)client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                    .Where(predicate).OrderByDescending(e => e.ReceptionTimeStamp).AsDocumentQuery();
            result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();           
            List<GlobalDataDocument> globalDocuments = result.ToList();

            TrackOperation ("FACELCosmosQuery_0", start, query.ToString(), result.RequestCharge);

            return (((IDocumentQuery<GlobalDataDocument>)query).HasMoreResults,
                    result.ResponseContinuation,
                    GlobalDocuments: globalDocuments);
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
                    var start = DateTime.UtcNow;
                    var result = client.UpsertDocumentAsync(collections[collectionName].SelfLink, document).Result;
                    TrackOperation("FACELCosmosUpsert_0", start, "upsert Document" , result.RequestCharge);
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
                //
                var collectionName = GetCollectionName(documentTagMessage.Date);
                if (!collections.ContainsKey(collectionName))
                    Instance(documentTagMessage.Date);

                

                //
                var documentTag = Mapper<DocumentTagMessage, DocumentTag>(documentTagMessage);

                //
                string discriminator = documentTagMessage.DocumentKey.Substring(0, 2);

                //
                var partitionKey = $"co|{documentTagMessage.Date.Day.ToString().PadLeft(2, '0')}|{discriminator}";

                //
                var meta = tableManager.Find<GlobalDocValidatorDocumentMeta>(documentTagMessage.DocumentKey, documentTagMessage.DocumentKey);

                // id en cosmos
                var id = ToGuid($"{meta.SenderCode}{meta.DocumentTypeId}{meta.SerieAndNumber}").ToString();

                // get cosmos document
                var document = await GetAsync(id, partitionKey, meta.EmissionDate);

                //
                if (document == null) return;

                //
                if (document.DocumentTags != null && document.DocumentTags.Any(x => x.Code == documentTag.Code && x.Value == documentTag.Value))
                    return;

                //
                if (document.DocumentTags == null)
                    document.DocumentTags = new List<DocumentTag>();

                //
                document.DocumentTags.Add(documentTag);

                //
                var start = DateTime.UtcNow;
                var result=await client.UpsertDocumentAsync(collections[collectionName].SelfLink, document);

                TrackOperation("FACELCosmosUpdate_0", start, "update Document", result.RequestCharge);
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

        public async Task<List<GlobalDataDocument>> ReadDocumentByReceiverCodeAsync(string receiverCode, DateTime date)
        {

            var collectionName = GetCollectionName(date);
            var collectionLink = collections[collectionName].SelfLink;

            var options = new FeedOptions()
            {
                EnableCrossPartitionQuery = true,
            };

            IOrderedQueryable<GlobalDataDocument> query = null;

            
            var start = DateTime.UtcNow;


            query = (IOrderedQueryable<GlobalDataDocument>)
                   client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options).OrderBy(e => e.SerieAndNumber)
                   .Where(e => e.ReceiverCode == receiverCode && (e.DocumentTypeId == "102" || e.DocumentTypeId == "103")).AsEnumerable();
            var result = await ((IDocumentQuery<GlobalDataDocument>)query).ExecuteNextAsync<GlobalDataDocument>();
            
            var lista = result.ToList<GlobalDataDocument>();

            TrackOperation ("FACELCosmosQuery_5", start, query.ToString(), result.RequestCharge);

            return lista;
        }


        public async Task<int> CountDocumentsAsync(DateTime? from,
                                                    DateTime? to,
                                                    int status,
                                                    string documentTypeId,
                                                    string senderCode,
                                                    string serieAndNumber,
                                                    string receiverCode,
                                                    string providerCode,
                                                    string referenceType,
                                                    List<string> pks = null)
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

            string collectionName = GetCollectionName(to.Value);
            string collectionLink = collections[collectionName].SelfLink;

            FeedOptions options = new FeedOptions()
            {

                EnableCrossPartitionQuery = true

            };


            

            
            var start = DateTime.UtcNow;

            var query = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
            .Where(
                e => e.EmissionDateNumber >= fromNumber && e.EmissionDateNumber <= toNumber
                && (status == 0 || e.ValidationResultInfo.Status == status)
                && (documentTypeId == "00"
                    || e.DocumentTypeId == documentTypeId
                    || e.DocumentTypeId == documentTypeOption1
                    || e.DocumentTypeId == documentTypeOption2)
                && (referenceType == "00"
                    || e.References.Any(r => r.DocumentTypeId == referenceType)
                    || e.References.Any(r => r.DocumentTypeId == referenceTypeOption1)
                    || e.References.Any(r => r.DocumentTypeId == referenceTypeOption2))
                && (senderCode == null || e.SenderCode == senderCode)
                && (serieAndNumber == null || e.SerieAndNumber == serieAndNumber)
                && (receiverCode == null || e.ReceiverCode == receiverCode)
                && (providerCode == null || e.TechProviderInfo.TechProviderCode == providerCode)
            );
            int res = await query.CountAsync();

            TrackOperation("FACELCosmosQuery_6", start, query.ToString(), -1);
            return res;
        }

        #region Utils
        
        
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

        private static Guid ToGuid(string code)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(code);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return new Guid(hashBytes);
            }
        }

        private static List<string> GetChunks(string value, int chunkLength)
        {
            var res = new List<string>();
            int count = (value.Length / chunkLength) + (value.Length % chunkLength > 0 ? 1 : 0);
            Enumerable.Range(0, count).ToList().ForEach(f => res.Add(value.Skip(f * chunkLength).Take(chunkLength).Select(z => z.ToString()).Aggregate((a, b) => a + b)));
            return res;
        }
        
        private static void TrackOperation(string eventName, DateTime start, string queryString, double rus)
        {
            string trackCosmos = Environment.GetEnvironmentVariable("trackCosmos");
            int maxSize = 24000;
            if ("true".Equals(trackCosmos))
            {
                
                queryString = queryString.Replace("\\", "");
                int size = ASCIIEncoding.UTF8.GetByteCount(queryString);
                if (size > maxSize)
                {
                    queryString = queryString.Substring(queryString.Length - maxSize);
                }
                List<string> lstQuerys = GetChunks(queryString, 8100);
                Dictionary<string, string> datos = new Dictionary<string, string>();
                int k = 1;
                foreach (var queryPart in lstQuerys)
                {
                    datos.Add($"Query {k++}", queryPart);

                }                          
                datos.Add("Time", DateTime.UtcNow.Subtract(start).TotalSeconds.ToString());
                datos.Add("RUs", rus.ToString());
                telemetryClient.TrackEvent(eventName, datos);
                telemetryClient.Flush();
            }
        }

    }


    public class TrackCosmos : TableEntity
    {
        public TrackCosmos() { }

        public TrackCosmos(string pk, string rk) : base(pk, rk)
        {

        }
        public string Query { get; set; }
        public double Time { get; set; }
        public double RUS { get; set; }
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