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

        public async Task<NumberingRange> GetNumberingRangeByOtherDocElecContributor(string accountId, long otherDocElecContributorId)
        {
            try
            {
                var uri = UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange");
                var options = new FeedOptions { MaxItemCount = -1 };
                
                IDocumentQuery<NumberingRange> QueryData = client
                    .CreateDocumentQuery<NumberingRange>(uri, options)
                    .Where(t => t.PartitionKey == accountId && t.OtherDocElecContributorOperation == otherDocElecContributorId)
                    .AsDocumentQuery();

                var result = await QueryData.ExecuteNextAsync<NumberingRange>();

                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
    }
}