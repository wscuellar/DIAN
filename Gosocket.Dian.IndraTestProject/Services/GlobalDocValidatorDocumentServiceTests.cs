using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class GlobalDocValidatorDocumentServiceTests
    {
        private GlobalDocValidatorDocumentService _current;

        private readonly Mock<ITableManager> globalDocValidatorDocumentTableManager = new Mock<ITableManager>(); 

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalDocValidatorDocumentService(globalDocValidatorDocumentTableManager.Object);
        }

        [TestMethod()]
        public void EventVerificationTest()
        {
            //arrange  
            _ = globalDocValidatorDocumentTableManager.Setup(x => x.FindByDocumentKey<GlobalDocValidatorDocument>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).
                Returns(  new GlobalDocValidatorDocument() {DocumentKey = "test071"  });
            //act
            var result = _current.EventVerification(new GlobalDocValidatorDocumentMeta());
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocument));
            Assert.AreEqual("test071", result.DocumentKey);
        }

        [TestMethod()]
        public void FindByGlobalDocumentIdTest()
        {
            //arrange  
            _ = globalDocValidatorDocumentTableManager.Setup(x => x.FindByGlobalDocumentId<GlobalDocValidatorDocument>(It.IsAny<string>())).
               Returns(new GlobalDocValidatorDocument() { DocumentKey = "test072" });
            //act
            var result = _current.FindByGlobalDocumentId("code");
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocument));
            Assert.AreEqual("test072", result.DocumentKey);
        }
    }
}