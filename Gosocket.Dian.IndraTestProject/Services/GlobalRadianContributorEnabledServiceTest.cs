using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Gosocket.Dian.IndraTestProject.Services
{
    [TestClass()]
    public class GlobalRadianContributorEnabledServiceTest
    {
        private GlobalRadianContributorEnabledService _current;

        private readonly Mock<ITableManager> globalRadianContributorEnabled = new Mock<ITableManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalRadianContributorEnabledService(globalRadianContributorEnabled.Object); 
        }

        [TestMethod()]
        public void InsertTest()
        {
            //arrange
            _ = globalRadianContributorEnabled.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalRadianContributorEnabled>())).Returns(true);
            //act
            var result = _current.Insert(new GlobalRadianContributorEnabled() { PartitionKey = "12345", RowKey = Guid.NewGuid().ToString() });
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);

        }

    }
}
