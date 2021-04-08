using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class GlobalDocValidatorTrackingServiceTests
    {
        private GlobalDocValidatorTrackingService _current;

        private readonly Mock<ITableManager> globalDocValidatorTrackingTableManager = new Mock<ITableManager>(); 

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalDocValidatorTrackingService(globalDocValidatorTrackingTableManager.Object);
        }

        [TestMethod()]
        public void ListTrackingTest()
        {
            //arrange  
            _ = globalDocValidatorTrackingTableManager.Setup(x => x.FindByPartition<GlobalDocValidatorTracking>(It.IsAny<string>())).
                Returns(new List<GlobalDocValidatorTracking>() { new GlobalDocValidatorTracking() { DocumentTypeCode="test07"} });
            //act
            var result = _current.ListTracking("eventDocumentKey");
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalDocValidatorTracking>));
            Assert.AreEqual("test07", result.First().DocumentTypeCode);
        }
    }
}