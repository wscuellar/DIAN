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

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class GlobalDocPayrollServiceTest
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
            Assert.IsNotNull(true); 
        }
    }
}
