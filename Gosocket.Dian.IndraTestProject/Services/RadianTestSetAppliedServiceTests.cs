using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianTestSetAppliedServiceTests
    {
        private RadianTestSetAppliedService _current;

        private readonly Mock<IRadianTestSetResultManager> _radianTestSetResultManager = new Mock<IRadianTestSetResultManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianTestSetAppliedService(_radianTestSetResultManager.Object);
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
            string partitionKey = "partitionKey";
            string rowKey = "rowKey";
            //arrange  
            _ = _radianTestSetResultManager.Setup(x => x.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(new RadianTestSetResult() { Id = "202117" });
            //act
            var Result = _current.GetTestSetResult(partitionKey, rowKey);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianTestSetResult));
            Assert.IsTrue(Result.Id.Equals("202117"));
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin ContributorTypeId")]
        [DataRow(2, DisplayName = "Sin SoftwareId")]
        [DataRow(3, DisplayName = "Sin SenderCode")]
        [DataRow(4, DisplayName = "Finist")]
        public void InsertOrUpdateTestSetTest(int input)
        {
            RadianTestSetResult entity = new RadianTestSetResult(); 
            bool result;
            switch (input)
            {
                case 1:
                    //arrange
                    //act
                    result = _current.InsertOrUpdateTestSet(entity);
                    //Assert  
                    Assert.IsNotNull(result);
                    Assert.IsTrue(!result);
                    break;
                case 2:
                    entity.ContributorTypeId = "ContributorTypeId";
                    //arrange
                    //act
                    result = _current.InsertOrUpdateTestSet(entity);
                    //Assert  
                    Assert.IsNotNull(result);
                    Assert.IsTrue(!result);
                    break;
                case 3:
                    entity.ContributorTypeId = "ContributorTypeId";
                    entity.SoftwareId = "SoftwareId";
                    //arrange
                    //act
                    result = _current.InsertOrUpdateTestSet(entity);
                    //Assert  
                    Assert.IsNotNull(result);
                    Assert.IsTrue(!result);
                    break;
                case 4:
                    entity.ContributorTypeId = "ContributorTypeId";
                    entity.SoftwareId = "SoftwareId";
                    entity.SenderCode = "SenderCode";
                    //arrange
                    _radianTestSetResultManager.Setup(x => x.InsertOrUpdateTestSetResult(It.IsAny<RadianTestSetResult>())).Returns(true);
                    _radianTestSetResultManager.Setup(x => x.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSetResult() { Id = "2021" });
                    //act
                    result = _current.InsertOrUpdateTestSet(entity);
                    //Assert 
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result);
                    break;
            }
        }

        [TestMethod()]
        public void ResetPreviousCountsTest()
        {
            string testSetId = "testSetId";
            //arrange  
            _ = _radianTestSetResultManager.Setup(x => x.ResetPreviousCounts(It.IsAny<string>()))
               .Returns(true);
            //act
            var Result = _current.ResetPreviousCounts(testSetId);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsTrue(Result);
        }
    }
}