using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class TestSetOthersDocumentsResultServiceTests
    {
        private TestSetOthersDocumentsResultService _current;

        private readonly Mock<ITestSetOthersDocumentsResultManager> _testSetOthersDocumentsResultManager = new Mock<ITestSetOthersDocumentsResultManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new TestSetOthersDocumentsResultService(_testSetOthersDocumentsResultManager.Object);
        }

        [TestMethod()]
        public void GetAllTestSetResultTest()
        {
            //arrange 
            _ = _testSetOthersDocumentsResultManager.Setup(x => x.GetAllTestSetResult()).Returns(new List<GlobalTestSetOthersDocumentsResult> { new GlobalTestSetOthersDocumentsResult() { Id = "701" } });
            //act
            var Result = _current.GetAllTestSetResult();

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<GlobalTestSetOthersDocumentsResult>));
            Assert.IsTrue(Result.First().Id.Equals("701"));
        }

        [TestMethod()]
        public void GetTestSetResultTest()
        {
            //arrange 
            _ = _testSetOthersDocumentsResultManager.Setup(x => x.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new GlobalTestSetOthersDocumentsResult() { Id = "702" });
            //act
            var Result = _current.GetTestSetResult("partitionKey", "rowKey");

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(GlobalTestSetOthersDocumentsResult));
            Assert.IsTrue(Result.Id.Equals("702"));
        }

        [TestMethod()]
        public void InsertTestSetResultTest()
        {
            //arrange 
            _ = _testSetOthersDocumentsResultManager.Setup(x => x.InsertOrUpdateTestSetResult(It.IsAny<GlobalTestSetOthersDocumentsResult>()))
                .Returns(true);
            //act
            var Result = _current.InsertTestSetResult(It.IsAny<GlobalTestSetOthersDocumentsResult>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(bool));
            Assert.IsTrue(Result.Equals(true));
        }

        [TestMethod()]
        public void GetTestSetResultByNitTest()
        {
            //arrange 
            _ = _testSetOthersDocumentsResultManager.Setup(x => x.GetTestSetResultByNit(It.IsAny<string>()))
                .Returns(new List<GlobalTestSetOthersDocumentsResult> { new GlobalTestSetOthersDocumentsResult() { Id = "703" } });
            //act
            var Result = _current.GetTestSetResultByNit(It.IsAny<string>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<GlobalTestSetOthersDocumentsResult>));
            Assert.IsTrue(Result.First().Id.Equals("703"));
        }
    }
}