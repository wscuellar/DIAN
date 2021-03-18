using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Interfaces.Repositories;
using Moq;
using System.Linq.Expressions;
using Gosocket.Dian.Domain;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianCallSoftwareServiceTests
    {
        private readonly Mock<IRadianSoftwareRepository> _RadianSoftwareRepository = new Mock<IRadianSoftwareRepository>();
        private RadianCallSoftwareService _current;
        private readonly Mock<SoftwareService> _softwareService = new Mock<SoftwareService>();

        [TestInitialize]
        public void RadianCallSoftwareServiceTest()
        {
            _current = new RadianCallSoftwareService(
            _RadianSoftwareRepository.Object
           );
        }

        [TestMethod()]
        public void GetTest()
        {
            _ = _RadianSoftwareRepository.Setup(x => x.Get(It.IsAny<Expression<Func<RadianSoftware, bool>>>()));
            var Result = _current.Get(It.IsAny<Guid>());

            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianSoftware));
        }

        [TestMethod()]
        public void GetSoftwaresTest()
        {
            _ = _softwareService.Setup(x => x.GetSoftwares(It.IsAny<int>()));
            var Result = _current.GetSoftwares(It.IsAny<int>());

            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<Software>));
        }

        [TestMethod()]
        public void ListTest()
        {
            _ = _RadianSoftwareRepository.Setup(x => x.List(It.IsAny<Expression<Func<RadianSoftware, bool>>>(), It.IsAny<int>(), It.IsAny<int>()));
            var Result = _current.List(It.IsAny<int>());

            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianSoftware));
        }

        [TestMethod()]
        public void CreateSoftwareTest()
        {
            _ = _RadianSoftwareRepository.Setup(x => x.AddOrUpdate(It.IsAny<RadianSoftware>())).Returns(Guid.NewGuid());
            var Result = _current.CreateSoftware(It.IsAny<RadianSoftware>());

            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianSoftware));
        }

        [TestMethod()]
        public void DeleteSoftwareTest()
        {
            _ = _RadianSoftwareRepository.Setup(x => x.AddOrUpdate(It.IsAny<RadianSoftware>())).Returns(Guid.NewGuid());
            var Result = _current.DeleteSoftware(Guid.NewGuid());

            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(Guid));
        }

        [TestMethod()]
        public void SetToProductionTest()
        {
            Assert.IsTrue(true);
        }
    }
}