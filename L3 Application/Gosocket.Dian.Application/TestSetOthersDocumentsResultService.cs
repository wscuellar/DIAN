﻿using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class TestSetOthersDocumentsResultService : ITestSetOthersDocumentsResultService
    {
        private readonly ITestSetOthersDocumentsResultManager _testSetOthersDocumentsResultManager;

        public TestSetOthersDocumentsResultService(ITestSetOthersDocumentsResultManager testSetOthersDocumentsResultManager)
        {
            _testSetOthersDocumentsResultManager = testSetOthersDocumentsResultManager;
        }


        public List<GlobalTestSetOthersDocumentsResult> GetAllTestSetResult()
        {
            return _testSetOthersDocumentsResultManager.GetAllTestSetResult().ToList();
        }


        public GlobalTestSetOthersDocumentsResult GetTestSetResult(string partitionKey, string rowKey)
        {
            return _testSetOthersDocumentsResultManager.GetTestSetResult(partitionKey, rowKey);
        }

        public bool InsertTestSetResult(GlobalTestSetOthersDocumentsResult testSet)
        {
            return _testSetOthersDocumentsResultManager.InsertOrUpdateTestSetResult(testSet);
        }

        public List<GlobalTestSetOthersDocumentsResult> GetTestSetResultByNit(string nit)
        {
            return _testSetOthersDocumentsResultManager.GetTestSetResultByNit(nit);
        } 
    }
}