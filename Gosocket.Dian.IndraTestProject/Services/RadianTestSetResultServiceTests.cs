using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianTestSetResultServiceTests
    {
        private RadianTestSetResultService _current;

        private readonly Mock<IRadianTestSetResultManager> _radianTestSetResultManager = new Mock<IRadianTestSetResultManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianTestSetResultService(_radianTestSetResultManager.Object);
        }

        [TestMethod()]
        public void GetAllTestSetResultTest()
        {
            //arrange  
            _ = _radianTestSetResultManager.Setup(x => x.GetAllTestSetResult()).Returns(new List<RadianTestSetResult> { new RadianTestSetResult() { Id = "2021" } });
            //act
            var Result = _current.GetAllTestSetResult();
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianTestSetResult>));
            Assert.IsTrue(Result.First().Id.Equals("2021"));
        }

        [TestMethod()]
        public void GetTestSetResultTest()
        {
            string partitionKey= "partitionKey";
            string rowKey = "rowKey"; 
            //arrange  
            _ = _radianTestSetResultManager.Setup(x => x.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>()))
               .Returns( new RadianTestSetResult() { Id = "202117" });
            //act
            var Result = _current.GetTestSetResult(partitionKey, rowKey);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianTestSetResult));
            Assert.IsTrue(Result.Id.Equals("202117"));
        }

        [TestMethod()]
        public void InsertTestSetResultTest()
        {
            RadianTestSetResult testSet = new RadianTestSetResult();
            //arrange  
            _ = _radianTestSetResultManager.Setup(x => x.InsertOrUpdateTestSetResult(It.IsAny<RadianTestSetResult>()))
               .Returns(true);
            //act
            var Result = _current.InsertTestSetResult(testSet);
            //assert
            Assert.IsNotNull(Result); 
            Assert.IsTrue(Result);
        }

        [TestMethod()]
        public void GetTestSetResultByNitTest()
        {
            string nit = "890009";
            //arrange  
            _ = _radianTestSetResultManager.Setup(x => x.GetTestSetResultByNit(It.IsAny<string>()))
               .Returns(new List<RadianTestSetResult>   {
                    new RadianTestSetResult() { Id ="1703" },
                    new RadianTestSetResult() { Id ="1704" }
               });
            //act
            var Result = _current.GetTestSetResultByNit(nit);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianTestSetResult>));
            Assert.IsTrue(Result.Count == 2);
        }
    }
}