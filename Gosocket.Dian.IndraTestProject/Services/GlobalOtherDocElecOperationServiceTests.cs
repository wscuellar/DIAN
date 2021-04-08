using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class GlobalOtherDocElecOperationServiceTests
    {
        private GlobalOtherDocElecOperationService _current;

        private readonly Mock<ITableManager> globalSoftware = new Mock<ITableManager>();
        private readonly Mock<ITableManager> globalOtherDocElecOperation = new Mock<ITableManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalOtherDocElecOperationService(globalSoftware.Object, globalOtherDocElecOperation.Object);
        }


        [TestMethod]
        public void InsertTest()
        {
            //arrange 
            _ = globalSoftware.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalSoftware>())).Returns(true);
            _ = globalOtherDocElecOperation.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalOtherDocElecOperation>())).Returns(true);
            //act
            var result = _current.Insert(new GlobalOtherDocElecOperation(), new OtherDocElecSoftware() { Id = Guid.NewGuid(), Pin = string.Empty });
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            //arrange  
            _ = globalOtherDocElecOperation.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalOtherDocElecOperation>())).Returns(true);
            //act
            var result = _current.Update(new GlobalOtherDocElecOperation());
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void OperationListTest()
        {
            //arrange  
            _ = globalOtherDocElecOperation.Setup(x => x.FindByPartition<GlobalOtherDocElecOperation>(It.IsAny<string>())).
                Returns(new List<GlobalOtherDocElecOperation>() { new GlobalOtherDocElecOperation() { SoftwareId = "test071" } });
            //act
            var result = _current.OperationList("code");
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalOtherDocElecOperation>));
            Assert.AreEqual("test071", result.First().SoftwareId);
        }

        [TestMethod()]
        public void GetOperationTest()
        {
            //arrange  
            _ = globalOtherDocElecOperation.Setup(x => x.Find<GlobalOtherDocElecOperation>(It.IsAny<string>(), It.IsAny<string>())).
                Returns(new GlobalOtherDocElecOperation() { SoftwareId = "test072" });
            //act
            var result = _current.GetOperation("code", Guid.NewGuid());
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalOtherDocElecOperation));
            Assert.AreEqual("test072", result.SoftwareId);
        }

        [TestMethod()]
        public void IsActiveTest()
        {
            //arrange  
            _ = globalOtherDocElecOperation.Setup(x => x.FindSoftwareRowKey<GlobalOtherDocElecOperation>(It.IsAny<string>(), It.IsAny<string>())).
                Returns(new GlobalOtherDocElecOperation() { SoftwareId = "test072", State = Domain.Common.OtherDocElecState.Habilitado.ToString() });
            //act
            var result = _current.IsActive("code", Guid.NewGuid());
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "State !=  Tests")]
        [DataRow(2, DisplayName = "State = Tests")]
        public void EnableParticipantTest(int input)
        {
            GlobalOtherDocElecOperation result;

            switch (input)
            {
                case 1:
                    //arrange  
                    _ = globalOtherDocElecOperation.Setup(x => x.Find<GlobalOtherDocElecOperation>(It.IsAny<string>(), It.IsAny<string>())).
                      Returns(new GlobalOtherDocElecOperation() { SoftwareId = "Soft07", State = string.Empty });
                    //act
                    result = _current.EnableParticipant("code", "softwareId");
                    //assert
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(GlobalOtherDocElecOperation));
                    break;

                case 2:
                    //arrange  
                    _ = globalOtherDocElecOperation.Setup(x => x.Find<GlobalOtherDocElecOperation>(It.IsAny<string>(), It.IsAny<string>())).
                      Returns(new GlobalOtherDocElecOperation() { SoftwareId = "Soft07", State = Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Test) });
                    _ = globalOtherDocElecOperation.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalOtherDocElecOperation>())).Returns(true);
                    //act
                    result = _current.EnableParticipant("code", "softwareId");
                    //assert
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(GlobalOtherDocElecOperation));
                    Assert.AreEqual("Soft07", result.SoftwareId);
                    break;
            }
        }

        [TestMethod()]
        public void SoftwareAddTest()
        {
            //arrange 
            _ = globalSoftware.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalSoftware>())).Returns(true);
            //act
            var result = _current.SoftwareAdd(new GlobalSoftware());
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "State !=  Tests")]
        [DataRow(2, DisplayName = "State = Tests")]
        public void EnableParticipantOtherDocumentTest(int input)
        {
            GlobalOtherDocElecOperation result;

            switch (input)
            {
                case 1:
                    //arrange  
                    _ = globalOtherDocElecOperation.Setup(x => x.FindSoftwareId<GlobalOtherDocElecOperation>(It.IsAny<string>(), It.IsAny<string>())).
                      Returns(new GlobalOtherDocElecOperation() { SoftwareId = "Soft07", State = string.Empty });
                    //act
                    result = _current.EnableParticipantOtherDocument("code", "softwareId", new OtherDocElecContributor() { OtherDocElecContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    //assert
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(GlobalOtherDocElecOperation));
                    break;

                case 2:
                    //arrange  
                    _ = globalOtherDocElecOperation.Setup(x => x.FindSoftwareId<GlobalOtherDocElecOperation>(It.IsAny<string>(), It.IsAny<string>())).
                      Returns(new GlobalOtherDocElecOperation() { SoftwareId = "Soft07", State = Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Test) });
                    _ = globalOtherDocElecOperation.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalOtherDocElecOperation>())).Returns(true);
                    //act
                    result = _current.EnableParticipantOtherDocument("code", "softwareId", new OtherDocElecContributor() { OtherDocElecContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    //assert
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(GlobalOtherDocElecOperation));
                    Assert.AreEqual("Soft07", result.SoftwareId);
                    break;
            }
        }

        [TestMethod()]
        public void DeleteTest()
        {
            //arrange  
            _ = globalOtherDocElecOperation.Setup(x => x.Find<GlobalOtherDocElecOperation>(It.IsAny<string>(), It.IsAny<string>())).
                Returns(new GlobalOtherDocElecOperation() { SoftwareId = "test071" });
            _ = globalOtherDocElecOperation.Setup(x => x.Delete(It.IsAny<GlobalOtherDocElecOperation>())).
                Returns(true);
            //act
            var result = _current.Delete("code", "softwareId");
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }
    }
}