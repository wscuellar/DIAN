using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianLoggerServiceTests
    {
        private readonly Mock<IRadianLoggerService> _loggerService = new Mock<IRadianLoggerService>();



        [TestMethod()]
        public void RadianLoggerServiceTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertOrUpdateRadianLoggerTest()
        {
            // Arrange
            RadianLogger logger = new RadianLogger();

            logger = new RadianLogger("InsertOrUpdateRadianLoggerTest", DateTime.Now.Ticks.ToString())
            {
                Action = "New Register on Logger",
                Controller = "RadianLoggerService",
                Message = "Test add log",
                RouteData = "",
                StackTrace = ""
            };

            // Act
            bool actual = new bool();
            _loggerService.Setup(t => t.InsertOrUpdateRadianLogger(logger)).Returns(actual);

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        public void GetRadianLoggerTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAllTestSetTest()
        {
            Assert.Fail();
        }
    }
}