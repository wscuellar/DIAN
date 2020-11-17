using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Interfaces.Services;
using Moq;
using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianTestSetResultServiceTests
    {
        private Mock<IRadianTestSetResultService> _current = new Mock<IRadianTestSetResultService>();


      
        [TestMethod()]
        public void GetAllTestSetResultTest()
        {
            // Arrange 

            _current.Setup(t => t.GetAllTestSetResult()).Returns(It.IsAny<List<RadianTestSetResult>>());
            List<RadianTestSetResult> expected = null;

            // Act
            var actual = _current.Object.GetAllTestSetResult();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetTestSetResultTest()
        {
            // arrange 
            String partitionKey = String.Empty;
            String rowKey = string.Empty;
            _current.Setup(t => t.GetTestSetResult(partitionKey, rowKey)).Returns(It.IsAny<RadianTestSetResult>());
            var expected = new RadianTestSetResult();
            //ACT
            var actual = _current.Object.GetTestSetResult(partitionKey, rowKey);

            //Assert
            Assert.IsNull(actual);
        }

        [TestMethod()]
        public void InsertTestSetResultTest()
        {
            // arrange 
            RadianTestSetResult radianTestSetResult = new RadianTestSetResult();
            _current.Setup(t => t.InsertTestSetResult(radianTestSetResult)).Returns(true);
            var expected =false;
            radianTestSetResult = new RadianTestSetResult() {
            };
            //ACT
            var actual = _current.Object.InsertTestSetResult(radianTestSetResult);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetTestSetResultByNitTest()
        {
            // Arrange
            var nit = "1020304050";
            List<RadianTestSetResult> expected = new List<RadianTestSetResult>();
            _current.Setup(t => t.GetTestSetResultByNit(nit)).Returns(expected);

            //Act
            var actual = _current.Object.GetTestSetResultByNit(nit);

            //Assert
            Assert.AreEqual(expected, actual);
        }
    }
}