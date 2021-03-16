using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Application;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Web.Models;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class OthersElectronicDocumentsControllerTests
    {

        private OthersElectronicDocumentsController _current;

        private readonly Mock<IOthersElectronicDocumentsService> othersElectronicDocumentsService = new Mock<IOthersElectronicDocumentsService>();
        private readonly Mock<IOthersDocsElecContributorService> othersDocsElecContributorService = new Mock<IOthersDocsElecContributorService>();
        private readonly Mock<IContributorService> contributorService = new Mock<IContributorService>();
        private readonly Mock<IElectronicDocumentService> electronicDocumentService = new Mock<IElectronicDocumentService>();
        private readonly Mock<IOthersDocsElecSoftwareService> othersDocsElecSoftwareService = new Mock<IOthersDocsElecSoftwareService>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersElectronicDocumentsController(
                 othersElectronicDocumentsService.Object,
             othersDocsElecContributorService.Object,
             contributorService.Object,
             electronicDocumentService.Object,
             othersDocsElecSoftwareService.Object
                );
        }

        [TestMethod()]
        public void IndexTest()
        {
            var result = _current.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void AddOrUpdateTest()
        {
            var data = new ValidacionOtherDocsElecViewModel();
            var result = _current.AddOrUpdate(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void AddOrUpdateContributorTest()
        {
            var data = new OthersElectronicDocumentsViewModel();
            var result = _current.AddOrUpdateContributor(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void AddParticipantsTest()
        {
            var result = _current.AddParticipants(1, "");
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void AddTest()
        {
            var data = new ValidacionOtherDocsElecViewModel();
            var result = _current.Add(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void ValidationTest()
        {
            var data = new ValidacionOtherDocsElecViewModel();
            var result = _current.Validation(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetSoftwaresByContributorIdTest()
        {
            var result = _current.GetSoftwaresByContributorId(1, 1);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetDataBySoftwareIdTest()
        {
            var result = _current.GetDataBySoftwareId(Guid.NewGuid());
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void CancelRegisterTest()
        {
            var result = _current.CancelRegister(1, "");
            Assert.IsNotNull(result);

        }
    }
}