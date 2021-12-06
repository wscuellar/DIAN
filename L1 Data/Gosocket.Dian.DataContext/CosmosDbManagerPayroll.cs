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
    public class CosmosDbManagerPayroll
    {
        private static readonly string endpointUrl = ConfigurationManager.GetValue("CosmosDbEndpointUrl");
        private static readonly string authorizationKey = ConfigurationManager.GetValue("CosmosDbAuthorizationKey");
        private static readonly string databaseId = ConfigurationManager.GetValue("CosmosDbDataBaseIdPayroll");
        private static readonly string collectionId = ConfigurationManager.GetValue("CosmosDbCollectionIDPayroll_all");
        private static readonly ConnectionPolicy connectionPolicy = new ConnectionPolicy { UserAgentSuffix = " samples-net/3" };
        

        //Reusable instance of DocumentClient which represents the connection to a DocumentDB endpoint
        private static DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey);


        

        public async Task<bool> UpsertDocumentPayroll_All(Payroll_All document)
        {
            try
            {
                var collection = "Payroll_All";
                Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collection);
                var response = await client.UpsertDocumentAsync(collectionLink, document);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpsertDocumentPayroll(Payroll document)
        {
            try
            {
                var collection = "Payroll";
                Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collection);
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
