//using Gosocket.Dian.Application;
//using Gosocket.Dian.Domain.Entity;
//using Gosocket.Dian.Infrastructure;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Gosocket.Dian.TestProject.Tables
//{
//    [TestClass]
//    public class AzureTableTest
//    {
//        private static readonly ContributorService contributorService = new ContributorService();
//        private static readonly string globalStorageProduction = ConfigurationManager.GetValue("GlobalStorageProduction");
//        private static readonly TableManager docValidatorMetaTableManager = new TableManager("GlobalDocValidatorDocumentMeta");
//        private static readonly TableManager exchangeEmailTableManager = new TableManager("GlobalExchangeEmail");
//        private static readonly TableManager exchangeEmailTableManagerProdcution = new TableManager("GlobalExchangeEmail", globalStorageProduction);


//        [TestMethod]
//        public void TestUpdateUpperCaseToLowerCase()
//        {
//            try
//            {
//                var list = new List<GlobalDocValidatorDocumentMeta>();
//                var result = docValidatorMetaTableManager.FindAll<GlobalDocValidatorDocumentMeta>();

//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex.Message);
//            }
//        }

//        [TestMethod]
//        public void TestInsertOrUpdateExchangeEmails()
//        {
//            try
//            {
//                //var results = exchangeEmailTableManager.FindAll<GlobalExchangeEmail>();
//                var statuses = new int[] { 3, 4 };
//                var results = contributorService.GetContributorsByAcceptanceStatusesId(statuses);
//                results = results.Where(r => !string.IsNullOrEmpty(r.ExchangeEmail)).ToList();
//                Parallel.ForEach(results, new ParallelOptions { MaxDegreeOfParallelism = 1000 }, item =>
//                {
//                    if (item.ExchangeEmail != null)
//                    {
//                        var exchangeEmail = exchangeEmailTableManager.Find<GlobalExchangeEmail>(item.Code, item.Code);
//                        if (exchangeEmail == null || exchangeEmail.Email != item.ExchangeEmail)
//                        {
//                            exchangeEmail = new GlobalExchangeEmail { PartitionKey = item.Code, RowKey = item.Code, Email = item.ExchangeEmail.ToLower() };
//                            exchangeEmailTableManager.InsertOrUpdate(exchangeEmail);
//                        }

//                        exchangeEmail = exchangeEmailTableManagerProdcution.Find<GlobalExchangeEmail>(item.Code, item.Code);
//                        if (exchangeEmail == null || exchangeEmail.Email != item.ExchangeEmail)
//                        {
//                            exchangeEmail = new GlobalExchangeEmail { PartitionKey = item.Code, RowKey = item.Code, Email = item.ExchangeEmail.ToLower() };
//                            exchangeEmailTableManagerProdcution.InsertOrUpdate(exchangeEmail);
//                        }
//                    }

//                });
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex.Message);
//            }
//        }
//    }
//}
