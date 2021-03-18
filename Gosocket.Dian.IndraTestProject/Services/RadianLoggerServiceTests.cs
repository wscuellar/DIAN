using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianLoggerServiceTests
    {
        private RadianLoggerService _current;

        private readonly Mock<IRadianLoggerManager> _radianLoggerManager = new Mock<IRadianLoggerManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianLoggerService(_radianLoggerManager.Object);
        } 

        [TestMethod()]
        public void InsertOrUpdateRadianLoggerTest()
        {
            //arrange  
            _ = _radianLoggerManager.Setup(x => x.InsertOrUpdateRadianLogger(It.IsAny<RadianLogger>())).Returns(true);
            //act
            var Result = _current.InsertOrUpdateRadianLogger(new RadianLogger());
            //assert
            Assert.IsNotNull(Result);
            Assert.IsTrue(Result);
        }

        [TestMethod()]
        public void GetRadianLoggerTest()
        {
            //arrange  
            _ = _radianLoggerManager.Setup(x => x.GetRadianLogger(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianLogger() { Message = "Message2021" });
            //act
            var Result = _current.GetRadianLogger("partitionKey", "rowKey");
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianLogger));
            Assert.IsTrue(Result.Message.Equals("Message2021"));
        }

        [TestMethod()]
        public void GetAllLoggerTest()
        {
            //arrange  
            _ = _radianLoggerManager.Setup(x => x.GetAllRadianLogger()).Returns(new List<RadianLogger> { new RadianLogger() { Message = "Message2022" } });
            //act
            var Result = _current.GetAllLogger();
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianLogger>));
            Assert.IsTrue(Result.First().Message.Equals("Message2022"));
        }
    }
}