using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianTestSetServiceTests
    {

        private RadianTestSetService _current;

        private readonly Mock<IRadianContributorService> _contributorService = new Mock<IRadianContributorService>();
        private readonly Mock<IRadianTestSetManager> _testSetManager = new Mock<IRadianTestSetManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianTestSetService(_testSetManager.Object, _contributorService.Object);
        }

        [TestMethod()]
        public void GetAllTestSetTest()
        {
            //arrange 
            _ = _testSetManager.Setup(x => x.GetAllTestSet())
                .Returns(new List<RadianTestSet> { new RadianTestSet() { TestSetId = "07" } });
            //act
            var Result = _current.GetAllTestSet();

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianTestSet>));
            Assert.IsTrue(Result.First().TestSetId.Equals("07"));
        }

        [TestMethod]
        [DataRow(1, DisplayName = "RadianOperationMode 1")]
        [DataRow(2, DisplayName = "RadianOperationMode 2")]
        [DataRow(3, DisplayName = "RadianOperationMode 3")]
        [DataRow(4, DisplayName = "RadianOperationMode 4")]
        [DataRow(5, DisplayName = "RadianOperationMode default")]
        public void GetOperationModeTest(int input)
        {
            Domain.RadianOperationMode result = new Domain.RadianOperationMode();
            switch (input)
            {
                case 1:
                    //arrange
                    //act
                    result = _current.GetOperationMode(1);
                    //Assert 
                    Assert.IsTrue(result.Id.Equals((int)RadianOperationModeTestSet.OwnSoftware));
                    break;
                case 2:
                    //arrange
                    //act
                    result = _current.GetOperationMode(2);
                    //Assert ;
                    Assert.IsTrue(result.Id.Equals((int)RadianOperationModeTestSet.SoftwareTechnologyProvider));
                    break;
                case 3:
                    //arrange
                    //act
                    result = _current.GetOperationMode(3);
                    //Assert 
                    Assert.IsTrue(result.Id.Equals((int)RadianOperationModeTestSet.SoftwareTradingSystem));
                    break;
                case 4:
                    //arrange
                    //act
                    result = _current.GetOperationMode(4);
                    //Assert 
                    Assert.IsTrue(result.Id.Equals((int)RadianOperationModeTestSet.SoftwareFactor));
                    break;
                case 5:
                    //arrange
                    //act
                    result = _current.GetOperationMode(5); 
                    break;
            }
            //Assert 
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Domain.RadianOperationMode));
        }

        [TestMethod]
        public void OperationModeListTest()
        {
            //arrange  
            //act
            var Result = _current.OperationModeList(RadianOperationMode.None);

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<Domain.RadianOperationMode>));
            Assert.IsTrue(Result.Any());
        }

        [TestMethod()]
        public void GetTestSetTest()
        {
            //arrange 
            _ = _testSetManager.Setup(x => x.GetTestSet(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new RadianTestSet() { TestSetId = "07" });
            //act
            var Result = _current.GetTestSet("partitionKey", "rowKey");

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianTestSet));
            Assert.IsTrue(Result.TestSetId.Equals("07"));
        }

        [TestMethod()]
        public void InsertTestSetTest()
        {
            _ = _testSetManager.Setup(x => x.InsertTestSet(It.IsAny<RadianTestSet>()))
                .Returns(true);
            //act
            var Result = _current.InsertTestSet(It.IsAny<RadianTestSet>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(bool));
            Assert.IsTrue(Result.Equals(true));
        }
    }
}