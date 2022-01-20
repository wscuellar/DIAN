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
                Uri collectionLink = UriFactory.CreateDocumentCollectionUri("List", "NumberingRange");
                await client.CreateDocumentAsync(collectionLink, numberingRange);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<NumberingRange> GetNumberingRangeByOtherDocElecContributor(long otherDocElecContributorId)
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                string sql = "SELECT * FROM c where  c.Prefix='" + prefijo + "'  and  c.NumberFrom  <=" + range+ "  AND c.NumberTo >=" + range  + "  AND c.IdDocumentTypePayroll  ='" + tipo + "'  and c.State = 1";
                var DepartamentData = new List<NumberingRange>();
                IDocumentQuery<NumberingRange> QueryData = client.CreateDocumentQuery<NumberingRange>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange"), sql).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<NumberingRange>().Result;
                return result.ToList();


            }
            catch (Exception e)
            {
                return new List<NumberingRange>();

            }
        }
    }
}