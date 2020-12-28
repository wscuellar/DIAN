using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Managers
{
    public class TestSetOthersDocumentsManager
    {
        private static readonly TableManager testSetManager = new TableManager("GlobalTestSetOthersDocuments");

        public TestSetOthersDocumentsManager()
        {

        }

        public GlobalTestSetOthersDocuments GetTestSet(string partitionKey, string rowKey)
        {
            try
            {
                return testSetManager.Find<GlobalTestSetOthersDocuments>(partitionKey, rowKey);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return null;
        }

        public bool InsertTestSet(GlobalTestSetOthersDocuments testSet)
        {
            return testSetManager.InsertOrUpdate(testSet);
        }

        public bool UpdateTestSet(GlobalTestSetOthersDocuments testSet)
        {
            return testSetManager.InsertOrUpdate(testSet);
        }

        public IEnumerable<GlobalTestSetOthersDocuments> GetAllTestSet(List<ElectronicDocument> electronicDocuments)
        {
            List<GlobalTestSetOthersDocuments> testSets = new List<GlobalTestSetOthersDocuments>();
            try
            {

                foreach (var electronicDocument in electronicDocuments)
                {
                    foreach (OperationMode item in new ContributorService().GetOperationModes())
                    {
                        var res = testSetManager.GetOthersDocuments<GlobalTestSetOthersDocuments>(electronicDocument.Id.ToString(), item.Id.ToString());

                        if (res != null && res.Count > 0)
                            testSets.AddRange(res);
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return new List<GlobalTestSetOthersDocuments>();
            }

            return testSets;
        }

    }
}