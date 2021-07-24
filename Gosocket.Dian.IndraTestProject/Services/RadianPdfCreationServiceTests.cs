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
        private readonly Mock<IAssociateDocuments> _associateDocuments = new Mock<IAssociateDocuments>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianPdfCreationService(
                              _queryAssociatedEventsService.Object,
                              _fileManager.Object,
                              _globalDocValidationDocumentMetaService.Object,
                              _associateDocuments.Object
                            );

        }
       
    }
}