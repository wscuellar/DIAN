using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.IndraTestProject.Services
{
    class GlobalDocValidatorTrackingServiceTest
    {
        private IGlobalDocValidatorTrackingService _current;
        private readonly Mock<TableManager> _TableManager = new Mock<TableManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalDocValidatorTrackingService();
        }

        [TestMethod()]
        public void ListTracking()
        {

            _ = _TableManager.Setup(x => x.FindByDocumentKey<GlobalDocValidatorTracking>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new GlobalDocValidatorTracking());
            var result = _current.ListTracking(It.IsAny<string>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorTracking));
        } 
    }
}
