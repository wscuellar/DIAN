using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.DataContext
{
    public class CosmosDbManager
    {
        private static readonly string endpointUrl = ConfigurationManager.GetValue("CosmosDbEndpointUrl");
        private static readonly string authorizationKey = ConfigurationManager.GetValue("CosmosDbAuthorizationKey");
        private static readonly string databaseId = ConfigurationManager.GetValue("CosmosDbDataBaseId");
        private static readonly string collectionId = ConfigurationManager.GetValue("CosmosDbCollectionID");
        private static readonly ConnectionPolicy connectionPolicy = new ConnectionPolicy { UserAgentSuffix = " samples-net/3" };
        private static readonly Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

        //Reusable instance of DocumentClient which represents the connection to a DocumentDB endpoint
        private static DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey);

        /// <summary>
        /// Read all documents whit filters
        /// </summary>
        /// <param name="continuationToken"></param>
        /// <param name="datesRange"></param>
        /// <param name="status"></param>
        /// <param name="documentTypeId"></param>
        /// <param name="senderCode"></param>
        /// <param name="receiverCode"></param>
        /// <param name="providerCode"></param>
        /// <param name="maxItemCount"></param>
        /// <returns>
        /// A tuple containing the following information:
        /// <list type="bullet">
        /// <item><see cref="Tuple{T1,T2,T3}.Item1"/>: A bool determining if the query has more results.</item>
        /// <item><see cref="Tuple{T1,T2,T3}.Item2"/>: Continuation token to get next results of the query.</item>
        /// <item><see cref="Tuple{T1,T2,T3}.Item3"/>: The list of GlobalDataDocument</item>
        /// </list>
        /// </returns>
        public async Task<Tuple<bool, string, List<GlobalDataDocument>>> ReadAllAsync(string continuationToken, List<string> datesRange, int status, string documentTypeId,
            string senderCode, string receiverCode, string providerCode, int maxItemCount)
        {
            try
            {
                var options = new FeedOptions()
                {
                    MaxItemCount = maxItemCount,
                    EnableCrossPartitionQuery = true,
                    RequestContinuation = continuationToken
                };

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

                var query = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                .Where(e => datesRange.Contains(e.PartitionKey)
                && (status == 0 || e.ValidationResultInfo.Status == status)
                && (documentTypeId == "00" || e.DocumentTypeId == documentTypeId || e.DocumentTypeId == documentTypeOption1 || e.DocumentTypeId == documentTypeOption2)
                && (senderCode == null || e.SenderCode == senderCode)
                && (receiverCode == null || e.ReceiverCode == receiverCode)
                && (providerCode == null || e.TechProviderInfo.TechProviderCode == providerCode)
                ).OrderByDescending(e => e.Timestamp)
                .AsDocumentQuery();
                var result = await query.ExecuteNextAsync<GlobalDataDocument>();

                return Tuple.Create(query.HasMoreResults, result.ResponseContinuation, result.ToList());
            }
            catch (Exception)
            {

                return Tuple.Create<bool, string, List<GlobalDataDocument>>(false, null, new List<GlobalDataDocument>());
            }
        }

        /// <summary>
        /// Read all documents whit filters
        /// </summary>
        /// <param name="continuationToken"></param>
        /// <param name="datesRange"></param>
        /// <param name="status"></param>
        /// <param name="documentTypeId"></param>
        /// <param name="senderCode"></param>
        /// <param name="receiverCode"></param>
        /// <param name="providerCode"></param>
        /// <param name="maxItemCount"></param>
        /// <returns>
        /// A tuple containing the following information:
        /// <list type="bullet">
        /// <item><see cref="Tuple{T1,T2,T3}.Item1"/>: A bool determining if the query has more results.</item>
        /// <item><see cref="Tuple{T1,T2,T3}.Item2"/>: Continuation token to get next results of the query.</item>
        /// <item><see cref="Tuple{T1,T2,T3}.Item3"/>: The list of GlobalDataDocument</item>
        /// </list>
        /// </returns>
        public async Task<Tuple<bool, string, List<GlobalDataDocument>>> ReadAllAsync(string continuationToken, List<string> datesRange, string senderCode,
            string receiverCode, string providerCode, int maxItemCount, string referencesType)
        {
            try
            {
                var options = new FeedOptions()
                {
                    MaxItemCount = maxItemCount,
                    EnableCrossPartitionQuery = true,
                    RequestContinuation = continuationToken
                };

                var optionsSubQuery = new FeedOptions()
                {
                    EnableCrossPartitionQuery = true,
                };

                var subQuery = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, optionsSubQuery)
                                          .Where(invoice => datesRange.Contains(invoice.PartitionKey) 
                                          && invoice.DocumentTypeId == referencesType)
                                          //.SelectMany(references => references.References)
                                          //.Select(references => references.DocumentKey)
                                          .AsDocumentQuery();
                var resultSubQuery = await subQuery.ExecuteNextAsync<string>();

                var query = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink, options)
                                        .Where(invoiceReferenced => datesRange.Contains(invoiceReferenced.PartitionKey))
                                        .Where(invoiceReferenced => resultSubQuery.Contains(invoiceReferenced.DocumentKey))
                                        .OrderByDescending(e => e.Timestamp)
                                        .AsDocumentQuery();
                var result = await query.ExecuteNextAsync<GlobalDataDocument>();

                return Tuple.Create(query.HasMoreResults, result.ResponseContinuation, result.ToList());
            }
            catch (Exception ex)
            {

                return Tuple.Create<bool, string, List<GlobalDataDocument>>(false, null, new List<GlobalDataDocument>());
            }
        }

        public async Task<GlobalDataDocument> ReadOne(string cufe, string partitionKey = null, string senderCode = null, string providerCode = null)
        {
            try
            {
                var query = client.CreateDocumentQuery<GlobalDataDocument>(collectionLink)
                            .Where(e => e.Id == cufe
                            && (partitionKey == null || e.PartitionKey == partitionKey)
                            && (senderCode == null || e.SenderCode == senderCode)
                            && (providerCode == null || e.TechProviderInfo.TechProviderCode == providerCode)
                            ).AsDocumentQuery();
                var result = await query.ExecuteNextAsync<GlobalDataDocument>();
                if (result != null && result.ToList().Count > 0)
                {
                    return result.ToList().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<bool> UpsertDocument(GlobalDataDocument document)
        {
            try
            {
                var response = await client.UpsertDocumentAsync(collectionLink, document);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
