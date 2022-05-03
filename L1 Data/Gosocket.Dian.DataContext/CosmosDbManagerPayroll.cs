using Gosocket.Dian.Domain.Cosmos;
using Newtonsoft.Json;
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
        private static readonly string databaseId = "PayrollElectronic";

        private static readonly ConnectionPolicy connectionPolicy = new ConnectionPolicy { UserAgentSuffix = " samples-net/3" };


        //Reusable instance of DocumentClient which represents the connection to a DocumentDB endpoint
        private static DocumentClient client = new DocumentClient(new Uri(endpointUrl), authorizationKey);


        public async Task<bool> UpsertDocumentPayroll_All(Payroll_All document)
        {
            try
            {
                var collection = "Payroll_All";
                var databaseId = "PayrollElectronic";
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
                Uri collectionLink = UriFactory.CreateDocumentCollectionUri("PayrollElectronic", collection);
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
                Uri collectionLink = UriFactory.CreateDocumentCollectionUri("PayrollElectronic", collection);
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
                Uri collectionLink = UriFactory.CreateDocumentCollectionUri("PayrollElectronic", collection);
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
                var sql = "Select * from List c where  c.IdList=2 and c.PartitionKey=2 ";
                var result = new List<CoinType>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.Select(x => new CoinType() { IdCoinType = x.IdSubList, CompositeNameCoinType = x.CompositeName, NameCoinType = x.ListName }).ToList();

                }
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
                var result = new List<ContractType>();
                var sql = "Select * from List c where  c.IdList=3  and c.PartitionKey=3 ";
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    result = r.Select(x => new ContractType() { IdContractType = x.IdSubList, CompositeName = x.CompositeName, NameContractType = x.ListName }).ToList();

                }
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
                var sql = "Select * from List c where  c.IdList=4  and c.PartitionKey=4";
                var result = new List<DocumentTypes>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.Select(x => new DocumentTypes() { IdDocumentType = x.IdSubList, CompositeName = x.CompositeName, NameDocumentType = x.ListName }).ToList();

                }
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
                var sql = "Select * from List c where  c.IdList=9  and c.PartitionKey=9 ";
                var result = new List<SubWorkerType>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.Select(x => new SubWorkerType() { IdSubWorkerType = x.IdSubList, CompositeName = x.CompositeName, NameSubWorkerType = x.ListName }).ToList();

                }
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
                var sql = "Select * from List c where  c.IdList=8  and c.PartitionKey=8";
                var result = new List<WorkerType>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.Select(x => new WorkerType() { IdWorkerType = x.IdSubList, CompositeName = x.CompositeName, NameWorkerType = x.ListName }).ToList();

                }
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
                var sql = "Select * from List c where  c.IdList=7  and c.PartitionKey=7";
                var result = new List<PeriodPayroll>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.Select(x => new PeriodPayroll() { IdPeriodPayroll = x.IdSubList, CompositeNamePeriodPayroll = x.CompositeName, NamePeriodPayroll = x.ListName }).ToList();

                }
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
                var sql = "Select * from List c where  c.IdList=5  and c.PartitionKey=5";
                var result = new List<PaymentForm>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.Select(x => new PaymentForm() { IdPaymentForm = x.IdSubList, CompositeName = x.CompositeName, NamePaymentForm = x.ListName }).ToList();

                }
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
                var sql = "Select * from List c where  c.IdList=6   and c.PartitionKey=6";
                var result = new List<PaymentMethod>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.Select(x => new PaymentMethod() { IdPaymentMethod = x.IdSubList, CompositeName = x.CompositeName, NamePaymentMethod = x.ListName }).ToList();

                }
                return result.ToList();

            }
            catch (Exception e)
            {
                return new List<PaymentMethod>();
            }
        }
        public async Task<List<NumberingRangeCos>> getNumberingRange()
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

                var DepartamentData = new List<NumberingRangeCos>();
                IDocumentQuery<NumberingRangeCos> QueryData = client.CreateDocumentQuery<NumberingRangeCos>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange"), queryOptions).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<NumberingRangeCos>().Result;
                return result.ToList();
            }
            catch (Exception e)
            {
                return new List<NumberingRangeCos>();
            }
        }

        public async Task<List<NumberingRangeCos>> GetNumberingRangeByTypeDocument(string prefijo, string range,string tipo,string account)
        {
            try
            {

                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
                string sql = "SELECT * FROM c where  c.Prefix='" + prefijo + "'  and  c.NumberFrom  <=" + range + "  AND c.NumberTo >=" + range + "  AND c.IdDocumentTypePayroll  ='" + tipo + "'  and c.State = 1 and c.PartitionKey= '" + account + "'";
                var DepartamentData = new List<NumberingRangeCos>();
                IDocumentQuery<NumberingRangeCos> QueryData = client.CreateDocumentQuery<NumberingRangeCos>(
                              UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange"), sql).AsDocumentQuery();
                var result = (QueryData).ExecuteNextAsync<NumberingRangeCos>().Result;
                return result.ToList();


            }
            catch (Exception e)
            {
                return new List<NumberingRangeCos>();

            }
        }

        public async Task<NumberingRangeCos> ConsumeNumberingRange(string IdNumberingRange, string account)
        {
            var ret = new List<NumberingRangeCos>();
            string sql = "SELECT * FROM c where  c.id='" + IdNumberingRange + "' and c.PartitionKey='" + account + "'";
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            IDocumentQuery<NumberingRangeCos> query = client.CreateDocumentQuery<NumberingRangeCos>(UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange"), sql).AsDocumentQuery();

            while (query.HasMoreResults)
                ret.AddRange(await query.ExecuteNextAsync<NumberingRangeCos>());
            if (ret.FirstOrDefault().CurrentNumber > ret.FirstOrDefault().NumberTo)
                return null;
            NumberingRangeCos result = ret.FirstOrDefault();
            Int64 currentValue = Int64.Parse(result.CurrentNumber.ToString());
            if (currentValue <= result.NumberTo)
            {
                result.CurrentNumber = currentValue + 1;

                await client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri("Lists", "NumberingRange"), result);

                return ret.FirstOrDefault();
            }

            return null;
        }
        public async Task<List<List>> getRegimen()
        {
            try
            {
                var sql = "Select * from List c where  c.IdList=11   and c.PartitionKey=11";
                var result = new List<List>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.ToList();

                }
                return result.ToList();

            }
            catch (Exception e)
            {
                return new List<List>();
            }
        }
        public async Task<List> getUnidad(string id)
        {
            try
            {
                var sql = "Select * from List c where  c.IdList=14   and c.PartitionKey=14 and  c.IdSubList='" + id + "'  ";
                var result = new List<List>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.ToList();

                }
                return result.FirstOrDefault();

            }
            catch (Exception e)
            {
                return new List();
            }
        }
        public async Task<List<List>> getFormapago()
        {
            try
            {
                var sql = "Select * from List c where  c.IdList=22   and c.PartitionKey=22";
                var result = new List<List>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.ToList();

                }
                return result.ToList();

            }
            catch (Exception e)
            {
                return new List<List>();
            }
        }

        public async Task<List<List>> getTipoOperacion()
        {
            try
            {
                var sql = "Select * from List c where  c.IdList=24  and c.PartitionKey=24";
                var result = new List<List>();
                IDocumentQuery<List> query = client.CreateDocumentQuery<List>(
                  UriFactory.CreateDocumentCollectionUri("Lists", "List"),
                  sql)
                  .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var r = await query.ExecuteNextAsync<List>();
                    //						ret.Add(new List< AccountType> { IdAccountType = r.FirstOrDefault().IdSubList, CompositeName = r.FirstOrDefault().CompositeName,NameAccountType=r.FirstOrDefault().ListName });
                    result = r.ToList();

                }
                return result.ToList();

            }
            catch (Exception e)
            {
                return new List<List>();
            }
        }
    }
    public partial class NumberingRangeCos
    {
        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("DocumentKey")]
        public string DocumentKey { get; set; }

        [JsonProperty("AccountId")]
        public Guid AccountId { get; set; }

        [JsonProperty("IDNumberingRange")]
        public long IdNumberingRange { get; set; }

        [JsonProperty("IdDocumentTypePayroll")]
        public string IdDocumentTypePayroll { get; set; }

        [JsonProperty("DocumentTypePayroll")]
        public string DocumentTypePayroll { get; set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; set; }

        [JsonProperty("NumberFrom")]
        public long NumberFrom { get; set; }

        [JsonProperty("NumberTo")]
        public long NumberTo { get; set; }

        [JsonProperty("CurrentNumber")]
        public long CurrentNumber { get; set; }

        [JsonProperty("TypeXML")]
        public object TypeXml { get; set; }

        [JsonProperty("Current")]
        public string Current { get; set; }

        [JsonProperty("N102")]
        public string N102 { get; set; }

        [JsonProperty("N103")]
        public string N103 { get; set; }

        [JsonProperty("State")]
        public long State { get; set; }

        [JsonProperty("CreationDate")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("id")]
        public Guid id { get; set; }

        public string ResolutionNumber { get; set; }
        public string ExpirationDate { get; set; }
        public long OtherDocElecContributorOperation { get; set; }
    }
}
