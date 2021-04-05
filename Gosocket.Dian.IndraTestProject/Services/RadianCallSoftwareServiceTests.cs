using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
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
    public class RadianCallSoftwareServiceTests
    { 
        private RadianCallSoftwareService _current;
        private readonly Mock<SoftwareService> _softwareService = new Mock<SoftwareService>();

        private readonly Mock<IRadianSoftwareRepository> _RadianSoftwareRepository = new Mock<IRadianSoftwareRepository>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianCallSoftwareService(_RadianSoftwareRepository.Object);
        } 

        [TestMethod()]
        public void GetTest()
        {
            Guid guidPrueba = Guid.NewGuid();
            //arrange  
            _ = _RadianSoftwareRepository.Setup(x => x.Get(It.IsAny<Expression<Func<RadianSoftware, bool>>>())).Returns(new RadianSoftware() { Id = guidPrueba });
            //act
            var Result = _current.Get(guidPrueba);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianSoftware));
            Assert.IsTrue(Result.Id.Equals(guidPrueba));
        }

        [TestMethod()]
        public void ListTest()
        {
            int id = 707;
            Guid guidPrueba = Guid.NewGuid();
            //arrange  
            _ = _RadianSoftwareRepository.Setup(x => x.List(It.IsAny<Expression<Func<RadianSoftware, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PagedResult<RadianSoftware> { Results = new List<RadianSoftware> { new RadianSoftware() { Id = guidPrueba } } });
            //act
            var Result = _current.List(id);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianSoftware>));
            Assert.IsTrue(Result.First().Id.Equals(guidPrueba));
        }

        [TestMethod()]
        public void CreateSoftwareTest()
        {
            Guid guidPrueba = Guid.NewGuid();
            //arrange  
            _ = _RadianSoftwareRepository.Setup(x => x.AddOrUpdate(It.IsAny<RadianSoftware>())).Returns(guidPrueba);
            //act
            var Result = _current.CreateSoftware(new RadianSoftware());
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianSoftware));
            Assert.IsTrue(Result.Id.Equals(guidPrueba));
        }

        [TestMethod()]
        public void DeleteSoftwareTest()
        {
            Guid guidPrueba = Guid.NewGuid();
            //arrange  
            _ = _RadianSoftwareRepository.Setup(x => x.Get(It.IsAny<Expression<Func<RadianSoftware, bool>>>())).Returns(new RadianSoftware() { Id = guidPrueba });
            _ = _RadianSoftwareRepository.Setup(x => x.AddOrUpdate(It.IsAny<RadianSoftware>())).Returns(guidPrueba);
            //act
            var Result = _current.DeleteSoftware(guidPrueba);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(Guid));
            Assert.IsTrue(Result.Equals(guidPrueba));
        }

        [TestMethod()]
        public void SetToProductionTest()
        {
            Assert.IsTrue(true);
        } 
    }
}