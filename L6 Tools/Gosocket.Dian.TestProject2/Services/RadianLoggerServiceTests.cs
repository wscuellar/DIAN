using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianLoggerServiceTests
    {
        private Mock<IRadianLoggerService> _current = new Mock<IRadianLoggerService>();

        private readonly Mock<IRadianLoggerManager> _radianLoggerManager = new Mock<IRadianLoggerManager>();


        [TestInitialize()]
        public void TestInitialize()
        {
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
            _current.Setup(t => t.InsertOrUpdateRadianLogger(logger)).Returns(true);

            // Act
            bool actual = _current.Object.InsertOrUpdateRadianLogger(logger);

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        public void GetRadianLoggerTest()
        {
            // arrange 
            string partitionKey = string.Empty;
            string rowKey = string.Empty;
            _current.Setup(t => t.GetRadianLogger(partitionKey,rowKey)).Returns(It.IsAny<RadianLogger>());

            // Act
            var actual = _current.Object.GetRadianLogger(partitionKey, rowKey);

            //Assert
            Assert.AreEqual(null, actual);
        }

        [TestMethod()]
        public void GetAllTestSetTest()
        {
            // arrange 
            _current.Setup(t => t.GetAllLogger()).Returns(It.IsAny<List<RadianLogger>>());

            //act
            var actual =_current.Object.GetAllLogger();
            //Assert
            Assert.IsNull(actual);
        }
    }
}