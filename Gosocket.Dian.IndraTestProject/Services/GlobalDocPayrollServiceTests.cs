using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class GlobalDocPayrollServiceTests
    {
        private GlobalDocPayrollService _current;

        private readonly Mock<ITableManager> payrollTableManager = new Mock<ITableManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalDocPayrollService(payrollTableManager.Object);
        }

        [TestMethod()]
        public void FindTest()
        {
            //arrange  
            _ = payrollTableManager.Setup(x => x.FindByPartition<GlobalDocPayroll>(It.IsAny<string>())).
                Returns(new List<GlobalDocPayroll> { new GlobalDocPayroll() { SoftwareSC = "test071" } });
            //act
            var result = _current.Find("partitionKey");
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocPayroll));
            Assert.AreEqual("test071", result.SoftwareSC);
        }
    }
}