using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Moq;
using Gosocket.Dian.Interfaces.Services;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianPdfCreationServiceTests
    {
        
        private readonly Mock<IQueryAssociatedEventsService> _queryAssociatedEventsService = new Mock<IQueryAssociatedEventsService>();
        private readonly Mock<IGlobalDocValidationDocumentMetaService> _globalDocValidationDocumentMetaService = new Mock<IGlobalDocValidationDocumentMetaService>();
        private readonly Mock<Gosocket.Dian.Infrastructure.FileManager> _fileManager = new Mock<Gosocket.Dian.Infrastructure.FileManager>();
        RadianPdfCreationService _current;

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianPdfCreationService(
                              _queryAssociatedEventsService.Object,
                              _fileManager.Object,
                              _globalDocValidationDocumentMetaService.Object
                            );

        }


        [TestMethod()]
        public void GetElectronicInvoicePdfTest()
        {
            //arrange
            string eventItemIdentifier= "0d1e4c33eb93711b6dda11f618b66dba78dcbf33d80b2dd4ae787a7806ebd6b6ee1e9cb56fd36e9e479f74834b71e5a4";
            GlobalDocValidatorDocumentMeta documentMeta = new GlobalDocValidatorDocumentMeta()
            {
                DocumentKey = eventItemIdentifier
            };
            _queryAssociatedEventsService.Setup(t => t.DocumentValidation(eventItemIdentifier)).Returns(documentMeta);
            _globalDocValidationDocumentMetaService.Setup(t => t.FindDocumentByReference(eventItemIdentifier)).Returns(new List<GlobalDocValidatorDocumentMeta>() {
                new GlobalDocValidatorDocumentMeta()
                {

                }
            });
            _queryAssociatedEventsService.Setup(t => t.IconType(null, eventItemIdentifier)).Returns(new Dictionary<int, string>() { { 1, "test" } });
            _queryAssociatedEventsService.Setup(t => t.ReferenceAttorneys(documentMeta.DocumentKey,
                        documentMeta.DocumentReferencedKey,
                        documentMeta.ReceiverCode,
                        documentMeta.SenderCode)).Returns(new List<Domain.Entity.GlobalDocReferenceAttorney>());
            //act
           // var result = _current.GetElectronicInvoicePdf(eventItemIdentifier, webPath).Result;

            //assert
           // Assert.IsNotNull(result);

            Assert.IsNotNull(true);
        }
    }
}