using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class GlobalDocValidationDocumentMetaServiceTests
    {

        private GlobalDocValidationDocumentMetaService _current;

        private readonly Mock<ITableManager> documentMetaTableManager = new Mock<ITableManager>();
        private readonly Mock<ITableManager> documentTableManager = new Mock<ITableManager>();
        private readonly Mock<ITableManager> ReferenceAttorneyTableManager = new Mock<ITableManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalDocValidationDocumentMetaService(documentMetaTableManager.Object, documentTableManager.Object, ReferenceAttorneyTableManager.Object);
        }


        [TestMethod()]
        public void DocumentValidationTest()
        {
            //arrange 
            _ = documentMetaTableManager.Setup(x => x.Find<GlobalDocValidatorDocumentMeta>(It.IsAny<string>(), It.IsAny<string>())).
                Returns(new GlobalDocValidatorDocumentMeta() { DocumentKey = "07" });
            //act
            var result = _current.DocumentValidation(It.IsAny<string>());
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocumentMeta));
            Assert.AreEqual("07", result.DocumentKey);
        }

        [TestMethod()]
        public void ReferenceAttorneysTest()
        {
            //arrange 
            _ = ReferenceAttorneyTableManager.Setup(x => x.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(It.IsAny<string>())).
                Returns(new GlobalDocReferenceAttorney() { DocReferencedEndAthorney = "07" });
            //act
            var result = _current.ReferenceAttorneys("documentKey", "documentReferencedKey", "receiverCode", "senderCode");
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalDocReferenceAttorney>));
            Assert.AreEqual("07", result.First().DocReferencedEndAthorney);
        }

        [TestMethod()]
        public void GetAssociatedDocumentsTest()
        {
            //arrange 
            _ = documentMetaTableManager.Setup(x => x.FindpartitionKey<GlobalDocValidatorDocumentMeta>(It.IsAny<string>())).
                Returns(new List<GlobalDocValidatorDocumentMeta>() { new GlobalDocValidatorDocumentMeta() { DocumentKey = "07",EventCode="07" } });
            //act
            var result = _current.GetAssociatedDocuments("documentReferencedKey", "07");
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalDocValidatorDocumentMeta>));
            Assert.AreEqual("07", result.First().DocumentKey);
        }

        [TestMethod()]
        public void EventValidatorTest()
        {
            //arrange 
            _ = documentTableManager.Setup(x => x.FindByDocumentKey<GlobalDocValidatorDocument>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).
                Returns(new GlobalDocValidatorDocument() { DocumentKey = "07" });
            //act
            var result = _current.EventValidator(new GlobalDocValidatorDocumentMeta() { Identifier = "07" });
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocument));
            Assert.AreEqual("07", result.DocumentKey);
        }
    }
}