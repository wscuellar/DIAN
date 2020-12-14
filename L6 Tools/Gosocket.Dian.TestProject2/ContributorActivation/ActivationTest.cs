//using Gosocket.Dian.Application;
//using Gosocket.Dian.Domain;
//using Gosocket.Dian.Domain.Entity;
//using Gosocket.Dian.Infrastructure;
//using Gosocket.Dian.Services.Utils.Helpers;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.WindowsAzure.Storage.Table;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace Gosocket.Dian.TestProject.ContributorActivation
//{
//    [TestClass]
//    public class ActivationTest
//    {
//        private static readonly ContributorService contributorService = new ContributorService();
//        private static readonly ContributorOperationsService contributorOperationsService = new ContributorOperationsService();
//        private static readonly TableManager globalTestSetResultTableManager = new TableManager("GlobalTestSetResult");
//        private static readonly TableManager globalContributorPendingActivationTableManager = new TableManager("GlobalContributorPendingActivation");
//        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
//        private static readonly string sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");
//        private static readonly string sendToActivateContributorUrl = "https://gtpa-function-commons-hab.azurewebsites.net/api/SendToActivateContributor?code=y4RO4OXiqXdFJuerlceuGRfio/DrRngj1YEJ0bmFXfr4C3scQta2Fg==";
//        private static readonly string sendToActivateContributorUrlStaging = "https://gtpa-function-commons-hab-staging.azurewebsites.net/api/SendToActivateContributor?code=C8b/ePRSPG4lHoclCxdZruPGZi4q6lNYf53bcj6jRrNj7lXdcMmbrw==";

//        private static readonly string GlobalStorageProd = "DefaultEndpointsProtocol=https;AccountName=gtpastorageprod;AccountKey=MA56GRAObMBTAitESy+Q8hNkAOvU+VOhyr0KBhL9VBwbpIqDdBT6uAo6JOg8yuMpHECNt2EBHzXTa8LcYUjs8A==;EndpointSuffix=core.windows.net";
//        private static readonly TableManager globalContributorTableManagerProd = new TableManager("GlobalContributor", GlobalStorageProd);
//        private static readonly TableManager globaldocumentValidatorMetaTableManagerProd = new TableManager("GlobalDocValidatorDocumentMeta", GlobalStorageProd);
//        private static readonly TableManager globalDocumentKeyUpperCaseTableManagerProd = new TableManager("GlobalDocumentKeyUpperCaseToFix", GlobalStorageProd);

//        [TestMethod]
//        public void TestRandom()
//        {
//            //var chain = "a|b|c|d".Split('|');
//            var chain = "a".Split('|');
//            Random random = new Random();
//            int zero = 0;
//            int one = 0;
//            int two = 0;
//            int three = 0;
//            int four = 0;
//            for (int i = 0; i < 10000; i++)
//            {
//                int randomNumber = random.Next(0, chain.Length);
//                if (randomNumber == 0) zero++;
//                if (randomNumber == 1) one++;
//                if (randomNumber == 2) two++;
//                if (randomNumber == 3) three++;
//                if (randomNumber == 4) four++;

//                var value = chain[randomNumber];
//            }
//            Console.ReadKey();
//        }

//        [TestMethod]
//        public void TestCheckTestSetResult()
//        {
//            try
//            {
//                var tableManager = new TableManager("GlobalTestSetResultToFix");

//                var toFix = tableManager.FindAll<GlobalTestSetResultToFix>();
//                int count = 0;
//                foreach (var tf in toFix)
//                {
//                    var r = globalTestSetResultTableManager.Find<GlobalTestSetResult>(tf.PartitionKey, tf.RowKey);
//                    var code = r.SenderCode;
//                    r.Deleted = !r.Deleted;
//                    globalTestSetResultTableManager.InsertOrUpdate(r);
//                    count++;
//                }

//                Console.ReadKey();

//                var testSetResults = globalTestSetResultTableManager.FindAll<GlobalTestSetResult>();
//                testSetResults = testSetResults.Where(t => t.Deleted);
//                count = 0;
//                foreach (var t in testSetResults)
//                {
//                    try
//                    {
//                        var softwareId = Guid.Parse(t.SoftwareId);
//                        var operations = contributorOperationsService.Find(t.ContributorId, t.OperationModeId, t.ProviderId, softwareId);
//                        var operation = operations.SingleOrDefault(o => !o.Deleted);
//                        if (operation != null)
//                        {
//                            var entity = new GlobalTestSetResultToFix(t.PartitionKey, t.RowKey) { ContributorId = t.ContributorId, Deleted = t.Deleted };
//                            tableManager.InsertOrUpdate(entity);
//                        }
//                        count++;
//                    }
//                    catch (Exception)
//                    {
//                        Console.ReadKey();
//                    }
//                }
//            }
//            catch (Exception)
//            {

//                throw;
//            }
//        }

//        [TestMethod]
//        public void TestCheckDocumentKeyUpperCase()
//        {
//            var metas = globaldocumentValidatorMetaTableManagerProd.FindAll<GlobalDocValidatorDocumentMeta>();

//            BlockingCollection<GlobalDocumentKeyUpperCaseToFix> documentsKeyUpperCaseToFix = new BlockingCollection<GlobalDocumentKeyUpperCaseToFix>();
//            Parallel.ForEach(metas, new ParallelOptions { MaxDegreeOfParallelism = 100 }, m =>
//            {
//                var hasUpperCase = Regex.IsMatch(m.PartitionKey, "[A-Z]");
//                if (hasUpperCase)
//                {
//                    var globalDocumentKeyUpperCaseToFix = new GlobalDocumentKeyUpperCaseToFix(m.PartitionKey, m.RowKey) { DocumentKeyLowerCase = m.PartitionKey.ToLower() };
//                    documentsKeyUpperCaseToFix.Add(globalDocumentKeyUpperCaseToFix);
//                }
//            });

//            Parallel.ForEach(documentsKeyUpperCaseToFix, new ParallelOptions { MaxDegreeOfParallelism = 100 }, d =>
//            {
//                globalDocumentKeyUpperCaseTableManagerProd.InsertOrUpdate(d);
//            });

//            Console.ReadKey();
//        }

//        [TestMethod]
//        public void TestUpdateGlobalContributorStatus()
//        {
//            var result = globalContributorTableManagerProd.FindAll<GlobalContributor>();
//            result = result.Where(r => r.StatusId == 1);

//            Parallel.ForEach(result, new ParallelOptions { MaxDegreeOfParallelism = 100 }, contributor =>
//            {
//                contributor.StatusId = 4;
//                globalContributorTableManagerProd.InsertOrUpdate(contributor);
//            });

//            Console.ReadKey();
//        }
//        [TestMethod]
//        public void TestFixedErrorsContributors()
//        {
//            var codes = "";
//            var activations = contributorActivationTableManager.FindAll<GlobalContributorActivation>();
//            foreach (var item in activations)
//            {
//                var contributor = contributorService.GetByCode(item.ContributorCode, sqlConnectionStringProd);
//                if (contributor.AcceptanceStatusId != 4)
//                {
//                    codes = $"{codes},{contributor.Code}";
//                }
//            }

//            Console.ReadKey();
//        }


//        public class GlobalContributorToFix : TableEntity
//        {
//            public GlobalContributorToFix() { }

//            public GlobalContributorToFix(string pk, string rk) : base(pk, rk)
//            {
//                // PartitionKey represent nit contributor
//                // RowKey represent contributor type id and software id
//            }

//            public int Id { get; set; }
//        }

//        public class GlobalDocumentKeyUpperCaseToFix : TableEntity
//        {
//            public GlobalDocumentKeyUpperCaseToFix() { }

//            public GlobalDocumentKeyUpperCaseToFix(string pk, string rk) : base(pk, rk)
//            {
//                // PartitionKey represent cufe
//                // RowKey represent cufe
//            }

//            public string DocumentKeyLowerCase { get; set; }
//        }

//        public class GlobalTestSetResultToFix : TableEntity
//        {
//            public GlobalTestSetResultToFix() { }

//            public GlobalTestSetResultToFix(string pk, string rk) : base(pk, rk)
//            {

//            }
//            public int ContributorId { get; set; }
//            public bool Deleted { get; set; }
//        }

//        [TestMethod]
//        public void TestSendToActivateContributor()
//        {
//            try
//            {
//                var contributors = new List<Contributor>();
//                contributors = contributorService.GetContributorsByAcceptanceStatusId((int)Domain.Common.ContributorStatus.Enabled);

//                var count = 0;
//                foreach (var c in contributors)
//                {
//                    var contributorProd = contributorService.Get(c.Id, sqlConnectionStringProd);
//                    if (contributorProd.AcceptanceStatusId != (int)Domain.Common.ContributorStatus.Enabled)
//                    {
//                        var requestObject = new { contributorId = c.Id };
//                        var result = ApiHelpers.ExecuteRequest<string>(sendToActivateContributorUrlStaging, requestObject);
//                        count++;
//                    }
//                }

//                Console.ReadKey();


//                var pendings = globalContributorPendingActivationTableManager.FindAll<ContributorPendingActivation>();

//                count = 0;
//                foreach (var c in pendings)
//                {
//                    var id = int.Parse(c.PartitionKey);
//                    var contributorProd = contributorService.Get(id, sqlConnectionStringProd);
//                    if (contributorProd.AcceptanceStatusId != (int)Domain.Common.ContributorStatus.Enabled)
//                    {
//                        var requestObject = new { contributorId = id };
//                        var result = ApiHelpers.ExecuteRequest<string>(sendToActivateContributorUrlStaging, requestObject);
//                        count++;
//                    }
//                }

//                Console.ReadKey();
//            }
//            catch (Exception)
//            {
//                Assert.IsTrue(false);
//            }
//        }

//        [TestMethod]
//        public void TestProcessPendingActivation()
//        {
//            var statuses = new int[] { 3 };
//            var contributors = contributorService.GetContributorsByAcceptanceStatusesId(statuses);
//            contributors = contributors.Where(c => c.ContributorTypeId == (int)Domain.Common.ContributorType.Biller).ToList();
//            BlockingCollection<int> ids = new BlockingCollection<int>();
//            Parallel.ForEach(contributors, new ParallelOptions { MaxDegreeOfParallelism = 1000 }, c =>
//            {
//                var results = globalTestSetResultTableManager.FindByPartition<GlobalTestSetResult>(c.Code);
//                results = results.Where(r => !r.Deleted && r.Status == (int)Domain.Common.TestSetStatus.Accepted).ToList();
//                if (results.Any())
//                    ids.Add(c.Id);
//            });

//            var count = 0;
//            foreach (var i in ids)
//            {
//                var id = i.ToString();
//                var contributorPendingActivation = new ContributorPendingActivation(id, id);
//                globalContributorPendingActivationTableManager.InsertOrUpdate(contributorPendingActivation);
//                //var requestObject = new { contributorId = i };
//                //var result = ApiHelpers.ExecuteRequest<string>(sendToActivateContributorUrl, requestObject);
//                count++;
//            }

//            Console.ReadKey();
//        }

//        public class ContributorPendingActivation : TableEntity
//        {
//            public ContributorPendingActivation() { }
//            public ContributorPendingActivation(string pk, string rk) : base(pk, rk)
//            { }
//        }
//    }
//}
