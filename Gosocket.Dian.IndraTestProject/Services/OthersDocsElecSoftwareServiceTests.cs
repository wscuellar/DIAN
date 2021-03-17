using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class OthersDocsElecSoftwareServiceTests
    {

        private OthersDocsElecSoftwareService _current;

        private readonly Mock<IOthersDocsElecSoftwareRepository> _othersDocsElecSoftwareRepository = new Mock<IOthersDocsElecSoftwareRepository>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersDocsElecSoftwareService(_othersDocsElecSoftwareRepository.Object);
        }

        [TestMethod()]
        public void GetTest()
        {
            Guid guidPrueba = Guid.NewGuid();
            //arrange 
            _ = _othersDocsElecSoftwareRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecSoftware, bool>>>()))
                .Returns(new OtherDocElecSoftware() { Id = guidPrueba });
            //act
            var Result = _current.Get(guidPrueba);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(OtherDocElecSoftware));
            Assert.IsTrue(Result.Id.Equals(guidPrueba));
        }

        [TestMethod()]
        public void GetBySoftwareIdTest()
        {
            Guid guidPrueba = Guid.NewGuid();
            //arrange 
            _ = _othersDocsElecSoftwareRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecSoftware, bool>>>()))
                .Returns(new OtherDocElecSoftware() { Id = guidPrueba });
            //act
            var Result = _current.GetBySoftwareId(guidPrueba);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(OtherDocElecSoftware));
            Assert.IsTrue(Result.Id.Equals(guidPrueba));
        }

        [TestMethod()]
        public void ListTest()
        {
            Guid guidPrueba = Guid.NewGuid();
            int ContributorId = 0;
            //arrange 
            _ = _othersDocsElecSoftwareRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecSoftware, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PagedResult<OtherDocElecSoftware> { Results = new List<OtherDocElecSoftware> { new OtherDocElecSoftware() { Id = guidPrueba } } });
            //act
            var Result = _current.List(ContributorId);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<OtherDocElecSoftware>));
            Assert.IsTrue(Result.First().Id.Equals(guidPrueba));
        }

        [TestMethod()]
        public void DeleteSoftwareTest()
        {
            Guid guidPrueba = Guid.NewGuid(); 
            //arrange 
            _ = _othersDocsElecSoftwareRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecSoftware, bool>>>()))
                .Returns(new OtherDocElecSoftware() { Id = guidPrueba });
            _ = _othersDocsElecSoftwareRepository.Setup(x => x.AddOrUpdate(It.IsAny<OtherDocElecSoftware>())).Returns(guidPrueba);
            //act
            var Result = _current.DeleteSoftware(guidPrueba);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(Guid));
            Assert.IsTrue(Result.Equals(guidPrueba));
        }

        [TestMethod()]
        public void GetSoftwareStatusNameTest()
        {
            string resultPrueba = "0707";
            int id = 0;
            //arrange 
            _ = _othersDocsElecSoftwareRepository.Setup(x => x.GetSoftwareStatusName(It.IsAny<int>())).Returns(resultPrueba);
            //act
            var Result = _current.GetSoftwareStatusName(id);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(string));
            Assert.IsTrue(Result.Equals(resultPrueba));
        }

        [TestMethod()]
        public void GetSoftwaresByProviderTechnologicalServicesTest()
        { 
            Guid guidPrueba = Guid.NewGuid();
            int contributorId = 7;
            int electronicDocumentId = 77;
            int contributorTypeId = 777;
            string state= "state";

            //arrange 
            _ = _othersDocsElecSoftwareRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecSoftware, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PagedResult<OtherDocElecSoftware> { Results = new List<OtherDocElecSoftware> { new OtherDocElecSoftware() { Id = guidPrueba } } });
            //act
            var Result = _current.GetSoftwaresByProviderTechnologicalServices(contributorId, electronicDocumentId, contributorTypeId, state);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<OtherDocElecSoftware>));
            Assert.IsTrue(Result.First().Id.Equals(guidPrueba));
        }
    }
}