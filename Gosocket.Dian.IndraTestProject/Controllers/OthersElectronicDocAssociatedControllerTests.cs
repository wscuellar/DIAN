using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Interfaces;
using Moq;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class OthersElectronicDocAssociatedControllerTests
    {
        private OthersElectronicDocAssociatedController _current;

        private readonly Mock<IContributorService> contributorService = new Mock<IContributorService>();
        private readonly Mock<IOthersDocsElecContributorService> othersDocsElecContributorService = new Mock<IOthersDocsElecContributorService>();
        private readonly Mock<IOthersElectronicDocumentsService> othersElectronicDocumentsService = new Mock<IOthersElectronicDocumentsService>();
        private readonly Mock<ITestSetOthersDocumentsResultService> testSetOthersDocumentsResultService = new Mock<ITestSetOthersDocumentsResultService>();
        private readonly Mock<IOthersDocsElecSoftwareService> othersDocsElecSoftwareService = new Mock<IOthersDocsElecSoftwareService>();
        private readonly Mock<IGlobalOtherDocElecOperationService> globalOtherDocElecOperationService = new Mock<IGlobalOtherDocElecOperationService>();

        public void TestInitialize()
        {
            _current = new OthersElectronicDocAssociatedController(
                  contributorService.Object,
              othersDocsElecContributorService.Object,
              othersElectronicDocumentsService.Object,
              testSetOthersDocumentsResultService.Object,
              othersDocsElecSoftwareService.Object,
              globalOtherDocElecOperationService.Object
                );
        }

        [TestMethod()]
        public void IndexTest()
        {
           var result= _current.Index(1);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void CancelRegisterTest()
        {
            var result = _current.CancelRegister(1, "Prueba");

            Assert.IsNotNull(result); 
        }

        [TestMethod()]
        public void EnviarContributorTest()
        {
            var data = new OthersElectronicDocAssociatedViewModel();
            var result = _current.EnviarContributor(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetSetTestResultTest()
        {
            var result = _current.GetSetTestResult(1);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void SetTestDetailsTest()
        {
            var result = _current.SetTestDetails(1);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void CustomersListTest()
        {
            var result = _current.CustomersList(1, "", OtherDocElecState.Habilitado, 1, 10);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void SetupOperationModeTest()
        {
            var result = _current.SetupOperationMode(1);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void SetupOperationModePostTest()
        {
            var data = new OtherDocElecSetupOperationModeViewModel();
            var result = _current.SetupOperationModePost(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void DeleteOperationModeTest()
        {
            var result = _current.DeleteOperationMode(1);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void RestartSetTestResultTest()
        {
            var data = new GlobalTestSetOthersDocumentsResult();
            var result = _current.RestartSetTestResult(data, Guid.NewGuid());
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetSoftwaresByContributorIdTest()
        {
            var result = _current.GetSoftwaresByContributorId(1,1);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetDataBySoftwareIdTest()
        {
            var result = _current.GetDataBySoftwareId(Guid.NewGuid());
            Assert.IsNotNull(result);
        }
    }
}