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
    public class GlobalRadianOperationServiceTests
    {
        private GlobalRadianOperationService _current;

        private readonly Mock<ITableManager> globalSoftware = new Mock<ITableManager>();
        private readonly Mock<ITableManager> globalRadianOperations = new Mock<ITableManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalRadianOperationService(globalSoftware.Object, globalRadianOperations.Object);
        }
       
        [TestMethod()]
        public void InsertTest()
        {
            //arrange 
            _ = globalSoftware.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalSoftware>())).Returns(true);
            _ = globalRadianOperations.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalRadianOperations>())).Returns(true);
            //act
            var result = _current.Insert(new GlobalRadianOperations(), new RadianSoftware() { Id = Guid.NewGuid(), Pin = string.Empty });
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            //arrange  
            _ = globalRadianOperations.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalRadianOperations>())).Returns(true);
            //act
            var result = _current.Update(new GlobalRadianOperations());
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void OperationListTest()
        {
            //arrange  
            _ = globalRadianOperations.Setup(x => x.FindByPartition<GlobalRadianOperations>(It.IsAny<string>())).
                Returns(new List<GlobalRadianOperations>() { new GlobalRadianOperations() { RadianState = "test071" } });
            //act
            var result = _current.OperationList("code");
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalRadianOperations>));
            Assert.AreEqual("test071", result.First().RadianState);
        }

        [TestMethod()]
        public void GetOperationTest()
        {
            //arrange  
            _ = globalRadianOperations.Setup(x => x.Find<GlobalRadianOperations>(It.IsAny<string>(), It.IsAny<string>())).
                Returns(new GlobalRadianOperations() { RadianState = "test072" });
            //act
            var result = _current.GetOperation("code", Guid.NewGuid());
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalRadianOperations));
            Assert.AreEqual("test072", result.RadianState);
        }

        [TestMethod()]
        public void IsActiveTest()
        {
            //arrange  
            _ = globalRadianOperations.Setup(x => x.Find<GlobalRadianOperations>(It.IsAny<string>(), It.IsAny<string>())).
                Returns(new GlobalRadianOperations() { RadianState = Domain.Common.OtherDocElecState.Habilitado.ToString() });
            //act
            var result = _current.IsActive("code", Guid.NewGuid());
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
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
        public void EnableParticipantRadianTest(int input)
        {
            GlobalRadianOperations result;

            switch (input)
            {
                case 1:
                    //arrange  
                    _ = globalRadianOperations.Setup(x => x.Find<GlobalRadianOperations>(It.IsAny<string>(), It.IsAny<string>())).
                      Returns(new GlobalRadianOperations() { RadianState = string.Empty });
                    //act
                    result = _current.EnableParticipantRadian("code", "softwareId", new RadianContributor());
                    //assert
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(GlobalRadianOperations));
                    break;

                case 2:
                    //arrange  
                    _ = globalRadianOperations.Setup(x => x.Find<GlobalRadianOperations>(It.IsAny<string>(), It.IsAny<string>())).
                      Returns(new GlobalRadianOperations() { RadianState = Domain.Common.EnumHelper.GetDescription(Domain.Common.OtherDocElecState.Test) });
                    _ = globalRadianOperations.Setup(x => x.InsertOrUpdate(It.IsAny<GlobalRadianOperations>())).Returns(true);
                    //act
                    result = _current.EnableParticipantRadian("code", "softwareId", new RadianContributor());
                    //assert
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(GlobalRadianOperations));
                    Assert.AreEqual("Habilitado", result.RadianState);
                    break;
            }
        }

        [TestMethod()]
        public void DeleteTest()
        {
            //arrange  
            _ = globalRadianOperations.Setup(x => x.Find<GlobalRadianOperations>(It.IsAny<string>(), It.IsAny<string>())).
                Returns(new GlobalRadianOperations() { RadianState = "test071" });
            _ = globalRadianOperations.Setup(x => x.Delete(It.IsAny<GlobalRadianOperations>())).
                Returns(true);
            //act
            var result = _current.Delete("code", "v");
            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }
    }
}