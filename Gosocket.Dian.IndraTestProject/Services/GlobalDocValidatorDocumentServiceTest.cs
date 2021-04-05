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
    class GlobalDocValidatorDocumentServiceTest
    {
        private IGlobalDocValidatorDocumentService _current;
        private readonly Mock<TableManager> _TableManager = new Mock<TableManager>();


        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalDocValidatorDocumentService();
        }

        [TestMethod()]
        public void EventVerification()
        {

            _ = _TableManager.Setup(x => x.FindByDocumentKey<GlobalDocValidatorDocument>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new GlobalDocValidatorDocument());
            var result = _current.EventVerification(It.IsAny<GlobalDocValidatorDocumentMeta>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocPayroll));
        }
        [TestMethod()]
        public void FindByGlobalDocumentId()
        {

            _ = _TableManager.Setup(x => x.FindByPartition<GlobalDocValidatorDocument>(It.IsAny<string>())).Returns(new List<GlobalDocValidatorDocument>());
            var result = _current.FindByGlobalDocumentId(It.IsAny<string>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocument));
        }
    }
}
