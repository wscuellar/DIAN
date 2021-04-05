using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.IndraTestProject.Services
{
    class GlobalDocPayrollServiceTest
    {

        private GlobalDocPayrollService _current;
        private readonly Mock<TableManager> _TableManager = new Mock<TableManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalDocPayrollService();
        }

        [TestMethod()]
        public void Find()
        {

            _ = _TableManager.Setup(x => x.FindByPartition<GlobalDocPayroll>(It.IsAny<string>())).Returns(new List<GlobalDocPayroll>());
            var result = _current.Find(It.IsAny<string>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocPayroll));
        }
    }
}
