using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Entity;
using Moq;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Infrastructure;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class GlobalDocValidationDocumentMetaServiceTests
    {
        private GlobalDocValidationDocumentMetaService _current;
        private readonly Mock<TableManager> _TableManager = new Mock<TableManager>();
        [TestInitialize]
        public void TestInitialize()
        {
            _current = new GlobalDocValidationDocumentMetaService();
        }



        public void DocumentValidation()
        {
            _ = _TableManager.Setup(x => x.Find<GlobalDocValidatorDocumentMeta>(It.IsAny<string>(), It.IsAny<string>())).Returns(new GlobalDocValidatorDocumentMeta());
            var result = _current.DocumentValidation(It.IsAny<string>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocumentMeta));
        }

        [TestMethod()]
        public void ReferenceAttorneys(string documentKey, string documentReferencedKey, string receiverCode, string senderCode)
        {
            _ = _TableManager.Setup(x => x.FindDocumentReferenceAttorney<GlobalDocReferenceAttorney>(It.IsAny<string>())).Returns(new GlobalDocReferenceAttorney());
            var result = _current.ReferenceAttorneys(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalDocReferenceAttorney>));
        }

        [TestMethod()]
        public void GetAssociatedDocuments()
        {
            _ = _TableManager.Setup(x => x.FindDocumentReferenced<GlobalDocValidatorDocumentMeta>(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<GlobalDocValidatorDocumentMeta>());
            var result = _current.GetAssociatedDocuments(It.IsAny<string>(), It.IsAny<string>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocumentMeta));
        }

        [TestMethod()]
        public void FindReferencedDocuments()
        {
            _ = _TableManager.Setup(x => x.FindDocumentReferenced_TypeId<GlobalDocValidatorDocumentMeta>(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<GlobalDocValidatorDocumentMeta>());
            var result = _current.FindReferencedDocuments(It.IsAny<string>(), It.IsAny<string>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocumentMeta));
        }

        [TestMethod()]
        //Find All referenced documents
        public void FindDocumentByReference()
        {
            _ = _TableManager.Setup(x => x.FindDocumentReferenced_TypeId<GlobalDocValidatorDocumentMeta>(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<GlobalDocValidatorDocumentMeta>());
            var result = _current.FindDocumentByReference(It.IsAny<string>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocumentMeta));
        }

        [TestMethod()]
        public void EventValidator()
        {
            _ = _TableManager.Setup(x => x.FindByDocumentKey<GlobalDocValidatorDocument>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new GlobalDocValidatorDocument());
            var result = _current.EventValidator(It.IsAny<GlobalDocValidatorDocumentMeta>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocumentMeta));
        }
    }
}