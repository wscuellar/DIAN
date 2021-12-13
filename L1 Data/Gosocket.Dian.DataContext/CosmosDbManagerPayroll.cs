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
                var response = await client.CreateDocumentAsync(collectionLink, document);
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
                var response = await client.CreateDocumentAsync(collectionLink, document);
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
                var response = await client.CreateDocumentAsync(collectionLink, document);
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
                var response = await client.CreateDocumentAsync(collectionLink, document);
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
                IDocumentQuery<Departament> QueryData = client.CreateDocumentQuery<Departament>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "Department"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<Departament>().Result;
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
                IDocumentQuery<CoinType> QueryData = client.CreateDocumentQuery<CoinType>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "CoinType"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<CoinType>().Result;
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
                IDocumentQuery<ContractType> QueryData = client.CreateDocumentQuery<ContractType>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "ContractType"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<ContractType>().Result;
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
                IDocumentQuery<City> QueryData = client.CreateDocumentQuery<City>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "City"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<City>().Result;
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
                IDocumentQuery<DocumentTypes> QueryData = client.CreateDocumentQuery<DocumentTypes>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "DocumentType"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<DocumentTypes>().Result;
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
                IDocumentQuery<SubWorkerType> QueryData = client.CreateDocumentQuery<SubWorkerType>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "SubWorkerType"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<SubWorkerType>().Result;
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
                IDocumentQuery<WorkerType> QueryData = client.CreateDocumentQuery<WorkerType>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "WorkerType"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<WorkerType>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<WorkerType>();
            }
        }

        public async Task<List<PeriodPayroll>> getPeriodPayroll()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<PeriodPayroll>();
                IDocumentQuery<PeriodPayroll> QueryData = client.CreateDocumentQuery<PeriodPayroll>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "PeriodPayroll"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<PeriodPayroll>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<PeriodPayroll>();
            }
        }


        public async Task<List<PaymentForm>> getPaymentForm()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<PaymentForm>();
                IDocumentQuery<PaymentForm> QueryData = client.CreateDocumentQuery<PaymentForm>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "PaymentForm"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<PaymentForm>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<PaymentForm>();
            }
        }

        public async Task<List<PaymentMethod>> getPaymentMethod()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<PaymentMethod>();
                IDocumentQuery<PaymentMethod> QueryData = client.CreateDocumentQuery<PaymentMethod>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "PaymentMethod"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<PaymentMethod>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<PaymentMethod>();
            }
        }
        public async Task<List<NumberingRange>> getNumberingRange()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<NumberingRange>();
                IDocumentQuery<NumberingRange> QueryData = client.CreateDocumentQuery<NumberingRange>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<NumberingRange>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<NumberingRange>();
            }
        }

        public async Task<List<NumberingRange>> GetNumberingRangeByTypeDocument(string prefijo, double range,string tipo)
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

        public async Task<NumberingRange> ConsumeNumberingRange(string IdNumberingRange)
        {
            var ret = new List<NumberingRange>();
            string sql = "SELECT * FROM c where  c.id='"+IdNumberingRange+"'" ;
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            IDocumentQuery<NumberingRange> query = client.CreateDocumentQuery<NumberingRange>(UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange"),sql).AsDocumentQuery();

            while (query.HasMoreResults)
                    ret.AddRange(await query.ExecuteNextAsync<NumberingRange>());
            if (ret.FirstOrDefault().CurrentNumber > ret.FirstOrDefault().NumberTo)
                    return null;
                NumberingRange result = ret.FirstOrDefault();
                Int64 currentValue = Int64.Parse(result.CurrentNumber.ToString());
                if (currentValue <= result.NumberTo)
                {
                    result.CurrentNumber= currentValue + 1;

                await client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange"), result);
                    
                    return ret.FirstOrDefault();
                }
            
            return null;
        }

    }
}
