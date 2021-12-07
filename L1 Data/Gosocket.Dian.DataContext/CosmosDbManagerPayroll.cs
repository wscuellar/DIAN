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

        public async Task<bool> UpsertDocumentPayrollE(Payroll_Delete document)
        {
            try
            {
                var collection = "Payroll_E";
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

        public async Task<bool> UpsertDocumentPayrollR(Payroll_Replace document)
        {
            try
            {
                var collection = "Payroll_R";
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

        public async Task<List<Countries>> GetCountries()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var listCountries = new List<Countries>();
                IDocumentQuery<Countries> countryQuery = client.CreateDocumentQuery<Countries>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "Country"), queryOptions).AsDocumentQuery();

                var result = (countryQuery).ExecuteNextAsync<Countries>().Result;

                return result.ToList();
            }
            catch (Exception e)
            {


                return null;
            }

        }
        public async Task<List<Departament>> getDepartament()
        {

            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<Departament>();
                IDocumentQuery<Departament> DepartamentQuery = client.CreateDocumentQuery<Departament>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "Department"), queryOptions).AsDocumentQuery();
                var result = (DepartamentQuery).ExecuteNextAsync<Departament>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {


                return new List<Departament>();
            }

        }

        public async Task<List<CoinType>> getCoinType()
        {

            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<CoinType>();
                IDocumentQuery<CoinType> DepartamentQuery = client.CreateDocumentQuery<CoinType>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "CoinType"), queryOptions).AsDocumentQuery();
                var result = (DepartamentQuery).ExecuteNextAsync<CoinType>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<CoinType>();
            }

        }


        public async Task<List<ContractType>> getContractType()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<ContractType>();
                IDocumentQuery<ContractType> DepartamentQuery = client.CreateDocumentQuery<ContractType>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "ContractType"), queryOptions).AsDocumentQuery();
                var result = (DepartamentQuery).ExecuteNextAsync<ContractType>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<ContractType>();
            }
        }

        public async Task<List<City>> getCity()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<City>();
                IDocumentQuery<City> DepartamentQuery = client.CreateDocumentQuery<City>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "City"), queryOptions).AsDocumentQuery();
                var result = (DepartamentQuery).ExecuteNextAsync<City>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<City>();
            }
        }

        public async Task<List<DocumentTypes>> getDocumentType()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<DocumentTypes>();
                IDocumentQuery<DocumentTypes> DepartamentQuery = client.CreateDocumentQuery<DocumentTypes>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "DocumentType"), queryOptions).AsDocumentQuery();
                var result = (DepartamentQuery).ExecuteNextAsync<DocumentTypes>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<DocumentTypes>();
            }
        }

        public async Task<List<SubWorkerType>> getSubWorkerType()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<SubWorkerType>();
                IDocumentQuery<SubWorkerType> DepartamentQuery = client.CreateDocumentQuery<SubWorkerType>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "SubWorkerType"), queryOptions).AsDocumentQuery();
                var result = (DepartamentQuery).ExecuteNextAsync<SubWorkerType>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<SubWorkerType>();
            }
        }

        public async Task<List<WorkerType>> getWorkerType()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<WorkerType>();
                IDocumentQuery<WorkerType> DepartamentQuery = client.CreateDocumentQuery<WorkerType>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "WorkerType"), queryOptions).AsDocumentQuery();
                var result = (DepartamentQuery).ExecuteNextAsync<WorkerType>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<WorkerType>();
            }
        }

    }
}
