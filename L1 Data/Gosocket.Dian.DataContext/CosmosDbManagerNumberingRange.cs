using Gosocket.Dian.Domain.Cosmos;

using Gosocket.Dian.Infrastructure;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.DataContext
{
    public class CosmosDbManagerNumberingRange
    {
        private static readonly string endpointUrl = ConfigurationManager.GetValue("CosmosDbEndpointUrl");
        private static readonly string authorizationKey = ConfigurationManager.GetValue("CosmosDbAuthorizationKey");
        private static readonly string databaseId = ConfigurationManager.GetValue("CosmosDbDataBaseIdPayroll");
        private static readonly string collectionId = ConfigurationManager.GetValue("CosmosDbCollectionIDPayroll_all");
        private static readonly ConnectionPolicy connectionPolicy = new ConnectionPolicy { UserAgentSuffix = " samples-net/3" };


        //Reusable instance of DocumentClient which represents the connection to a DocumentDB endpoint
        private static DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey);

        public async Task<bool> SaveNumberingRange(NumberingRange numberingRange)
        {
            try
            {
                Uri collectionLink = UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange");
                await client.CreateDocumentAsync(collectionLink, numberingRange);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public NumberingRange GetNumberingRangeByOtherDocElecContributor(long otherDocElecContributorId)
        {
            try
            {
                string sql = $"SELECT * FROM c where c.OtherDocElecContributorOperation = {otherDocElecContributorId}";
                
                var uri = UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange");
                IDocumentQuery<NumberingRange> QueryData = client
                    .CreateDocumentQuery<NumberingRange>(uri, sql, new FeedOptions { MaxItemCount = -1 })
                    .AsDocumentQuery();
                
                var result = QueryData.ExecuteNextAsync<NumberingRange>().Result;
                
                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                return null;

            }
        }
    }
}